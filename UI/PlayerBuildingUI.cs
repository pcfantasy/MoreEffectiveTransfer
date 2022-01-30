using System.Collections.Generic;
using ColossalFramework.UI;
using UnityEngine;
using ColossalFramework;
using MoreEffectiveTransfer.Util;

namespace MoreEffectiveTransfer.UI
{
    public class PlayerBuildingUI : UIPanel
    {
        public static readonly string cacheName = "PlayerBuildingUI";
        private static readonly float SPACING = 15f;
        private Dictionary<string, UILabel> _valuesControlContainer = new Dictionary<string, UILabel>(16);
        public CityServiceWorldInfoPanel baseBuildingWindow;
        public static bool refeshOnce = false;
        private UILabel failedBuildingCount;
        public static UICheckBox generateDetail;
        private UILabel generateDetailText;
        public static UICheckBox localUse;
        private UILabel localUseText;

        public override void Update()
        {
            this.RefreshDisplayData();
            base.Update();
        }

        public override void Awake()
        {
            base.Awake();
            this.DoOnStartup();
        }

        public override void Start()
        {
            base.Start();
            this.canFocus = true;
            this.isInteractive = true;
            base.isVisible = true;
            this.BringToFront();
            base.opacity = 1f;
            base.cachedName = cacheName;
            this.RefreshDisplayData();
            base.Show();
        }

        private void DoOnStartup()
        {
            this.ShowOnGui();
            base.Show();
        }

        private void ShowOnGui()
        {
            this.failedBuildingCount = base.AddUIComponent<UILabel>();
            this.failedBuildingCount.text = Localization.Get("FAILED_BUILDING_COUNT");
            this.failedBuildingCount.relativePosition = new Vector3(SPACING, 0f);
            this.failedBuildingCount.autoSize = true;

            generateDetail = base.AddUIComponent<UICheckBox>();
            generateDetail.relativePosition = new Vector3(15f, failedBuildingCount.relativePosition.y + 20f);
            this.generateDetailText = base.AddUIComponent<UILabel>();
            this.generateDetailText.relativePosition = new Vector3(generateDetail.relativePosition.x + generateDetail.width + 20f, generateDetail.relativePosition.y + 5f);
            generateDetail.height = 16f;
            generateDetail.width = 16f;
            generateDetail.label = this.generateDetailText;
            generateDetail.text = Localization.Get("GENERATE_DETAIL_REPORT");
            UISprite uISprite0 = generateDetail.AddUIComponent<UISprite>();
            uISprite0.height = 20f;
            uISprite0.width = 20f;
            uISprite0.relativePosition = new Vector3(0f, 0f);
            uISprite0.spriteName = "check-unchecked";
            uISprite0.isVisible = true;
            UISprite uISprite1 = generateDetail.AddUIComponent<UISprite>();
            uISprite1.height = 20f;
            uISprite1.width = 20f;
            uISprite1.relativePosition = new Vector3(0f, 0f);
            uISprite1.spriteName = "check-checked";
            generateDetail.checkedBoxObject = uISprite1;
            generateDetail.isChecked = false;
            generateDetail.isEnabled = true;
            generateDetail.isVisible = true;
            generateDetail.canFocus = true;
            generateDetail.isInteractive = true;
            generateDetail.eventCheckChanged += delegate (UIComponent component, bool eventParam)
            {
                GenerateDetail_OnCheckChanged(component, eventParam);
            };

            localUse = base.AddUIComponent<UICheckBox>();
            localUse.relativePosition = new Vector3(15f, generateDetail.relativePosition.y + 20f);
            this.localUseText = base.AddUIComponent<UILabel>();
            this.localUseText.relativePosition = new Vector3(localUse.relativePosition.x + localUse.width + 20f, localUse.relativePosition.y + 5f);
            localUse.height = 16f;
            localUse.width = 16f;
            localUse.label = this.localUseText;
            localUse.text = Localization.Get("LOCAL_USE");
            UISprite uISprite2 = localUse.AddUIComponent<UISprite>();
            uISprite2.height = 20f;
            uISprite2.width = 20f;
            uISprite2.relativePosition = new Vector3(0f, 0f);
            uISprite2.spriteName = "check-unchecked";
            uISprite2.isVisible = true;
            UISprite uISprite3 = localUse.AddUIComponent<UISprite>();
            uISprite3.height = 20f;
            uISprite3.width = 20f;
            uISprite3.relativePosition = new Vector3(0f, 0f);
            uISprite3.spriteName = "check-checked";
            localUse.checkedBoxObject = uISprite3;
            localUse.isChecked = (Singleton<BuildingManager>.instance.m_buildings.m_buffer[MainDataStore.lastBuildingID].m_childHealth != 0);
            localUse.isEnabled = true;
            localUse.isVisible = true;
            localUse.canFocus = true;
            localUse.isInteractive = true;
            localUse.eventCheckChanged += delegate (UIComponent component, bool eventParam)
            {
                LocalUse_OnCheckChanged(component, eventParam);
            };
        }

