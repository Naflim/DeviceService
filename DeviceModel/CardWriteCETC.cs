using DeviceService.Model;
using DeviceService.Model.ExceptionModels;
using rfidLink.Extend;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceService.DeviceModel
{
    /// <summary>
    /// CETC(黑盒子)型号发卡器
    /// </summary>
    public class CardWriteCETC : IReader
    {
        readonly LinkageExtend link = new LinkageExtend();
        List<RadioInformation> eqList;
        ReadParms readParms;
        WriteParms writeParms;

        /// <summary>
        /// 查询间隙
        /// </summary>
        public int SelInterval { get; set; } = 0;

        /// <summary>
        /// 抛出日志
        /// </summary>
        public Action<string> ThrowLog { get; set; }

        /// <summary>
        /// 显示异常
        /// </summary>
        public Action<Exception> ErrorShow { get; set; }

        /// <summary>
        /// 自动串口连接
        /// </summary>
        public void Connect()
        {
            var flag = link.Initialization();

            if (flag != operResult.Ok) throw CETCException.AbnormalJudgment(flag);

            eqList = link.GetRadioEnumeration();

            eqList.ForEach(v =>
            {
                connectStatus status = link.GetRadioConnectStatus(v.radioHandle);
                if (status == connectStatus.Disconnected)
                {
                    var linkFlag = link.Connect(v.radioHandle);
                    if (linkFlag == operResult.Ok)
                    {
                        readParms = new ReadParms()
                        {
                            accesspassword = 0,
                            length = 1,
                            memBank = MemoryBank.EPC,
                            offset = 0
                        };

                    }
                    else
                        throw CETCException.AbnormalJudgment(linkFlag);
                }
            });
        }

        public void Disconnect()
        {
            eqList.ForEach(v =>
            {
                connectStatus status = link.GetRadioConnectStatus(v.radioHandle);
                if (status == connectStatus.Connected)
                    link.Disconnect(v.radioHandle);
            });
        }

        /// <summary>
        /// 设置功率
        /// </summary>
        /// <param name="ant">天线</param>
        /// <param name="power">功率</param>
        public void SetPower(byte power, int ant = 0)
        {
            eqList.ForEach(v =>
            {
                var flag = link.GetAntennaPortConfigurationAndStatus(v.radioHandle, ant, out AntennaPortConfigurationAndStatus result);

                if (flag == operResult.Ok)
                {
                    if (result.antennaPortState != AntennaPortState.Enabled)
                        throw new CETCException("天线未启用！");
                    else
                        SetAntPower(v.radioHandle, ant, result.antennaPortConfiguration, power);
                }
                else
                    if (flag != operResult.NoTag)
                    throw CETCException.AbnormalJudgment(flag);
            });
        }

        private void SetAntPower(int handle, int ant, AntennaPortConfiguration oldConfig, uint power)
        {
            AntennaPortConfiguration result = oldConfig;
            result.powerLevel = power * 10;
            var flag = link.SetAntennaPortConfiguration(handle, ant, result);

            if (flag != operResult.Ok && flag != operResult.NoTag)
                throw CETCException.AbnormalJudgment(flag);
        }

        /// <summary>
        /// 查询标签
        /// </summary>
        /// <returns>标签组</returns>
        public virtual string[] SelTag()
        {
            List<string> epcList = new List<string>();
            eqList.ForEach(v =>
            {
                var flag = link.TagInfoRead(v.radioHandle, readParms, out List<ReadResult> result);

                if (flag == operResult.Ok)
                {
                    result.ForEach(val =>
                    {
                        string epc = val.flagID;
                        if (!epcList.Contains(epc))
                            epcList.Add(epc);
                    });
                }
                else
                    if (flag != operResult.NoTag)
                    throw CETCException.AbnormalJudgment(flag);
            });
            return epcList.ToArray();
        }

        /// <summary>
        /// epc写入标签
        /// </summary>
        /// <param name="epc">写入的epc</param>
        public void WriteTag(string epc)
        {
            int count = epc.Length / 4;
            writeParms = new WriteParms()
            {
                accesspassword = 0,
                length = (ushort)(count + 1),
                memBank = MemoryBank.EPC,
                offset = 1
            };

            epc = GetPC(count) + epc;
            ushort[] writeData = new ushort[writeParms.length];
            for (int i = 0; i < writeData.Length; i++)
                if (!ValidateHex_ushort(epc.Substring(i * 4, 4), out writeData[i]))
                    throw new CETCException("EPC错误！");

            eqList.ForEach(v =>
            {
                var flag = link.TagInfoWrite(v.radioHandle, writeParms, writeData, out List<TagOperResult> tagOperResult);

                if (flag != operResult.Ok && flag != operResult.NoTag)
                    throw CETCException.AbnormalJudgment(flag);
            });
        }

        private string GetPC(int count)
        {
            if (count % 2 == 0)
            {
                string val = (count / 2).ToString("X");
                val += "000";
                return val;
            }
            else
            {
                string val = (count / 2).ToString("X");
                val += "800";
                return val;
            }
        }

        private bool ValidateHex_ushort(string input, out ushort value)
        {
            if (!ushort.TryParse(input, System.Globalization.NumberStyles.AllowHexSpecifier, null, out value))
                return false;

            return true;
        }

        public void Connect(ConnectModel connect)
        {
            throw new NotImplementedException();
        }

        async public Task<Tag[]> CyclicQueryTags(int seconds)
        {
            List<Tag> tags = new List<Tag>();
            try
            {
                DateTime startTime = DateTime.Now;

                while (DateTime.Now < startTime.AddSeconds(seconds))
                {
                    eqList.ForEach(v =>
                    {
                        var flag = link.TagInfoRead(v.radioHandle, readParms, out List<ReadResult> result);

                        if (flag == operResult.Ok)
                        {
                            result.ForEach(val =>
                            {
                                string epc = val.flagID;
                                if (!string.IsNullOrEmpty(epc))
                                {
                                    var tag = tags.Find(t => t.EPC == epc);

                                    if (tag is Tag)
                                    {
                                        tag.Frequency++;
                                        tag.QueryTime = DateTime.Now;
                                    }
                                    else tags.Add(new Tag(epc));
                                }
                            });
                        }
                        else
                            if (flag != operResult.NoTag)
                            throw CETCException.AbnormalJudgment(flag);
                    });

                    await Task.Delay(SelInterval);
                }

                return tags.ToArray();
            }
            catch (Exception ex)
            {
                ErrorShow?.Invoke(ex);
                return tags.ToArray();
            }
        }

        public Tag[] QueryTags(int ant = 0)
        {
            List<Tag> tags = new List<Tag>();
            eqList.ForEach(v =>
            {
                var flag = link.TagInfoRead(v.radioHandle, readParms, out List<ReadResult> result);

                if (flag == operResult.Ok)
                {
                    result.ForEach(val =>
                    {
                        string epc = val.flagID;
                        if (!string.IsNullOrEmpty(epc))
                        {
                            var tag = tags.Find(t => t.EPC == epc);

                            if (tag is Tag)
                            {
                                tag.Frequency++;
                                tag.QueryTime = DateTime.Now;
                            }
                            else tags.Add(new Tag(epc));
                        }
                    });
                }
                else
                    if (flag != operResult.NoTag)
                    throw CETCException.AbnormalJudgment(flag);
            });
            return tags.ToArray();
        }
    }
}
