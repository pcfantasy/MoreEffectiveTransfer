using HarmonyLib;
using System;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using MoreEffectiveTransfer.Util;
using System.Collections.Generic;
using System.Linq;

namespace MoreEffectiveTransfer.Patch
{

    public static class GarbageAIPatch
    {
        public const ushort GARBAGE_BUFFER_MIN_LEVEL = 800;
        public const float GARBAGE_DISTANCE_SEARCH = 150f;

        //prevent double-dispatching of multiple vehicles to same target
        private const int LRU_MAX_SIZE = 16;
        private static Dictionary<ushort, long> LRU_DISPATCH_LIST = new Dictionary<ushort, long>(LRU_MAX_SIZE);

        private static void AddBuildingLRU(ushort buildingID)
        {
            if (LRU_DISPATCH_LIST.Count > LRU_MAX_SIZE)
            {
                // remove oldest:
                LRU_DISPATCH_LIST.Remove(LRU_DISPATCH_LIST.OrderBy(x => x.Value).First().Key);
            }

            LRU_DISPATCH_LIST.Add(buildingID, DateTime.Now.Ticks);
        }

        /// <summary>
        /// Find close by building with garbage
        /// </summary>
        public static ushort FindBuildingWithGarbage(Vector3 pos, float maxDistance)
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
                        // CHeck Building garbage buffer
                        if (instance.m_buildings.m_buffer[currentBuilding].m_garbageBuffer >= GARBAGE_BUFFER_MIN_LEVEL)
                        {
                            // check if not already dispatched to
                            long value;
                            if (LRU_DISPATCH_LIST.TryGetValue(currentBuilding, out value))
                            {
                                // dont consider building
                            }
                            else
                            {
                                // not found in LRU, may consider this building
                                float distanceSqr = VectorUtils.LengthSqrXZ(pos - instance.m_buildings.m_buffer[currentBuilding].m_position);
                                if (distanceSqr < shortestSquaredDistance)
                                {
                                    result = currentBuilding;
                                    shortestSquaredDistance = distanceSqr;
                                }
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

            if (result != 0) 
                AddBuildingLRU(result);
            
            return result;
        }

    }


    [HarmonyPatch(typeof(GarbageTruckAI), "SimulationStep", new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    public static class GarbageTruckAIPatchSimulationStepPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            // garbage capacity left?
            if (vehicleData.m_transferSize >= (Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleID].Info?.m_vehicleAI as GarbageTruckAI).m_cargoCapacity)
                return;

            if ((vehicleData.m_flags & (Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget)) != 0)
            {
                ushort newTarget = GarbageAIPatch.FindBuildingWithGarbage(vehicleData.GetLastFramePosition(), GarbageAIPatch.GARBAGE_DISTANCE_SEARCH);
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
                    DebugLog.LogDebug((DebugLog.LogReason)TransferManager.TransferReason.Garbage, $"GarbageTruckAI: vehicle {vehicleName} set new target: {targetName}");
#endif
                }
            }
        }
    
    }


}
