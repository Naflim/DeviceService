/*************************************************************************************
 *
 * 文 件 名:   UHFRFIDException
 * 描    述:   UHFRFID设备异常类
 * 
 * 版    本：  V1.0
 * 创 建 者：  Naflim 
 * 创建时间：  2022/5/30 18:53:10
 * ======================================================
 * 历史更新记录
 * 版本： V          修改时间：         修改人：
 * 修改内容：
 * ======================================================
*************************************************************************************/

using DeviceService.SDK.UHFRFID;
using System;

namespace DeviceService.Model.ExceptionModels
{
    public class UHFRFIDException : Exception
    {
        public UHFRFIDException(string message) : base(message)
        {

        }

        /// <summary>
        /// 返回UHFRFID设备异常信息
        /// </summary>
        /// <param name="state">异常状态</param>
        /// <returns>异常</returns>
        public static UHFRFIDException AbnormalJudgment(int state)
        {
            return new UHFRFIDException(UHFRFIDSDK.UHF_RFID_GET_ERROR(state));
        }
    }
}
