using ColossalFramework;
using HarmonyLib;
using MoreEffectiveTransfer.Util;


namespace MoreEffectiveTransfer.Patch
{
    [HarmonyPatch(typeof(CarAI), "PathfindFailure")]
    public class CarAIPathfindFailurePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ushort vehicleID, ref Vehicle data)
        {
            PathFindFailure.RecordPathFindFailure(vehicleID, ref data);
        }

    }
}