using CitiesHarmony.API;
using ICities;
using MoreEffectiveTransfer.Util;
using System.IO;


namespace MoreEffectiveTransfer
{
    public class MoreEffectiveTransfer : IUserMod
    {
        public static bool IsEnabled = false;
        public static bool debugMode = false;

        // UNUSED options
        public static bool optionFixUnRouteTransfer = true;

        // SERVICE options
        public static bool optionPreferLocalService = false;
        
        // WAREHOUSE options
        public static bool optionWarehouseFirst = false;
        public static bool optionWarehouseReserveTrucks = false;
        public static bool optionWarehouseSpawnUnSpawnFix = false;
        
        // EXPORT options
        public static bool optionPreferExportShipPlaneTrain = false;

        // RUNTIME settings:
        public static float shipStationDistanceRandom = 1f;
        public static float trainStationDistanceRandom = 1f;
        public static float planeStationDistanceRandom = 1f;


        public string Name
        {
            get { return "More Effective Transfer Manager"; }
        }

        public string Description
        {
            get { return "Optimize transfer manager in vanilla game. Match the shortest transfer bettween offers."; }
        }

        public void OnEnabled()
        {
            IsEnabled = true;
            FileStream fs = File.Create("MoreEffectiveTransfer.txt");
            fs.Close();
            HarmonyHelper.EnsureHarmonyInstalled();
        }

        public void OnDisabled()
        {
            IsEnabled = false;
        }

        public static void SaveSetting()
        {
            FileStream fs = File.Create("MoreEffectiveTransfer_setting.txt");
            StreamWriter streamWriter = new StreamWriter(fs);

            streamWriter.WriteLine(optionFixUnRouteTransfer);
            streamWriter.WriteLine(false); //debugMode default false
            streamWriter.WriteLine(optionWarehouseReserveTrucks);
            streamWriter.WriteLine(optionWarehouseFirst);
            streamWriter.WriteLine(optionWarehouseSpawnUnSpawnFix);
            streamWriter.WriteLine(optionPreferExportShipPlaneTrain);
            streamWriter.WriteLine(optionPreferLocalService);
            
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
                optionFixUnRouteTransfer = (strLine == "True") ? true : false;
                
                strLine = sr.ReadLine();
                debugMode = false; //debugMode default false
                
                strLine = sr.ReadLine();
                optionWarehouseReserveTrucks = (strLine == "True") ? true : false;
                
                strLine = sr.ReadLine();
                optionWarehouseFirst = (strLine == "True") ? true : false;
                
                strLine = sr.ReadLine();
                optionWarehouseSpawnUnSpawnFix = (strLine == "True") ? true : false;
                
                strLine = sr.ReadLine();
                optionPreferExportShipPlaneTrain = (strLine == "True") ? true : false;
                
                strLine = sr.ReadLine();
                optionPreferLocalService = (strLine == "True") ? true : false;

                sr.Close();
                fs.Close();
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            LoadSetting();
            UIHelperBase group1 = helper.AddGroup(Localization.Get("FIX_UNROUTED_TRANSFER_MATCH_DESCRIPTION"));
            group1.AddCheckbox(Localization.Get("FIX_UNROUTED_TRANSFER_MATCH_ENALBE"), optionFixUnRouteTransfer, (index) => setOptionFixUnRouteTransfer(index));
            UIHelperBase group2 = helper.AddGroup(Localization.Get("DEBUG_MODE_DESCRIPTION"));
            group2.AddCheckbox(Localization.Get("DEBUG_MODE_DESCRIPTION_ENALBE"), debugMode, (index) => debugModeEnable(index));
            group2.AddCheckbox(Localization.Get("LOCAL_USE_DESCRIPTION_ENALBE"), optionPreferLocalService, (index) => setOptionPreferLocalService(index));
            UIHelperBase group3 = helper.AddGroup(Localization.Get("ADVANCED_WAREHOUSE"));
            group3.AddCheckbox(Localization.Get("WAREHOUSE_FIRST"), optionWarehouseFirst, (index) => setOptionWarehouseFirst(index));
            group3.AddCheckbox(Localization.Get("WAREHOUSE_SPAWN_UNSPAWN"), optionWarehouseSpawnUnSpawnFix, (index) => setOptionWarehouseSpawnUnSpawnFix(index));
            group3.AddCheckbox(Localization.Get("WAREHOUSE_RESEVER_FOR_CITY"), optionWarehouseReserveTrucks, (index) => setOptionWarehouseReserveTrucks(index));
            UIHelperBase group4 = helper.AddGroup(Localization.Get("EXPERIMENTAL_FUNCTION"));
            group4.AddCheckbox(Localization.Get("PREFER_SHIP_PLANE_TRAIN"), optionPreferExportShipPlaneTrain, (index) => setOptionPreferExportShipPlaneTrain(index));
            SaveSetting();
        }


        public void debugModeEnable(bool index)
        {
            debugMode = index;
            SaveSetting();
        }

        public void setOptionFixUnRouteTransfer(bool index)
        {
            optionFixUnRouteTransfer = index;
            SaveSetting();
        }

        public void setOptionPreferLocalService(bool index)
        {
            optionPreferLocalService = index;
            SaveSetting();
        }
        public void setOptionWarehouseFirst(bool index)
        {
            optionWarehouseFirst = index;
            SaveSetting();
        }

        public void setOptionWarehouseSpawnUnSpawnFix(bool index)
        {
            optionWarehouseSpawnUnSpawnFix = index;
            SaveSetting();
        }

        public void setOptionWarehouseReserveTrucks(bool index)
        {
            optionWarehouseReserveTrucks = index;
            SaveSetting();
        }

        public void setOptionPreferExportShipPlaneTrain(bool index)
        {
            optionPreferExportShipPlaneTrain = index;
            SaveSetting();
        }
    }
}
