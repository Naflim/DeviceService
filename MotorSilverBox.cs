/*************************************************************************************
 *
 * 文 件 名:   MotorSilverBox
 * 描    述:  医院银箱电机操作
 * 
 * 版    本：  V1.0
 * 创 建 者：  Naflim 
 * 创建时间：  2022/5/12 11:18:57
 * ======================================================
 * 历史更新记录
 * 版本： V          修改时间：         修改人：
 * 修改内容：
 * ======================================================
*************************************************************************************/

using System;
using System.Collections.Generic;
using DeviceService.DeviceModel;
using DeviceService.Model.ExceptionModels;
using NaflimHelperLibrary;

namespace DeviceService
{
    public class MotorSilverBox : AnsweringProtocol
    {
        /// <summary>
        /// 从站地址码
        /// </summary>
        public byte ADR { get; set; } = 1;

        /// <summary>
        /// 启动电机
        /// </summary>
        /// <param name="speed">速度</param>
        public void Start(int speed)
        {
            List<byte> sendData = new List<byte>() { ADR, 0x06, 0x00, 0x40 };
            if (speed > 1000 || speed < -1000)
                throw new ArgumentOutOfRangeException(nameof(speed), "速度超出可设置范围（-1000~1000）");

            int speedVal = speed < 0 ? 65536 + speed : speed;

            sendData.Add((byte)(speedVal / 256));
            sendData.Add((byte)(speedVal % 256));
            short crc = DataConversion.CRC16(sendData.ToArray(), sendData.Count);

            if (crc < 0)
            {
                sendData.Add((byte)(crc / 256 - 1));
                sendData.Add((byte)(crc % 256));
            }
            else
            {
                sendData.Add((byte)(crc / 256));
                sendData.Add((byte)(crc % 256));
            }

            var result = Communication(sendData.ToArray());

            if (result.Length > 2 && result[1] == 0x86)
                throw MotorSilverBoxException.AbnormalJudgment(result[2]);
        }

        /// <summary>
        /// 停止电机
        /// </summary>
        public void Stop()
        {
            Start(0);
        }
    }
}
