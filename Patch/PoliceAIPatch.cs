﻿using HarmonyLib;
using System;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using MoreEffectiveTransfer.Util;


namespace MoreEffectiveTransfer.Patch
{

    public static class PoliceAIPatch
    {
        public const ushort CRIME_BUFFER_MIN_LEVEL = 200;
        public const float CRIME_DISTANCE_SEARCH = 200f;

        /// <summary>
        /// Find close by building with crime
        /// </summary>
        public static ushort FindBuildingWithCrime(Vector3 pos, float maxDistance)
        {            
            BuildingManager instance = Singleton<BuildingManager>.instance;
            uint numUnits = instance.m_buildings.m_size;    //get number of building units

            // CHECK FORMULAS -> REFERENCE: SMARTERFIREFIGHTERSAI
            int minx = Mathf.Max((int)((pos.x - maxDistance) / 64f + 135f), 0);
            int minz = Mathf.Max((int)((pos.z - maxDistance) / 64f + 135f), 0);
            int maxx = Mathf.Min((int)((pos.x + maxDistance) / 64f + 135f), 269);
            int maxz = Mathf.Min((int)((pos.z + maxDistance) / 64f + 135f), 269);

            // Initialize default result if no building is found and specify maximum distance
            ushort result = 0;
            float shortestSquaredDistance = maxDistance * maxDistance;

            // Loop through every building grid within maximum distance
            for (int i = minz; i <= maxz; i++)
            {
                for (int j = minx; j <= maxx; j++)
                {
                    ushort currentBuilding = instance.m_buildingGrid[i * 270 + j];
                    int num7 = 0;

                    // Iterate through all buildings at this grid location
                    while (currentBuilding != 0)
                    {
                        // CHeck Building Crime buffer
                        if (instance.m_buildings.m_buffer[currentBuilding].m_crimeBuffer >= CRIME_BUFFER_MIN_LEVEL)
                        {
                            float currentSqauredDistance = VectorUtils.LengthSqrXZ(pos - instance.m_buildings.m_buffer[currentBuilding].m_position);
                            if (currentSqauredDistance < shortestSquaredDistance)
                            {
                                result = currentBuilding;
                                shortestSquaredDistance = currentSqauredDistance;
                            }
                        }
                        currentBuilding = instance.m_buildings.m_buffer[currentBuilding].m_nextGridBuilding;
                        if (++num7 >= numUnits)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            return result;
        }

    }


    [HarmonyPatch(typeof(PoliceCarAI), "SimulationStep", new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    public static class PoliceCarAISimulationStepPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            // police capacity left?
            if (vehicleData.m_transferSize >= (Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleID].Info?.m_vehicleAI as PoliceCarAI).m_crimeCapacity)
                return;

            if ((vehicleData.m_flags & (Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget)) != 0)
            {
                ushort newTarget = PoliceAIPatch.FindBuildingWithCrime(vehicleData.GetLastFramePosition(), PoliceAIPatch.CRIME_DISTANCE_SEARCH);
                if (newTarget != 0)
                {
                    // clear flag goingback and waiting target
                    vehicleData.m_flags = vehicleData.m_flags & (~Vehicle.Flags.GoingBack) & (~Vehicle.Flags.WaitingTarget);
                    // set new target
                    vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, newTarget);
#if (DEBUG)
                    var instB = default(InstanceID);
                    instB.Building = newTarget;
                    string targetName = $"ID={newTarget}: {Singleton<BuildingManager>.instance.m_buildings.m_buffer[newTarget].Info?.name} ({Singleton<InstanceManager>.instance.GetName(instB)})";
                    var instV = default(InstanceID);
                    instV.Vehicle = vehicleID;
                    string vehicleName = $"ID={vehicleID} ({Singleton<InstanceManager>.instance.GetName(instV)})";
                    DebugLog.LogDebug((DebugLog.LogReason)TransferManager.TransferReason.Crime, $"PoliceCarAI: vehicle {vehicleName} set new target: {targetName}");
#endif
                }
            }
        }
    
    }


    [HarmonyPatch(typeof(PoliceCopterAI), "SimulationStep", new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    public static class PoliceCopterAIAISimulationStepPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            // police capacity left?
            if (vehicleData.m_transferSize >= (Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleID].Info?.m_vehicleAI as PoliceCopterAI).m_crimeCapacity)
                return;

            if ((vehicleData.m_flags & (Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget)) != 0)
            {
                ushort newTarget = PoliceAIPatch.FindBuildingWithCrime(vehicleData.GetLastFramePosition(), PoliceAIPatch.CRIME_DISTANCE_SEARCH);
                if (newTarget != 0)
                {
                    // clear flag goingback and waiting target
                    vehicleData.m_flags = vehicleData.m_flags & (~Vehicle.Flags.GoingBack) & (~Vehicle.Flags.WaitingTarget);
                    // set new target
                    vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, newTarget);
#if (DEBUG)
                    var instB = default(InstanceID);
                    instB.Building = newTarget;
                    string targetName = $"ID={newTarget}: {Singleton<BuildingManager>.instance.m_buildings.m_buffer[newTarget].Info?.name} ({Singleton<InstanceManager>.instance.GetName(instB)})";
                    var instV = default(InstanceID);
                    instV.Vehicle = vehicleID;
                    string vehicleName = $"ID={vehicleID} ({Singleton<InstanceManager>.instance.GetName(instV)})";
                    DebugLog.LogDebug((DebugLog.LogReason)TransferManager.TransferReason.Crime, $"PoliceCopterAI: vehicle {vehicleName} set new target: {targetName}");
#endif
                }
            }
        }
    
    }

}
