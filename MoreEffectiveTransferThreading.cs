using ColossalFramework.UI;
using ICities;
using MoreEffectiveTransfer.Util;
using HarmonyLib;

namespace MoreEffectiveTransfer
{
    public class MoreEffectiveTransferThreading : ThreadingExtensionBase
    {
        public static bool isFirstTime = true;
        public const int HarmonyPatchNumExpected = 1; //was: 8


        /// <summary>
        /// Purpose: check harmony patches just before actual simulation is starting 
        /// Moved to OnCreated from OnBeforeSimulation
        /// </summary>
        /// <param name="threading"></param>
        public override void OnCreated(IThreading threading)
        {
            base.OnCreated(threading);

            if (!isFirstTime)
                return;

            if (Loader.CurrentLoadMode == LoadMode.LoadGame || Loader.CurrentLoadMode == LoadMode.NewGame)
            {
                if (MoreEffectiveTransfer.IsEnabled)
                {
                    CheckDetour();
                }
            }
        }


        /// <summary>
        /// Purpose: do actual checking of applied harmony patches vs expected patches
        /// </summary>
        public void CheckDetour()
        {
            if (isFirstTime && Loader.DetourInited && Loader.HarmonyDetourInited)
            {
                isFirstTime = false;
                if (Loader.DetourInited)
                {
                    DebugLog.LogToFileOnly("ThreadingExtension: First frame detected. Checking detours.");
                    if (Loader.HarmonyDetourFailed)
                    {
                        string error = "HarmonyDetourInit is failed, Send MoreEffectiveTransfer.txt to Author.";
                        DebugLog.LogAll(error);
                        UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("METM Incompatibility Issue", error, true);
                    }
                    else
                    {
                        var harmony = new Harmony(HarmonyDetours.ID);
                        var methods = harmony.GetPatchedMethods();
                        int i = 0;
                        foreach (var method in methods)
                        {
                            var info = Harmony.GetPatchInfo(method);
                            if (info.Owners?.Contains(harmony.Id) == true)
                            {
                                DebugLog.LogToFileOnly($"Harmony patch method = {method.FullDescription()}");
                                if (info.Prefixes.Count != 0)
                                {
                                    DebugLog.LogToFileOnly("Harmony patch method has PreFix");
                                }
                                if (info.Postfixes.Count != 0)
                                {
                                    DebugLog.LogToFileOnly("Harmony patch method has PostFix");
                                }
                                i++;
                            }
                        }

                        if (i != HarmonyPatchNumExpected)
                        {
                            string error = $"MoreEffectiveTransfer HarmonyDetour Patch Num is {i}, expected: {HarmonyPatchNumExpected}. Send MoreEffectiveTransfer.txt to Author.";
                            DebugLog.LogAll(error);
                            UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Incompatibility Issue", error, true);
                        }
                    }

#if (PROFILE)
                    DebugLog.LogToFileOnly("PROFILING MODE - statistics will be output at end!");
#endif

                }
            }
        }
    }
}