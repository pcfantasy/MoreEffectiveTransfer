using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using MoreEffectiveTransfer.CustomAI;
using MoreEffectiveTransfer.UI;
using MoreEffectiveTransfer.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MoreEffectiveTransfer
{
    public class MoreEffectiveTransferThreading : ThreadingExtensionBase
    {
        public static bool isFirstTime = true;
        public override void OnBeforeSimulationFrame()
        {
            base.OnBeforeSimulationFrame();
            if (Loader.CurrentLoadMode == LoadMode.LoadGame || Loader.CurrentLoadMode == LoadMode.NewGame)
            {
                if (MoreEffectiveTransfer.IsEnabled)
                {
                    uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
                    int num4 = (int)(currentFrameIndex & 255u);
                    if (num4 == 255)
                    {
                        BuildingUI.refeshOnce = true;
                        PlayerBuildingUI.refeshOnce = true;
                        UniqueFactoryUI.refeshOnce = true;
                        WareHouseUI.refeshOnce = true;
                        CustomHelicopterDepotAI.haveFireHelicopterDepotFinal = CustomHelicopterDepotAI.haveFireHelicopterDepot;
                        CustomHelicopterDepotAI.haveFireHelicopterDepot = false;
                        CustomHelicopterDepotAI.haveSickHelicopterDepotFinal = CustomHelicopterDepotAI.haveSickHelicopterDepot;
                        CustomHelicopterDepotAI.haveSickHelicopterDepot = false;
                    }
                    CheckDetour();
                }
            }
        }

        public void CheckDetour()
        {
            if (isFirstTime && Loader.DetourInited && Loader.HarmonyDetourInited)
            {
                isFirstTime = false;
                if (Loader.DetourInited)
                {
                    DebugLog.LogToFileOnly("ThreadingExtension.OnBeforeSimulationFrame: First frame detected. Checking detours.");
                    if (Loader.HarmonyDetourFailed)
                    {
                        string error = "MoreEffectiveTransferManager HarmonyDetourInit is failed, Send MoreEffectiveTransferManager.txt to Author.";
                        DebugLog.LogToFileOnly(error);
                        UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Incompatibility Issue", error, true);
                    }
                }
            }
        }
    }
}