/*************************************************************************************
 *
 * 文 件 名:   RSNetDevice
 * 描    述:   仁硕电子网络设备
 * 
 * 版    本：  V1.0
 * 创 建 者：  Naflim 
 * 创建时间：  2022/7/7 8:53:05
 * ======================================================
 * 历史更新记录
 * 版本： V          修改时间：         修改人：
 * 修改内容：
 * ======================================================
*************************************************************************************/

using DeviceService.Model;
using DeviceService.Model.ExceptionModels;
using RSNetDevice;
using RSNetDevice.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.DeviceModel
{
    /// <summary>
    /// 仁硕电子网络设备
    /// </summary>
    public abstract class RSNet : IDevice
    {
        RSServer _server = null!;

        /// <summary>
        /// 抛出日志
        /// </summary>
        public Action<string>? ThrowLog { get; set; }

        /// <summary>
        /// 显示异常
        /// </summary>
        public Action<Exception>? ErrorShow { get; set; }

        public void Connect(ConnectModel connect)
        {
            if (string.IsNullOrEmpty(connect.Ip) || connect.Port == int.MinValue)
                throw new RSException("连接参数异常！");
            _server = RSServer.Initiate(connect.Ip, connect.Port);
        }

        public void Disconnect()
        {
            if (_server.Stop()) _server.OnReceiveRealtimeData -= RsServer_OnReceiveRealtimeData;
            else throw new RSException("断开连接时出现异常！");
        }

        /// <summary>
        /// 启动监听
        /// </summary>
        /// <returns>是否成功</returns>
        public bool Start()
        {
            if (_server.Start())
            {
                _server.OnReceiveRealtimeData += RsServer_OnReceiveRealtimeData;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 接收设备实时数据
        /// </summary>
        /// <param name="server"></param>
        /// <param name="data"></param>
        protected abstract void RsServer_OnReceiveRealtimeData(RSServer server, RealTimeData data);
    }
}
