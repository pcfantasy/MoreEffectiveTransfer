using ColossalFramework;
using HarmonyLib;
using System;
using System.Reflection;

namespace MoreEffectiveTransfer.Patch
{
    [HarmonyPatch]
    public static class CargoTruckAIArriveAtTargetPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(CargoTruckAI).GetMethod("ArriveAtTarget", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null);
        }
        public static void Prefix(ref Vehicle data, ref bool __state)
        {
            __state = false;
            if (data.m_targetBuilding != 0)
            {
                if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].Info.m_buildingAI is WarehouseAI)
                {
                    __state = true;
                }
            }
        }

        public static void Postfix(ref bool __result, ref bool __state)
        {
            if (__state)
                __result = true;
        }
    }
}