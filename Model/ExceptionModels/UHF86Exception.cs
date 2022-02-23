using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Model.ExceptionModels
{
    public class UHF86Exception : Exception
    {
        public UHF86Exception(string message) : base(message)
        {

        }

        /// <summary>
        /// 返回UHF288设备异常信息
        /// </summary>
        /// <param name="state">异常状态</param>
        /// <returns></returns>
        public static UHF86Exception AbnormalJudgment(int state)
        {
            switch (state)
            {
                case 48:
                    return new UHF86Exception("通讯错误。");
                case 248:
                    return new UHF86Exception("天线检测错误。");
                case 250:
                    return new UHF86Exception("有电子标签，但通信不畅，无法操作");
                default:
                    return new UHF86Exception($"错误码：{state}");
            }
        }
    }
}
