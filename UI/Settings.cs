using CitiesHarmony.API;
using ColossalFramework.UI;
using ICities;
using MoreEffectiveTransfer.Util;
using System.IO;
using UnityEngine;


namespace MoreEffectiveTransfer
{
    public sealed class ModSettings
    {
        public const string SETTINGS_VERSION = "2.0.0";


        // MAIN switch, mainly for debugging/profiling
        public static bool optionEnableNewTransferManager = true;

        // SERVICE options
        public static bool optionPreferLocalService = false;

        // WAREHOUSE options
        public static bool optionWarehouseFirst = false;
        public static bool optionWarehouseReserveTrucks = false;
        public static bool optionWarehouseNewBalanced = false;

        // EXPORT options
        public static bool optionPreferExportShipPlaneTrain = false;

        // PATHFINDING
        public static bool optionPathfindChirper = true;


        // RUNTIME settings:
        public static float shipStationDistanceRandom = 1f;
        public static float trainStationDistanceRandom = 1f;
        public static float planeStationDistanceRandom = 1f;


        public static void SaveSetting()
        {
            FileStream fs = File.Create("MoreEffectiveTransfer_setting.txt");
            StreamWriter streamWriter = new StreamWriter(fs);

            streamWriter.WriteLine(SETTINGS_VERSION);

            streamWriter.WriteLine(optionPreferLocalService);
            streamWriter.WriteLine(optionWarehouseFirst);
            streamWriter.WriteLine(optionWarehouseReserveTrucks);
            streamWriter.WriteLine(optionPreferExportShipPlaneTrain);
            streamWriter.WriteLine(optionWarehouseNewBalanced);
            streamWriter.WriteLine(optionPathfindChirper);

            streamWriter.Flush();
            fs.Close();
        }

        public static void LoadSetting()
        {
            if (File.Exists("MoreEffectiveTransfer_setting.txt"))
            {
                FileStream fs = new FileStream("MoreEffectiveTransfer_setting.txt", FileMode.Open);
                StreamReader sr = new StreamReader(fs);

                string strLine = sr.ReadLine();
                if (strLine != SETTINGS_VERSION)
                {
                    DebugLog.LogInfo($"Loading Settings - version mismatch detected. Found version: {strLine}, expected version: {SETTINGS_VERSION}. A new settings file will be generated.");
                    sr.Close();
                    fs.Close();
                    return;
                }

                strLine = sr.ReadLine();
                optionPreferLocalService = (strLine == "True") ? true : false;

                strLine = sr.ReadLine();
                optionWarehouseFirst = (strLine == "True") ? true : false;

                strLine = sr.ReadLine();
                optionWarehouseReserveTrucks = (strLine == "True") ? true : false;

                strLine = sr.ReadLine();
                optionPreferExportShipPlaneTrain = (strLine == "True") ? true : false;

                strLine = sr.ReadLine();
                optionWarehouseNewBalanced = (strLine == "True") ? true : false;

                strLine = sr.ReadLine();
                optionPathfindChirper = (strLine == "True") ? true : false;

                sr.Close();
                fs.Close();
            }
        }

