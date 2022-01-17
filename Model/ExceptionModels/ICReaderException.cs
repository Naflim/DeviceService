using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Model.ExceptionModels
{
    public class ICReaderException : Exception
    {
        public ICReaderException(string message) : base(message)
        {

        }

        public static ICReaderException AbnormalJudgment(int state)
        {
            switch (state)
            {
                case 1:
                    return new ICReaderException("命令操作失败 .....");
                default:
                    return new ICReaderException($"错误码：{state}");
            }
        }
    }
}
