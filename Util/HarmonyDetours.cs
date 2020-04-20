using HarmonyLib;

namespace MoreEffectiveTransfer.Util
{
    internal static class HarmonyDetours
    {
        public const string ID = "pcfantasy.moreeffectivetransfer";
        public static void Apply()
        {
            var harmony = new Harmony(ID);
            harmony.PatchAll();
            Loader.HarmonyDetourFailed = false;
            DebugLog.LogToFileOnly("Harmony patches applied");
        }

        public static void DeApply()
        {
            var harmony = new Harmony(ID);
            harmony.UnpatchAll(ID);
            DebugLog.LogToFileOnly("Harmony patches DeApplied");
        }
    }
}