        public static void OnSettingsUI(UIHelperBase helper)
        {
            LoadSetting();

#if (DEBUG || PROFILE)
            UIHelperBase group0 = helper.AddGroup(Localization.Get("DEBUGPROFILE"));
            group0.AddCheckbox(Localization.Get("optionEnableNewTransferManager"), optionEnableNewTransferManager, (index) => setOptionEnableNewTransferManager(index));
#endif
            UIHelperBase groupGeneral = helper.AddGroup(Localization.Get("GROUP_GENERAL"));
            groupGeneral.AddCheckbox(Localization.Get("optionPathfindChirper"), optionPathfindChirper, (index) => setOptionPathfindChirper(index));
            UIPanel txtPanel0 = (groupGeneral as UIHelper).self as UIPanel;
            UILabel txtLabel0 = AddDescription(txtPanel0, "optionPathfindChirper_txt", txtPanel0, 1.0f, Localization.Get("optionPathfindChirper_txt"));


            UIHelperBase group1 = helper.AddGroup(Localization.Get("GROUP_SERVICE_OPTIONS"));
            group1.AddCheckbox(Localization.Get("optionPreferLocalService"), optionPreferLocalService, (index) => setOptionPreferLocalService(index));
            UIPanel txtPanel1 = (group1 as UIHelper).self as UIPanel;
            UILabel txtLabel1 = AddDescription(txtPanel1, "optionPreferLocalService_txt", txtPanel1, 1.0f, Localization.Get("optionPreferLocalService_txt"));


            UIHelperBase group2 = helper.AddGroup(Localization.Get("GROUP_WAREHOUSE_OPTIONS"));
            group2.AddCheckbox(Localization.Get("optionWarehouseFirst"), optionWarehouseFirst, (index) => setOptionWarehouseFirst(index));
            UIPanel txtPanel2 = (group2 as UIHelper).self as UIPanel;
            UILabel txtLabel21 = AddDescription(txtPanel2, "optionWarehouseFirst_txt", txtPanel2, 1.0f, Localization.Get("optionWarehouseFirst_txt"));
            UILabel txtLabel21_spacer = AddDescription(txtPanel2, "txtLabel21_spacer", txtPanel2, 1.0f, "");

            group2.AddCheckbox(Localization.Get("optionWarehouseReserveTrucks"), optionWarehouseReserveTrucks, (index) => setOptionWarehouseReserveTrucks(index));
            UILabel txtLabel22 = AddDescription(txtPanel2, "optionWarehouseReserveTrucks_txt", txtPanel2, 1.0f, Localization.Get("optionWarehouseReserveTrucks_txt"));
            UILabel txtLabel22_spacer = AddDescription(txtPanel2, "txtLabel22_spacer", txtPanel2, 1.0f, "");

            group2.AddCheckbox(Localization.Get("optionWarehouseNewBalanced"), optionWarehouseNewBalanced, (index) => setOptionWarehouseNewBalanced(index));
            UILabel txtLabel23 = AddDescription(txtPanel2, "optionWarehouseNewBalanced_txt", txtPanel2, 1.0f, Localization.Get("optionWarehouseNewBalanced_txt"));


            UIHelperBase group3 = helper.AddGroup(Localization.Get("GROUP_EXPORTIMPORT_OPTIONS"));
            group3.AddCheckbox(Localization.Get("optionPreferExportShipPlaneTrain"), optionPreferExportShipPlaneTrain, (index) => setOptionPreferExportShipPlaneTrain(index));
            UIPanel txtPanel3 = (group3 as UIHelper).self as UIPanel;
            UILabel txtLabel3 = AddDescription(txtPanel3, "optionPreferExportShipPlaneTrain_txt", txtPanel3, 1.0f, Localization.Get("optionPreferExportShipPlaneTrain_txt"));

            SaveSetting();
        }



        /* 
         * Code adapted from PropAnarchy under MIT license
         */
        private static readonly Color32 m_greyColor = new Color32(0xe6, 0xe6, 0xe6, 0xee);
        private static UILabel AddDescription(UIPanel panel, string name, UIComponent alignTo, float fontScale, string text)
        {
            UILabel desc = panel.AddUIComponent<UILabel>();
            desc.name = name;
            desc.width = panel.width - 80;
            desc.wordWrap = true;
            desc.autoHeight = true;
            desc.textScale = fontScale;
            desc.textColor = m_greyColor;
            desc.text = text;
            desc.relativePosition = new UnityEngine.Vector3(alignTo.relativePosition.x + 26f, alignTo.relativePosition.y + alignTo.height + 10);
            return desc;
        }


        public static void setOptionEnableNewTransferManager(bool index)
        {
            optionEnableNewTransferManager = index;
            SaveSetting();
            DebugLog.LogDebug(DebugLog.REASON_ALL, $"** OPTION ENABLE/DISABLE: {optionEnableNewTransferManager} **");
        }

        public static void setOptionPreferLocalService(bool index)
        {
            optionPreferLocalService = index;
            SaveSetting();
        }

        public static void setOptionWarehouseFirst(bool index)
        {
            optionWarehouseFirst = index;
            SaveSetting();
        }

        public static void setOptionWarehouseReserveTrucks(bool index)
        {
            optionWarehouseReserveTrucks = index;
            SaveSetting();
        }

        public static void setOptionPreferExportShipPlaneTrain(bool index)
        {
            optionPreferExportShipPlaneTrain = index;
            SaveSetting();
        }

        public static void setOptionWarehouseNewBalanced(bool index)
        {
            optionWarehouseNewBalanced = index;
            SaveSetting();
        }

        public static void setOptionPathfindChirper(bool index)
        {
            optionPathfindChirper = index;
            SaveSetting();
        }

    }
}
