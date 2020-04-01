using ICities;
using MoreEffectiveTransfer.Util;
using System.IO;


namespace MoreEffectiveTransfer
{
    public class MoreEffectiveTransfer : IUserMod
    {
        public static bool IsEnabled = false;
        public static bool fixUnRouteTransfer = true;
        public static bool debugMode = false;
        public static bool warehouseFirst = false;
        public static bool warehouseSpawnUnSpawnFix = false;
        public static bool warehouseTransfer = false;

        public string Name
        {
            get { return "More Effective Transfer Manager"; }
        }

        public string Description
        {
            get { return "Optimize transfer manager in vanilla game. match the shortest transfer bettween offers"; }
        }

        public void OnEnabled()
        {
            IsEnabled = true;
            FileStream fs = File.Create("MoreEffectiveTransfer.txt");
            fs.Close();
        }

        public void OnDisabled()
        {
            IsEnabled = false;
        }

        public static void SaveSetting()
        {
            FileStream fs = File.Create("MoreEffectiveTransfer_setting.txt");
            StreamWriter streamWriter = new StreamWriter(fs);
            streamWriter.WriteLine(fixUnRouteTransfer);
            streamWriter.WriteLine(debugMode);
            streamWriter.WriteLine("false");//preserve
            streamWriter.WriteLine(warehouseFirst);
            streamWriter.WriteLine(warehouseSpawnUnSpawnFix);
            streamWriter.WriteLine(warehouseTransfer);
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
                fixUnRouteTransfer = (strLine == "True") ? true : false;
                strLine = sr.ReadLine();
                debugMode = (strLine == "True") ? true : false;
                sr.ReadLine(); //Preserve
                strLine = sr.ReadLine();
                warehouseFirst = (strLine == "True") ? true : false;
                strLine = sr.ReadLine();
                warehouseSpawnUnSpawnFix = (strLine == "True") ? true : false;
                strLine = sr.ReadLine();
                warehouseTransfer = (strLine == "True") ? true : false;

                sr.Close();
                fs.Close();
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            LoadSetting();
            UIHelperBase group1 = helper.AddGroup(Localization.Get("FIX_UNROUTED_TRANSFER_MATCH_DESCRIPTION"));
            group1.AddCheckbox(Localization.Get("FIX_UNROUTED_TRANSFER_MATCH_ENALBE"), fixUnRouteTransfer, (index) => fixUnRouteTransferEnable(index));
            UIHelperBase group2 = helper.AddGroup(Localization.Get("DEBUG_MODE_DESCRIPTION"));
            group2.AddCheckbox(Localization.Get("DEBUG_MODE_DESCRIPTION_ENALBE"), debugMode, (index) => debugModeEnable(index));
            UIHelperBase group3 = helper.AddGroup(Localization.Get("ADVANCED_WAREHOUSE"));
            group3.AddCheckbox(Localization.Get("WAREHOUSE_FIRST"), warehouseFirst, (index) => warehouseFirstEnable(index));
            group3.AddCheckbox(Localization.Get("WAREHOUSE_SPAWN_UNSPAWN"), warehouseSpawnUnSpawnFix, (index) => warehouseSpawnUnSpawnFixEnable(index));
            group3.AddCheckbox(Localization.Get("WAREHOUSE_TRANSFER"), warehouseTransfer, (index) => warehouseTransferEnable(index));
            SaveSetting();
        }

        public void fixUnRouteTransferEnable(bool index)
        {
            fixUnRouteTransfer = index;
            SaveSetting();
        }

        public void debugModeEnable(bool index)
        {
            debugMode = index;
            SaveSetting();
        }

        public void warehouseFirstEnable(bool index)
        {
            warehouseFirst = index;
            SaveSetting();
        }

        public void warehouseSpawnUnSpawnFixEnable(bool index)
        {
            warehouseSpawnUnSpawnFix = index;
            SaveSetting();
        }

        public void warehouseTransferEnable(bool index)
        {
            warehouseTransfer = index;
            SaveSetting();
        }

    }
}
