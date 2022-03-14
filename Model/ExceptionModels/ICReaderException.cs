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
            return state switch
            {
                1 => new ICReaderException("命令操作失败 ....."),
                5 => new ICReaderException("数据包流水号不正确 ....."),
                _ => new ICReaderException($"错误码：{state}"),
            };
        }
    }
}
