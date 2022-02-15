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
            switch (state)
            {
                case 48:
                    return new UHFGateException("通讯错误。");
                case 248:
                    return new UHFGateException("天线检测错误。");
                default:
                    return new UHFGateException($"错误码：{state}");
            }
        }
    }
}
