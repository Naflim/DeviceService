using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Model.ExceptionModels
{
    public class ZKTECOException : Exception
    {
        public ZKTECOException(string message) : base(message)
        {

        }

        /// <summary>
        /// 返回UHFGate设备异常信息
        /// </summary>
        /// <param name="state">异常状态</param>
        /// <returns></returns>
        public static ZKTECOException AbnormalJudgment(int state)
        {
            return state switch
            {
                -1 => new ZKTECOException("初始化算法库失败"),
                -7=> new ZKTECOException("无效句柄"),
                -8 => new ZKTECOException("取像失败"),
                -17 => new ZKTECOException("操作失败"),
                _ => new ZKTECOException($"错误码：{state}"),
            };
        }
    }
}
