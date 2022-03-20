using CitiesHarmony.API;
using ColossalFramework.UI;
using ICities;
using MoreEffectiveTransfer.Util;
using System.IO;
using UnityEngine;

namespace MoreEffectiveTransfer
{
    public class MoreEffectiveTransfer : IUserMod
    {
        public static bool IsEnabled = false;

        public const string MOD_VERSION = "2.1.2.20220318";
#if (DEBUG)
        public const string BUILD_TYPE = "DEBUG";
#elif (PROFILE)
        public const string BUILD_TYPE = "PROFILE";
#elif (RELEASE)
        public const string BUILD_TYPE = "RELEASE";
#else
        public const string BUILD_TYPE = "UNKNOWN";
#endif


        public string Name
        {
            get { return "More Effective Transfer Mngr (continued)"; }
        }

        public string Description
        {
            get { return "Optimize transfer manager in vanilla game. Match the shortest transfer between offers."; }
        }

        public void OnEnabled()
        {
            IsEnabled = true;

            HarmonyHelper.EnsureHarmonyInstalled();
            DebugLog.LogInfo(Name + ": VERSION: " + MOD_VERSION + ", BUILD TYPE: " + BUILD_TYPE);
        }

        public void OnDisabled()
        {
            IsEnabled = false;
        } 

        public void OnSettingsUI(UIHelperBase helper)
        {
            ModSettings.OnSettingsUI(helper);
        }


    }
}
