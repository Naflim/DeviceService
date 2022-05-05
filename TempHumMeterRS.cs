using DeviceService.Model;
using DeviceService.Model.ExceptionModels;
using RSNetDevice;
using RSNetDevice.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService
{
    public class TempHumMeterRS : ITempHumMeter
    {
        RSServer sServer = null!;

        bool registerFlag;
        List<TempHumModel> tempHums = null!;

        public void Connect(ConnectModel connect)
        {
            if (string.IsNullOrEmpty(connect.Ip) || connect.Port == int.MinValue)
                throw new RSException("连接参数异常！");
            sServer = RSServer.Initiate(connect.Ip, connect.Port);
        }

        /// <summary>
        /// 启动监听
        /// </summary>
        /// <returns>是否成功</returns>
        public bool Start()
        {
            if (sServer.Start())
            {
                sServer.OnReceiveRealtimeData += RsServer_OnReceiveRealtimeData;
                tempHums = new List<TempHumModel>();
                registerFlag = true;
                return true;
            }else
                return false;
        }

        private void RsServer_OnReceiveRealtimeData(RSServer server, RealTimeData data)
        {
            var tempHum = tempHums.Find(v => v.DeviceID == data.DeviceID);

            if (tempHum != null)
            {
                
                foreach (RSNetDevice.Model.NodeData ndata in data.NodeList)//遍历节点数据。数据包括网络设备的数据以及各个节点数据。模拟量一二数据存放在节点数据中
                {
                    tempHum.Temperature = Math.Round(ndata.Tem, 2);
                    tempHum.Humidity = Math.Round(ndata.Hum, 2);
                }
            }
            else
            {
                tempHum = new TempHumModel();
                tempHum.DeviceID = data.DeviceID;
                foreach (RSNetDevice.Model.NodeData ndata in data.NodeList)//遍历节点数据。数据包括网络设备的数据以及各个节点数据。模拟量一二数据存放在节点数据中
                {
                    tempHum.Temperature = Math.Round(ndata.Tem, 2);
                    tempHum.Humidity = Math.Round(ndata.Hum, 2);
                }
                tempHums.Add(tempHum);
            }
            
        }

        public void Disconnect()
        {
            if (sServer.Stop())
            {
                sServer.OnReceiveRealtimeData -= RsServer_OnReceiveRealtimeData;
                registerFlag = false;
            }
            else
                throw new RSException("断开连接时出现异常！");
        }

        public List<TempHumModel> GetTempHumList()
        {
            if (!registerFlag)
                throw new RSException("前置条件未完成！");
            return tempHums;
        }

        public TempHumModel GetTempHum(int deviceID)
        {
            if (!registerFlag)
                throw new RSException("前置条件未完成！");
            if(tempHums.Find(v => v.DeviceID == deviceID) is not TempHumModel tempHum)
                throw new RSException("设备不存在！");
            return tempHum;
        }
    }
}
