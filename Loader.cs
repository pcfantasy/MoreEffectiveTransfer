using ColossalFramework.UI;
using ICities;
using UnityEngine;
using System.IO;
using ColossalFramework;
using System.Reflection;
using System;
using System.Linq;
using ColossalFramework.Math;
using System.Collections.Generic;
using ColossalFramework.PlatformServices;
using MoreEffectiveTransfer.Util;
using MoreEffectiveTransfer.UI;
using MoreEffectiveTransfer.CustomManager;

namespace MoreEffectiveTransfer
{
    public class Loader : LoadingExtensionBase
    {
        public static LoadMode CurrentLoadMode;
        public static bool isRealCityRunning = false;
        public static bool isEmployOvereducatedWorkersRunning = false;
        public class Detour
        {
            public MethodInfo OriginalMethod;
            public MethodInfo CustomMethod;
            public RedirectCallsState Redirect;

            public Detour(MethodInfo originalMethod, MethodInfo customMethod)
            {
                this.OriginalMethod = originalMethod;
                this.CustomMethod = customMethod;
                this.Redirect = RedirectionHelper.RedirectCalls(originalMethod, customMethod);
            }
        }
        public static List<Detour> Detours { get; set; }
        public static bool DetourInited = false;
        public static bool HarmonyDetourInited = false;
        public static bool isGuiRunning = false;
        public static UIPanel buildingInfo;
        public static UIPanel playerBuildingInfo;
        public static UIPanel uniqueFactoryInfo;
        public static UIPanel wareHouseInfo;
        public static UniqueFactoryUI uniqueFactoryPanel;
        public static WareHouseUI wareHousePanel;
        public static PlayerBuildingUI playerBuildingPanel;
        public static BuildingUI buildingPanel;
        public static GameObject BuildingWindowGameObject;
        public static GameObject PlayerBuildingWindowGameObject;
        public static GameObject UniqueFactoryWindowGameObject;
        public static GameObject WareHouseWindowGameObject;

