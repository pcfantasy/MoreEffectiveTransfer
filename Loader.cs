using ICities;
using MoreEffectiveTransfer.Util;
using CitiesHarmony.API;
using ColossalFramework.UI;
using HarmonyLib;
using MoreEffectiveTransfer.CustomManager;

namespace MoreEffectiveTransfer
{
    public class Loader : LoadingExtensionBase
    {
        public static LoadMode CurrentLoadMode;
        public static bool DetourInited = false;
        public static bool HarmonyDetourInited = false;
        public static bool HarmonyDetourFailed = true;

        public static bool isFirstTime = true;
#if (DEBUG_VANILLA)
        public const int HarmonyPatchNumExpected = 3;
#else
        public const int HarmonyPatchNumExpected = 2;
#endif

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            Loader.CurrentLoadMode = mode;

            if (MoreEffectiveTransfer.IsEnabled)
            {
                if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
                {
                    DebugLog.LogInfo("OnLevelLoaded");

                    InitDetour();
                    HarmonyInitDetour();
                    CheckDetour();

                    ModSettings.LoadSetting();

                    // Create TransferJobPool and initialize
                    TransferJobPool.Instance.Initialize();

                    // Create TransferDispatcher and initialize
                    CustomTransferDispatcher.Instance.Initialize();

                    // Create TransferManager background thread and start
                    CustomTransferDispatcher._transferThread = new System.Threading.Thread(CustomTransferManager.MatchOffersThread);
                    CustomTransferDispatcher._transferThread.IsBackground = true;
                    CustomTransferDispatcher._transferThread.Start();

                    DebugLog.FlushImmediate();
                }
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();

            if (CurrentLoadMode == LoadMode.LoadGame || CurrentLoadMode == LoadMode.NewGame)
            {
                if (MoreEffectiveTransfer.IsEnabled)
                {
                    Profiling.PrintProfilingStats();

                    // Stop thread & deinit dispatcher and jobpool
                    CustomTransferManager._runThread = false;
                    CustomTransferDispatcher._waitHandle.Set();
                    CustomTransferDispatcher._transferThread.Join();
                    CustomTransferDispatcher.Instance.Delete();
                    TransferJobPool.Instance.Delete();

                    RevertDetour();
                    HarmonyRevertDetour();

                    DebugLog.StopLogging();
                }
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
        }

        public void HarmonyInitDetour()
        {
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                if (!HarmonyDetourInited)
                {
                    DebugLog.LogInfo("Init harmony detours");
                    HarmonyDetours.Apply();
                    HarmonyDetourInited = true;
                }
            }
            else
            {
                DebugLog.LogInfo("ERROR: Harmony not found!");
            }
        }

        public void HarmonyRevertDetour()
        {
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                if (HarmonyDetourInited)
                {
                    DebugLog.LogInfo("Revert harmony detours");
                    HarmonyDetours.DeApply();
                    HarmonyDetourInited = false;
                    HarmonyDetourFailed = true;
                }
            }
            else
            {
                DebugLog.LogInfo("ERROR: Harmony not found!");
            }
        }

        public void InitDetour()
        {
            if (!DetourInited)
            {
                DebugLog.LogInfo("Init detours");
                DetourInited = true;
            }
        }

        public void RevertDetour()
        {
            if (DetourInited)
            {
                DebugLog.LogInfo("Revert detours");
                DetourInited = false;
            }
            
            isFirstTime = true;
        }

        public void CheckDetour()
        {
            if (isFirstTime && Loader.DetourInited && Loader.HarmonyDetourInited)
            {
                isFirstTime = false;
                if (Loader.DetourInited)
                {
                    DebugLog.LogInfo("LoadingExtension: Checking detours.");
                    if (Loader.HarmonyDetourFailed)
                    {
                        string error = "HarmonyDetourInit is failed, Send MoreEffectiveTransfer.log to Author.";
                        DebugLog.LogError(error);
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
                                DebugLog.LogInfo($"Harmony patch method = {method.FullDescription()}");
                                if (info.Prefixes.Count != 0)
                                {
                                    DebugLog.LogInfo("Harmony patch method has PreFix");
                                }
                                if (info.Postfixes.Count != 0)
                                {
                                    DebugLog.LogInfo("Harmony patch method has PostFix");
                                }
                                i++;
                            }
                        }

                        if (i != HarmonyPatchNumExpected)
                        {
                            string error = $"MoreEffectiveTransfer HarmonyDetour Patch Num is {i}, expected: {HarmonyPatchNumExpected}. Send MoreEffectiveTransfer.log to Author.";
                            DebugLog.LogError(error);
                            UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Incompatibility Issue", error, true);
                        }
                    }

#if (PROFILE)
                    DebugLog.LogInfo("PROFILING MODE - statistics will be output at end!");
#endif

                }
            }
        }


    }
}

