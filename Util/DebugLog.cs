using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MoreEffectiveTransfer.Util
{
    public static class DebugLog
    {
        public static void LogToFileOnly(string msg)
        {
            using (FileStream fileStream = new FileStream("MoreEffectiveTransfer.txt", FileMode.Append))
            {
                StreamWriter streamWriter = new StreamWriter(fileStream);
                streamWriter.WriteLine(msg);
                streamWriter.Flush();
            }
        }


        [Conditional("DEBUG")]
        public static void DebugMsg(string msg)
        {
            //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "[METM] "+msg);
            LogToFileOnly(msg);
        }

    }
}
