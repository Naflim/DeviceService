using DeviceService.DeviceModel;
using libzkfpcsharp;
using System;
using System.Collections.Generic;
using System.Text;
using DeviceService.Model.ExceptionModels;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using DeviceService.SDK;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DeviceService.Product
{
    /// <summary>
    /// 熵基中控指纹采集仪
    /// </summary>
    public class Live20FS : ZKTECO
    {
        /// <summary>
        /// 显示指纹
        /// </summary>
        public Action<Stream>? FingerprintShow { get; set; }


        const int TMPSIZE = 2048;

        void ParametersTo(int index, out byte[] FPBuffer, out byte[] CapTmp, out int cbCapTmp)
        {
            var device = devices[index];
            JudgeWorkingStatus(device.WorkingState);
            devices[index].WorkingState = WorkingState.CollectFingerprints;

            cbCapTmp = TMPSIZE;
            CapTmp = new byte[TMPSIZE];
            FPBuffer = new byte[device.ImgSize];
        }

        /// <summary>
        /// 获取指纹模板
        /// </summary>
        /// <param name="deviceIndex">设备编号</param>
        /// <returns>指纹模板</returns>
        public byte[] GetFingerprints(out string fingerprintImg, int deviceIndex = 0)
        {
            fingerprintImg = string.Empty;
            ParametersTo(deviceIndex, out byte[] FPBuffer, out byte[] CapTmp, out int cbCapTmp);
            while (devices[deviceIndex].WorkingState != WorkingState.Idle)
            {
                int aFlag = zkfp2.AcquireFingerprint(devices[deviceIndex].Handle, FPBuffer, CapTmp, ref cbCapTmp);
                if (aFlag != 0 && aFlag != -8)
                    throw ZKTECOException.AbnormalJudgment(aFlag);

                if (aFlag == 0)
                {
                    if (GetFingerprintPic(devices[deviceIndex], FPBuffer) is not Stream stream) continue;

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        Bitmap bitmap = new(stream);
                        MemoryStream ms = new();
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                        byte[] buffer = new byte[ms.Length];
                        ms.Position = 0;
                        ms.Read(buffer, 0, (int)ms.Length);
                        ms.Close();
                        fingerprintImg = Convert.ToBase64String(buffer);
                    }

                    ExitWorking(deviceIndex);
                    return CapTmp;
                }
            }
            ExitWorking(deviceIndex);
            return Array.Empty<byte>();
        }

        /// <summary>
        /// 合成指纹模板
        /// </summary>
        /// <param name="tmp1">模板1</param>
        /// <param name="tmp2">模板2</param>
        /// <param name="tmp3">模板3</param>
        /// <returns>合成模板</returns>
        public byte[] SynthesisFingerprints(byte[] tmp1, byte[] tmp2, byte[] tmp3)
        {
            int cbRegTmp = TMPSIZE;
            byte[] RegTmp = new byte[TMPSIZE];
            int mFlag = zkfp2.DBMerge(dbHandle, tmp1, tmp2, tmp3, RegTmp, ref cbRegTmp);
            if (mFlag != 0)
                throw ZKTECOException.AbnormalJudgment(mFlag);

            byte[] fingerprint = new byte[cbRegTmp];
            Array.Copy(RegTmp, fingerprint, cbRegTmp);
            return fingerprint;
        }

        /// <summary>
        /// 指纹记录
        /// </summary>
        public void CollectFingerprints(Action<byte[]> GetTmp)
        {
            for (int i = 0; i < DeviceCount; i++)
                CollectFingerprints(i, GetTmp);
        }

        /// <summary>
        /// 指定设备记录指纹
        /// </summary>
        /// <param name="index">设备索引</param>
        public void CollectFingerprints(int index, Action<byte[]> GetTmp)
        {
            ParametersTo(index, out byte[] FPBuffer, out byte[] CapTmp, out int cbCapTmp);

            int tmpCount = 0;
            Task.Run(async () =>
            {
                ThrowLog?.Invoke($"设备{index}开始采集指纹!");
                byte[][] tmps = new byte[3][];

                while (tmpCount < 3 && devices[index].WorkingState != WorkingState.Idle)
                {
                    int aFlag = zkfp2.AcquireFingerprint(devices[index].Handle, FPBuffer, CapTmp, ref cbCapTmp);
                    if (aFlag != 0 && aFlag != -8)
                        ErrorShow?.Invoke(ZKTECOException.AbnormalJudgment(aFlag));

                    if (aFlag == 0)
                    {
                        if (GetFingerprintPic(devices[index], FPBuffer) is not Stream stream) continue;
                        FingerprintShow?.Invoke(stream);
                        tmps[tmpCount] = CapTmp;
                        tmpCount++;
                        ThrowLog?.Invoke($"已收录指纹{tmpCount}");
                    }

                    await Task.Delay(200);
                }
                if (tmpCount < 3) return;

                int cbRegTmp = TMPSIZE;
                byte[] RegTmp = new byte[TMPSIZE];
                int mFlag = zkfp2.DBMerge(dbHandle, tmps[0], tmps[1], tmps[2], RegTmp, ref cbRegTmp);
                if (mFlag != 0)
                {
                    ErrorShow?.Invoke(ZKTECOException.AbnormalJudgment(mFlag));
                    return;
                }

                byte[] fingerprint = new byte[cbRegTmp];
                Array.Copy(RegTmp, fingerprint, cbRegTmp);
                ThrowLog?.Invoke($"收集完成!");
                GetTmp(fingerprint);
                ExitWorking(index);
            });
        }

        /// <summary>
        /// 识别指纹
        /// </summary>
        public void FingerprintIdentification()
        {
            for (int i = 0; i < DeviceCount; i++)
                FingerprintIdentification(i);
        }

        /// <summary>
        /// 指定设备识别指纹
        /// </summary>
        /// <param name="index">设备索引</param>
        public void FingerprintIdentification(int index)
        {
            ParametersTo(index, out byte[] FPBuffer, out byte[] CapTmp, out int cbCapTmp);

            Task.Run(async () =>
            {
                ThrowLog?.Invoke($"设备{index}开始识别指纹!");
                while (devices[index].WorkingState != WorkingState.Idle)
                {
                    int fid = 0, score = 0;
                    int aFlag = zkfp2.AcquireFingerprint(devices[index].Handle, FPBuffer, CapTmp, ref cbCapTmp);
                    if (aFlag != 0 && aFlag != -8)
                        ErrorShow?.Invoke(ZKTECOException.AbnormalJudgment(aFlag));

                    if (aFlag == 0)
                    {
                        if (GetFingerprintPic(devices[index], FPBuffer) is not Stream stream) continue;
                        FingerprintShow?.Invoke(stream);
                        int iFlag = zkfp2.DBIdentify(dbHandle, CapTmp, ref fid, ref score);

                        if (iFlag != 0 && iFlag != -17)
                            ErrorShow?.Invoke(ZKTECOException.AbnormalJudgment(iFlag));

                        if (iFlag == 0)
                            ThrowLog?.Invoke($"识别成功！id：{fid}，相似度：{score}");
                    }

                    await Task.Delay(200);
                }
            });
        }

        /// <summary>
        /// 比较指纹
        /// </summary>
        /// <param name="tmp">指纹模板</param>
        public void FingerprintIdentification(byte[] tmp)
        {
            for (int i = 0; i < DeviceCount; i++)
                FingerprintIdentification(i, tmp);
        }

        /// <summary>
        /// 指定设备对比指纹
        /// </summary>
        /// <param name="index">设备索引</param>
        /// <param name="tmp">指纹模板</param>
        public void FingerprintIdentification(int index, byte[] tmp)
        {
            ParametersTo(index, out byte[] FPBuffer, out byte[] CapTmp, out int cbCapTmp);

            Task.Run(async () =>
            {
                ThrowLog?.Invoke($"设备{index}开始对比指纹!");
                while (devices[index].WorkingState != WorkingState.Idle)
                {
                    int aFlag = zkfp2.AcquireFingerprint(devices[index].Handle, FPBuffer, CapTmp, ref cbCapTmp);
                    if (aFlag != 0 && aFlag != -8)
                        ErrorShow?.Invoke(ZKTECOException.AbnormalJudgment(aFlag));

                    if (aFlag == 0)
                    {
                        if (GetFingerprintPic(devices[index], FPBuffer) is not Stream stream) continue;
                        FingerprintShow?.Invoke(stream);
                        int similarity = zkfp2.DBMatch(dbHandle, CapTmp, tmp);
                        ThrowLog?.Invoke($"相似度：{similarity}");
                    }

                    await Task.Delay(200);
                }
            });
        }

        /// <summary>
        /// 退出工作状态
        /// </summary>
        public void ExitWorking()
        {
            for (int i = 0; i < DeviceCount; i++)
                ExitWorking(i);
        }

        /// <summary>
        /// 指定设备退出工作状态
        /// </summary>
        /// <param name="index">设备索引</param>
        public void ExitWorking(int index)
        {
            ThrowLog?.Invoke($"设备{index}退出工作状态!");
            devices[index].WorkingState = WorkingState.Idle;
        }

        /// <summary>
        /// 保存指纹到内存
        /// </summary>
        /// <param name="uid">指纹id号</param>
        /// <param name="tmp">指纹模板</param>
        public void SaveFingerprint(int uid, byte[] tmp)
        {
            zkfp2.DBAdd(dbHandle, uid, tmp);
        }

        Stream? GetFingerprintPic(ZKTECODevice device, byte[] img)
        {
            try
            {
                MemoryStream ms = new();
                BitmapFormat.GetBitmap(img, device.ImgWidth, device.ImgHeight, ref ms);
                return ms;
            }
            catch (Exception ex)
            {
                ErrorShow?.Invoke(ex);
                return null;
            }
        }

    }
}
