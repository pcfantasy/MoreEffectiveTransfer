using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEffectiveTransfer.Util
{
    public class MainDataStore
    {
        public static ushort lastBuildingID = 0;
        public static ushort[,] canNotConnectedBuildingID = new ushort[49152, 255];
        public static byte[] refreshCanNotConnectedBuildingIDCount = new byte[49152];
        public static byte[] canNotConnectedBuildingIDCount = new byte[49152];
    }
}
