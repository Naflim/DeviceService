using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Model.ExceptionModels
{
    public class UHF288Exception : Exception
    {
        public UHF288Exception(string message) : base(message)
        {

        }

        /// <summary>
        /// 返回UHF288设备异常信息
        /// </summary>
        /// <param name="state">异常状态</param>
        /// <returns></returns>
        public static UHF288Exception AbnormalJudgment(int state)
        {
            switch (state)
            {
                case 48:
                    return new UHF288Exception("通讯错误。");
                case 248:
                    return new UHF288Exception("天线检测错误。");
                default:
                    return new UHF288Exception($"错误码：{state}");
            }
        }
    }
}
