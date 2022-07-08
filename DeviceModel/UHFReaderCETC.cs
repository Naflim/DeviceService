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
    public class UHFReaderCETC : IReader
    {
        protected RFIDClient client;
        private string ip;
        bool workFlag;

        /// <summary>
        /// 查询时间
        /// </summary>
        public int SelTime { get; set; }

        /// <summary>
        /// 查询间隙
        /// </summary>
        public int SelInterval { get; set; } = 0;

        /// <summary>
        /// Ip地址
        /// </summary>
        public string Ip
        {
            get { return ip; }
        }


        /// <summary>
        /// 抛出日志
        /// </summary>
        public Action<string> ThrowLog { get; set; }

        /// <summary>
        /// 显示异常
        /// </summary>
        public Action<Exception> ErrorShow { get; set; }

        public void Connect(ConnectModel connect)
        {
            client = new RFIDClient();
            client.m_OnDisconnect += Reader_OnDisconnect;
            client.m_OnErrorcallback += Reader_OnErrorcallback;
            client.m_OnInventoryReport += Reader_OnInventoryReport;

            OperationResult operationResult;
            switch (connect.ConnectMode)
            {
                case ConnectMode.Tcp:
                    operationResult = client.Connect(connect.Ip, connect.Port);
                    break;
                case ConnectMode.SerialPort:
                    operationResult = client.ConnectSerial(connect.Com.ToString(), connect.BaudRate);
                    break;
                default:
                    throw new UHFCETCException("意外的连接模式");
            }

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

        /// <summary>
        /// 查询标签
        /// </summary>
        /// <param name="reSelTag">返回查询结果</param>
        public void QueryTag(Action<string[]> reSelTag)
        {
            QueryTag(SelTime, reSelTag);
        }



        void Reader_OnDisconnect(object sender, DisconnectEventArgs e)
        {
            Console.WriteLine("reader_OnDisconnect");
        }

        void Reader_OnErrorcallback(object sender, ErrorReportEventArgs e)
        {
            Console.WriteLine("errreader_OnErrorcallbackor");
        }

        void Reader_OnInventoryReport(object sender, InventoryReportEventArgs e)
        {
            //Console.WriteLine(e.m_stInventoryResult.);
        }

        public Model.Tag[] QueryTags(int ant = 0)
        {
            List<Model.Tag> tags = new List<Model.Tag>();
            TagReport tagReport = new TagReport();

            var flag = client.InventoryCycle(ant, ref tagReport);

            if (flag != OperationResult.SUCCESS)
                ErrorShow?.Invoke(UHFCETCException.AbnormalJudgment(flag));

            foreach (var tag in tagReport.m_listTags)
            {
                if (string.IsNullOrEmpty(tag.m_strEPC)) continue;

                if (tags.Exists(v => v.EPC == tag.m_strEPC))
                {
                    var nowTag = tags.Find(v => v.EPC == tag.m_strEPC);
                    nowTag.Frequency++;
                    nowTag.QueryTime = DateTime.Now;
                }
                else tags.Add(new Model.Tag(tag.m_strEPC));
            }

            return tags.ToArray();
        }

        public async Task<Model.Tag[]> CyclicQueryTags(int seconds)
        {
            List<Model.Tag> tags = new List<Model.Tag>();
            try
            {
                DateTime startTime = DateTime.Now;

                while (DateTime.Now < startTime.AddSeconds(seconds))
                {
                    TagReport tagReport = new TagReport();

                    var flag = client.InventoryCycle(0, ref tagReport);
                    if (flag != OperationResult.SUCCESS)
                        ErrorShow?.Invoke(UHFCETCException.AbnormalJudgment(flag));

                    foreach (var tag in tagReport.m_listTags)
                    {
                        if (string.IsNullOrEmpty(tag.m_strEPC)) continue;

                        if (tags.Exists(v => v.EPC == tag.m_strEPC))
                        {
                            var nowTag = tags.Find(v => v.EPC == tag.m_strEPC);
                            nowTag.Frequency++;
                            nowTag.QueryTime = DateTime.Now;
                        }
                        else tags.Add(new Model.Tag(tag.m_strEPC));
                    }

                    await Task.Delay(SelInterval);
                }
                return tags.ToArray();
            }
            catch (Exception ex)
            {
                ErrorShow?.Invoke(ex);
                return tags.ToArray();
            }
        }
    }
}
