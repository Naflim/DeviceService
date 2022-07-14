using DeviceService.Model;
using RFIDReaderNetwork_SerialSDK_ForCSharp.DataStructureLayer;
using RFIDReaderNetwork_SerialSDK_ForCSharp.ExternalInterfaceLayer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DeviceService.Model.ExceptionModels;

namespace DeviceService.DeviceModel
{
    /// <summary>
    /// 中电海康读写器服务端模式
    /// </summary>
    public class UHFReaderCETCServer : IDeviceServer
    {
        protected RFIDServer server;
        protected List<RFIDServerModel> clientList;

        /// <summary>
        /// 日志抛出
        /// </summary>
        public Action<string> ThrowLog { get; set; }

        /// <summary>
        /// 设备连接
        /// </summary>
        public Action<string> DeviceConnect { get; set; }

        /// <summary>
        /// 设备断开连接
        /// </summary>
        public Action<string> DeviceDisconnect { get; set; }


        public UHFReaderCETCServer()
        {
            Init();
        }

        void Init()
        {
            server = new RFIDServer();
            clientList = new List<RFIDServerModel>();
            server.m_OnErrorOccured += OnErrorOccured;
            server.m_OnRegistered += OnRegistered;
            server.m_OnUnregistered += OnUnregistered;
            server.m_OnInventoryReport += OnInventoryReport;
        }

        public void OpenService()
        {
            server.StartServer();
        }
        public void CloseService()
        {
            server.StopServer();
        }

        /// <summary>
        /// 开始盘点
        /// </summary>
        /// <param name="clientIp">设备ip</param>
        public void StartInventory(string clientIp)
        {
            var clent = clientList.Find(v => v.m_strReaderIP == clientIp);
            if (clientList is null) throw new UHFCETCException("设备未连接");
            //clent.m_OnInventoryReport += OnInventoryReport;

            var operationResult = clent.StartPerioInventory();
            if (operationResult != OperationResult.SUCCESS)
                throw UHFCETCException.AbnormalJudgment(operationResult);
        }

        private void OnUnregistered(object sender, UnregisteredEventArgs e)
        {
            var clent = clientList.Find(v => v.m_strReaderIP == e.m_strDeviceID);
            if (clientList != null) clientList.Remove(clent);
            DeviceDisconnect?.Invoke(e.m_strReaderIp);
        }

        private void OnRegistered(object sender, RegisteredEventArgs e)
        {
            ThrowLog?.Invoke($"{e.m_strReaderIp}已连接");
            clientList.Add(new RFIDServerModel(e.m_strDeviceID));
            DeviceConnect?.Invoke(e.m_strReaderIp);
        }

        private void OnErrorOccured(object sender, ErrorReportEventArgs e)
        {
            ThrowLog?.Invoke("读写器发生错误事件");
        }

        void OnInventoryReport(object sender, InventoryReportEventArgs e)
        {
            ThrowLog?.Invoke("reader_OnInventoryReport");
        }
    }
}
