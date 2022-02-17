using DeviceService.Model;
using System;
using System.IO;
using System.IO.Ports;

namespace DeviceService.DeviceModel
{
#pragma warning disable CA1416 // 验证平台兼容性
    public abstract class Protocol : IDevice
    {
        SerialPort serialPort;
        ConnectMode mode;
        public int ReceiveTimeOut { get; set; } = 60000;

        public void Connect(ConnectModel connect)
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

        public byte[] Communication(byte[] sendMsg)
        {
            switch (mode)
            {
                case ConnectMode.SerialPort:
                    return SerialCommunication(sendMsg);
                default:
                    throw new Exception("未定义的连接模式！");
            }
        }

        byte[] SerialCommunication(byte[] sendMsg)
        {
            if (serialPort is null)
                throw new Exception("串口对象未定义");
            serialPort.Write(sendMsg,0,sendMsg.Length);
            MemoryStream memory = new MemoryStream();
            byte[] buffer = new byte[1024];
            DateTime start = DateTime.Now;

            while (true)
            {
                System.Threading.Thread.Sleep(100);
                if(serialPort.BytesToRead > 0)
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
#pragma warning restore CA1416 // 验证平台兼容性
}
