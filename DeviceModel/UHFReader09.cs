extern alias UHF09Dll;
using UHF09Dll.ReaderB;
using DeviceService.Model;
using DeviceService.Model.ExceptionModels;
using NaflimHelperLibrary;
using System;
using System.Text;

namespace DeviceService.DeviceModel
{
    public class UHFReader09 : IDevice
    {
        protected int port;                       //端口
        protected byte comAdr = 0xFF;             //读写器地址
        protected byte baud = 4;                  //波特率
        protected int handle;                  //读写器链接句柄

        const byte ADR_TID = 02;
        const byte LEN_TID = 04;
        const byte TID_FLAG = 0;
        readonly byte[] epcList = new byte[5000];
        int Totallen = 0;
        int cardNum = 0;

        /// <summary>
        /// 自动串口连接
        /// </summary>
        public void Connect()
        {
            int linkflag = StaticClassReaderB.AutoOpenComPort(ref port, ref comAdr, baud, ref handle);

            if (linkflag != 0) throw UHF09Exception.AbnormalJudgment(linkflag);
        }
        public void Connect(ConnectModel connect)
        {
            int linkflag = StaticClassReaderB.OpenComPort(connect.Port, ref comAdr, baud, ref handle);

            if (linkflag != 0) throw UHF09Exception.AbnormalJudgment(linkflag);

            port = connect.Port;
        }

        public void Disconnect()
        {
            int linkflag = StaticClassReaderB.CloseComPort();

            if (linkflag != 0) throw UHF09Exception.AbnormalJudgment(linkflag);
        }

        /// <summary>
        /// 设置功率
        /// </summary>
        /// <param name="power">功率值</param>
        public void SetPower(byte power)
        {
            int powerflag = StaticClassReaderB.SetPowerDbm(ref comAdr, power, handle);

            if (powerflag != 0) throw UHF09Exception.AbnormalJudgment(powerflag);
        }

        /// <summary>
        /// 查询标签
        /// </summary>
        /// <returns>标签组</returns>
        public virtual string[] SelTag()
        {
            int state = StaticClassReaderB.Inventory_G2(ref comAdr, ADR_TID, LEN_TID, TID_FLAG, epcList, ref Totallen, ref cardNum, handle);

            if (state == 0x01 || state == 0x02 || state == 0x03 || state == 0x04)
            {

                int index = 0;

                string[] epcArr = new string[cardNum];

                for (int i = 0; i < cardNum; i++)
                {
                    int len = epcList[index];

                    StringBuilder sb = new StringBuilder();

                    for (int j = 0; j < len; j++)
                        sb.Append(Convert.ToString(epcList[index + 1 + j], 16).PadLeft(2, '0').ToUpper());

                    epcArr[i] = sb.ToString();

                    index += len + 1;
                }

                return epcArr;
            }
            else return null;
        }

        /// <summary>
        /// epc写入标签
        /// </summary>
        /// <param name="epc">写入的epc</param>
        public void WriteTag(string epc)
        {
            int errorcode = 0;
            byte[] epcArr = DataConversion.StrToHexByteArr(epc);
            int writeFlag = StaticClassReaderB.WriteEPC_G2(ref comAdr, new byte[] { 0, 0, 0, 0 }, epcArr, (byte)epcArr.Length, ref errorcode, handle);

            if (writeFlag != 0) throw UHF288Exception.AbnormalJudgment(writeFlag);
        }
    }
}
