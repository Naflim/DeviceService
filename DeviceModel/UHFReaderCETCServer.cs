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
    public class UHFReaderCETCServer : IDeviceServer
    {
        protected RFIDServer? server;

        public void CloseService()
        {
            throw new NotImplementedException();
        }

        public void OpenService()
        {
            server = new RFIDServer();
            server.m_OnErrorOccured += OnErrorOccured;
            server.m_OnRegistered += OnRegistered;
            server.m_OnUnregistered += OnUnregistered;
            server.m_OnInventoryReport += reader_OnInventoryReport;
            server.StartServer();

        }

        private void OnUnregistered(object? sender, UnregisteredEventArgs e)
        {
            Console.WriteLine($"Unregistered");
        }

        private void OnRegistered(object? sender, RegisteredEventArgs e)
        {
            Console.WriteLine($"{e.m_strReaderIp}已连接");
        }

        private void OnErrorOccured(object? sender, ErrorReportEventArgs e)
        {
            Console.WriteLine("ErrorOccured");
        }

        void reader_OnInventoryReport(object? sender, InventoryReportEventArgs e)
        {
            Console.WriteLine("reader_OnInventoryReport");
        }
    }
}
