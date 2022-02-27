#define TRACE

using System;
using System.Diagnostics;
using System.IO;
using ColossalFramework;
using ColossalFramework.Plugins;
using HarmonyLib;
using ICities;
using static ColossalFramework.Plugins.PluginManager;


namespace MoreEffectiveTransfer.Util
{
    public static class DebugLog
    {
        private const string LOG_FILE_NAME = "MoreEffectiveTransfer.log";
        private static TraceListener _listener = new TextWriterTraceListener(LOG_FILE_NAME);
        private static bool _init = false;

        private static void InitLogging()
        {
            if (!_init)
            {
                FileStream fs = File.Create(LOG_FILE_NAME);
                fs.Close();

                Trace.Listeners.Add(_listener);
                Trace.AutoFlush = true;
                _init = true;
            }
        }


        public static void LogError(string msg, bool popup = false)
        {
            LogInfo($"[METM] ERROR: {msg}");
            UnityEngine.Debug.LogError($"[METM] ERROR: {msg}");
            if (popup)
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, $"[METM] ERROR: {msg}");
        }

        public static void LogInfo(string msg, bool outputlog = true)
        {
            if (!_init) InitLogging();

            Trace.WriteLine(msg);

            if (outputlog)
                UnityEngine.Debug.Log($"[METM] {msg}");
        }

        [Conditional("DEBUG")]
        public static void LogDebug(string msg)
        {
            LogInfo(msg, false);
        }


        public static void ReportAllHarmonyPatches()
        {
            DebugLog.LogInfo($"-- HARMONY PATCH REPORT --");
            var harmony = new Harmony(HarmonyDetours.ID);
            var methods = harmony.GetPatchedMethods();
            foreach (var method in methods)
            {
                var info = Harmony.GetPatchInfo(method);

                DebugLog.LogInfo($"- Harmony patched method = {method.FullDescription()} - #patchers: {info.Owners.Count} - Prefixes:{info.Prefixes.Count}, Postfixes:{info.Postfixes.Count}");
                foreach (var owner in info.Owners)
                {
                    DebugLog.LogInfo($"   ->Patched by: {owner.ToString()}");
                }
            }
        }

        /* Below code adapted from TMPE under MIT license */
        /* original copyright The TMPE team */

        public static void ReportAllMods()
        {
            DebugLog.LogInfo($"-- INSTALLED MOD REPORT --");
            foreach (PluginInfo mod in Singleton<PluginManager>.instance.GetPluginsInfo())
            {
                if (!mod.isCameraScript)
                {
                    string strModName = GetModName(mod);
                    ulong workshopID = mod.publishedFileID.AsUInt64;
                    bool isLocal = workshopID == ulong.MaxValue;

                    DebugLog.LogInfo($"Installed Mod: {strModName}, Id: {workshopID}, local={isLocal}, enabled={mod.isEnabled}, assemblies: {mod.assembliesString}");
                }
            }
        }


        /// <summary>
        /// Gets the name of the specified mod.
        /// It will return the <see cref="IUserMod.Name"/> if found, otherwise it will return
        /// <see cref="PluginInfo.name"/> (assembly name).
        /// </summary>
        /// <param name="plugin">The <see cref="PluginInfo"/> associated with the mod.</param>
        /// <returns>The name of the specified plugin.</returns>
        private static string GetModName(PluginInfo plugin)
        {
            try
            {
                if (plugin == null)
                {
                    return "(PluginInfo is null)";
                }

                if (plugin.userModInstance == null)
                {
                    return string.IsNullOrEmpty(plugin.name)
                        ? "(userModInstance and name are null)"
                        : $"({plugin.name})";
                }

                return ((IUserMod)plugin.userModInstance).Name;
            }
            catch
            {
                return $"(error retreiving Name)";
            }
        }

        /// <summary>
        /// Gets the <see cref="Guid"/> of a mod.
        /// </summary>
        /// <param name="plugin">The <see cref="PluginInfo"/> associated with the mod.</param>
        /// <returns>The <see cref="Guid"/> of the mod.</returns>
        private static Guid GetModGuid(PluginInfo plugin)
        {
            return plugin.userModInstance.GetType().Assembly.ManifestModule.ModuleVersionId;
        }

    }
}
