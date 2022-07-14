/*************************************************************************************
 *
 * 文 件 名:   CentralControlRS
 * 描    述:   仁硕电子中控机
 * 
 * 版    本：  V1.0
 * 创 建 者：  Naflim 
 * 创建时间：  2022/7/7 9:31:57
 * ======================================================
 * 历史更新记录
 * 版本： V          修改时间：         修改人：
 * 修改内容：
 * ======================================================
*************************************************************************************/

using DeviceService.DeviceModel;
using DeviceService.Model;
using DeviceService.Model.ExceptionModels;
using RSNetDevice;
using RSNetDevice.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Product
{
    /// <summary>
    /// 仁硕中控机
    /// </summary>
    public class CentralControlRS : RSNet, ITempHumMeter
    {
        readonly List<TempHumModel> _tempHums = new();
        readonly Dictionary<int, bool> _smokeStateDic = new();
        readonly Dictionary<int, bool> _waterInvasionDic = new();

        /// <summary>
        /// 温湿度通道id
        /// </summary>
        public List<int> TempHumIdList { get; set; } = new();

        /// <summary>
        /// 烟感通道id
        /// </summary>
        public List<int> SmokeIdList { get; set; } = new();

        /// <summary>
        /// 水浸通道id
        /// </summary>
        public List<int> WaterInvasionList { get; set; } = new();

        /// <summary>
        /// 烟雾报警事件
        /// </summary>
        public event Action<int, bool>? OnSmokeStateChange;

        /// <summary>
        /// 水浸报警事件
        /// </summary>
        public event Action<int, bool>? OnWaterInvasionChange;

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

        protected override void RsServer_OnReceiveRealtimeData(RSServer server, RealTimeData data)
        {
            data.NodeList.ForEach(node =>
            {
                if (TempHumIdList.Contains(node.NodeID))
                {
                    if (_tempHums.Find(v => v.DeviceID == node.NodeID) is not TempHumModel tempHum) _tempHums.Add(new TempHumModel(node.NodeID, node.Tem, node.Hum));
                    else
                    {
                        tempHum.Temperature = node.Tem;
                        tempHum.Humidity = node.Hum;
                    }
                }
                else if (SmokeIdList.Contains(node.NodeID))
                {
                    bool hasSmoke= node.Tem > 1;
                    if (!_smokeStateDic.ContainsKey(node.NodeID)) _smokeStateDic.Add(node.NodeID, false);

                    if (hasSmoke && !_smokeStateDic[node.NodeID])
                    {
                        OnSmokeStateChange?.Invoke(node.NodeID, true);
                        _smokeStateDic[node.NodeID] = true;
                    }
                    else if (!hasSmoke && _smokeStateDic[node.NodeID])
                    {
                        OnSmokeStateChange?.Invoke(node.NodeID, false);
                        _smokeStateDic[node.NodeID] = false;
                    }
                }else if (WaterInvasionList.Contains(node.NodeID))
                {
                    bool hasWater = node.Tem > 1;
                    if (!_waterInvasionDic.ContainsKey(node.NodeID)) _waterInvasionDic.Add(node.NodeID, false);

                    if (hasWater && !_waterInvasionDic[node.NodeID])
                    {
                        OnWaterInvasionChange?.Invoke(node.NodeID, true);
                        _waterInvasionDic[node.NodeID] = true;
                    }
                    else if (!hasWater && _waterInvasionDic[node.NodeID])
                    {
                        OnWaterInvasionChange?.Invoke(node.NodeID, false);
                        _waterInvasionDic[node.NodeID] = false;
                    }
                }
            });
        }
    }
}
