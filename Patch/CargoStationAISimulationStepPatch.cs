using ColossalFramework;
using HarmonyLib;
using MoreEffectiveTransfer.Util;
using System;
using System.Reflection;

namespace MoreEffectiveTransfer.Patch
{
    [HarmonyPatch]
    public class CargoStationAISimulationStepPatch
    {
        public static ushort preBuildingID = 0;
        public static float shipStationDistanceRandom = 1f;
        public static float trainStationDistanceRandom = 1f;
        public static float planeStationDistanceRandom = 1f;
        public static ushort stationBuildingNum = 0;
        public static ushort stationBuildingNumFinal = 0;
        public static ushort[] stationBuildingID = new ushort[49152];
        public static ushort[] stationBuildingIDFinal = new ushort[49152];

        public static MethodBase TargetMethod()
        {
            return typeof(CargoStationAI).GetMethod("SimulationStep", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Building).MakeByRefType(), typeof(Building.Frame).MakeByRefType() }, null);
        }
        
        public static void Postfix(ushort buildingID)
        {
            if (buildingID <= preBuildingID)
            { 
                stationBuildingNumFinal = stationBuildingNum;
                stationBuildingNum = 0;
                for (int i = 0; i < stationBuildingNumFinal; i++)
                {
                    stationBuildingIDFinal[i] = stationBuildingID[i];
                    stationBuildingID[i] = 0;
                    if (MoreEffectiveTransfer.debugMode)
                    {
                        DebugLog.LogToFileOnly($"Find station ID = {stationBuildingIDFinal[i]}");
                    }
                }

                if (MoreEffectiveTransfer.debugMode)
                {
                    DebugLog.LogToFileOnly($"Find station Num = {stationBuildingNumFinal}");
                }

                shipStationDistanceRandom = (float)Singleton<SimulationManager>.instance.m_randomizer.Int32(100, 300) * 0.00005f;
                trainStationDistanceRandom = (float)Singleton<SimulationManager>.instance.m_randomizer.Int32(100, 300) * 0.00005f;
                planeStationDistanceRandom = (float)Singleton<SimulationManager>.instance.m_randomizer.Int32(100, 300) * 0.00005f;
            }

            if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].m_flags.IsFlagSet(Building.Flags.Active))
            {
                stationBuildingID[stationBuildingNum] = buildingID;
                stationBuildingNum++;
            }
            preBuildingID = buildingID;
        }
    }
}
