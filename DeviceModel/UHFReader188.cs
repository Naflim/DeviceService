/*************************************************************************************
 *
 * 文 件 名:   UHFReader188
 * 描    述:   UHF188读写器SDk封装
 * 
 * 版    本：  V1.0
 * 创 建 者：  Naflim 
 * 创建时间：  2022/5/26 10:13:39
 * ======================================================
 * 历史更新记录
 * 版本： V          修改时间：         修改人：
 * 修改内容：
 * ======================================================
*************************************************************************************/

extern alias UHF188Dll;
using UHF188Dll.ReaderB;
using DeviceService.Model;
using DeviceService.Model.ExceptionModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.DeviceModel
{
    /// <summary>
    /// UHFReader188型号读写器
    /// </summary>
    public class UHFReader188 : IReader
    {
        /// <summary>
        /// 查询时间
        /// </summary>
        public int SelTime { get; set; }

        /// <summary>
        /// 查询间隙
        /// </summary>
        public int SelInterval { get; set; } = 0;

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

        protected ConnectMode mode;
        protected int handle = -1;
        protected byte comAdr = 255;
        protected string ip;
        private int port;

        public void Connect(ConnectModel connect)
        {
            mode = connect.ConnectMode;
            int isLink;
            switch (connect.ConnectMode)
            {
                case ConnectMode.Tcp:
                    isLink = StaticClassReaderB.OpenNetPort(connect.Port,connect.Ip,ref comAdr,ref handle);
                    break;
                case ConnectMode.SerialPort:
                    if(connect.Com == 0)
                        isLink = StaticClassReaderB.AutoOpenComPort(ref port, ref comAdr, (byte)connect.BaudRate, ref handle);
                    else
                        isLink = StaticClassReaderB.OpenComPort(connect.Com, ref comAdr, (byte)connect.BaudRate, ref handle);
                    break;
                default:
                    throw new UHF188Exception("意外的连接模式");
            }

            if (isLink != 0)
                throw UHF188Exception.AbnormalJudgment(isLink);

            ip = connect.Ip;
        }

        public void Disconnect()
        {
            int isBreak;
            switch (mode)
            {
                case ConnectMode.Tcp:
                    isBreak = StaticClassReaderB.CloseNetPort(handle);
                    break;
                case ConnectMode.SerialPort:
                    isBreak = StaticClassReaderB.CloseComPort();
                    break;
                default:
                    throw new UHF188Exception("意外的连接模式");
            }

            if (isBreak != 0)
                throw UHF188Exception.AbnormalJudgment(isBreak);

            ip = string.Empty;
        }

        /// <summary>
        /// 设置功率
        /// </summary>
        /// <param name="power">功率大小</param>
        public void SetPower(byte power)
        {
            int isPower = StaticClassReaderB.SetPowerDbm(ref comAdr, power, handle);

            if (isPower != 0) throw UHF188Exception.AbnormalJudgment(isPower);
        }

        public async Task<Tag[]> CyclicQueryTags(int seconds)
        {
            List<Tag> tags = new List<Tag>();
            try
            {
                DateTime startTime = DateTime.Now;

                while (DateTime.Now < startTime.AddSeconds(seconds))
                {
                    byte Qvalue = 4;//可变
                    byte TIDFlag = 0;//可变
                    byte Session = 0;
                    byte[] MaskAdr = new byte[2];
                    byte[] MaskData = new byte[100];
                    byte[] EPC = new byte[50000];
                    int Totallen = 0;
                    byte AdrTID = 0;
                    byte LenTID = 0;
                    int CardNum = 0;
                    int isReader = StaticClassReaderB.Inventory_G2(ref comAdr, Qvalue, Session, AdrTID, LenTID, TIDFlag, EPC, ref Totallen, ref CardNum, handle);
                    if ((isReader == 1) | (isReader == 2) | (isReader == 3) | (isReader == 4) | (isReader == 0xFB))
                    {
                        if (CardNum == 0) continue;
                        byte[] buffer = new byte[Totallen];
                        Array.Copy(EPC, buffer, Totallen);
                        List<string> EPCarr = new List<string>();
                        string temps = ByteArrayToHexString(buffer);
                        int index = 0;
                        for (int i = 0; i < CardNum; i++)
                        {
                            byte EPClen = buffer[index];
                            string sEPC = temps.Substring(index * 2 + 2, EPClen * 2);
                            EPCarr.Add(sEPC);
                        }
                        foreach (string epc in EPCarr)
                        {
                            if (string.IsNullOrEmpty(epc)) continue;

                            if (tags.Exists(v => v.EPC == epc))
                            {
                                var tag = tags.Find(v => v.EPC == epc);
                                tag.Frequency++;
                                tag.QueryTime = DateTime.Now;
                            }
                            else tags.Add(new Tag(epc));
                        }
                    }
                    else throw UHF188Exception.AbnormalJudgment(isReader);
                    await Task.Delay(SelInterval);
                }
                return tags.ToArray();
            }
            catch (Exception ex)
            {
                ErrorShow?.Invoke(ex);
                return tags.ToArray();
            }
        }

        

        public Tag[] QueryTags(int ant = 0)
        {
            List<Tag> tags = new List<Tag>();
            byte Qvalue = 4;//可变
            byte TIDFlag = 0;//可变
            byte Session = 0;
            byte[] MaskAdr = new byte[2];
            byte[] MaskData = new byte[100];
            byte[] EPC = new byte[50000];
            int Totallen = 0;
            byte AdrTID = 0;
            byte LenTID = 0;
            int CardNum = 0;
            int isReader = StaticClassReaderB.Inventory_G2(ref comAdr, Qvalue, Session, AdrTID, LenTID, TIDFlag, EPC, ref Totallen, ref CardNum, handle);
            if ((isReader == 1) | (isReader == 2) | (isReader == 3) | (isReader == 4) | (isReader == 0xFB))
            {
                if (CardNum == 0)
                    return null;
                byte[] buffer = new byte[Totallen];
                Array.Copy(EPC, buffer, Totallen);
                List<string> EPCarr = new List<string>();
                string temps = ByteArrayToHexString(buffer);
                int index = 0;
                for (int i = 0; i < CardNum; i++)
                {
                    byte EPClen = buffer[index];
                    string sEPC = temps.Substring(index * 2 + 2, EPClen * 2);
                    EPCarr.Add(sEPC);
                }
                foreach (string epc in EPCarr)
                {
                    if (string.IsNullOrEmpty(epc)) continue;

                    if (tags.Exists(v => v.EPC == epc))
                    {
                        var tag = tags.Find(v => v.EPC == epc);
                        tag.Frequency++;
                        tag.QueryTime = DateTime.Now;
                    }
                    else tags.Add(new Tag(epc));
                }
                return tags.ToArray();
            }else throw UHF188Exception.AbnormalJudgment(isReader);
        }

        private string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
            return sb.ToString().ToUpper();
        }
    }
}
