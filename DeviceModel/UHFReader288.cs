using DeviceService.Model;
using DeviceService.Model.ExceptionModels;
using DeviceService.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NaflimHelperLibrary;

namespace DeviceService.DeviceModel
{
    public abstract class UHFReader288 : IDevice
    {
        public int Handle { get { return handle; } set { } }
        public List<byte> AntennaList { get; set; }
        public List<string> SelEPC { get { return cacheEPC; } set { } }
        public int SelInterval { get; set; } = 100;
        protected int port;

        protected int handle = -1;
        protected byte comAdr = 255;
        protected bool selFlag;
        protected bool workFlag;
        protected readonly List<string> cacheEPC = new List<string>();

        public void Connect(ConnectModel connect)
        {
            int linkflag = RWDev.OpenNetPort(connect.Port, connect.Ip, ref comAdr, ref handle);

            if (linkflag != 0) throw UHF288Exception.AbnormalJudgment(linkflag);
        }

        /// <summary>
        /// 串口连接
        /// </summary>
        /// <param name="connect">连接参数</param>
        public void ComConnect(ConnectModel connect)
        {
            byte baud = Convert.ToByte(connect.Com);
            if (baud > 2)
                baud = Convert.ToByte(baud + 2);
            int linkflag;
            if (connect.Com == 0)
                linkflag = RWDev.AutoOpenComPort(ref port, ref comAdr, baud, ref handle);
            else
                linkflag = RWDev.OpenComPort(connect.Com, ref comAdr, baud, ref handle);

            if (linkflag != 0)
                throw UHF288Exception.AbnormalJudgment(linkflag);
        }

        public void Disconnect()
        {
            int linkflag = RWDev.CloseNetPort(handle);

            if (linkflag != 0) throw UHF288Exception.AbnormalJudgment(linkflag);
        }

        /// <summary>
        /// 关闭串口连接
        /// </summary>
        public void ComDisconnect()
        {
            int linkflag = RWDev.CloseComPort();

            if (linkflag != 0) throw UHF288Exception.AbnormalJudgment(linkflag);
        }

        /// <summary>
        /// 设置功率
        /// </summary>
        /// <param name="power"></param>
        public void SetPower(byte power)
        {
            int powerflag = RWDev.SetRfPower(ref comAdr, power, handle);

            if (powerflag != 0) throw UHF288Exception.AbnormalJudgment(powerflag);
        }

        public virtual void StartSel()
        {
            if (selFlag || AntennaList is null)
                return;
            selFlag = true;

            Task.Factory.StartNew(async () =>
            {
                int count = 0;
                try
                {
                    while (selFlag)
                    {
                        if (workFlag)
                            continue;

                        #region 防止线程撞车
                        workFlag = true;
                        if (count >= AntennaList.Count)
                            count = 0;

                        byte InAnt = AntennaList[count];//不可变
                        byte FastFlag = 1;//不可变
                        byte Qvalue = 4;//可变
                        byte tidAddr = 0;//可变
                        byte tidLen = 0;//可变
                        byte TIDFlag = 0;//可变
                        byte Scantime = 20;//可变
                        byte Target = 0;
                        byte Ant = 0;
                        byte MaskMem = 0;
                        byte MaskLen = 0;
                        byte MaskFlag = 0;
                        byte Session = 0;
                        byte[] MaskAdr = new byte[2];
                        byte[] MaskData = new byte[100];
                        byte[] EPC = new byte[50000];
                        int TagNum = 0;
                        int Totallen = 0;

                        int fCmdRet = RWDev.Inventory_G2(ref comAdr, Qvalue, Session, MaskMem, MaskAdr, MaskLen, MaskData, MaskFlag, tidAddr, tidLen, TIDFlag, Target, InAnt, Scantime, FastFlag, EPC, ref Ant, ref Totallen, ref TagNum, handle);
                        workFlag = false;
                        #endregion

                        if (fCmdRet != 0x01 && fCmdRet != 0x02)
                            throw UHF288Exception.AbnormalJudgment(fCmdRet);
                        string[] EPCarr = DataConversion.GetEPC(EPC);//byte数组以EPC格式转换为字符串数组
                        foreach (string item in EPCarr)
                        {
                            if (item != null && !cacheEPC.Contains(item))
                                cacheEPC.Add(item);
                        }
                        count++;
                        await Task.Delay(SelInterval);
                    }
                }
                catch (Exception ex)
                {
                    Log.PrintError(ex);
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void StopSel()
        {
            selFlag = false;
        }

        public void ClearCache()
        {
            cacheEPC.Clear();
        }
    }
}
