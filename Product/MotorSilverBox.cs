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
using System.Threading.Tasks;
using DeviceService.DeviceModel;
using DeviceService.Model.ExceptionModels;
using NaflimHelperLibrary;

namespace DeviceService
{
    /// <summary>
    /// 银箱电机驱动板
    /// </summary>
    public class MotorSilverBox : AnsweringProtocol
    {
        /// <summary>
        /// 从站地址码
        /// </summary>
        public byte ADR { get; set; } = 1;

        /// <summary>
        /// 是否监听设备
        /// </summary>
        public bool CanMonitor { get; set; }

        bool canStop;

        /// <summary>
        /// 电机停止触发
        /// </summary>
        public Action MotorStop { get; set; }

        public void StartMonitorDevice()
        {
            CanMonitor = true;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    byte[] GetSQ(byte sq)
                    {
                        List<byte> sendData = new List<byte>() { ADR, 0x03, 0x00, sq, 0x00, 0x01 };
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

                        return Communication(sendData.ToArray());
                    }

                    bool oldSq1 = true;
                    bool oldSq2 = true;

                    while (CanMonitor)
                    {
                        byte[] SQ1 = GetSQ(0x18);
                        if (SQ1.Length > 2 && SQ1[1] == 0x86)
                            throw MotorSilverBoxException.AbnormalJudgment(SQ1[2]);
                        bool isSq1 = false;
                        if (SQ1.Length > 4)
                            isSq1 = Convert.ToBoolean(SQ1[4]);

                        byte[] SQ2 = GetSQ(0x19);
                        if (SQ2.Length > 2 && SQ2[1] == 0x86)
                            throw MotorSilverBoxException.AbnormalJudgment(SQ2[2]);
                        bool isSq2 = false;
                        if (SQ2.Length > 4)
                            isSq2 = Convert.ToBoolean(SQ2[4]);

                        if (oldSq1 != isSq1 || oldSq2 != isSq2)
                            SQChange(isSq1, isSq2);

                        oldSq1 = isSq1;
                        oldSq2 = isSq2;
                    }
                }
                catch (Exception ex)
                {
                    ErrorShow?.Invoke(ex);
                }
            }, TaskCreationOptions.LongRunning);

        }

        void SQChange(bool sq1, bool sq2)
        {
            if (canStop && (!sq1 || !sq2))
                Stop();
        }

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

            canStop = true;
        }

        /// <summary>
        /// 停止电机
        /// </summary>
        public void Stop()
        {
            Start(0);

            canStop = false;

            MotorStop?.Invoke();
        }
    }
}
