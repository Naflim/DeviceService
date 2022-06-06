using DeviceService.DeviceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NaflimHelperLibrary;

namespace DeviceService.Product
{
    public class CabinetSilverBox : Protocol, ICabinet
    {
        public byte BoardPort { get; set; } = 0x01;
        public void CloseDoor()
        {
            throw new NotImplementedException();
        }

        public void OpenAllDoor()
        {
            byte[] cmd = new byte[] { 0x8A, BoardPort, 0x00, 0x11, 0x9A };
            Communication(cmd);
        }

        public void OpenDoor(int doorID)
        {
            List<byte> cmd = new List<byte> { 0x8A, BoardPort, (byte)doorID, 0x11 };
            int bcc = DataConversion.BCC(cmd.ToArray());
            cmd.Add((byte)bcc);
            Communication(cmd.ToArray());
        }
    }
}
