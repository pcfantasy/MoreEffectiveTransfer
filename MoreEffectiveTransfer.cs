using ICities;
using MoreEffectiveTransfer.Util;
using System.IO;


namespace MoreEffectiveTransfer
{
    public class MoreEffectiveTransfer : IUserMod
    {
        public static bool IsEnabled = false;
        public static bool fixUnRouteTransfer = false;
        public static bool debugMode = false;
        public static byte policeMode = 2;
        public static byte fireMode = 2;
        public static byte deadMode = 2;
        public static byte taxiMode = 2;
        public static byte garbageMode = 2;


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
            streamWriter.WriteLine(policeMode);
            streamWriter.WriteLine(fireMode);
            streamWriter.WriteLine(deadMode);
            streamWriter.WriteLine(taxiMode);
            streamWriter.WriteLine(garbageMode);
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
                strLine = sr.ReadLine();
                if (!byte.TryParse(strLine, out policeMode)) policeMode = 2;
                strLine = sr.ReadLine();
                if (!byte.TryParse(strLine, out fireMode)) fireMode = 2;
                strLine = sr.ReadLine();
                if (!byte.TryParse(strLine, out deadMode)) deadMode = 2;
                strLine = sr.ReadLine();
                if (!byte.TryParse(strLine, out taxiMode)) taxiMode = 2;
                strLine = sr.ReadLine();
                if (!byte.TryParse(strLine, out garbageMode)) garbageMode = 2;

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
            UIHelperBase group3 = helper.AddGroup(Localization.Get("TRANSFER_MATCH_MODE"));
            group3.AddDropdown(Localization.Get("POLICE"), new string[] { Localization.Get("INCOMING_ONLY"), Localization.Get("OUTGOING_ONLY"), Localization.Get("BALANCE"), Localization.Get("INCOMING_FIRST") }, policeMode, (index) => GetEffortIdex(index));
            group3.AddDropdown(Localization.Get("FIRETRUCK"), new string[] { Localization.Get("INCOMING_ONLY"), Localization.Get("OUTGOING_ONLY"), Localization.Get("BALANCE"), Localization.Get("INCOMING_FIRST") }, fireMode, (index) => GetEffortIdex1(index));
            group3.AddDropdown(Localization.Get("HEARSE"), new string[] { Localization.Get("INCOMING_ONLY"), Localization.Get("OUTGOING_ONLY"), Localization.Get("BALANCE"), Localization.Get("INCOMING_FIRST") }, deadMode, (index) => GetEffortIdex2(index));
            group3.AddDropdown(Localization.Get("TAXI"), new string[] { Localization.Get("INCOMING_ONLY"), Localization.Get("OUTGOING_ONLY"), Localization.Get("BALANCE"), Localization.Get("INCOMING_FIRST") }, taxiMode, (index) => GetEffortIdex3(index));
            group3.AddDropdown(Localization.Get("GARBAGE"), new string[] { Localization.Get("INCOMING_ONLY"), Localization.Get("OUTGOING_ONLY"), Localization.Get("BALANCE"), Localization.Get("INCOMING_FIRST") }, garbageMode, (index) => GetEffortIdex4(index));
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

        public void GetEffortIdex(int index)
        {
            policeMode = (byte)index;
            SaveSetting();
        }

        public void GetEffortIdex1(int index1)
        {
            fireMode = (byte)index1;
            SaveSetting();
        }

        public void GetEffortIdex2(int index2)
        {
            deadMode = (byte)index2;
            SaveSetting();
        }

        public void GetEffortIdex3(int index3)
        {
            taxiMode = (byte)index3;
            SaveSetting();
        }

        public void GetEffortIdex4(int index4)
        {
            garbageMode = (byte)index4;
            SaveSetting();
        }
    }
}
