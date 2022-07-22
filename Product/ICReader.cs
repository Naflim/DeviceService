using DeviceService.Model.ExceptionModels;
using DeviceService.SDK;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceService
{
    /// <summary>
    /// IC读卡器
    /// </summary>
    public class ICReader : IReaderIC
    {

        /// <summary>
        /// 显示异常
        /// </summary>
        public Action<Exception> ErrorShow { get; set; }
        readonly List<string> ICsCache = new List<string>();
        private bool isWork = false;

        /// <summary>
        /// 设备工作状态
        /// </summary>
        public bool IsWork
        {
            get { return isWork; }
        }

        /// <summary>
        /// 存储识别卡
        /// </summary>
        /// <param name="cardId">卡号</param>
        public void SaveCard(string cardId)
        {
            ICsCache.Add(cardId);
        }

        /// <summary>
        /// 清空识别卡
        /// </summary>
        public void ClearCards()
        {
            ICsCache.Clear();
        }

        /// <summary>
        /// 识别IC卡
        /// </summary>
        /// <param name="distinguish">识别成功事件</param>
        public void IdentificationIC(Action<string> distinguish)
        {
            isWork = true;
            Task.Run(() =>
            {
                try
                {
                    while (isWork)
                    {
                        var cardID = GetCardID();

                        if (ICsCache.Contains(cardID))
                        {
                            distinguish(cardID);
                            ExitWorking();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorShow?.Invoke(ex);
                }
            });
        }

        /// <summary>
        /// 监听IC卡
        /// </summary>
        /// <param name="distinguish"></param>
        public void MonitorIC(Action<string> distinguish)
        {
            isWork = true;
            Task.Run(() =>
            {
                try
                {
                    while (isWork)
                    {
                        var cardID = GetCardID();

                        if (!string.IsNullOrEmpty(cardID))
                        {
                            distinguish(cardID);
                            ExitWorking();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorShow?.Invoke(ex);
                }
            });
        }

        /// <summary>
        /// 退出工作状态
        /// </summary>
        public void ExitWorking()
        {
            isWork = false;
        }

        public string GetCardID()
        {
            byte[] snr = new byte[7];
            byte mode = 0x01;

            int nRet = ICReaderSDK.UL_Request(mode, snr);

            if (nRet != 0)
            {
                if (nRet == 1)
                    return null;
                else
                    throw ICReaderException.AbnormalJudgment(nRet);
            }

            byte[] date = GetData(snr, 0, 7);

            string cardID = null;

            int len = date.Length;
            for (int i = 0; i < len; i++)
                cardID += date[i].ToString("X2");
            return cardID;
        }

        protected byte[] GetData(byte[] data, int start, int end)
        {
            for (int i = 0; i < end; i++)
                if (data[start + i] < 0)
                    data[start + i] = Convert.ToByte(Convert.ToInt32(data[start + i]) + 256);

            return data;
        }
    }
}
