using DeviceService.DeviceModel;
using DeviceService.Model;
using DeviceService.Model.ExceptionModels;
using DeviceService.SDK;
using NaflimHelperLibrary;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService
{
    /// <summary>
    /// UHF288无GPIO设备报警器
    /// </summary>
    public class AlarmUHF288NotGPIO : UHFReader288, IAlarm
    {
        Action<AlarmUHF288NotGPIO, AlarmModel> adoptTrigger;
        bool alarmFlag;

        /// <summary>
        /// 开始报警服务
        /// </summary>
        /// <param name="adoptTrigger">报警触发事件</param>
        public void StartAlarmServer(Action<IAlarm, AlarmModel> adoptTrigger)
        {
            this.adoptTrigger = adoptTrigger;
            StartSel();
        }

        /// <summary>
        /// 结束报警服务
        /// </summary>
        public void EndAlarmServer()
        {
            selFlag = false;
        }

        /// <summary>
        /// 开始查询
        /// </summary>
        public override void StartSel()
        {
            if (selFlag || AntennaList is null)
                return;
            selFlag = true;

            Task.Factory.StartNew(async () =>
            {
                int count = 0;
                try
                {
                    while (selFlag)
                    {
                        if (count >= AntennaList.Count)
                            count = 0;

                        byte InAnt = AntennaList[count];//不可变
                        byte FastFlag = 1;//不可变
                        byte Qvalue = 4;//可变
                        byte tidAddr = 0;//可变
                        byte tidLen = 0;//可变
                        byte TIDFlag = 0;//可变
                        byte Scantime = 20;//可变
                        byte Target = 0;
                        byte Ant = 0;
                        byte MaskMem = 0;
                        byte MaskLen = 0;
                        byte MaskFlag = 0;
                        byte Session = 0;
                        byte[] MaskAdr = new byte[2];
                        byte[] MaskData = new byte[100];
                        byte[] EPC = new byte[50000];
                        int TagNum = 0;
                        int Totallen = 0;

                        int fCmdRet = UHF288SDK.Inventory_G2(ref comAdr, Qvalue, Session, MaskMem, MaskAdr, MaskLen, MaskData, MaskFlag, tidAddr, tidLen, TIDFlag, Target, InAnt, Scantime, FastFlag, EPC, ref Ant, ref Totallen, ref TagNum, handle);

                        if (fCmdRet != 0x01 && fCmdRet != 0x02)
                        {
                            if (errorNum < 3)
                            {
                                errorNum++;
                                ThrowLog?.Invoke($"{ip}-查询：{UHF288Exception.AbnormalJudgment(fCmdRet).Message}");
                            }
                            else
                                throw UHF288Exception.AbnormalJudgment(fCmdRet);
                        }
                        string[] EPCarr = DataConversion.GetEPC(EPC).Where(v => !string.IsNullOrEmpty(v)).ToArray();//byte数组以EPC格式转换为字符串数组

                        if (EPCarr.Length > 0)
                        {
                            if (!alarmFlag)
                            {
                                alarmFlag = true;
                                if (GreenPort != byte.MaxValue)
                                    UHF288SDK.SetGPIO(ref comAdr, GreenPort, handle);
                            }
                        }
                        else
                        {
                            if (alarmFlag)
                            {
                                alarmFlag = false;
                                UHF288SDK.SetGPIO(ref comAdr, 0, handle);
                                adoptTrigger?.Invoke(this, new AlarmModel(cacheTags));
                                Reset();
                            }
                        }

                        foreach (string item in EPCarr)
                            AddCacheEpcs(item);

                        count++;
                        await Task.Delay(100);
                    }
                }
                catch (Exception ex)
                {
                    if (ErrorShow is null)
                        Log.PrintError(ex);
                    else
                        ErrorShow(ex);
                }
            }, TaskCreationOptions.LongRunning);
        }

        void Reset()
        {
            ThrowLog?.Invoke($"{DeviceName}-重置");
            ClearCache();
        }
    }
}
