using DeviceService.DeviceModel;
using DeviceService.Model;
using DeviceService.Model.ExceptionModels;
using DeviceService.SDK;
using NaflimHelperLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Product
{
    public class EntranceGuardHIK : HIKVISION
    {
        public EntranceGuardHIK()
        {

        }

        EntranceGuardHIK(int userID)
        {
            this.userID = userID;
        }

        /// <summary>
        /// 单次执行数量
        /// </summary>
        public int Batch { get; set; } = 100;

        /// <summary>
        /// 门禁触发事件
        /// </summary>
        static Action<string, PersonnelModel, DateTime>? MonitoringEvent;

        static CHCNetSDK.MSGCallBack? callBack;

        /// <summary>
        /// 设置报警回调函数
        /// </summary>
        /// <param name="action">回调函数</param>
        /// <exception cref="HIKException">海康SDK内部错误</exception>
        public static void SetCallBack(Action<string, PersonnelModel, DateTime> action)
        {
            MonitoringEvent = action;
            callBack = MSGCallBack;

            if (!CHCNetSDK.NET_DVR_SetDVRMessageCallBack_V30(callBack, IntPtr.Zero))
                throw new HIKException("事件注册失败");
        }

        /// <summary>
        /// 设备布防
        /// </summary>
        /// <param name="guardHIK">门禁机</param>
        /// <param name="action">回调函数</param>
        public static void StartMonitoring(EntranceGuardHIK guardHIK, Action<string, PersonnelModel, DateTime>? action = null)
        {
            CHCNetSDK.NET_DVR_SETUPALARM_PARAM struSetupAlarmParam = new CHCNetSDK.NET_DVR_SETUPALARM_PARAM();
            struSetupAlarmParam.dwSize = (uint)Marshal.SizeOf(struSetupAlarmParam);
            struSetupAlarmParam.byLevel = 1;
            struSetupAlarmParam.byAlarmInfoType = 1;
            struSetupAlarmParam.byDeployType = 1;
            if (CHCNetSDK.NET_DVR_SetupAlarmChan_V41(guardHIK.userID, ref struSetupAlarmParam) < 0)
                throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());

            if (action != null)
                SetCallBack(action);
        }

        static void MSGCallBack(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            if (lCommand != CHCNetSDK.COMM_ALARM_ACS)
                return;
#pragma warning disable CS8605 // 取消装箱可能为 null 的值。
            CHCNetSDK.NET_DVR_ACS_ALARM_INFO struAcsAlarmInfo = (CHCNetSDK.NET_DVR_ACS_ALARM_INFO)Marshal.PtrToStructure(pAlarmInfo,
               typeof(CHCNetSDK.NET_DVR_ACS_ALARM_INFO));
#pragma warning restore CS8605 // 取消装箱可能为 null 的值。


            uint employeeID = struAcsAlarmInfo.struAcsEventInfo.dwEmployeeNo;
            if (employeeID == 0)
                return;

            PersonnelModel personnel = new PersonnelModel();
            var struTime = struAcsAlarmInfo.struTime;

            DateTime date = Convert.ToDateTime($"{struTime.dwYear}-{struTime.dwMonth}-{struTime.dwDay} {struTime.dwHour}:{struTime.dwMinute}:{struTime.dwSecond}");
            personnel.CardID = Encoding.UTF8.GetString(struAcsAlarmInfo.struAcsEventInfo.byCardNo).TrimEnd('\0');
            string ip = Encoding.Default.GetString(pAlarmer.sDeviceIP).Replace("\0", "");
            List<CUserInfo> user = new EntranceGuardHIK(pAlarmer.lUserID).GetUserInfos(new List<string> { employeeID.ToString() });
            if (user != null && user.Count > 0)
            {
                personnel.Name = user[0].name;
                personnel.DoorRight = user[0].doorRight;
            }

            personnel.EmployeeID = employeeID;

            MonitoringEvent?.Invoke(ip, personnel, date);
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
                if (Marshal.PtrToStringAnsi(struOuput.lpOutBuffer) is not string json) return string.Empty;

                string count = DataConversion.GetJsonItem(json, new string[] { "UserInfoCount", "userNumber" });
                return count;
            }
            else
                throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());
        }

        /// <summary>
        /// 获取全部用户信息
        /// </summary>
        /// <param name="count">请求大小</param>
        /// <returns>用户列表</returns>
        public List<CUserInfo> GetUserInfos(int count)
        {
            List<CUserInfo> userInfos = new List<CUserInfo>();

            string sURL = "POST /ISAPI/AccessControl/UserInfo/Search?format=json";
            IntPtr ptrURL = Marshal.StringToHGlobalAnsi(sURL);
            int m_lSetUserCfgHandle = CHCNetSDK.NET_DVR_StartRemoteConfig(userID, CHCNetSDK.NET_DVR_JSON_CONFIG, ptrURL, sURL.Length, null, IntPtr.Zero);

            if (m_lSetUserCfgHandle < 0)
                throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());

            int endNum = count / Batch;

            CUserInfoSearchCondCfg JsonUserInfoSearchCondCfg = new CUserInfoSearchCondCfg();
            JsonUserInfoSearchCondCfg.UserInfoSearchCond = new CUserInfoSearchCond();
            JsonUserInfoSearchCondCfg.UserInfoSearchCond.searchID = userID.ToString();
            JsonUserInfoSearchCondCfg.UserInfoSearchCond.maxResults = 100;

            for (int i = 0; i < endNum + 1; i++)
            {
                JsonUserInfoSearchCondCfg.UserInfoSearchCond.searchResultPosition = i * Batch;

                string strUserInfoSearchCondCfg = TypeConversion.ObjectToJson(JsonUserInfoSearchCondCfg);
                IntPtr ptrUserInfoSearchCondCfg = Marshal.StringToHGlobalAnsi(strUserInfoSearchCondCfg);
                userInfos.AddRange(SendWithRecvRemoteConfig(m_lSetUserCfgHandle, ptrUserInfoSearchCondCfg, (uint)strUserInfoSearchCondCfg.Length));
            }

            return userInfos;
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="lUserID">设备句柄</param>
        /// <param name="EmployeeNoList">工号列表</param>
        /// <returns>用户列表</returns>
        public List<CUserInfo> GetUserInfos(List<string> EmployeeNoList)
        {
            int count = EmployeeNoList.Count;

            string sURL = "POST /ISAPI/AccessControl/UserInfo/Search?format=json";
            IntPtr ptrURL = Marshal.StringToHGlobalAnsi(sURL);
            int m_lSetUserCfgHandle = CHCNetSDK.NET_DVR_StartRemoteConfig(userID, CHCNetSDK.NET_DVR_JSON_CONFIG, ptrURL, sURL.Length, null, IntPtr.Zero);

            if (m_lSetUserCfgHandle < 0)
                throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());

            int endNum = count / Batch;

            CUserInfoSearchCondCfg JsonUserInfoSearchCondCfg = new CUserInfoSearchCondCfg();
            JsonUserInfoSearchCondCfg.UserInfoSearchCond = new CUserInfoSearchCond();
            JsonUserInfoSearchCondCfg.UserInfoSearchCond.searchID = userID.ToString();
            JsonUserInfoSearchCondCfg.UserInfoSearchCond.searchResultPosition = 0;
            JsonUserInfoSearchCondCfg.UserInfoSearchCond.maxResults = count;
            JsonUserInfoSearchCondCfg.UserInfoSearchCond.EmployeeNoList = new List<CEmployeeNoList>();
            EmployeeNoList.ForEach(v =>
            {
                CEmployeeNoList singleEmployeeNoList = new CEmployeeNoList();
                singleEmployeeNoList.employeeNo = v;
                JsonUserInfoSearchCondCfg.UserInfoSearchCond.EmployeeNoList.Add(singleEmployeeNoList);
            });

            string strUserInfoSearchCondCfg = TypeConversion.ObjectToJson(JsonUserInfoSearchCondCfg);
            IntPtr ptrUserInfoSearchCondCfg = Marshal.StringToHGlobalAnsi(strUserInfoSearchCondCfg);
            return SendWithRecvRemoteConfig(m_lSetUserCfgHandle, ptrUserInfoSearchCondCfg, (uint)strUserInfoSearchCondCfg.Length);
        }

        /// <summary>
        /// 设置门权限
        /// </summary>
        /// <param name="employeeNo">工号</param>
        /// <param name="flag">是否可开门</param>
        public void SetDoorFlag(string employeeNo, bool flag)
        {
            string sURL = "PUT /ISAPI/AccessControl/UserInfo/SetUp?format=json";
            IntPtr ptrURL = Marshal.StringToHGlobalAnsi(sURL);
            int m_lSetUserCfgHandle = CHCNetSDK.NET_DVR_StartRemoteConfig(userID, CHCNetSDK.NET_DVR_JSON_CONFIG, ptrURL, sURL.Length, null, IntPtr.Zero);

            if (m_lSetUserCfgHandle < 0)
                throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());

            CUserInfoCfg JsonUserInfo = new CUserInfoCfg();
            JsonUserInfo.UserInfo = new CUserInfo();
            JsonUserInfo.UserInfo.employeeNo = employeeNo;
            JsonUserInfo.UserInfo.userType = flag ? "normal" : "blackList";

            string strJsonUserInfo = JsonConvert.SerializeObject(JsonUserInfo, Formatting.Indented,
                                                        new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });

            byte[] byJsonUserInfo = Encoding.UTF8.GetBytes(strJsonUserInfo);
            IntPtr ptrJsonUserInfo = Marshal.AllocHGlobal(byJsonUserInfo.Length);
            Marshal.Copy(byJsonUserInfo, 0, ptrJsonUserInfo, byJsonUserInfo.Length);

            IntPtr ptrJsonData = Marshal.AllocHGlobal(1024);
            for (int i = 0; i < 1024; i++)
                Marshal.WriteByte(ptrJsonData, i, 0);

            uint dwReturned = 0;
            int dwState = CHCNetSDK.NET_DVR_SendWithRecvRemoteConfig(m_lSetUserCfgHandle, ptrJsonUserInfo, (uint)byJsonUserInfo.Length, ptrJsonData, 1024, ref dwReturned);
            if (Marshal.PtrToStringAnsi(ptrJsonData) is not string strJsonData) return;

            switch (dwState)
            {
                case (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_SUCCESS:
                    if (JsonConvert.DeserializeObject<CResponseStatus>(strJsonData) is not CResponseStatus JsonResponseStatus) break;

                    if (JsonResponseStatus.statusCode == 1)
                        ThrowLog?.Invoke("设置成功！");
                    else
                        ThrowLog?.Invoke($"设置失败！状态码：{JsonResponseStatus.statusCode}");
                    break;
                case (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_NEEDWAIT:
                    SetDoorFlag(employeeNo, flag);
                    break;
                case (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_FINISH:
                    ThrowLog?.Invoke("设置完成！");
                    break;
                case (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_FAILED:
                    ThrowLog?.Invoke(HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError()).Message);
                    SetDoorFlag(employeeNo, flag);
                    break;
                case (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_EXCEPTION:
                    CHCNetSDK.NET_DVR_StopRemoteConfig(m_lSetUserCfgHandle);
                    throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());
                default:
                    CHCNetSDK.NET_DVR_StopRemoteConfig(m_lSetUserCfgHandle);
                    throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());
            }
        }

        /// <summary>
        /// 发送json透传
        /// </summary>
        /// <param name="m_lSetUserCfgHandle">NET_DVR_StartRemoteConfig的返回值 </param>
        /// <param name="ptrUserInfoSearchCondCfg">请求数据</param>
        /// <param name="CfgSize">请求数据大小/param>
        /// <returns>用户列表</returns>
        List<CUserInfo> SendWithRecvRemoteConfig(int m_lSetUserCfgHandle, IntPtr ptrUserInfoSearchCondCfg, uint CfgSize)
        {
            try
            {
                List<CUserInfo> userInfos = new();
                IntPtr ptrJsonData = Marshal.AllocHGlobal(1024 * Batch);
                for (int i = 0; i < 1024; i++)
                    Marshal.WriteByte(ptrJsonData, i, 0);
                uint dwReturned = 0;
                int dwState;

                dwState = CHCNetSDK.NET_DVR_SendWithRecvRemoteConfig(m_lSetUserCfgHandle, ptrUserInfoSearchCondCfg, CfgSize, ptrJsonData, (uint)(1024 * Batch), ref dwReturned);

                switch (dwState)
                {
                    case (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_SUCCESS:
                        byte[] bUserInfoSearch = new byte[1024 * 10];
                        Marshal.Copy(ptrJsonData, bUserInfoSearch, 0, (int)dwReturned);
                        string strUserInfoSearch = Encoding.UTF8.GetString(bUserInfoSearch);

                        CUserInfoSearchCfg JsonUserInfoSearchCfg = new CUserInfoSearchCfg();
                        JsonUserInfoSearchCfg = TypeConversion.JsonToObject<CUserInfoSearchCfg>(strUserInfoSearch);
                        if (JsonUserInfoSearchCfg.UserInfoSearch == null)
                        {
                            //null说明返回的Json报文不是UserInfoSearch，而是ResponseStatus
                            CResponseStatus JsonResponseStatus = new CResponseStatus();
                            JsonResponseStatus = TypeConversion.JsonToObject<CResponseStatus>(strUserInfoSearch);
                            if (JsonResponseStatus.statusCode == 1)
                                ThrowLog?.Invoke("获取用户成功");
                            else
                                ThrowLog?.Invoke($"获取用户失败，{JsonResponseStatus.errorMsg}");
                        }
                        else
                        {
                            //解析UserInfoSearch报文
                            if (JsonUserInfoSearchCfg.UserInfoSearch.totalMatches == 0)
                                ThrowLog?.Invoke("没有符合条件的记录！");
                            else
                                userInfos = JsonUserInfoSearchCfg.UserInfoSearch.UserInfo;
                        }
                        break;
                    case (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_NEEDWAIT:
                        return SendWithRecvRemoteConfig(m_lSetUserCfgHandle, ptrUserInfoSearchCondCfg, CfgSize);
                    case (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_FINISH:
                        ThrowLog?.Invoke("获取完成！");
                        break;
                    case (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_FAILED:
                        ThrowLog?.Invoke(HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError()).Message);
                        return SendWithRecvRemoteConfig(m_lSetUserCfgHandle, ptrUserInfoSearchCondCfg, CfgSize);
                    case (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_EXCEPTION:
                        CHCNetSDK.NET_DVR_StopRemoteConfig(m_lSetUserCfgHandle);
                        throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());
                    default:
                        CHCNetSDK.NET_DVR_StopRemoteConfig(m_lSetUserCfgHandle);
                        throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());
                }
                return userInfos;
            }
            finally
            {
                CHCNetSDK.NET_DVR_StopRemoteConfig(m_lSetUserCfgHandle);
            }
        }

        /// <summary>
        /// 获取人脸信息
        /// </summary>
        /// <param name="lUserID">设备句柄</param>
        /// <param name="count">请求大小</param>
        /// <param name="EmployeeNo">工号</param>
        /// <returns>base64图片信息</returns>
        public string GetFaceInfos(string EmployeeNo)
        {
            string sURL = "POST /ISAPI/Intelligent/FDLib/FDSearch?format=json";
            IntPtr ptrURL = Marshal.StringToHGlobalAnsi(sURL);
            int m_lSetUserCfgHandle = CHCNetSDK.NET_DVR_StartRemoteConfig(userID, CHCNetSDK.NET_DVR_FACE_DATA_SEARCH, ptrURL, sURL.Length, null, IntPtr.Zero);

            if (m_lSetUserCfgHandle < 0)
                throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());

            CSearchFaceDataCond JsonSearchFaceDataCond = new CSearchFaceDataCond();
            JsonSearchFaceDataCond.searchResultPosition = 0;
            JsonSearchFaceDataCond.maxResults = 1;
            JsonSearchFaceDataCond.faceLibType = "blackFD";
            //人脸库ID
            JsonSearchFaceDataCond.FDID = "1";
            //人脸记录ID
            JsonSearchFaceDataCond.FPID = EmployeeNo;

            string strSearchFaceDataCond = TypeConversion.ObjectToJson(JsonSearchFaceDataCond);
            IntPtr ptrSearchFaceDataCond = Marshal.StringToHGlobalAnsi(strSearchFaceDataCond);
            return SendWithFaceRemoteConfig(m_lSetUserCfgHandle, ptrSearchFaceDataCond, (uint)strSearchFaceDataCond.Length);
        }

        /// <summary>
        /// 发送json透传
        /// </summary>
        /// <param name="m_lGetFaceCfgHandle">NET_DVR_StartRemoteConfig的返回值</param>
        /// <param name="ptrJsonSearchFaceDataCond">请求数据</param>
        /// <param name="CfgSize">请求数据大小</param>
        /// <returns>base64图片信息</returns>
        string SendWithFaceRemoteConfig(int m_lGetFaceCfgHandle, IntPtr ptrJsonSearchFaceDataCond, uint CfgSize)
        {
            CHCNetSDK.NET_DVR_JSON_DATA_CFG struJsonDataCfg = new CHCNetSDK.NET_DVR_JSON_DATA_CFG();
            struJsonDataCfg.dwSize = (uint)Marshal.SizeOf(struJsonDataCfg);
            IntPtr ptrJsonDataCfg = Marshal.AllocHGlobal((int)struJsonDataCfg.dwSize);
            Marshal.StructureToPtr(struJsonDataCfg, ptrJsonDataCfg, false);
            uint dwReturned = 0;
            try
            {
                int dwState = CHCNetSDK.NET_DVR_SendWithRecvRemoteConfig(m_lGetFaceCfgHandle, ptrJsonSearchFaceDataCond, CfgSize, ptrJsonDataCfg, struJsonDataCfg.dwSize, ref dwReturned); ;
                switch (dwState)
                {
                    case CHCNetSDK.NET_SDK_GET_NEXT_STATUS_SUCCESS:
                        return ProcessFaceData(ptrJsonDataCfg);
                    case CHCNetSDK.NET_SDK_GET_NEXT_STATUS_FAILED:
                        throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());
                    default:
                        throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());
                }
            }
            finally
            {
                if (!CHCNetSDK.NET_DVR_StopRemoteConfig(m_lGetFaceCfgHandle))
                    throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());
                Marshal.FreeHGlobal(ptrJsonSearchFaceDataCond);
                Marshal.FreeHGlobal(ptrJsonDataCfg);
            }
        }

        /// <summary>
        /// 人脸图片转base64
        /// </summary>
        /// <param name="lpBuffer">人脸信息句柄</param>
        /// <returns>base64字符值</returns>
        private string ProcessFaceData(IntPtr lpBuffer)
        {
#pragma warning disable CS8605 // 取消装箱可能为 null 的值。
            CHCNetSDK.NET_DVR_JSON_DATA_CFG m_struJsonDataCfg = (CHCNetSDK.NET_DVR_JSON_DATA_CFG)Marshal.PtrToStructure(lpBuffer, typeof(CHCNetSDK.NET_DVR_JSON_DATA_CFG));
#pragma warning restore CS8605 // 取消装箱可能为 null 的值。
            string strSearchFaceDataReturn = Marshal.PtrToStringAnsi(m_struJsonDataCfg.lpJsonData, (int)m_struJsonDataCfg.dwJsonDataSize);
            CSearchFaceDataReturn m_JsonSearchFaceDataReturn = TypeConversion.JsonToObject<CSearchFaceDataReturn>(strSearchFaceDataReturn);
            if (m_JsonSearchFaceDataReturn.totalMatches == 0)
            {
                ThrowLog?.Invoke("无人脸数据");
                return string.Empty;
            }

            int size = (int)m_struJsonDataCfg.dwPicDataSize;
            byte[] buffer = new byte[size];
            Marshal.Copy(m_struJsonDataCfg.lpPicData, buffer, 0, size);
            return Convert.ToBase64String(buffer);
        }
    }
}
