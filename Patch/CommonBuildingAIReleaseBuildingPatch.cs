using HarmonyLib;
using MoreEffectiveTransfer.Util;
using System.Reflection;

namespace MoreEffectiveTransfer.Patch
{
    [HarmonyPatch]
    public static class CommonBuildingAIReleaseBuildingPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(CommonBuildingAI).GetMethod("ReleaseBuilding");
        }
        public static void Postfix(ushort buildingID)
        {
            MainDataStore.refreshCanNotConnectedBuildingIDCount[buildingID] = 0;
            MainDataStore.canNotConnectedBuildingIDCount[buildingID] = 0;
            for (int j = 0; j < 255; j++)
            {
                MainDataStore.canNotConnectedBuildingID[buildingID, j] = 0;
            }
        }
    }
}