        public override void OnCreated(ILoading loading)
        {
            Detours = new List<Detour>();
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
                    DataInit();
                    InitDetour();
                    HarmonyInitDetour();
                    SetupGui();
                    MoreEffectiveTransfer.LoadSetting();
                    if (mode == LoadMode.NewGame)
                    {
                        DebugLog.LogToFileOnly("New Game");
                    }
                }
            }
        }

        public void DataInit()
        {
            for (int i = 0; i < 49152; i++)
            {
                MainDataStore.refreshCanNotConnectedBuildingIDCount[i] = 0;
                MainDataStore.canNotConnectedBuildingIDCount[i] = 0;
                for (int j = 0; j < 255; j++)
                {
                    MainDataStore.canNotConnectedBuildingID[i, j] = 0;
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
                    RemoveGui();
                }
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
        }

        public static void SetupGui()
        {
            SetupBuidingGui();
            SetupPlayerBuidingGui();
            SetupWareHouseGui();
            SetupUniqueFactoryGui();
            Loader.isGuiRunning = true;
        }

        public static void RemoveGui()
        {
            Loader.isGuiRunning = false;

            if (uniqueFactoryPanel != null)
            {
                if (uniqueFactoryPanel.parent != null)
                {
                    uniqueFactoryPanel.parent.eventVisibilityChanged -= uniqueFactoryInfo_eventVisibilityChanged;
                }
            }
            if (wareHousePanel != null)
            {
                if (wareHousePanel.parent != null)
                {
                    wareHousePanel.parent.eventVisibilityChanged -= wareHouseInfo_eventVisibilityChanged;
                }
            }
            if (playerBuildingPanel != null)
            {
                if (playerBuildingPanel.parent != null)
                {
                    playerBuildingPanel.parent.eventVisibilityChanged -= playerbuildingInfo_eventVisibilityChanged;
                }
            }
            if (buildingPanel != null)
            {
                if (buildingPanel.parent != null)
                {
                    buildingPanel.parent.eventVisibilityChanged -= buildingInfo_eventVisibilityChanged;
                }
            }

            if (PlayerBuildingWindowGameObject != null)
            {
                UnityEngine.Object.Destroy(PlayerBuildingWindowGameObject);
            }

            if (BuildingWindowGameObject != null)
            {
                UnityEngine.Object.Destroy(BuildingWindowGameObject);
            }

            if (WareHouseWindowGameObject != null)
            {
                UnityEngine.Object.Destroy(WareHouseWindowGameObject);
            }

            if (UniqueFactoryWindowGameObject != null)
            {
                UnityEngine.Object.Destroy(UniqueFactoryWindowGameObject);
            }
        }

        public static void SetupBuidingGui()
        {
            BuildingWindowGameObject = new GameObject("buildingWindowObject");
            buildingPanel = (BuildingUI)BuildingWindowGameObject.AddComponent(typeof(BuildingUI));
            buildingInfo = UIView.Find<UIPanel>("(Library) ZonedBuildingWorldInfoPanel");
            if (buildingInfo == null)
            {
                DebugLog.LogToFileOnly("UIPanel not found (update broke the mod!): (Library) ZonedBuildingWorldInfoPanel\nAvailable panels are:\n");
            }
            buildingPanel.transform.parent = buildingInfo.transform;
            buildingPanel.size = new Vector3(buildingInfo.size.x, buildingInfo.size.y/2f);
            buildingPanel.baseBuildingWindow = buildingInfo.gameObject.transform.GetComponentInChildren<ZonedBuildingWorldInfoPanel>();
            buildingPanel.position = new Vector3(buildingInfo.size.x, 0);
            buildingInfo.eventVisibilityChanged += buildingInfo_eventVisibilityChanged;
        }

        public static void SetupUniqueFactoryGui()
        {
            UniqueFactoryWindowGameObject = new GameObject("UniqueFactoryWindowGameObject");
            uniqueFactoryPanel = (UniqueFactoryUI)UniqueFactoryWindowGameObject.AddComponent(typeof(UniqueFactoryUI));
            uniqueFactoryInfo = UIView.Find<UIPanel>("(Library) UniqueFactoryWorldInfoPanel");
            if (uniqueFactoryInfo == null)
            {
                DebugLog.LogToFileOnly("UIPanel not found (update broke the mod!): (Library) UniqueFactoryWorldInfoPanel\nAvailable panels are:\n");
            }
            uniqueFactoryPanel.transform.parent = uniqueFactoryInfo.transform;
            uniqueFactoryPanel.size = new Vector3(uniqueFactoryInfo.size.x, uniqueFactoryInfo.size.y/2f);
            uniqueFactoryPanel.baseBuildingWindow = uniqueFactoryInfo.gameObject.transform.GetComponentInChildren<UniqueFactoryWorldInfoPanel>();
            uniqueFactoryPanel.position = new Vector3(uniqueFactoryInfo.size.x, 0);
            uniqueFactoryInfo.eventVisibilityChanged += uniqueFactoryInfo_eventVisibilityChanged;
        }

        public static void SetupWareHouseGui()
        {
            WareHouseWindowGameObject = new GameObject("WareHouseWindowGameObject");
            wareHousePanel = (WareHouseUI)WareHouseWindowGameObject.AddComponent(typeof(WareHouseUI));
            wareHouseInfo = UIView.Find<UIPanel>("(Library) WarehouseWorldInfoPanel");
            if (wareHouseInfo == null)
            {
                DebugLog.LogToFileOnly("UIPanel not found (update broke the mod!): (Library) WarehouseWorldInfoPanel\nAvailable panels are:\n");
            }
            wareHousePanel.transform.parent = wareHouseInfo.transform;
            wareHousePanel.size = new Vector3(wareHouseInfo.size.x, wareHouseInfo.size.y/2f);
            wareHousePanel.baseBuildingWindow = wareHouseInfo.gameObject.transform.GetComponentInChildren<WarehouseWorldInfoPanel>();
            wareHousePanel.position = new Vector3(wareHouseInfo.size.x, 0);
            wareHouseInfo.eventVisibilityChanged += wareHouseInfo_eventVisibilityChanged;
        }

        public static void SetupPlayerBuidingGui()
        {
            PlayerBuildingWindowGameObject = new GameObject("PlayerbuildingWindowGameObject");
            playerBuildingPanel = (PlayerBuildingUI)PlayerBuildingWindowGameObject.AddComponent(typeof(PlayerBuildingUI));
            playerBuildingInfo = UIView.Find<UIPanel>("(Library) CityServiceWorldInfoPanel");
            if (playerBuildingInfo == null)
            {
                DebugLog.LogToFileOnly("UIPanel not found (update broke the mod!): (Library) CityServiceWorldInfoPanel\nAvailable panels are:\n");
            }
            playerBuildingPanel.transform.parent = playerBuildingInfo.transform;
            playerBuildingPanel.size = new Vector3(playerBuildingInfo.size.x, playerBuildingInfo.size.y/2f);
            playerBuildingPanel.baseBuildingWindow = playerBuildingInfo.gameObject.transform.GetComponentInChildren<CityServiceWorldInfoPanel>();
            playerBuildingPanel.position = new Vector3(playerBuildingInfo.size.x, 0);
            playerBuildingInfo.eventVisibilityChanged += playerbuildingInfo_eventVisibilityChanged;
        }

        public static void playerbuildingInfo_eventVisibilityChanged(UIComponent component, bool value)
        {
            playerBuildingPanel.isEnabled = value;
            if (value)
            {
                Loader.playerBuildingPanel.transform.parent = Loader.playerBuildingInfo.transform;
                Loader.playerBuildingPanel.size = new Vector3(Loader.playerBuildingInfo.size.x, Loader.playerBuildingInfo.size.y/2f);
                Loader.playerBuildingPanel.baseBuildingWindow = Loader.playerBuildingInfo.gameObject.transform.GetComponentInChildren<CityServiceWorldInfoPanel>();
                Loader.playerBuildingPanel.position = new Vector3(Loader.playerBuildingInfo.size.x, 0);
                playerBuildingPanel.Show();
            }
            else
            {
                playerBuildingPanel.Hide();
            }
        }

        public static void buildingInfo_eventVisibilityChanged(UIComponent component, bool value)
        {
            buildingPanel.isEnabled = value;
            if (value)
            {
                buildingPanel.transform.parent = buildingInfo.transform;
                buildingPanel.size = new Vector3(buildingInfo.size.x, buildingInfo.size.y / 2f);
                buildingPanel.baseBuildingWindow = buildingInfo.gameObject.transform.GetComponentInChildren<ZonedBuildingWorldInfoPanel>();
                buildingPanel.position = new Vector3(buildingInfo.size.x, 0);
                buildingPanel.Show();
            }
            else
            {
                buildingPanel.Hide();
            }
        }

        public static void uniqueFactoryInfo_eventVisibilityChanged(UIComponent component, bool value)
        {
            uniqueFactoryPanel.isEnabled = value;
            if (value)
            {
                uniqueFactoryPanel.transform.parent = uniqueFactoryInfo.transform;
                uniqueFactoryPanel.size = new Vector3(uniqueFactoryInfo.size.x, uniqueFactoryInfo.size.y / 2f);
                uniqueFactoryPanel.baseBuildingWindow = uniqueFactoryInfo.gameObject.transform.GetComponentInChildren<UniqueFactoryWorldInfoPanel>();
                uniqueFactoryPanel.position = new Vector3(uniqueFactoryInfo.size.x, 0);
                uniqueFactoryPanel.Show();
            }
            else
            {
                uniqueFactoryPanel.Hide();
            }
        }

        public static void wareHouseInfo_eventVisibilityChanged(UIComponent component, bool value)
        {
            wareHousePanel.isEnabled = value;
            if (value)
            {
                wareHousePanel.transform.parent = wareHouseInfo.transform;
                wareHousePanel.size = new Vector3(wareHouseInfo.size.x, wareHouseInfo.size.y / 2f);
                wareHousePanel.baseBuildingWindow = wareHouseInfo.gameObject.transform.GetComponentInChildren<WarehouseWorldInfoPanel>();
                wareHousePanel.position = new Vector3(wareHouseInfo.size.x, 0);
                wareHousePanel.Show();
            }
            else
            {
                wareHousePanel.Hide();
            }
        }

        public void HarmonyInitDetour()
        {
            if (!HarmonyDetourInited)
            {
                DebugLog.LogToFileOnly("Init harmony detours");
                HarmonyDetours.Apply();
            }
        }

        public void HarmonyRevertDetour()
        {
            if (HarmonyDetourInited)
            {
                DebugLog.LogToFileOnly("Revert harmony detours");
                HarmonyDetours.DeApply();
            }
        }

        public void InitDetour()
        {
            isRealCityRunning = CheckRealCityIsLoaded();
            if (!DetourInited)
            {
                DebugLog.LogToFileOnly("Init detours");
                bool detourFailed = false;

                //1
                DebugLog.LogToFileOnly("Detour TransferManager::MatchOffers calls");
                try
                {
                    Detours.Add(new Detour(typeof(TransferManager).GetMethod("MatchOffers", BindingFlags.NonPublic | BindingFlags.Instance),
                                           typeof(CustomTransferManager).GetMethod("MatchOffers", BindingFlags.NonPublic | BindingFlags.Instance)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour TransferManager::MatchOffers");
                    detourFailed = true;
                }

                if (detourFailed)
                {
                    DebugLog.LogToFileOnly("Detours failed");
                }
                else
                {
                    DebugLog.LogToFileOnly("Detours successful");
                }
                DetourInited = true;
            }
        }

        public void RevertDetour()
        {
            if (DetourInited)
            {
                DebugLog.LogToFileOnly("Revert detours");
                Detours.Reverse();
                foreach (Detour d in Detours)
                {
                    RedirectionHelper.RevertRedirect(d.OriginalMethod, d.Redirect);
                }
                DetourInited = false;
                Detours.Clear();
                DebugLog.LogToFileOnly("Reverting detours finished.");
            }
            MoreEffectiveTransferThreading.isFirstTime = true;
        }

        private bool Check3rdPartyModLoaded(string namespaceStr, bool printAll = false)
        {
            bool thirdPartyModLoaded = false;

            var loadingWrapperLoadingExtensionsField = typeof(LoadingWrapper).GetField("m_LoadingExtensions", BindingFlags.NonPublic | BindingFlags.Instance);
            List<ILoadingExtension> loadingExtensions = (List<ILoadingExtension>)loadingWrapperLoadingExtensionsField.GetValue(Singleton<LoadingManager>.instance.m_LoadingWrapper);

            if (loadingExtensions != null)
            {
                foreach (ILoadingExtension extension in loadingExtensions)
                {
                    if (printAll)
                        DebugLog.LogToFileOnly($"Detected extension: {extension.GetType().Name} in namespace {extension.GetType().Namespace}");
                    if (extension.GetType().Namespace == null)
                        continue;

                    var nsStr = extension.GetType().Namespace.ToString();
                    if (namespaceStr.Equals(nsStr))
                    {
                        DebugLog.LogToFileOnly($"The mod '{namespaceStr}' has been detected.");
                        thirdPartyModLoaded = true;
                        break;
                    }
                }
            }
            else
            {
                DebugLog.LogToFileOnly("Could not get loading extensions");
            }

            return thirdPartyModLoaded;
        }

        private bool CheckRealCityIsLoaded()
        {
            return this.Check3rdPartyModLoaded("RealCity", true);
        }
    }
}

