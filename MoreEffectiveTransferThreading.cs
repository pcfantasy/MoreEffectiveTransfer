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
using System.Threading.Tasks;
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
                    List<string> list = new List<string>();
                    foreach (Loader.Detour current in Loader.Detours)
                    {
                        if (!RedirectionHelper.IsRedirected(current.OriginalMethod, current.CustomMethod))
                        {
                            list.Add(string.Format("{0}.{1} with {2} parameters ({3})", new object[]
                            {
                    current.OriginalMethod.DeclaringType.Name,
                    current.OriginalMethod.Name,
                    current.OriginalMethod.GetParameters().Length,
                    current.OriginalMethod.DeclaringType.AssemblyQualifiedName
                            }));
                        }
                    }
                    DebugLog.LogToFileOnly(string.Format("ThreadingExtension.OnBeforeSimulationFrame: First frame detected. Detours checked. Result: {0} missing detours", list.Count));
                    if (list.Count > 0)
                    {
                        string error = "MoreEffectiveTransferManager detected an incompatibility with another mod! You can continue playing but it's NOT recommended. RealGasStation will not work as expected. Send MoreEffectiveTransferManager.txt to Author.";
                        DebugLog.LogToFileOnly(error);
                        string text = "The following methods were overriden by another mod:";
                        foreach (string current2 in list)
                        {
                            text += string.Format("\n\t{0}", current2);
                        }
                        DebugLog.LogToFileOnly(text);
                        UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Incompatibility Issue", text, true);
                    }

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