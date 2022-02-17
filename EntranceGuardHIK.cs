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

            //var struTime = struAcsAlarmInfo.struTime;
            //string date = $"{struTime.dwYear}-{struTime.dwMonth}-{struTime.dwDay} {struTime.dwHour}:{struTime.dwMinute}:{struTime.dwSecond}";
            //string card = System.Text.Encoding.UTF8.GetString(struAcsAlarmInfo.struAcsEventInfo.byCardNo).TrimEnd('\0');
            //DataRow row = userInfo.NewRow();
            //string name = null;
            //List<CUserInfo> user = GetUserInfos(pAlarmer.lUserID, 1, new List<string> { employeeID.ToString() });
            //if (user != null && user.Count > 0)
            //    name = user[0].name;
            //row["设备"] = lUserIDs[pAlarmer.lUserID];
            //row["工号"] = employeeID;
            //row["姓名"] = name;
            //row["卡号"] = card;
            //row["触发时间"] = date;
            //userInfo.Rows.Add(row);
            //RefreshTable();

            //string eqName = System.Text.RegularExpressions.Regex.Replace(lUserIDs[pAlarmer.lUserID], @"[^0-9]+", "");

            //MySQLHelper mySQL = new MySQLHelper(ConfigurationManager.ConnectionStrings["warehouseDB"].ConnectionString);
            //string sql = "insert into t_AC_info(eq_name,tgr_time,user_name,job_ID,card_ID) values(@eq_name,@time, @userName, @userID,@cardID)";
            //MySqlParameter[] parameters = {
            //                new MySqlParameter("eq_name",eqName),
            //                new MySqlParameter("time",date),
            //                new MySqlParameter("userName",name),
            //                new MySqlParameter("userID",employeeID),
            //                new MySqlParameter("cardID",card)
            //};
            //if (mySQL.GetExecuteNonQuery(sql, parameters))
            //    Invoke(new Action(() => FrmLog("上传成功！")));
            //else
            //    Invoke(new Action(() => FrmLog("上传失败！")));
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
