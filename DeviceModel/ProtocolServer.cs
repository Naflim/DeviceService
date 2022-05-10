using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.DeviceModel
{
    public class ProtocolServer : IDeviceServer
    {
        /// <summary>
        /// 服务器端口
        /// </summary>
        public int ServerPort { get; set; } = 6000;

        /// <summary>
        /// 设备连接
        /// </summary>
        public Action<string> DeviceConnect { get; set; }

        /// <summary>
        /// 设备断开连接
        /// </summary>
        public Action<string> DeviceDisconnect { get; set; }

        /// <summary>
        /// 抛出日志
        /// </summary>
        public Action<string> ThrowLog { get; set; }

        /// <summary>
        /// 日志显示
        /// </summary>
        public Action<Exception> ErrowShow { get; set; }

        Dictionary<string, Socket> clientList;
        Socket server;

        /// <summary>
        /// 客户端消息
        /// </summary>
        protected Action<Socket, string, string> clientMsg;

        public void OpenService()
        {
            clientList = new Dictionary<string, Socket>();
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint point = new IPEndPoint(IPAddress.Any, ServerPort);
            server.Bind(point);
            server.Listen(10);
            Task.Factory.StartNew(() => Listen(server), TaskCreationOptions.LongRunning);
        }

        private void Listen(Socket server)
        {
            try
            {
                while (true)
                {
                    Socket client = server.Accept();

                    string ip = (client.RemoteEndPoint as IPEndPoint).Address.ToString();
                    if (clientList.ContainsKey(ip)) continue;
                    clientList.Add(ip, client);
                    DeviceConnect?.Invoke(ip);
                    Task.Factory.StartNew(() =>
                    {
                        while (!ClientMonitor(ip, client)) ;
                    }, TaskCreationOptions.LongRunning);
                    Task.Factory.StartNew(() => Receive(client, ip), TaskCreationOptions.LongRunning);
                }
            }
            catch (Exception ex)
            {
                ErrowShow?.Invoke(ex);
            }
        }

        private void Receive(Socket client, string ip)
        {
            try
            {
                while (clientList.ContainsKey(ip))
                {
                    byte[] buffer = new byte[1024 * 1024];

                    int len = client.Receive(buffer);
                    if (len == 0) continue;

                    string msg = Encoding.Default.GetString(buffer, 0, len);
                    clientMsg?.Invoke(client, ip, msg);
                }
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

        protected void SendClientMessage(string ip, string msg)
        {
            if (clientList.ContainsKey(ip))
                clientList[ip].Send(Encoding.Default.GetBytes(msg));
        }

        protected virtual bool ClientMonitor(string ip, Socket client)
        {
            bool flag = client.Poll(1000, SelectMode.SelectRead);
            if (flag)
            {
                clientList.Remove(ip);
                client.Close();
                DeviceDisconnect?.Invoke(ip);
            }
            return flag;
        }

        public void CloseService()
        {
            server.Close();
        }
    }
}
