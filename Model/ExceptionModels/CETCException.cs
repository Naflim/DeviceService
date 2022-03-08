using rfidLink.Extend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Model.ExceptionModels
{
    class CETCException : Exception
    {
        public CETCException(string message) : base(message)
        {

        }
        public static CETCException AbnormalJudgment(operResult operResult)
        {
            switch (operResult)
            {
                case operResult.Failure:
                    return new CETCException("常规错误");
                case operResult.InvalidParameter:
                    return new CETCException("无效的参数");
                case operResult.AlreadyOpen:
                    return new CETCException("尝试打开一个已经打开的RFID读写模块");
                default:
                    return new CETCException($"错误码：{operResult}");
            }
        }
    }
}
