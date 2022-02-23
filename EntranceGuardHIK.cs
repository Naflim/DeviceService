using DeviceService.DeviceModel;
using DeviceService.Model.ExceptionModels;
using DeviceService.SDK;
using NaflimHelperLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService
{
    public class EntranceGuardHIK : HIKVISION
    {
        public void StartMonitoring()
        {
            if (!CHCNetSDK.NET_DVR_SetDVRMessageCallBack_V50(0, MSGCallBack, IntPtr.Zero))
                throw new HIKException("事件注册失败");
        }

        void MSGCallBack(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            if (lCommand != CHCNetSDK.COMM_ALARM_ACS)
                return;
            CHCNetSDK.NET_DVR_ACS_ALARM_INFO struAcsAlarmInfo = (CHCNetSDK.NET_DVR_ACS_ALARM_INFO)Marshal.PtrToStructure(pAlarmInfo,
               typeof(CHCNetSDK.NET_DVR_ACS_ALARM_INFO));

            uint employeeID = struAcsAlarmInfo.struAcsEventInfo.dwEmployeeNo;
            if (employeeID == 0)
                return;
        }

        /// <summary>
        /// 获取用户总数
        /// </summary>
        /// <param name="m_UserID">设备句柄</param>
        /// <returns>数量</returns>
        public string GetUserCount()
        {
            IntPtr ptrOutBuf = Marshal.AllocHGlobal(1024);
            IntPtr ptrStatusBuffer = Marshal.AllocHGlobal(1024);
            for (int i = 0; i < 1024; i++)
            {
                Marshal.WriteByte(ptrOutBuf, i, 0);
                Marshal.WriteByte(ptrStatusBuffer, i, 0);
            }

            CHCNetSDK.NET_DVR_XML_CONFIG_INPUT struInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
            CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT struOuput = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();

            string sUrl = "GET /ISAPI/AccessControl/UserInfo/Count?format=json";
            IntPtr ptrURL = Marshal.StringToHGlobalAnsi(sUrl);
            struInput.dwSize = (uint)Marshal.SizeOf(struInput);
            struInput.lpRequestUrl = ptrURL;
            struInput.dwRequestUrlLen = (uint)sUrl.Length;

            IntPtr ptrInput = Marshal.AllocHGlobal(Marshal.SizeOf(struInput));
            Marshal.StructureToPtr(struInput, ptrInput, false);


            struOuput.dwSize = (uint)Marshal.SizeOf(struInput);
            struOuput.lpOutBuffer = ptrOutBuf;
            struOuput.dwOutBufferSize = 1024;

            IntPtr ptrOuput = Marshal.AllocHGlobal(Marshal.SizeOf(struOuput));
            Marshal.StructureToPtr(struOuput, ptrOuput, false);
            if (CHCNetSDK.NET_DVR_STDXMLConfig(userID, ptrInput, ptrOuput))
            {
                string json = Marshal.PtrToStringAnsi(struOuput.lpOutBuffer);
                string count = DataConversion.GetJsonItem(json, new string[] { "UserInfoCount", "userNumber" });
                return count;
            }
            else
                throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());
        }
    }
}
