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
    public enum AlarmLightColor
    {
        Black,
        Red,
        Green
    }

    public enum InGPIO { Init, None, In1, In2, All }

    public abstract class UHFReader288 : IDevice
    {
        /// <summary>
        /// 红灯端口
        /// </summary>
        public byte RedPort { get; set; } = byte.MaxValue;
        /// <summary>
        /// 绿灯端口
        /// </summary>
        public byte GreenPort { get; set; } = byte.MaxValue;
        /// <summary>
        /// 设备句柄
        /// </summary>
        public int Handle { get { return handle; } set { } }
        /// <summary>
        /// 天线列表
        /// </summary>
        public List<byte> AntennaList { get; set; }
        /// <summary>
        /// 当前存储的EPC
        /// </summary>
        public List<string> SelEPC { get { return cacheEPC; } set { } }

        /// <summary>
        /// 显示异常
        /// </summary>
        public Action<Exception> ErrorShow { get; set; }

        /// <summary>
        /// 抛出日志
        /// </summary>
        public Action<string> ThrowLog { get; set; }

        /// <summary>
        /// 断线事件
        /// </summary>
        public Action<UHFReader288> Reconnection { get; set; }

        /// <summary>
        /// 设备ip
        /// </summary>
        public string IP
        {
            get { return ip; }
        }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port
        {
            get { return port; }
        }


        /// <summary>
        /// 设备名
        /// </summary>
        public string DeviceName { get; set; }

        protected int comPort;
        protected int handle = -1;
        protected byte comAdr = 255;
        protected bool selFlag;
        protected byte errorNum;
        protected string ip;
        private int port;
        protected readonly List<string> cacheEPC = new List<string>();
        protected ConnectMode mode;

        /// <summary>
        /// GPIO转换为IN口
        /// </summary>
        /// <param name="gpio">gpio值</param>
        /// <returns>IN口</returns>
        protected InGPIO GetInGPIO(byte gpio)
        {
            List<InGPIO> inGPIOs = new List<InGPIO>();
            if ((gpio & 0x01) == 1)
                inGPIOs.Add(InGPIO.In1);

            if ((gpio & 0x02) == 2)
                inGPIOs.Add(InGPIO.In2);

            if (inGPIOs.Count == 0)
                return InGPIO.None;
            else if (inGPIOs.Count >= 2)
                return InGPIO.All;
            else
                return inGPIOs[0];
        }

        public void Connect(ConnectModel connect)
        {
            int linkflag = UHF288SDK.OpenNetPort(connect.Port, connect.Ip, ref comAdr, ref handle);
            if (linkflag != 0) throw UHF288Exception.AbnormalJudgment(linkflag);


            ip = connect.Ip;
            port = connect.Port;
            mode = ConnectMode.Tcp;
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
                linkflag = UHF288SDK.AutoOpenComPort(ref comPort, ref comAdr, baud, ref handle);
            else
                linkflag = UHF288SDK.OpenComPort(connect.Com, ref comAdr, baud, ref handle);

           
            if (linkflag != 0)
                throw UHF288Exception.AbnormalJudgment(linkflag);

            mode = ConnectMode.SerialPort;
        }

        public void Disconnect()
        {
            int linkflag = UHF288SDK.CloseNetPort(handle);
            if (linkflag != 0) throw UHF288Exception.AbnormalJudgment(linkflag);
        }

        /// <summary>
        /// 关闭串口连接
        /// </summary>
        public void ComDisconnect()
        {
            int linkflag = UHF288SDK.CloseComPort();

            if (linkflag != 0) throw UHF288Exception.AbnormalJudgment(linkflag);
        }

        /// <summary>
        /// 刷新句柄
        /// </summary>
        /// <param name="reader288">设备句柄</param>
        public void RefreshHandle(UHFReader288 reader288)
        {
            handle = reader288.handle;
        }

        /// <summary>
        /// 获取设备信息
        /// </summary>
        public void GetUHF288Info()
        {
            byte TrType = 0;
            byte[] VersionInfo = new byte[2];
            byte ReaderType = 0;
            byte ScanTime = 0;
            byte dmaxfre = 0;
            byte dminfre = 0;
            byte powerdBm = 0;
            byte Ant = 0;
            byte BeepEn = 0;
            byte OutputRep = 0;
            byte CheckAnt = 0;
            int infoflag = UHF288SDK.GetReaderInformation(ref comAdr, VersionInfo, ref ReaderType, ref TrType, ref dmaxfre, ref dminfre, ref powerdBm, ref ScanTime, ref Ant, ref BeepEn, ref OutputRep, ref CheckAnt, handle);

            if (infoflag != 0) throw UHF288Exception.AbnormalJudgment(infoflag);

            AntennaList = new List<byte>();

            if ((Ant & 0x01) == 1)
                AntennaList.Add(0x80);

            if ((Ant & 0x02) == 2)
                AntennaList.Add(0x81);

            if ((Ant & 0x04) == 4)
                AntennaList.Add(0x82);

            if ((Ant & 0x08) == 8)
                AntennaList.Add(0x83);
        }

        /// <summary>
        /// 设置功率
        /// </summary>
        /// <param name="power"></param>
        public void SetPower(byte power)
        {
            int powerflag = UHF288SDK.SetRfPower(ref comAdr, power, handle);

            if (powerflag != 0) throw UHF288Exception.AbnormalJudgment(powerflag);
        }

        /// <summary>
        /// 获取天线功率
        /// </summary>
        /// <param name="len">天线数</param>
        /// <returns>天线功率</returns>
        public byte[] GetAntPower(int len)
        {
            byte[] powerDbm = new byte[len];
            int powerflag = UHF288SDK.GetRfPowerByAnt(ref comAdr, powerDbm, handle);

            if (powerflag != 0) throw UHF288Exception.AbnormalJudgment(powerflag);

            return powerDbm;
        }

        /// <summary>
        /// 设置天线功率
        /// </summary>
        /// <param name="antPower">天线功率</param>
        /// <param name="save">是否掉电保存</param>
        public void SetAntPower(byte[] antPower, bool save)
        {
            if (!save)
            {
                int len = antPower.Length;
                for (int i = 0; i < len; i++)
                    antPower[i] |= 0x80;
            }

            int antflag = UHF288SDK.SetRfPowerByAnt(ref comAdr, antPower, handle);

            if (antflag != 0) throw UHF288Exception.AbnormalJudgment(antflag);
        }

        /// <summary>
        /// 开始查询
        /// </summary>
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

                        await Task.Delay(0);
                        int fCmdRet = UHF288SDK.Inventory_G2(ref comAdr, Qvalue, Session, MaskMem, MaskAdr, MaskLen, MaskData, MaskFlag, tidAddr, tidLen, TIDFlag, Target, InAnt, Scantime, FastFlag, EPC, ref Ant, ref Totallen, ref TagNum, handle);

                        if (fCmdRet != 0x01 && fCmdRet != 0x02)
                        {
                            if (errorNum < 3)
                            {
                                errorNum++;
                                ThrowLog?.Invoke($"{ip}-查询：{UHF288Exception.AbnormalJudgment(fCmdRet).Message}");
                            }
                            else
                                throw UHF288Exception.AbnormalJudgment(fCmdRet);
                        }
                        string[] EPCarr = DataConversion.GetEPC(EPC).Where(v => !string.IsNullOrEmpty(v)).ToArray();//byte数组以EPC格式转换为字符串数组
                        foreach (string item in EPCarr)
                        {
                            if (item != null && !cacheEPC.Contains(item))
                                cacheEPC.Add(item);
                        }
                        count++;

                    }
                }
                catch (Exception ex)
                {
                    if (ErrorShow is null)
                        Log.PrintError(ex);
                    else
                        ErrorShow(ex);
                    Reconnection?.Invoke(this);
                    if (selFlag) StartSel();
                }
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 查询标签
        /// </summary>
        public virtual void SelTag(int antIndex)
        {

            byte InAnt = AntennaList[antIndex];//不可变
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

            int fCmdRet = UHF288SDK.Inventory_G2(ref comAdr, Qvalue, Session, MaskMem, MaskAdr, MaskLen, MaskData, MaskFlag, tidAddr, tidLen, TIDFlag, Target, InAnt, Scantime, FastFlag, EPC, ref Ant, ref Totallen, ref TagNum, handle);

            if (fCmdRet != 0x01 && fCmdRet != 0x02)
            {
                if (errorNum < 3)
                {
                    errorNum++;
                    ThrowLog?.Invoke($"{ip}-查询：{UHF288Exception.AbnormalJudgment(fCmdRet).Message}");
                }
                else
                    throw UHF288Exception.AbnormalJudgment(fCmdRet);
            }
            string[] EPCarr = DataConversion.GetEPC(EPC).Where(v => !string.IsNullOrEmpty(v)).ToArray();//byte数组以EPC格式转换为字符串数组
            foreach (string item in EPCarr)
            {
                if (item != null && !cacheEPC.Contains(item))
                    cacheEPC.Add(item);
            }
        }

        /// <summary>
        /// 获取GPIO值
        /// </summary>
        /// <returns>GPIO</returns>
        public byte GetGPIO()
        {
            byte outupPin = 0;
            int GPIOflag = UHF288SDK.GetGPIOStatus(ref comAdr, ref outupPin, handle);
            if (GPIOflag != 0) throw UHF288Exception.AbnormalJudgment(GPIOflag);
            return outupPin;
        }

        /// <summary>
        /// 结束查询
        /// </summary>
        public void StopSel()
        {
            selFlag = false;
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public void ClearCache()
        {
            cacheEPC.Clear();
        }

        /// <summary>
        /// 断线重连
        /// </summary>
        protected void FReconnection()
        {
            ThrowLog?.Invoke($"{ip}-开始断线重连...");
            int count = 1;
            try
            {
                switch (mode)
                {
                    case ConnectMode.Tcp:
                        Disconnect();
                        break;
                    case ConnectMode.SerialPort:
                        ComDisconnect();
                        break;
                }
            }
            catch (Exception ex)
            {
                ThrowLog($"{ex.Message}");
            }
            

            while (true)
            {
                ThrowLog?.Invoke($"{ip}-重连开始，重连次数{count}...");
                try
                {
                    switch (mode)
                    {
                        case ConnectMode.Tcp:
                            ThrowLog?.Invoke($"ip:{ip}port:{port}");
                            Connect(new ConnectModel(ip,port));
                            ThrowLog?.Invoke("重连成功！");
                            return;
                    }
                }
                catch (Exception ex)
                {
                    ThrowLog($"{ex.Message}");
                }

                count++;
                System.Threading.Thread.Sleep(5000);
            };

        }

        /// <summary>
        /// 打开警报灯
        /// </summary>
        /// <param name="lightColor">警报灯颜色</param>
        /// <param name="alarmTime">警报时长</param>
        public void OpenAlarmLight(AlarmLightColor lightColor, int alarmTime)
        {
            try
            {
                if (RedPort == byte.MaxValue || GreenPort == byte.MaxValue)
                    throw new UHF288Exception("警报灯端口未设置！");

                switch (lightColor)
                {
                    case AlarmLightColor.Black:
                        UHF288SDK.SetGPIO(ref comAdr, 0, handle);
                        break;
                    case AlarmLightColor.Red:
                        UHF288SDK.SetGPIO(ref comAdr, RedPort, handle);
                        break;
                    case AlarmLightColor.Green:
                        UHF288SDK.SetGPIO(ref comAdr, GreenPort, handle);
                        break;
                }
                System.Threading.Thread.Sleep(alarmTime);
                UHF288SDK.SetGPIO(ref comAdr, 0, handle);
                byte test = 0;
                int GPIOflag = UHF288SDK.GetGPIOStatus(ref comAdr, ref test, handle);
            }
            catch (Exception ex)
            {
                if (ErrorShow is null)
                    Log.PrintError(ex);
                else
                    ErrorShow(ex);
            }
        }
    }
}
