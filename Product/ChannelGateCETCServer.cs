using DeviceService.DeviceModel;
using DeviceService.Enum;
using DeviceService.Model;
using NaflimHelperLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Xml;
using System.Xml.Linq;

namespace DeviceService
{
    /// <summary>
    /// 中电海康通道门sdk服务端模式
    /// </summary>
    public class ChannelGateCETCServer : ProtocolServer, IChannelGate
    {
        Action<ChannelGateCETCServer, ChannelGateModel> adoptTrigger;
        public void StartChannelGateServer(Action<IChannelGate, ChannelGateModel> adoptTrigger)
        {
            this.adoptTrigger = adoptTrigger;
            OpenService();
            clientMsg = ClientMessage;
        }

        void ClientMessage(Socket client, string ip, string msg)
        {
            RFIDMessage message = XMLHelper.DESerializer<RFIDMessage>(msg);
            if (message.CommandType == "REQUEST")
            {
                switch (message.Command)
                {
                    case "REGISTER":
                        ResultRFIDMessage sendREGISTER = new ResultRFIDMessage("REGISTER")
                        {
                            Params = new 
                            {
                                KeepAliveSeconds = 60
                            }
                        };
                        SendClientMessage(ip, sendREGISTER.ToXml("RFIDMessage"));
                        break;
                    case "KEEPALIVE":
                        ResultRFIDMessage sendKEEPALIVE = new ResultRFIDMessage("KEEPALIVE");
                        SendClientMessage(ip, sendKEEPALIVE.ToXml("RFIDMessage"));
                        break;
                    case "INVENTORYREPORT":
                        InventoryReport inventoryReport = XMLHelper.DESerializer<InventoryReport>(message.Params as XmlNode[]);
                        if (inventoryReport.OpResultItem is null || inventoryReport.OpResultItem.Length == 0)
                            return;
                        int inOutType = inventoryReport.OpResultItem[0].InOutType;
                        Direction direction;
                        switch (inOutType)
                        {
                            case 0:
                                direction = Direction.Out;
                                break;
                            case 1:
                                direction = Direction.In;
                                break;
                            default:
                                direction = Direction.Null;
                                break;
                        }
                        List<Tag> epcs = inventoryReport.OpResultItem.Select(v => new Tag(v.EPC)).ToList();
                        adoptTrigger?.Invoke(this, new ChannelGateModel(direction, epcs, ip));
                        break;
                    default:
                        ThrowLog?.Invoke(message.Command);
                        new Log("accidentXML").PrintLog(msg);
                        break;
                }
            }

            if (message.CommandType == "RESPONSE")
            {
                switch (message.WhichCommand)
                {
                    case "GPOCONTROL":
                        if (message.Status != 200)
                            ThrowLog?.Invoke($"GPO异常：{message.Description}");
                        else
                            ThrowLog?.Invoke($"GPO响应！");
                        break;
                    default:
                        ThrowLog?.Invoke($"意外结果：{message.CommandType}");
                        break;
                }
            }
        }

        /// <summary>
        /// 打开警报灯
        /// </summary>
        /// <param name="lightColor">警报灯颜色</param>
        /// <param name="alarmTime">警报时长</param>
        public void OpenAlarmLight(AlarmLightColor lightColor, string ip)
        {
            Model.GpoControl.RFIDMessage message;
            switch (lightColor)
            {
                case AlarmLightColor.Red:
                    message = new Model.GpoControl.RFIDMessage(new Model.GpoControl.RFIDMessageGpo[]
                    {
                        new Model.GpoControl.RFIDMessageGpo(0,1),
                        new Model.GpoControl.RFIDMessageGpo(1,0),
                        new Model.GpoControl.RFIDMessageGpo(2,0),
                        new Model.GpoControl.RFIDMessageGpo(3,0),
                    });
                    break;
                case AlarmLightColor.Green:
                    message = new Model.GpoControl.RFIDMessage(new Model.GpoControl.RFIDMessageGpo[]
                    {
                        new Model.GpoControl.RFIDMessageGpo(0,0),
                        new Model.GpoControl.RFIDMessageGpo(1,1),
                        new Model.GpoControl.RFIDMessageGpo(2,0),
                        new Model.GpoControl.RFIDMessageGpo(3,0),
                    });
                    break;
                case AlarmLightColor.Black:
                    message = new Model.GpoControl.RFIDMessage(new Model.GpoControl.RFIDMessageGpo[]
                   {
                        new Model.GpoControl.RFIDMessageGpo(0,0),
                        new Model.GpoControl.RFIDMessageGpo(1,0),
                        new Model.GpoControl.RFIDMessageGpo(2,0),
                        new Model.GpoControl.RFIDMessageGpo(3,0),
                   });
                    break;
                default:
                    return;
            }

            SendClientMessage(ip, XMLHelper.XmlSerialize(message));
        }

        public void SendClientMessage(string ip, XElement element)
        {
            XDocument xmlDoc = new XDocument(new XDeclaration("1.0", "utf-8", null), element);
            SendClientMessage(ip, xmlDoc);
        }

        public void SendClientMessage(string ip, XDocument xml)
        {
            string output = xml.Declaration.ToString() + xml.ToString();
            SendClientMessage(ip, output);
           
        }
    }
}
