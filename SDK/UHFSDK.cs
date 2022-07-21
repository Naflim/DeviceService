using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.SDK
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RFIDTag
    {
        public byte PacketParam;
        public byte LEN;
        public string UID;
        public byte RSSI;
        public byte ANT;
        public Int32 Handles;
    }

    public delegate void RFIDCallBack(IntPtr p, Int32 nEvt);
}
