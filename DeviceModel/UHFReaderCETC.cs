﻿using DeviceService.Model;
using RFIDReaderNetwork_SerialSDK_ForCSharp.DataStructureLayer;
using RFIDReaderNetwork_SerialSDK_ForCSharp.ExternalInterfaceLayer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DeviceService.Model.ExceptionModels;

namespace DeviceService.DeviceModel
{
    public class UHFReaderCETC : IDevice
    {
        RFIDClient client;
        private string ip;
        bool workFlag;

        /// <summary>
        /// 查询时间
        /// </summary>
        public int SelTime { get; set; }

        /// <summary>
        /// Ip地址
        /// </summary>
        public string Ip
        {
            get { return ip; }
            set { }
        }


        /// <summary>
        /// 显示异常
        /// </summary>
        public Action<Exception> ErrorShow { get; set; }

        public void Connect(ConnectModel connect)
        {
            client = new RFIDClient();
            client.m_OnDisconnect += reader_OnDisconnect;
            client.m_OnErrorcallback += reader_OnErrorcallback;
            client.m_OnInventoryReport += reader_OnInventoryReport;



            OperationResult operationResult = new OperationResult();
            switch (connect.ConnectMode)
            {
                case ConnectMode.Tcp:
                    client.Connect(connect.Ip, connect.Port);
                    break;
                case ConnectMode.SerialPort: client.ConnectSerial(connect.Com.ToString(), connect.BaudRate); break;
                default: throw new UHFCETCException("意外的连接模式");
            };
            if (operationResult != OperationResult.SUCCESS)
                throw UHFCETCException.AbnormalJudgment(operationResult);

            ip = connect.Ip;
        }

        public void Disconnect()
        {
            var linkflag = client.Disconnect();

            if (linkflag != OperationResult.SUCCESS)
                throw UHFCETCException.AbnormalJudgment(linkflag);
        }

        /// <summary>
        /// 查询标签
        /// </summary>
        /// <param name="selTime">查询时间</param>
        /// <param name="reSelTag">返回查询结果</param>
        public void QueryTag(int selTime, Action<string[]> reSelTag)
        {
            if (workFlag) return;
            workFlag = true;
            DateTime time = DateTime.Now + TimeSpan.FromSeconds(selTime);
            List<string> tags = new List<string>();

            Task.Run(() =>
            {
                try
                {
                    TagReport tagReport = new TagReport();
                    while (DateTime.Now < time)
                    {
                        var flag = client.InventoryCycle(0, ref tagReport);

                        if (flag != OperationResult.SUCCESS)
                            ErrorShow?.Invoke(UHFCETCException.AbnormalJudgment(flag));

                        string epc;
                        tagReport.m_listTags.ForEach(v =>
                        {
                            epc = v.m_strEPC;
                            if (!tags.Contains(epc)) tags.Add(epc);
                        });
                    }

                    reSelTag(tags.ToArray());
                    workFlag = false;
                }
                catch (Exception ex)
                {
                    ErrorShow(ex);
                }

            });
        }

        public string[] test()
        {
            List<string> tags = new List<string>();
            TagReport tagReport = new TagReport();
            var flag = client.InventoryCycle(0, ref tagReport);

            if (flag != OperationResult.SUCCESS)
                ErrorShow?.Invoke(UHFCETCException.AbnormalJudgment(flag));

            string epc;
            tagReport.m_listTags.ForEach(v =>
            {
                epc = v.m_strEPC;
                if (!tags.Contains(epc)) tags.Add(epc);
            });
            return tags.ToArray();
        }

        /// <summary>
        /// 查询标签
        /// </summary>
        /// <param name="reSelTag">返回查询结果</param>
        public void QueryTag(Action<string[]> reSelTag)
        {
            QueryTag(SelTime, reSelTag);
        }

        void reader_OnDisconnect(object sender, DisconnectEventArgs e)
        {
            Console.WriteLine("reader_OnDisconnect");
        }

        void reader_OnErrorcallback(object sender, ErrorReportEventArgs e)
        {
            Console.WriteLine("errreader_OnErrorcallbackor");
        }

        void reader_OnInventoryReport(object sender, InventoryReportEventArgs e)
        {
            Console.WriteLine("reader_OnInventoryReport");
        }


    }
}
