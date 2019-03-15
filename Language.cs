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
            "Radical Service Select(Enable to respond very quickly, leads to less building problems but use more service cars)",
            "Radical Garbage Service(Suggest Disable)",
            "Radical Police Service(Suggest Enable)",
            "Radical Dead Service(Suggest Enable)",
            "Radical Hospital Service(Suggest Enable)",
            "Radical Road Service(Suggest Disable)",
            "Radical Fire Service(Suggest Enable)",
        };



        public static string[] Chinese =
        {
            "激进的服务策略(迅速响应,可减少楼房问题的概率但会增加城市服务车子的数量)",  //0
            "激进的垃圾服务(建议关闭)",          //1
            "激进的警察服务(建议打开)",          //2
            "激进的遗体服务(建议打开)",        //3
            "激进的医疗服务(建议打开)",        //4
            "激进的道路服务(建议关闭)",        //5
            "激进的消防服务(建议打开)",        //6
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
