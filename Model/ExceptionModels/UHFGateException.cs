using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Model.ExceptionModels
{
    public class UHFGateException:Exception
    {
        public UHFGateException(string message) : base(message)
        {

        }

        /// <summary>
        /// 返回UHFGate设备异常信息
        /// </summary>
        /// <param name="state">异常状态</param>
        /// <returns></returns>
        public static UHFGateException AbnormalJudgment(int state)
        {
            return state switch
            {
                48 => new UHFGateException("通讯错误。"),
                55 => new UHFGateException("无效的句柄。"),
                248 => new UHFGateException("天线检测错误。"),
                _ => new UHFGateException($"错误码：{state}"),
            };
        }
    }
}
