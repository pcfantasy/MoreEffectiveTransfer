using ICities;
using MoreEffectiveTransfer.Util;
using CitiesHarmony.API;
using ColossalFramework.UI;
using HarmonyLib;

namespace MoreEffectiveTransfer
{
    public class Loader : LoadingExtensionBase
    {
        public static LoadMode CurrentLoadMode;
        public static bool DetourInited = false;
        public static bool HarmonyDetourInited = false;
        public static bool HarmonyDetourFailed = true;

        public static bool isFirstTime = true;
#if (DEBUG)
        public const int HarmonyPatchNumExpected = 2;
#else
        public const int HarmonyPatchNumExpected = 1;
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
                    DebugLog.LogToFileOnly("OnLevelLoaded");

                    InitDetour();
                    HarmonyInitDetour();
                    CheckDetour();

                    MoreEffectiveTransfer.LoadSetting();
                    if (mode == LoadMode.NewGame)
                    {
                        DebugLog.LogToFileOnly("New Game");
                    }

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
                    RevertDetour();
                    HarmonyRevertDetour();

#if (PROFILE)
                    DebugLog.LogToFileOnly("--- PROFILING STATISTICS ---");
                    float msPerInvVanilla = (1.0f * MoreEffectiveTransfer.timerVanilla.ElapsedMilliseconds / MoreEffectiveTransfer.timerCounterVanilla / 1.0f);
                    float msPerInvMETM = (1.0f * MoreEffectiveTransfer.timerMETM.ElapsedMilliseconds / MoreEffectiveTransfer.timerCounterMETM / 1.0f);
                    DebugLog.LogToFileOnly($"- VANILLA TRANSFER MANAGER: NUM INVOCATIONS: {MoreEffectiveTransfer.timerCounterVanilla}, TOTAL MS: {MoreEffectiveTransfer.timerVanilla.ElapsedMilliseconds}, AVG TIME/INVOCATION: {msPerInvVanilla}ms");
                    DebugLog.LogToFileOnly($"-     NEW TRANSFER MANAGER: NUM INVOCATIONS: {MoreEffectiveTransfer.timerCounterMETM}, TOTAL MS: {MoreEffectiveTransfer.timerMETM.ElapsedMilliseconds}, AVG TIME/INVOCATION: {msPerInvMETM}ms");
                    DebugLog.LogToFileOnly("--- END PROFILING STATISTICS ---");
#endif

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
                    DebugLog.LogToFileOnly("Init harmony detours");
                    HarmonyDetours.Apply();
                    HarmonyDetourInited = true;
                }
            }
            else
            {
                DebugLog.LogToFileOnly("ERROR: Harmony not found!");
            }
        }

        public void HarmonyRevertDetour()
        {
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                if (HarmonyDetourInited)
                {
                    DebugLog.LogToFileOnly("Revert harmony detours");
                    HarmonyDetours.DeApply();
                    HarmonyDetourInited = false;
                    HarmonyDetourFailed = true;
                }
            }
            else
            {
                DebugLog.LogToFileOnly("ERROR: Harmony not found!");
            }
        }

        public void InitDetour()
        {
            if (!DetourInited)
            {
                DebugLog.LogToFileOnly("Init detours");
                DetourInited = true;
            }
        }

        public void RevertDetour()
        {
            if (DetourInited)
            {
                DebugLog.LogToFileOnly("Revert detours");
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
                    DebugLog.LogToFileOnly("LoadingExtension: Checking detours.");
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

