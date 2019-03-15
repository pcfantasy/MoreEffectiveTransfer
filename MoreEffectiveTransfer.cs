using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEffectiveTransfer
{
    public class MoreEffectiveTransfer : IUserMod
    {
        public static bool IsEnabled = false;
        public static bool radicalGarbageService = false;
        public static bool radicalPoliceService = false;
        public static bool radicalDeadService = false;
        public static bool radicalHospitalService = false;
        public static bool radicalRoadService = false;
        public static bool radicalFireService = false;

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
            //save langugae
            FileStream fs = File.Create("MoreEffectiveTransfer_setting.txt");
            StreamWriter streamWriter = new StreamWriter(fs);
            streamWriter.WriteLine(radicalGarbageService);
            streamWriter.WriteLine(radicalPoliceService);
            streamWriter.WriteLine(radicalDeadService);
            streamWriter.WriteLine(radicalHospitalService);
            streamWriter.WriteLine(radicalRoadService);
            streamWriter.WriteLine(radicalFireService);
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

                if (strLine == "False")
                {
                    radicalGarbageService = false;
                }
                else
                {
                    radicalGarbageService = true;
                }

                strLine = sr.ReadLine();

                if (strLine == "False")
                {
                    radicalPoliceService = false;
                }
                else
                {
                    radicalPoliceService = true;
                }

                strLine = sr.ReadLine();

                if (strLine == "False")
                {
                    radicalDeadService = false;
                }
                else
                {
                    radicalDeadService = true;
                }

                strLine = sr.ReadLine();

                if (strLine == "False")
                {
                    radicalHospitalService = false;
                }
                else
                {
                    radicalHospitalService = true;
                }

                strLine = sr.ReadLine();

                if (strLine == "False")
                {
                    radicalRoadService = false;
                }
                else
                {
                    radicalRoadService = true;
                }

                strLine = sr.ReadLine();

                if (strLine == "False")
                {
                    radicalFireService = false;
                }
                else
                {
                    radicalFireService = true;
                }

                sr.Close();
                fs.Close();
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {

            LoadSetting();
            if (SingletonLite<LocaleManager>.instance.language.Contains("zh"))
            {
                Language.LanguageSwitch(1);
            }
            else
            {
                Language.LanguageSwitch(0);
            }

            UIHelperBase group1 = helper.AddGroup(Language.OptionUI[0]);
            group1.AddCheckbox(Language.OptionUI[1], radicalGarbageService, (index) => radicalGarbageServiceEnable(index));
            group1.AddCheckbox(Language.OptionUI[2], radicalPoliceService, (index) => radicalPoliceServiceEnable(index));
            group1.AddCheckbox(Language.OptionUI[3], radicalDeadService, (index) => radicalDeadServiceEnable(index));
            group1.AddCheckbox(Language.OptionUI[4], radicalHospitalService, (index) => radicalHospitalServiceEnable(index));
            group1.AddCheckbox(Language.OptionUI[5], radicalRoadService, (index) => radicalRoadServiceEnable(index));
            group1.AddCheckbox(Language.OptionUI[6], radicalFireService, (index) => radicalFireServiceEnable(index));
            SaveSetting();
        }

        public void radicalGarbageServiceEnable(bool index)
        {
            radicalGarbageService = index;
            SaveSetting();
        }

        public void radicalPoliceServiceEnable(bool index)
        {
            radicalPoliceService = index;
            SaveSetting();
        }

        public void radicalDeadServiceEnable(bool index)
        {
            radicalDeadService = index;
            SaveSetting();
        }

        public void radicalHospitalServiceEnable(bool index)
        {
            radicalHospitalService = index;
            SaveSetting();
        }

        public void radicalRoadServiceEnable(bool index)
        {
            radicalRoadService = index;
            SaveSetting();
        }

        public void radicalFireServiceEnable(bool index)
        {
            radicalFireService = index;
            SaveSetting();
        }
    }
}
