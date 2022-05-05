using DeviceService.Model;
using DeviceService.Model.ExceptionModels;
using DeviceService.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DeviceService.DeviceModel
{
    public abstract class UHFGate : IDevice
    {
        ConnectMode mode;
        protected int handle = -1;
        protected byte comAdr = 255;
        protected int port;
        protected string? ip;

        /// <summary>
        /// 显示异常
        /// </summary>
        public Action<Exception>? ErrorShow { get; set; }

        /// <summary>
        /// 抛出日志
        /// </summary>
        public Action<string>? ThrowLog { get; set; }

        /// <summary>
        /// 设备ip
        /// </summary>
        public string? IP
        {
            get { return ip; }
        }

        public void Connect(ConnectModel connect)
        {
            mode = connect.ConnectMode;
            switch (mode)
            {
                case ConnectMode.Tcp:
                    int tcpFlag = UHFGateSDK.OpenNetPort(connect.Port, connect.Ip, ref comAdr, ref handle);
                    if (tcpFlag != 0) throw UHFGateException.AbnormalJudgment(tcpFlag);
                    ip = connect.Ip;
                    break;
                default:
                    throw new Exception("未定义的连接模式！");
            }
        }

        public void Disconnect()
        {
            switch (mode)
            {
                case ConnectMode.Tcp:
                    int tcpFlag = UHFGateSDK.CloseNetPort(handle);
                    if (tcpFlag != 0) throw UHFGateException.AbnormalJudgment(tcpFlag);
                    break;
                case ConnectMode.SerialPort:
                    int comFlag = UHFGateSDK.CloseComPort();
                    if (comFlag != 0) throw UHFGateException.AbnormalJudgment(comFlag);
                    break;
                default:
                    throw new Exception("未定义的连接模式！");
            }
        }

        /// <summary>
        /// 设置功率
        /// </summary>
        /// <param name="power"></param>
        public void SetPower(byte[] power)
        {
            int powerflag = UHFGateSDK.ConfigureController(comAdr, 35, 8, power, handle);
            if (powerflag != 0) throw UHFGateException.AbnormalJudgment(powerflag);
        }

        /// <summary>
        /// 获取功率
        /// </summary>
        /// <returns>功率</returns>
        public byte[] GetPower()
        {
            byte CFGLen = 255;
            byte[] CFGData = new byte[CFGLen];
            int powerflag = UHFGateSDK.GetControllerConfig(comAdr, 35, ref CFGLen, CFGData, handle);
            if (powerflag != 0) throw UHFGateException.AbnormalJudgment(powerflag);
            byte[] power = new byte[8];
            Array.Copy(CFGData, 0, power, 0, 8);
            return power;
        }

        /// <summary>
        /// 获取设备消息
        /// </summary>
        /// <returns>设备消息</returns>
        public UHFGateMsg? GetChannelMessage()
        {
            UHFGateMsg gateMsg = new UHFGateMsg();
            byte packetNo = 0;
            byte msgLength = 0;
            byte msgType = 0;
            byte[] msg = new byte[256];
            int msgFlag = UHFGateSDK.GetChannelMessage(comAdr, ref packetNo, msg, ref msgLength, ref msgType, handle);
            if (msgFlag != 0) throw UHFGateException.AbnormalJudgment(msgFlag);
            if (msgLength == 0) return null;
            gateMsg.Type = (MsgType)msgType;
            gateMsg.MsgData = new byte[msgLength];
            Array.Copy(msg, 0, gateMsg.MsgData, 0, msgLength);
            return gateMsg;
        }

        /// <summary>
        /// 响应设备消息
        /// </summary>
        public void Acknowledge()
        {
            UHFGateSDK.Acknowledge(comAdr, handle);
        }

        /// <summary>
        /// 报警灯报警
        /// </summary>
        public void AlarmLightAlarm()
        {
            UHFGateSDK.SetBuzzer(comAdr, 50, 50, 3, 5, handle);
            UHFGateSDK.SetLED(comAdr, 50, 50, 3, 0x01, handle);
        }
    }

    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MsgType
    {
        NullMsg, TakeInventory, EAS, Personnel, Infrared
    };

    /// <summary>
    /// UHF命令对象
    /// </summary>
    public class UHFGateMsg
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public MsgType Type { get; set; }

        /// <summary>
        /// 消息数据
        /// </summary>
        public byte[] MsgData { get; set; } = Array.Empty<byte>();
    }
}
