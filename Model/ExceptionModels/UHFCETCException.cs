using RFIDReaderNetwork_SerialSDK_ForCSharp.DataStructureLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Model.ExceptionModels
{
    public class UHFCETCException : Exception
    {
        public UHFCETCException(string message) : base(message)
        {

        }

        /// <summary>
        /// 返回UHFGate设备异常信息
        /// </summary>
        /// <param name="state">异常状态</param>
        /// <exception cref="UHFCETCException">异常</exception>
        public static UHFCETCException AbnormalJudgment(OperationResult state)
        {
            return state switch
            {
                OperationResult.READER_NOT_ONLINE => new UHFCETCException("设备连接失败"),
                _ => new UHFCETCException(state.ToString()),
            };
        }
    }
}
