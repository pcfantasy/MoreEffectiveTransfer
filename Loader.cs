using ICities;
using MoreEffectiveTransfer.Util;
using CitiesHarmony.API;

namespace MoreEffectiveTransfer
{
    public class Loader : LoadingExtensionBase
    {
        public static LoadMode CurrentLoadMode;
        public static bool DetourInited = false;
        public static bool HarmonyDetourInited = false;
        public static bool HarmonyDetourFailed = true;


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
                    //SetupGui();

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
            MoreEffectiveTransferThreading.isFirstTime = true;
        }
    }
}

