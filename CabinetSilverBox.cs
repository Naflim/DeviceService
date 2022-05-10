using DeviceService.DeviceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NaflimHelperLibrary;
using System.IO.Ports;

namespace DeviceService
{
    public enum LockState { None, Open, Close }
    public class CabinetSilverBox : CommunicationProtocol, ICabinet
    {
        /// <summary>
        /// 锁板地址
        /// </summary>
        public byte BoardPort { get; set; } = 0x01;

        /// <summary>
        /// 锁板状态变化
        /// </summary>
        public Action<byte, int, LockState> LockChange { get; set; }

        public CabinetSilverBox()
        {
        }

        public CabinetSilverBox(byte boardPort)
        {
            BoardPort = boardPort;
        }

        protected override void SerialPortReceive()
        {
            if (serialPort is null)
                throw new NotImplementedException("未连接串口");

            Task.Factory.StartNew(v =>
            {
                try
                {
                    List<byte> feedback = new List<byte>();
                    while (true)
                    {
                        byte[] buffer = new byte[1024];

                        int len = serialPort.Read(buffer, 0, buffer.Length);
                        if (len == 0) continue;

                        byte[] data = new byte[len];
                        Array.Copy(buffer, data, len);

                        for (int i = 0; i < len; i++)
                        {

                            if (feedback.Count == 0 && data[i] == 0x81)
                                feedback.Add(0x81);
                            else if (feedback.Count < 5 && feedback.Count > 0)
                            {
                                feedback.Add(data[i]);

                                if (feedback.Count == 5)
                                {
                                    byte[] val = new byte[4];
                                    Array.Copy(feedback.ToArray(), val, 4);

                                    if (DataConversion.BCC(val) == feedback[4])
                                    {
                                        LockState lockState = LockState.None;
                                        if (feedback[3] == 0x00)
                                            lockState = LockState.Close;
                                        else if (feedback[3] == 0x11)
                                            lockState = LockState.Open;

                                        LockChange?.Invoke(feedback[1], feedback[2], lockState);
                                    }
                                    feedback.Clear();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrowShow.Invoke(ex);
                }
                
            }, TaskCreationOptions.LongRunning);
        }

        public void CloseDoor()
        {
            throw new NotImplementedException();
        }

        public void OpenAllDoor()
        {
            byte[] cmd = new byte[] { 0x8A, BoardPort, 0x00, 0x11, 0x9A };
            SendData(cmd);
        }

        public void OpenDoor(int doorID)
        {
            List<byte> cmd = new List<byte> { 0x8A, BoardPort, (byte)doorID, 0x11 };
            int bcc = DataConversion.BCC(cmd.ToArray());
            cmd.Add((byte)bcc);
            SendData(cmd.ToArray());
        }
    }
}
