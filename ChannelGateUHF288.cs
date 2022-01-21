using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceService.DeviceModel;
using DeviceService.Model;
using DeviceService.Model.ExceptionModels;
using DeviceService.SDK;
using NaflimHelperLibrary;

namespace DeviceService
{
    public class ChannelGateUHF288 : UHFReader288, IChannelGate
    {
        private byte gpio;

        byte ago = byte.MaxValue;
        byte rear = byte.MaxValue;
        byte oldGPIO;
        DateTime endStart;
        Direction direction = Direction.Null;
        bool directionFlag;
        Action<ChannelGateUHF288, ChannelGateModel> adoptTrigger;

        /// <summary>
        /// 显示GPIO
        /// </summary>
        public Action<ChannelGateUHF288, byte> ShowGPIO { get; set; }

        /// <summary>
        /// 超时结束查询
        /// </summary>
        public int EndTime { get; set; }

        /// <summary>
        /// 开启延时上传模式
        /// </summary>
        public bool DelayMode { get; set; }

        /// <summary>
        /// 设置红外模式
        /// </summary>
        /// <param name="mode">是否对射</param>
        public void SetInfraredMode(bool mode)
        {
            if (mode)
            {
                ago = (byte)(DefGPIO - 1);
                rear = (byte)(DefGPIO - 2);
            }
            else
            {
                ago = (byte)(DefGPIO + 1);
                rear = (byte)(DefGPIO + 2);
            }
        }

        /// <summary>
        /// 方向反向
        /// </summary>
        public void ReverseDirection()
        {
            ago ^= rear;
            rear = (byte)(ago ^ rear);
            ago ^= rear;
        }

        /// <summary>
        /// 开始监听GPIO
        /// </summary>
        /// <param name="adoptTrigger">方向判断成功触发事件</param>
        public void StartMonitorGPIO(Action<ChannelGateUHF288, ChannelGateModel> adoptTrigger)
        {
            if (DefGPIO == byte.MaxValue) throw new Exception("默认GPIO不可为空");
            this.adoptTrigger = adoptTrigger;
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    while (true)
                    {
                        byte outupPin = 0;

                        if (workFlag)
                            continue;

                        #region 防止线程撞车
                        workFlag = true;
                        await Task.Delay(100);
                        int GPIOflag = UHF288SDK.GetGPIOStatus(ref comAdr, ref outupPin, handle);
                        workFlag = false;
                        #endregion

                        if (GPIOflag == 0)
                        {
                            SetGPIO(outupPin);
                            ShowGPIO?.Invoke(this,outupPin);
                        }
                        else
                        {
                            if (errorNum < 3)
                            {
                                errorNum++;
                                TryLog?.Invoke($"{ip}-红外：{UHF288Exception.AbnormalJudgment(GPIOflag).Message}");
                            }
                            else
                                throw UHF288Exception.AbnormalJudgment(GPIOflag);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ErrorShow is null)
                        Log.PrintError(ex);
                    else
                        ErrorShow(ex);
                    Reset();
                    Reconnection();
                    StartMonitorGPIO(adoptTrigger);
                }
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 更新GPIO值
        /// </summary>
        /// <param name="GPIO">GPIO</param>
        void SetGPIO(byte GPIO)
        {
            gpio = GPIO;
            if (GPIO != DefGPIO)
                GPIO_ValueChanged();
            else
            {
                if (selFlag && (DateTime.Now - endStart).TotalMilliseconds > EndTime)
                {
                    if (DelayMode && direction != Direction.Null) AdoptTrigger(new ChannelGateModel(direction, cacheEPC));
                    else Reset();
                }
                directionFlag = false;
            }
        }

        /// <summary>
        /// GPIO改变触发
        /// </summary>
        void GPIO_ValueChanged()
        {
            StartSel();
            endStart = DateTime.Now;

            if (gpio != oldGPIO)
                DirectionJudgment();
        }

        /// <summary>
        /// 判断方向
        /// </summary>
        void DirectionJudgment()
        {
            if (ago == byte.MaxValue || rear == byte.MaxValue) throw new Exception("未设置红外模式");
            if (directionFlag) return;
            if (oldGPIO == ago && gpio == rear)
                direction = Direction.In;

            if (oldGPIO == rear && gpio == ago)
                direction = Direction.Out;

            if (direction != Direction.Null)
            {
                directionFlag = true;
                if (!DelayMode)
                    AdoptTrigger(new ChannelGateModel(direction, cacheEPC));
            }

            if (!directionFlag && (gpio == ago || gpio == rear))
                oldGPIO = gpio;
        }


        public void AdoptTrigger(ChannelGateModel channelGate)
        {
            adoptTrigger(this,channelGate);
            Reset();
        }

        void Reset()
        {
            TryLog?.Invoke($"{ip}-重置");
            StopSel();
            ClearCache();
            direction = Direction.Null;
            oldGPIO = 0;
        }
    }
}
