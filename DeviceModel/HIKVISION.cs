﻿using DeviceService.Model;
using DeviceService.Model.ExceptionModels;
using DeviceService.SDK;
using NaflimHelperLibrary;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DeviceService.DeviceModel
{
    public abstract class HIKVISION : IDevice
    {
        public bool LinkStatus { get { return linkStatus; } }
        ///// <summary>
        ///// 登录ID
        ///// </summary>
        //public int UserID { get { return userID; } }

        /// <summary>
        /// 设备IP
        /// </summary>
        public string Ip { get { return ip; } }

        /// <summary>
        /// 设备端口
        /// </summary>
        public int Port { get { return port; } }

        private CHCNetSDK.NET_DVR_DEVICEINFO_V40 DeviceInfo;

        protected int userID;
        protected string ip;
        protected int port;
        bool linkStatus;

        public Action<HIKVISION> LinkOk { get; set; }

        public void Connect(ConnectModel connect)
        {
            if (!CHCNetSDK.NET_DVR_Init())
                throw new HIKException("初始化失败！");

            CHCNetSDK.NET_DVR_USER_LOGIN_INFO struLogInfo = new CHCNetSDK.NET_DVR_USER_LOGIN_INFO();

            //设备IP地址或者域名
            byte[] byIP = Encoding.Default.GetBytes(connect.Ip);
            struLogInfo.sDeviceAddress = new byte[129];
            byIP.CopyTo(struLogInfo.sDeviceAddress, 0);

            //设备用户名
            byte[] byUserName = Encoding.Default.GetBytes(connect.UserID);
            struLogInfo.sUserName = new byte[64];
            byUserName.CopyTo(struLogInfo.sUserName, 0);

            //设备密码
            byte[] byPassword = Encoding.Default.GetBytes(connect.Pwd);
            struLogInfo.sPassword = new byte[64];
            byPassword.CopyTo(struLogInfo.sPassword, 0);

            struLogInfo.wPort = (ushort)connect.Port;//设备服务端口号

            struLogInfo.cbLoginResult = LoginCallBack;
            struLogInfo.bUseAsynLogin = true; //是否异步登录：0- 否，1- 是 
            DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V40();

            //登录设备 Login the device
            CHCNetSDK.NET_DVR_Login_V40(ref struLogInfo, ref DeviceInfo);
        }

        public void Disconnect()
        {
            if (!CHCNetSDK.NET_DVR_Logout(userID))
                throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());
        }

        /// <summary>
        /// 异步登录回调函数
        /// </summary>
        /// <param name="lUserID">用户ID</param>
        /// <param name="dwResult">登录状态</param>
        /// <param name="lpDeviceInfo">设备信息</param>
        /// <param name="pUser">用户数据</param>
        private void LoginCallBack(int lUserID, int dwResult, IntPtr lpDeviceInfo, IntPtr pUser)
        {
            try
            {
                if (dwResult == 0)
                    throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());
                userID = lUserID;
                linkStatus = true;
                GetDeviceInfo();
                LinkOk?.Invoke(this);
            }
            catch (Exception ex)
            {
                Log.PrintError(ex);
            }
        }

        void GetDeviceInfo()
        {
            CHCNetSDK.NET_DVR_NETCFG_V30 nETCFG_V30 = new CHCNetSDK.NET_DVR_NETCFG_V30();
            int size = Marshal.SizeOf(nETCFG_V30);
            IntPtr ptrNetCfg = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(nETCFG_V30, ptrNetCfg, false);

            uint dwReturn = 0;
            if (!CHCNetSDK.NET_DVR_GetDVRConfig(userID, CHCNetSDK.NET_DVR_GET_NETCFG_V30, -1, ptrNetCfg, (uint)size, ref dwReturn))
                throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());

            nETCFG_V30 = (CHCNetSDK.NET_DVR_NETCFG_V30)Marshal.PtrToStructure(ptrNetCfg, typeof(CHCNetSDK.NET_DVR_NETCFG_V30));
            ip = nETCFG_V30.struEtherNet[0].struDVRIP.sIpV4;
            port = nETCFG_V30.struEtherNet[0].wDVRPort;
        }
    }
}
