using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Model
{
    public enum ConnectMode
    {
        Tcp,
        SerialPort
    }
    public class ConnectModel
    {
        public ConnectMode ConnectMode { get; set; }
        public string Ip { get; set; } = string.Empty;
        public int Port { get; set; } = int.MinValue;
        public int Com { get; set; } = int.MinValue;
        public int BaudRate { get; set; } = int.MinValue;
        public int DataBits { get; set; } = int.MinValue;
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }
        public string UserID { get; set; } = string.Empty;
        public string Pwd { get; set; } = string.Empty;

        public ConnectModel() { }

        /// <summary>
        /// 网口连接
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口号</param>
        public ConnectModel(string ip, int port)
        {
            ConnectMode = ConnectMode.Tcp;
            Ip = ip;
            Port = port;
        }

        /// <summary>
        /// 串口连接
        /// </summary>
        /// <param name="com">串口号</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="parity">校验位</param>
        /// <param name="stopBits">停止位</param>
        public ConnectModel(int com, int baudRate, int dataBits, Parity parity, StopBits stopBits)
        {
            ConnectMode = ConnectMode.SerialPort;
            Com = com;
            BaudRate = baudRate;
            DataBits = dataBits;
            Parity = parity;
            StopBits = stopBits;
        }
    }
}
