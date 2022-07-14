using DeviceService.DeviceModel;
using DeviceService.Model;
using DeviceService.Model.ExceptionModels;
using RSNetDevice;
using RSNetDevice.Data;
using System;
using System.Collections.Generic;


namespace DeviceService.Product
{
    /// <summary>
    /// 仁硕温湿度
    /// </summary>
    public class TempHumMeterRS : RSNet, ITempHumMeter
    {
        readonly List<TempHumModel> _tempHums = new ();

        protected override void RsServer_OnReceiveRealtimeData(RSServer server, RealTimeData data)
        {
            var tempHum = _tempHums.Find(v => v.DeviceID == data.DeviceID);

            if (tempHum != null)
            {

                foreach (RSNetDevice.Model.NodeData ndata in data.NodeList)//遍历节点数据。数据包括网络设备的数据以及各个节点数据。模拟量一二数据存放在节点数据中
                {
                    tempHum.Temperature = ndata.Tem;
                    tempHum.Humidity = ndata.Hum;
                }
            }
            else
            {
                tempHum = new TempHumModel
                {
                    DeviceID = data.DeviceID
                };
                foreach (RSNetDevice.Model.NodeData ndata in data.NodeList)//遍历节点数据。数据包括网络设备的数据以及各个节点数据。模拟量一二数据存放在节点数据中
                {
                    tempHum.Temperature = ndata.Tem;
                    tempHum.Humidity = ndata.Hum;
                }
                _tempHums.Add(tempHum);
            }

        }

        public TempHumModel[] GetTempHums()
        {
            return _tempHums.ToArray();
        }

        public TempHumModel GetTempHum(int deviceID)
        {
            if (_tempHums.Find(v => v.DeviceID == deviceID) is not TempHumModel tempHum)
                throw new RSException("设备不存在！");

            return tempHum;
        }
    }
}
