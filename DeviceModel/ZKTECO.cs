using libzkfpcsharp;
using System;
using System.Collections.Generic;
using System.Text;
using DeviceService.Model.ExceptionModels;
using DeviceService.Model;

namespace DeviceService.DeviceModel
{
    public class ZKTECO : IDevice
    {
        /// <summary>
        /// 工作状态枚举
        /// </summary>
        protected enum WorkingState
        {
            Idle,
            CollectFingerprints,
            FingerprintIdentification
        }

        /// <summary>
        /// 熵基科技设备
        /// </summary>
        protected struct ZKTECODevice
        {
            public IntPtr handle;
            public int imgWidth;
            public int imgHeight;
            public WorkingState workingState;
            public ZKTECODevice(IntPtr handle, int width, int height)
            {
                this.handle = handle;
                imgWidth = width;
                imgHeight = height;
                workingState = WorkingState.Idle;
            }

            public ZKTECODevice(IntPtr handle, int width, int height, WorkingState workingState)
            {
                this.handle = handle;
                imgWidth = width;
                imgHeight = height;
                this.workingState = workingState;
            }

            public ZKTECODevice SetWorkState(WorkingState state)
            {
                workingState = state;
                return this;
            }

            public int ImgSize()
            {
                return imgHeight * imgWidth;
            }
        }

        private int deviceCount;

        /// <summary>
        /// 设备数
        /// </summary>
        public int DeviceCount
        {
            get { return deviceCount; }
        }

        /// <summary>
        /// 设备列表
        /// </summary>
        protected readonly List<ZKTECODevice> devices = new List<ZKTECODevice>();

        /// <summary>
        /// 算法库句柄
        /// </summary>
        protected IntPtr dbHandle = IntPtr.Zero;

        public ZKTECO()
        {
            Init();
        }

        void Init()
        {
            int flag = zkfp2.Init();
            if (flag != 0)
                throw ZKTECOException.AbnormalJudgment(flag);
            dbHandle = zkfp2.DBInit();
            if (dbHandle == IntPtr.Zero)
                throw new ZKTECOException("算法库初始化失败！");
        }

        /// <summary>
        /// 连接设备
        /// </summary>
        public void Connect()
        {
            deviceCount = zkfp2.GetDeviceCount();
            for (int i = 0; i < deviceCount; i++)
                devices.Add(GetParameters(zkfp2.OpenDevice(i)));
        }

        public void Connect(ConnectModel connect)
        {
            Connect();
        }

        public void Disconnect()
        {
            devices.ForEach(v => zkfp2.CloseDevice(v.handle));
            devices.Clear();
        }

        ZKTECODevice GetParameters(IntPtr handle)
        {
            byte[] paramValue = new byte[4];
            int size = 4;
            int mfpWidth = 0, mfpHeight = 0;
            zkfp2.GetParameters(handle, 1, paramValue, ref size);
            zkfp2.ByteArray2Int(paramValue, ref mfpWidth);

            size = 4;
            zkfp2.GetParameters(handle, 2, paramValue, ref size);
            zkfp2.ByteArray2Int(paramValue, ref mfpHeight);

            return new ZKTECODevice(handle, mfpWidth, mfpHeight);
        }

        protected void JudgeWorkingStatus(WorkingState workingState)
        {
            switch (workingState)
            {
                case WorkingState.CollectFingerprints:
                    throw new ZKTECOException("设备处于收录指纹状态，请先关闭此状态！");
                case WorkingState.FingerprintIdentification:
                    throw new ZKTECOException("设备处于指纹识别状态，请先关闭此状态！");
            }
        }
    }
}
