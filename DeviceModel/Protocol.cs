using DeviceService.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.DeviceModel
{
    /// <summary>
    /// 协议
    /// </summary>
    public abstract class Protocol : IDevice
    {
        /// <summary>
        /// 抛出日志
        /// </summary>
        public Action<string> ThrowLog { get; set; }

        /// <summary>
        /// 日志显示
        /// </summary>
        public Action<Exception> ErrowShow { get; set; }

        protected SerialPort serialPort;
        protected ConnectMode mode;
        public int ReceiveTimeOut { get; set; } = 60000;

        public virtual void Connect(ConnectModel connect)
        {
            mode = connect.ConnectMode;
            switch (mode)
            {
                case ConnectMode.Tcp:
                    break;
                case ConnectMode.SerialPort:
                    string portName = $"COM{connect.Com}";
                    serialPort = new SerialPort(portName, connect.BaudRate, connect.Parity, connect.DataBits, connect.StopBits);
                    Disconnect();
                    serialPort.Open();
                    break;
                default:
                    throw new Exception("未定义的连接模式！");
            }
        }

        public void Disconnect()
        {
            if (serialPort.IsOpen)
                serialPort.Close();
        }


    }

    /// <summary>
    /// 应答式协议
    /// </summary>
    public abstract class AnsweringProtocol : Protocol
    {
        /// <summary>
        /// 通信
        /// </summary>
        /// <param name="sendMsg">发送消息</param>
        /// <returns>返回消息</returns>
        protected byte[] Communication(byte[] sendMsg)
        {
            switch (mode)
            {
                case ConnectMode.SerialPort:
                    return SerialCommunication(sendMsg);
                default:
                    throw new Exception("未定义的连接模式！");
            }
        }

        /// <summary>
        /// 串口通信
        /// </summary>
        /// <param name="sendMsg">发送消息</param>
        /// <returns>返回消息</returns>
        byte[] SerialCommunication(byte[] sendMsg)
        {
            if (serialPort is null)
                throw new NotImplementedException("未连接串口");
            serialPort.Write(sendMsg, 0, sendMsg.Length);
            MemoryStream memory = new MemoryStream();
            byte[] buffer = new byte[1024];
            DateTime start = DateTime.Now;

            while (true)
            {
                System.Threading.Thread.Sleep(100);
                if (serialPort.BytesToRead > 0)
                {
                    int count = serialPort.Read(buffer, 0, buffer.Length);
                    memory.Write(buffer, 0, count);
                    break;
                }
                else
                {
                    if ((DateTime.Now - start).TotalMilliseconds > ReceiveTimeOut)
                    {
                        memory.Dispose();
                        return null;
                    }
                }
            }

            return memory.ToArray();
        }
    }

    /// <summary>
    /// 通讯式协议
    /// </summary>
    public abstract class CommunicationProtocol : Protocol
    {
        public override void Connect(ConnectModel connect)
        {
            base.Connect(connect);
            StartCommunication();
        }

        /// <summary>
        /// 串口数据
        /// </summary>
        protected Action<SerialPort, byte[]> serialPortData;

        /// <summary>
        /// 串口发送数据
        /// </summary>
        /// <param name="data">数据</param>
        protected void SendData(byte[] data)
        {
            if (serialPort is null)
                throw new NotImplementedException("未连接串口");
            serialPort.Write(data, 0, data.Length);
        }

        /// <summary>
        /// 开始通讯
        /// </summary>
        private void StartCommunication()
        {
            switch (mode)
            {
                case ConnectMode.SerialPort:
                    SerialPortReceive();
                    break;
                default:
                    throw new Exception("未定义的连接模式！");
            }
        }

        /// <summary>
        /// 接收串口数据
        /// </summary>
        protected virtual void SerialPortReceive()
        {
            if (serialPort is null)
                throw new NotImplementedException("未连接串口");
            try
            {
                Task.Factory.StartNew(v =>
                {
                    while (true)
                    {
                        byte[] buffer = new byte[1024 * 1024];

                        int len = serialPort.Read(buffer, 0, buffer.Length);
                        if (len == 0) continue;

                        byte[] data = new byte[len];
                        Array.Copy(buffer, data, len);

                        serialPortData?.Invoke(serialPort, data);
                    }
                }, TaskCreationOptions.LongRunning);
            }
            catch (ObjectDisposedException ex)
            {
                new NaflimHelperLibrary.Log().PrintLog(ex.Message);
            }
            catch (Exception ex)
            {
                ErrowShow?.Invoke(ex);
            }
        }
    }
}
