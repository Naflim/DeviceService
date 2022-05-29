/*************************************************************************************
 *
 * 文 件 名:   MotorSilverBoxException
 * 描    述:  医疗电机设备返回的异常
 * 
 * 版    本：  V1.0
 * 创 建 者：  Naflim 
 * 创建时间：  2022/5/12 15:04:52
 * ======================================================
 * 历史更新记录
 * 版本： V          修改时间：         修改人：
 * 修改内容：
 * ======================================================
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Model.ExceptionModels
{
    class MotorSilverBoxException : Exception
    {
        public MotorSilverBoxException(string message) : base(message)
        {

        }

        /// <summary>
        /// 返回医疗电机设备异常信息
        /// </summary>
        /// <param name="errcode">异常码</param>
        /// <returns></returns>
        public static MotorSilverBoxException AbnormalJudgment(byte errcode)
        {
            switch (errcode)
            {
                default:
                    return new MotorSilverBoxException($"错误码：{errcode}");
            }
        }
    }
}