        public static void GenerateDetail_OnCheckChanged(UIComponent UIComp, bool bValue)
        {
            if (bValue)
            {
                generateDetail.isChecked = true;
                var building1 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[MainDataStore.lastBuildingID];
                DebugLog.LogToFileOnly("DebugInfo: Current building m_class is " + building1.Info.m_class.ToString());
                DebugLog.LogToFileOnly("DebugInfo: Current building name is " + building1.Info.name.ToString());
                DebugLog.LogToFileOnly("Below is failed to connect building ------------------------------------");
                for (int j = 0; j < MainDataStore.canNotConnectedBuildingIDCount[MainDataStore.lastBuildingID]; j++)
                {
                    var building2 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[MainDataStore.canNotConnectedBuildingID[MainDataStore.lastBuildingID, j]];
                    DebugLog.LogToFileOnly("DebugInfo: Fail to connect to building" + j.ToString() + " m_class is " + building2.Info.m_class.ToString());
                    DebugLog.LogToFileOnly("DebugInfo: Fail to connect to building" + j.ToString() + " name is " + building2.Info.name.ToString());
                }
                DebugLog.LogToFileOnly("failed to connect building end ------------------------------------------");
            }
            else
            {
                generateDetail.isChecked = false;
            }
        }
        public static void LocalUse_OnCheckChanged(UIComponent UIComp, bool bValue)
        {
            if (bValue)
            {
                localUse.isChecked = true;
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[MainDataStore.lastBuildingID].m_childHealth = 1;
            }
            else
            {
                localUse.isChecked = false;
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[MainDataStore.lastBuildingID].m_childHealth = 0;
            }
        }

        private void RefreshDisplayData()
        {
            if (refeshOnce || (MainDataStore.lastBuildingID != WorldInfoPanel.GetCurrentInstanceID().Building))
            {
                if (base.isVisible)
                {
                    MainDataStore.lastBuildingID = WorldInfoPanel.GetCurrentInstanceID().Building;
                    this.failedBuildingCount.text = Localization.Get("FAILED_BUILDING_COUNT") + ":" + MainDataStore.canNotConnectedBuildingIDCount[MainDataStore.lastBuildingID].ToString();
                    if (generateDetail.isChecked == true)
                    {
                        generateDetail.isChecked = false;
                    }
                    this.BringToFront();
                    localUse.isChecked = (Singleton<BuildingManager>.instance.m_buildings.m_buffer[MainDataStore.lastBuildingID].m_childHealth != 0);
                }
                refeshOnce = false;
                this.Show();
            }

            var buildingAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[MainDataStore.lastBuildingID].Info.m_buildingAI;
            bool isLocalUseAvailable = MoreEffectiveTransfer.optionPreferLocalService && ((buildingAI is LandfillSiteAI) || (buildingAI is PoliceStationAI) || (buildingAI is FireStationAI));

            if (!MoreEffectiveTransfer.debugMode && !isLocalUseAvailable)
            {
                this.Hide();
            }
        }
    }
}