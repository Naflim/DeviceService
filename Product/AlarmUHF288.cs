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
    /// UHF288报警器
    /// </summary>
    public class AlarmUHF288 : UHFReader288, IAlarm
    {
        /// <summary>
        /// 默认输出口
        /// </summary>
        public InGPIO DefIN { get; set; } = InGPIO.Init;
        DateTime endStart;
        Action<AlarmUHF288, AlarmModel> adoptTrigger;

        /// <summary>
        /// 显示GPIO
        /// </summary>
        public Action<AlarmUHF288, InGPIO> ShowGPIO { get; set; }

        /// <summary>
        /// 超时结束查询
        /// </summary>
        public int EndTime { get; set; }

        /// <summary>
        /// 开启延时上传模式
        /// </summary>
        public bool DelayMode { get; set; }

        /// <summary>
        /// GPIO间隙
        /// </summary>
        public int GPIOinterval { get; set; } = 100;

        /// <summary>
        /// 开始报警服务
        /// </summary>
        /// <param name="adoptTrigger">报警触发事件</param>
        public void StartAlarmServer(Action<IAlarm, AlarmModel> adoptTrigger)
        {
            if (DefIN == InGPIO.Init) throw new Exception("未设置默认GPIO口");
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
                            var newIn = GetInGPIO(outupPin);
                            SetGPIO(newIn);
                            ShowGPIO?.Invoke(this, newIn);

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
                    //FReconnection();
                }
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 更新GPIO值
        /// </summary>
        /// <param name="GPIO">GPIO</param>
        void SetGPIO(InGPIO inGPIO)
        {
            if (inGPIO != DefIN)
                GPIO_ValueChanged();
            else
            {
                if (selFlag)
                {
                    if (DelayMode)
                    {
                        if (selFlag && (DateTime.Now - endStart).TotalMilliseconds > EndTime)
                            AdoptTrigger(new AlarmModel(cacheTags));
                    }else AdoptTrigger(new AlarmModel(cacheTags));
                }
            }
        }

        private void AdoptTrigger(AlarmModel alarm)
        {
            adoptTrigger(this, alarm);
            Reset();
        }

        /// <summary>
        /// GPIO改变触发
        /// </summary>
        void GPIO_ValueChanged()
        {
            selFlag = true;
            endStart = DateTime.Now;
        }


        void Reset()
        {
            ThrowLog?.Invoke($"{DeviceName}-重置");
            StopSel();
            ClearCache();
        }
    }
}
