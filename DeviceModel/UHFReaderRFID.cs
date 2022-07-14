/*************************************************************************************
 *
 * 文 件 名:   UHFReaderRFID
 * 描    述:   UHFRFID读写器设备封装类库
 * 
 * 版    本：  V1.0
 * 创 建 者：  Naflim 
 * 创建时间：  2022/5/30 17:43:34
 * ======================================================
 * 历史更新记录
 * 版本： V          修改时间：         修改人：
 * 修改内容：
 * ======================================================
*************************************************************************************/

using DeviceService.Model;
using DeviceService.Model.ExceptionModels;
using DeviceService.SDK.UHFRFID;
using NaflimHelperLibrary;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceService.DeviceModel
{
    /// <summary>
    /// UHFRFID读写器设备封装类库
    /// </summary>
    public class UHFReaderRFID : IReadOrWrite
    {
        int com;
        protected ConnectMode mode;
        protected string ip;

        /// <summary>
        /// 设备ip
        /// </summary>
        public string IP
        {
            get { return ip; }
        }

        /// <summary>
        /// 查询间隙
        /// </summary>
        public int SelInterval { get; set; } = 0;

        /// <summary>
        /// 显示异常
        /// </summary>
        public Action<Exception> ErrorShow { get; set; }

        /// <summary>
        /// 抛出日志
        /// </summary>
        public Action<string> ThrowLog { get; set; }

        public void Connect()
        {
            throw new UHFRFIDException("此设备不支持自动连接");
        }

        public void Connect(ConnectModel connect)
        {
            mode = connect.ConnectMode;
            int tryLink;
            switch (connect.ConnectMode)
            {
                case ConnectMode.Tcp:
                    throw new UHFRFIDException("暂不支持网口连接");
                case ConnectMode.SerialPort:
                    if (connect.Com == 0)
                        throw new UHFRFIDException("此设备不支持自动连接");
                    else
                        tryLink = UHFRFIDSDK.UHF_RFID_Open((byte)connect.Com, connect.BaudRate);
                    break;
                default:
                    throw new UHFRFIDException("意外的连接模式");
            }

            if (tryLink != 0)
                throw UHFRFIDException.AbnormalJudgment(tryLink);

            com = connect.Com;
            ip = connect.Ip;
        }

        public void Disconnect()
        {
            int tryBreak;
            switch (mode)
            {
                case ConnectMode.Tcp:
                    throw new UHFRFIDException("暂不支持网口连接");
                case ConnectMode.SerialPort:
                    tryBreak = UHFRFIDSDK.UHF_RFID_Close((byte)com);
                    break;
                default:
                    throw new UHFRFIDException("意外的连接模式");
            }

            if (tryBreak != 0)
                throw UHFRFIDException.AbnormalJudgment(tryBreak);

            ip = string.Empty;
        }

        public void SetPower(byte power, int ant = 0)
        {
            int InLen = 6;
            uint OutLen = 0;
            byte[] txpower = new byte[2];
            byte[] InputData = new byte[100];
            byte[] OutputData = new byte[100];

            txpower[0] = (byte)((power * 100) >> 8);
            txpower[1] = (byte)((power * 100) & 0xff);

            InputData[0] = 0x00; InputData[1] = 0x00;
            InputData[2] = txpower[0]; InputData[3] = txpower[1];
            InputData[4] = txpower[0]; InputData[5] = txpower[1];

            InputData[0] &= 0xFD;

            int trySet = UHFRFIDSDK.UHF_RFID_Set(Cmd.RFID_CMD_SET_TXPOWER, InputData, InLen, OutputData, ref OutLen);

            if (trySet != 0)
                throw UHFRFIDException.AbnormalJudgment(trySet);
        }

        public Tag[] QueryTags(int ant = 0)
        {
            byte[] pInBuf = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 10 };
            byte[] outputData = new byte[500];
            uint outlen = 0;
            int tryQuery = UHFRFIDSDK.UHF_RFID_Set(Cmd.RFID_CMD_READ_DATA, pInBuf, 14, outputData, ref outlen);

            if (tryQuery != 0) throw UHFRFIDException.AbnormalJudgment(tryQuery);

            if (outputData[0] != 0x01)
            {
                switch (outputData[1])
                {
                    case 0x01:
                        ThrowLog?.Invoke("无标签");
                        break;
                    case 0x02:
                        ThrowLog?.Invoke("访问密码错误");
                        break;
                    case 0x03:
                        ThrowLog?.Invoke("读操作失败");
                        break;
                }
                return new Tag[] { };
            }

            int len = (outputData[4] >> 3) * 2;

            byte[] epcbuf = new byte[len];

            Array.Copy(outputData, 6, epcbuf, 0, len);

            return new Tag[] { new Tag(DataConversion.ByteToHexStr(epcbuf)) };
        }

        public async Task<Tag[]> CyclicQueryTags(int seconds)
        {
            List<Tag> tags = new List<Tag>();
            try
            {
                DateTime startTime = DateTime.Now;

                while (DateTime.Now < startTime.AddSeconds(seconds))
                {
                    byte[] pInBuf = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 10 };
                    byte[] outputData = new byte[500];
                    uint outlen = 0;
                    int tryQuery = UHFRFIDSDK.UHF_RFID_Set(Cmd.RFID_CMD_READ_DATA, pInBuf, 14, outputData, ref outlen);

                    if (tryQuery != 0) throw UHFRFIDException.AbnormalJudgment(tryQuery);

                    if (outputData[0] != 0x01)
                    {
                        switch (outputData[1])
                        {
                            case 0x01:
                                ThrowLog?.Invoke("无标签");
                                break;
                            case 0x02:
                                ThrowLog?.Invoke("访问密码错误");
                                break;
                            case 0x03:
                                ThrowLog?.Invoke("读操作失败");
                                break;
                        }
                    }

                    int len = (outputData[4] >> 3) * 2;

                    byte[] epcbuf = new byte[len];

                    Array.Copy(outputData, 6, epcbuf, 0, len);

                    tags.Add(new Tag(DataConversion.ByteToHexStr(epcbuf)));
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

        public void WriteTag(string epc)
        {
            byte[] outputData = new byte[500];
            uint outLen = 0;

            if (epc.Length > 24)
                throw new UHFRFIDException("不支持24位以上epc");

            if (epc.Length < 24)
                epc = epc.PadLeft(24, '0');

            List<byte> pInBuf = new List<byte> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 7, 48, 0 };

            pInBuf.AddRange(DataConversion.StrToHexByteArr(epc));

            int tryWrite = UHFRFIDSDK.UHF_RFID_Set(Cmd.RFID_CMD_WRITE_DATA, pInBuf.ToArray(), 28, outputData, ref outLen);

            if (tryWrite != 0) throw UHFRFIDException.AbnormalJudgment(tryWrite);

            if (outputData[0] != 0x01)
            {
                switch (outputData[1])
                {
                    case 0x01:
                        ThrowLog?.Invoke("无标签");
                        break;
                    case 0x02:
                        ThrowLog?.Invoke("访问密码错误");
                        break;
                    case 0x03:
                        ThrowLog?.Invoke("写操作失败");
                        break;
                }
            }
        }
    }
}
