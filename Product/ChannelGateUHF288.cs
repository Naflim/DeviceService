using DeviceService.DeviceModel;
using DeviceService.Model;
using DeviceService.Model.ExceptionModels;
using DeviceService.SDK;
using NaflimHelperLibrary;
using System;
using System.Threading.Tasks;

namespace DeviceService
{
    /// <summary>
    /// UHF288通道门
    /// </summary>
    public class ChannelGateUHF288 : UHFReader288, IChannelGate
    {
        InGPIO inGpio = InGPIO.Init;
        InGPIO defIN = InGPIO.Init;
        InGPIO ago = InGPIO.Init;
        InGPIO rear = InGPIO.Init;
        InGPIO total = InGPIO.Init;
        InGPIO oldIN = InGPIO.Init;
        DateTime endStart;
        Direction direction = Direction.Null;
        bool directionFlag;
        Action<ChannelGateUHF288, ChannelGateModel> adoptTrigger;

        /// <summary>
        /// 显示GPIO
        /// </summary>
        public Action<ChannelGateUHF288, InGPIO> ShowGPIO { get; set; }

        /// <summary>
        /// 显示查询状态
        /// </summary>
        public Action<ChannelGateUHF288, bool> ShowSelState { get; set; }

        /// <summary>
        /// 开启延时上传模式
        /// </summary>
        public bool DelayMode { get; set; }

        /// <summary>
        /// 上报延时时间
        /// </summary>
        public int EndTime { get; set; }

        /// <summary>
        /// 超时清理时间
        /// </summary>
        public int ClearTime { get; set; } = 3000;

        /// <summary>
        /// GPIO间隙
        /// </summary>
        public int GPIOinterval { get; set; } = 100;



        /// <summary>
        /// 设置红外模式
        /// </summary>
        /// <param name="mode">是否对射</param>
        public void SetInfraredMode(bool mode)
        {
            if (mode)
            {
                defIN = InGPIO.All;
                ago = InGPIO.In2;
                rear = InGPIO.In1;
                total = InGPIO.None;
            }
            else
            {
                defIN = InGPIO.None;
                ago = InGPIO.In1;
                rear = InGPIO.In2;
                total = InGPIO.All;
            }
        }

        /// <summary>
        /// 方向反向
        /// </summary>
        public void ReverseDirection()
        {
            InGPIO count = ago;
            ago = rear;
            rear = count;
        }

        /// <summary>
        /// 开始监听GPIO
        /// </summary>
        /// <param name="adoptTrigger">方向判断成功触发事件</param>
        public void StartChannelGateServer(Action<IChannelGate, ChannelGateModel> adoptTrigger)
        {
            if (defIN == InGPIO.Init) throw new Exception("红外模式未设置");
            this.adoptTrigger = adoptTrigger;
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    int count = 0;
                    while (true)
                    {
                        if (count >= AntennaList.Count)
                            count = 0;

                        byte outupPin = 0;

                        int GPIOflag = UHF288SDK.GetGPIOStatus(ref comAdr, ref outupPin, handle);

                        if (GPIOflag == 0)
                        {
                            InGPIO gpio = GetInGPIO(outupPin);
                            SetGPIO(gpio);
                            ShowGPIO?.Invoke(this, gpio);

                            ShowSelState?.Invoke(this, selFlag);
                            if (selFlag)
                            {
                                SelTag(count);
                                count++;
                            }
                        }
                        else
                        {
                            if (errorNum < 3)
                            {
                                errorNum++;
                                ThrowLog?.Invoke($"{DeviceName}-红外：{UHF288Exception.AbnormalJudgment(GPIOflag).Message}");
                            }
                            else
                                throw UHF288Exception.AbnormalJudgment(GPIOflag);
                        }

                        await Task.Delay(GPIOinterval);
                    }
                }
                catch (Exception ex)
                {
                    if (ErrorShow is null)
                        Log.PrintError(ex);
                    else
                        ErrorShow(ex);
                    Reset();

                    switch (mode)
                    {
                        case ConnectMode.Tcp:
                            Disconnect();
                            break;
                        case ConnectMode.SerialPort:
                            ComDisconnect();
                            break;
                    }
                    Reconnection?.Invoke(this);
                }
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 更新GPIO值
        /// </summary>
        /// <param name="GPIO">GPIO</param>
        void SetGPIO(InGPIO inGPIO)
        {
            inGpio = inGPIO;
            if (inGPIO != defIN)
                GPIO_ValueChanged();
            else
            {
                if (selFlag)
                {
                    if (DelayMode && direction != Direction.Null && (DateTime.Now - endStart).TotalMilliseconds > EndTime)
                        AdoptTrigger(new ChannelGateModel(direction, cacheTags));
                    else if ((DateTime.Now - endStart).TotalMilliseconds > ClearTime)
                        Reset();
                }

                directionFlag = false;
            }
        }

        /// <summary>
        /// GPIO改变触发
        /// </summary>
        void GPIO_ValueChanged()
        {
            selFlag = true;
            endStart = DateTime.Now;

            if (inGpio != oldIN)
                DirectionJudgment();
        }

        /// <summary>
        /// 判断方向
        /// </summary>
        void DirectionJudgment()
        {
            if (directionFlag) return;

            if ((oldIN == ago && (inGpio == rear || inGpio == total)) || oldIN == total && inGpio == rear)
                direction = Direction.In;

            if (oldIN == rear && (inGpio == ago || inGpio == total) || oldIN == total && inGpio == ago)
                direction = Direction.Out;

            if (direction != Direction.Null)
            {
                directionFlag = true;
                if (!DelayMode)
                    AdoptTrigger(new ChannelGateModel(direction, cacheTags));
            }

            if (!directionFlag && inGpio != defIN)
                oldIN = inGpio;
        }

        public void AdoptTrigger(ChannelGateModel channelGate)
        {
            adoptTrigger(this, channelGate);
            Reset();
        }

        void Reset()
        {
            ThrowLog?.Invoke($"{DeviceName}-重置");
            StopSel();
            ClearCache();
            direction = Direction.Null;
            oldIN = InGPIO.Init;
        }
    }
}
