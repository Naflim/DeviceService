using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceService.SDK;
using DeviceService.Model.ExceptionModels;

namespace DeviceService
{
    public class ICReader : IReaderIC
    {
        public string GetCardID()
        {
            byte[] snr = new byte[7];
            byte mode = 0x01;

            int nRet = ICReaderSDK.UL_Request(mode, snr);

            if (nRet != 0)
            {
                if (nRet == 1)
                    return string.Empty;
                else
                    throw ICReaderException.AbnormalJudgment(nRet);
            }

            byte[] date = GetData(snr, 0, 7);

            string cardID = string.Empty;

            int len = date.Length;
            for (int i = 0; i < len; i++)
                cardID += date[i].ToString("X2");
            return string.Empty;
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
