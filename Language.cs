using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MoreEffectiveTransfer
{
    public class Language
    {
        public static string[] English =
        {
            "Fix UnRouted Transfer Match If you have district with broken roadnetwork",
            "Fix UnRouted Transfer Match Enable",
        };

        public static string[] Chinese =
        {
            "修正无法连接的运输,如果你有区域是和其它区域道路连接是隔离的",  //0
            "修正无法连接的运输",          //1
        };

        public static string[] OptionUI = new string[English.Length];

        public static void LanguageSwitch(byte language)
        {
            if (language == 1)
            {
                for (int i = 0; i < English.Length; i++)
                {
                    OptionUI[i] = Chinese[i];
                }
            }
            else if (language == 0)
            {
                for (int i = 0; i < English.Length; i++)
                {
                    OptionUI[i] = English[i];
                }
            }
            else
            {
                DebugLog.LogToFileOnly("unknow language!! use English");
                for (int i = 0; i < English.Length; i++)
                {
                    OptionUI[i] = English[i];
                }
            }
        }
    }
}
