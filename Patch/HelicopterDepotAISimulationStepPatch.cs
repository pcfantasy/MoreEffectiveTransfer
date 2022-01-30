//using HarmonyLib;
//using System;
//using System.Reflection;

//namespace MoreEffectiveTransfer.Patch
//{
//    [HarmonyPatch]
//    public class HelicopterDepotAISimulationStepPatch
//    {
//        public static bool haveFireHelicopterDepot = false;
//        public static bool haveFireHelicopterDepotFinal = false;

//        public static MethodBase TargetMethod()
//        {
//            return typeof(HelicopterDepotAI).GetMethod("SimulationStep", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Building).MakeByRefType(), typeof(Building.Frame).MakeByRefType() }, null);
//        }
        
//        public static void Postfix(ref Building buildingData)
//        {
//            if (buildingData.Info.m_class.m_service == ItemClass.Service.FireDepartment)
//            {
//                haveFireHelicopterDepot = true;
//            }
//        }
//    }
//}
