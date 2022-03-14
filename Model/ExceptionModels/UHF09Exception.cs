using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Model.ExceptionModels
{
    public class UHF09Exception : Exception
    {
        public UHF09Exception(string message) : base(message)
        {

        }

        /// <summary>
        /// 返回UHF288设备异常信息
        /// </summary>
        /// <param name="state">异常状态</param>
        /// <returns></returns>
        public static UHF09Exception AbnormalJudgment(int state)
        {
            return state switch
            {
                48 => new UHF09Exception("通讯错误。"),
                248 => new UHF09Exception("天线检测错误。"),
                _ => new UHF09Exception($"错误码：{state}"),
            };
        }
    }
}
