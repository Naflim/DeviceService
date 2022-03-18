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
        /// 熵基科技设备
        /// </summary>
        protected struct ZKTECODevice
        {
            public IntPtr handle;
            public int imgSize;
            public ZKTECODevice(IntPtr handle)
            {
                this.handle = handle;
                imgSize = 0;
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
                devices.Add(new ZKTECODevice(zkfp2.OpenDevice(i)));

            GetParameters();
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

        void GetParameters()
        {
            byte[] paramValue = new byte[4];
            int size = 4;
            devices.ForEach(v =>
            {
                int mfpWidth = 0, mfpHeight = 0;
                zkfp2.GetParameters(v.handle, 1, paramValue, ref size);
                zkfp2.ByteArray2Int(paramValue, ref mfpWidth);

                size = 4;
                zkfp2.GetParameters(v.handle, 2, paramValue, ref size);
                zkfp2.ByteArray2Int(paramValue, ref mfpHeight);

                v.imgSize = mfpWidth * mfpHeight;
            });

            
        }
    }
}
