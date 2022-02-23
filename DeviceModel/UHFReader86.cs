using DeviceService.Model;
using DeviceService.Model.ExceptionModels;
using DeviceService.SDK;
using NaflimHelperLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.DeviceModel
{
    public class UHFReader86 : IDevice
    {
        protected int port;                       //端口
        protected byte comAdr = 0xFF;             //读写器地址
        protected byte baud = 4;                  //波特率
        protected int handle;                  //读写器链接句柄

        /// <summary>
        /// 自动串口连接
        /// </summary>
        public void Connect()
        {
            int linkflag = UHF86SDK.AutoOpenComPort(ref port, ref comAdr, baud, ref handle);

            if (linkflag != 0) throw UHF86Exception.AbnormalJudgment(linkflag);
        }

        public void Connect(ConnectModel connect)
        {
            int linkflag = UHF86SDK.OpenComPort(connect.Port, ref comAdr, baud, ref handle);

            if (linkflag != 0) throw UHF86Exception.AbnormalJudgment(linkflag);

            port = connect.Port;
        }

        public void Disconnect()
        {
            int linkflag = UHF86SDK.CloseComPort();

            if (linkflag != 0) throw UHF86Exception.AbnormalJudgment(linkflag);
        }

        /// <summary>
        /// 设置功率
        /// </summary>
        /// <param name="power">功率值</param>
        public void SetPower(byte power)
        {
            int powerflag = UHF86SDK.SetRfPower(ref comAdr, power, handle);

            if (powerflag != 0) throw UHF86Exception.AbnormalJudgment(powerflag);
        }

        /// <summary>
        /// 查询标签
        /// </summary>
        /// <returns>标签组</returns>
        public virtual string[] SelTag()
        {

            byte Ant = 0;
            int CardNum = 0;
            int Totallen = 0;
            byte[] EPC = new byte[50000];
            byte MaskMem = 0;
            byte[] MaskAdr = new byte[2];
            byte MaskLen = 0;
            byte[] MaskData = new byte[100];
            byte MaskFlag = 0;
            byte AdrTID = 0;
            byte LenTID = 6;

            byte Qvalue = 0;
            byte Session = 0;
            byte TIDFlag = 0;
            byte Target = 0;
            byte InAnt = 0;
            byte Scantime = 0;
            byte FastFlag = 0;
            int state = UHF86SDK.Inventory_G2(ref comAdr, Qvalue, Session, MaskMem, MaskAdr, MaskLen, MaskData, MaskFlag, AdrTID, LenTID, TIDFlag, Target, InAnt, Scantime, FastFlag, EPC, ref Ant, ref Totallen, ref CardNum, handle);

            if (state == 0x01 || state == 0x02 || state == 0x03 || state == 0x04)
            {

                var epcArr = DataConversion.GetEPC(EPC);//byte数组以EPC格式转换为字符串数组

                string[] selEpc = new string[CardNum];

                for (int i = 0; i < CardNum; i++)
                    selEpc[i] = epcArr[i];

                return selEpc;
            }
            else throw UHF86Exception.AbnormalJudgment(state); ;
        }

        /// <summary>
        /// epc写入标签
        /// </summary>
        /// <param name="epc">写入的epc</param>
        public void WriteTag(string epc)
        {
            int errorcode = 0;
            byte[] epcArr = DataConversion.StrToHexByteArr(epc);
            int writeFlag = UHF86SDK.WriteEPC_G2(ref comAdr, new byte[] { 0, 0, 0, 0 }, epcArr, (byte)(epc.Length / 4), ref errorcode, handle);

            if (writeFlag != 0) throw UHF86Exception.AbnormalJudgment(writeFlag);
        }
    }
}
