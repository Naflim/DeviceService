using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceService;
using DeviceService.DeviceModel;
using DeviceService.Model;

namespace DeviceService
{
    public class ChannelGateUHFGate : UHFGate, IChannelGate
    {
        bool monitorFlag;
        Dictionary<DateTime, string> cacheEPC = new Dictionary<DateTime, string>();
        Action<ChannelGateUHFGate, ChannelGateModel>? adoptTrigger;

        /// <summary>
        /// 超时时间
        /// </summary>
        public int Timeout { get; set; } = 10000;

        /// <summary>
        /// 显示GPIO
        /// </summary>
        public Action<int, bool>? ShowGPIO { get; set; }

        /// <summary>
        /// 开始通道服务
        /// </summary>
        /// <param name="adoptTrigger">方向触发事件</param>
        public void StartChannelGateServer(Action<IChannelGate, ChannelGateModel> adoptTrigger)
        {
            monitorFlag = true;
            this.adoptTrigger = adoptTrigger;
            Task.Factory.StartNew(() =>
            {
                while (monitorFlag)
                {
                    if (GetChannelMessage() is not UHFGateMsg gateMsg || gateMsg is null) continue;
                    switch (gateMsg.Type)
                    {
                        case MsgType.TakeInventory:
                            ThrowLog?.Invoke("盘存消息");
                            TakeInventory(gateMsg.MsgData);
                            break;
                        case MsgType.EAS:
                            ThrowLog?.Invoke("EAS报警");
                            break;
                        case MsgType.Personnel:
                            ThrowLog?.Invoke("人员消息");
                            Personnel(gateMsg.MsgData);
                            break;
                        case MsgType.Infrared:
                            ThrowLog?.Invoke("红外消息");
                            Infrared(gateMsg.MsgData);
                            break;
                        default:
                            ThrowLog?.Invoke(gateMsg.Type.ToString());
                            break;
                    }
                    Acknowledge();
                }
            }, TaskCreationOptions.LongRunning);
        }

        void TakeInventory(byte[] data)
        {
            int Number = data[6]; //张数
            if (Number == 1)
            {
                int EPCLen = data[7];//EPC号长度
                byte[] EPC = new byte[EPCLen];
                Array.Copy(data, 8, EPC, 0, EPCLen);//标签的EPC号

                string epc = string.Empty;

                foreach (byte b in EPC)
                    epc += b.ToString("X2");

                if (!cacheEPC.ContainsValue(epc))
                    cacheEPC.Add(DateTime.Now, epc);
            }
        }

        void Personnel(byte[] data)
        {
            int dir_Type = data[6];//方向

            Direction direction;
            List<string> epc = new List<string>();
            switch (dir_Type)
            {
                case 0:
                    direction = Direction.In;
                    break;
                case 1:
                    direction = Direction.Out;
                    break;
                default:
                    direction = Direction.Null;
                    break;
            }

            foreach (var item in cacheEPC)
                if ((DateTime.Now - item.Key).TotalMilliseconds < Timeout)
                    epc.Add(item.Value);

            AdoptTrigger(new ChannelGateModel(direction, epc));

        }

        void Infrared(byte[] data)
        {
            int id = data[6];
            bool flag = Convert.ToBoolean(data[7]);
            ShowGPIO?.Invoke(id, flag);
        }

        public void AdoptTrigger(ChannelGateModel channelGate)
        {
            adoptTrigger?.Invoke(this, channelGate);
            cacheEPC.Clear();
        }
    }
}
