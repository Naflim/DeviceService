using DeviceService.Model;
using DeviceService.Model.ExceptionModels;
using GDotnet.Reader.Api.DAL;
using GDotnet.Reader.Api.Protocol.Gx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.DeviceModel
{
    public class GReader : IDevice
    {
        /// <summary>
        /// 抛出日志
        /// </summary>
        public Action<string>? ThrowLog { get; set; }

        /// <summary>
        /// 显示异常
        /// </summary>
        public Action<Exception>? ErrorShow { get; set; }

        /// <summary>
        /// Ip地址
        /// </summary>
        public string Ip
        {
            get { return ip; }
        }

        private string ip = string.Empty;
        protected GClient client = null!;
        public void Connect(ConnectModel connect)
        {
            client = new();
            var operationResult = connect.ConnectMode switch
            {
                ConnectMode.Tcp => client.OpenTcp(connect.Ip, connect.Port,out _),
                _ => throw new GReaderException("意外的连接模式"),
            };

            if (!operationResult)
                throw new GReaderException($"{connect.Ip}连接失败");

            client.OnEncapedTagEpcLog = OnEncapedTagEpcLog;
            client.OnGpiStart = OnGpiStart;

            ip = connect.Ip;
        }

        /// <summary>
        /// 设置功率
        /// </summary>
        /// <param name="power"></param>
        public void SetPower(Dictionary<byte,byte> power)
        {
            MsgBaseSetPower msgBaseSetPower = new()
            {
                DicPower = power
            };
            client.SendSynMsg(msgBaseSetPower);

            if (msgBaseSetPower.RtCode != 0) throw new GReaderException(msgBaseSetPower.RtMsg);
        }

        protected virtual void OnGpiStart(LogBaseGpiStart msg)
        {
            Console.WriteLine($"GOIO:{msg.GpiPort}");
        }

        private void OnEncapedTagEpcLog(EncapedLogBaseEpcInfo msg)
        {
            Console.WriteLine($"EPC:{msg.logBaseEpcInfo.Epc}");
        }

        public void Disconnect()
        {
            client.Close();
        }
    }
}
