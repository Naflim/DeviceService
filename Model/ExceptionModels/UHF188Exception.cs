/*************************************************************************************
 *
 * 文 件 名:   UHF188Exception
 * 描    述:   UHF188设备异常类
 * 
 * 版    本：  V1.0
 * 创 建 者：  Naflim 
 * 创建时间：  2022/5/26 10:25:05
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
    public class UHF188Exception : Exception
    {
        public UHF188Exception(string message) : base(message)
        {

        }

        /// <summary>
        /// 返回UHF188设备异常信息
        /// </summary>
        /// <param name="state">异常状态</param>
        /// <returns>异常</returns>
        public static UHF188Exception AbnormalJudgment(int state)
        {
            switch (state)
            {
                case 48:
                    return new UHF188Exception("通讯错误。");
                case 53:
                    return new UHF188Exception("端口已打开。");
                case 254:
                    return new UHF188Exception("不合法的命令。");
                default:
                    return new UHF188Exception($"错误码：{state}");
            }
        }
    }
}
