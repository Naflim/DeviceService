using System.Collections.Generic;
using System.ComponentModel;

namespace DeviceService.Model
{
    #region Json:ResponseStatus
    public class CResponseStatus
    {
        public string requestURL { get; set; }
        public int statusCode { get; set; }
        public string statusString { get; set; }
        public string subStatusCode { get; set; }
        public int errorCode { get; set; }
        public string errorMsg { get; set; }
    }
    #endregion

    #region Json:UserInfoSearchCond
    public class CUserInfoSearchCondCfg
    {
        public CUserInfoSearchCond UserInfoSearchCond { get; set; }
    }

    public class CUserInfoSearchCond
    {
        public string searchID { get; set; } //req
        public int searchResultPosition { get; set; } //req
        public int maxResults { get; set; } //req
        public List<CEmployeeNoList> EmployeeNoList { get; set; } //opt
    }

    public class CEmployeeNoList
    {
        public string employeeNo { get; set; } //opt
    }
    #endregion

    #region Json:UserInfoSearch
    public class CUserInfoSearchCfg
    {
        public CUserInfoSearch UserInfoSearch { get; set; }
    }

    public class CUserInfoSearch
    {
        public string searchID { get; set; }
        public string responseStatusStrg { get; set; }
        public int numOfMatches { get; set; }
        public int totalMatches { get; set; }
        public List<CUserInfo> UserInfo { get; set; }
    }

    public class CUserInfo
    {
        public string employeeNo { get; set; }
        public string name { get; set; }
        public string userType { get; set; }
        public bool closeDelayEnabled { get; set; }
        public CValid Valid { get; set; }
        public string belongGroup { get; set; }
        public string password { get; set; }
        public string doorRight { get; set; }
        public List<CRightPlan> RightPlan { get; set; }
        public int maxOpenDoorTime { get; set; }
        public int openDoorTime { get; set; }
        public int roomNumber { get; set; }
        public int floorNumber { get; set; }
        public bool doubleLockRight { get; set; }
        public bool alwaysOpenRight { get; set; }
        public bool localUIRight { get; set; }
        public string userVerifyMode { get; set; }
    }

    public class CValid
    {
        public bool enable { get; set; } //req
        public string beginTime { get; set; } //req
        public string endTime { get; set; } //req
        [DefaultValue(null)]
        public string timeType { get; set; }
    }

    #endregion

    #region Json:UserInfo
    public class CUserInfoCfg
    {
        public CUserInfo UserInfo { get; set; }
    }

    public class CRightPlan
    {
        [DefaultValue(-1)]
        public int doorNo { get; set; }
        [DefaultValue(null)]
        public string planTemplateNo { get; set; }
    }
    #endregion

    #region Json:SearchFaceDataCond
    public class CSearchFaceDataCond
    {
        public int searchResultPosition { get; set; } //req
        public int maxResults { get; set; } //req
        public string faceLibType { get; set; } //req
        public string FDID { get; set; } //req
        public string FPID { get; set; } //opt
    }
    #endregion

    #region Json:SearchFaceDataReturn
    public class CSearchFaceDataReturn
    {
        public string requestURL { get; set; } //opt
        public int statusCode { get; set; } //req
        public string statusString { get; set; } //req
        public string subStatusCode { get; set; } //req
        public int errorCode { get; set; } //opt
        public string errorMsg { get; set; } //opt
        public string responseStatusStrg { get; set; } //opt
        public int numOfMatches { get; set; } //opt
        public int totalMatches { get; set; } //opt
        public List<CMatchList> MatchList { get; set; } //opt
    }

    public class CMatchList
    {
        public string FPID { get; set; } //opt
        public int employeeNo { get; set; } //opt
    }
    #endregion

    #region Json:UserInfoDetail & UserInfoDetailDeleteProcess
    public class CUserInfoDetailCfg
    {
        public CUserInfoDetail UserInfoDetail { get; set; }
    }

    public class CUserInfoDetail
    {
        public string mode { get; set; } //req
        public List<CEmployeeNoList> EmployeeNoList { get; set; } //opt
    }

    public class CUserInfoDetailDeleteProcessCfg
    {
        public CUserInfoDetailDeleteProcess UserInfoDetailDeleteProcess { get; set; }
    }

    public class CUserInfoDetailDeleteProcess
    {
        public string status { get; set; } //req
    }

    #endregion

    public class CSetFaceDataCond
    {
        public string faceLibType { get; set; } //req
        public string FDID { get; set; } //req
        public string FPID { get; set; } //opt
        public bool deleteFP { get; set; } //opt
    }

    public class CFaceRecordDelete
    {
        public List<CFPID> FPID { get; set; }
    }

    public class CFPID
    {
        public string value { get; set; }
    }

    public class CCardInfoCfg
    {
        public CCardInfo CardInfo { get; set; }
    }

    public class CCardInfo
    {
        public string employeeNo { get; set; } //req
        public string cardNo { get; set; } //req
        public bool deleteCard { get; set; } //opt
        public string cardType { get; set; } //req
    }

    public class CCardInfoSearchCondCfg
    {
        public CCardInfoSearchCond CardInfoSearchCond { get; set; }
    }

    public class CCardInfoSearchCond
    {
        public string searchID { get; set; } //req
        [DefaultValue(-1)]
        public int searchResultPosition { get; set; } //req
        public int maxResults { get; set; } //req
        public List<CEmployeeNoList> EmployeeNoList { get; set; } //opt
        public List<CCardNoList> CardNoList { get; set; } //opt
    }

    public class CCardNoList
    {
        public string cardNo { get; set; } //opt
    }

    public class CCardInfoSearchCfg
    {
        public CCardInfoSearch CardInfoSearch { get; set; }
    }

    public class CCardInfoSearch
    {
        public string searchID { get; set; }
        public string responseStatusStrg { get; set; }
        public int numOfMatches { get; set; }
        public int totalMatches { get; set; }
        public List<CCardInfo> CardInfo { get; set; }
    }

    public class CCardInfoDelCondCfg
    {
        public CCardInfoDelCond CardInfoDelCond { get; set; }
    }

    public class CCardInfoDelCond
    {
        public List<CEmployeeNoList> EmployeeNoList { get; set; } //opt
        public List<CCardNoList> CardNoList { get; set; } //opt
    }

    public class CFingerPrintCondCfg
    {
        public CFingerPrintCond FingerPrintCond { get; set; }
    }

    public class CFingerPrintCond
    {
        public string searchID { get; set; } //req
        public string employeeNo { get; set; } //req
        public int cardReaderNo { get; set; } //opt
        public int fingerPrintID { get; set; } //opt
    }

    public class CFingerPrintInfoCfg
    {
        public CFingerPrintInfo FingerPrintInfo { get; set; }
    }

    public class CFingerPrintInfo
    {
        public string searchID { get; set; } //req
        public string status { get; set; } //req
        public List<CFingerPrintList> FingerPrintList { get; set; } //opt
    }

    public class CFingerPrintList
    {
        public int cardReaderNo { get; set; } //req
        public int fingerPrintID { get; set; } //req
        public string fingerType { get; set; } //req
        public string fingerData { get; set; } //req
        public int[] leaderFP { get; set; } //opt
    }

    public class CFingerPrintCfgCfg
    {
        public CFingerPrintCfg FingerPrintCfg { get; set; }
    }

    public class CFingerPrintCfg
    {
        public string employeeNo { get; set; } //req
        public int[] enableCardReader { get; set; } //req
        public int fingerPrintID { get; set; } //req
        public bool deleteFingerPrint { get; set; } //opt
        public string fingerType { get; set; } //req
        public string fingerData { get; set; } //opt
        public int[] leaderFP { get; set; } //opt
        public bool checkEmployeeNo { get; set; } //opt
    }

    public class CFingerPrintStatusCfg
    {
        public CFingerPrintStatus FingerPrintStatus { get; set; }
    }

    public class CFingerPrintStatus
    {
        public string status { get; set; } //opt
        public List<CStatusList> StatusList { get; set; } //opt
    }

    public class CStatusList
    {
        public int id { get; set; } //opt
        public int cardReaderRecvStatus { get; set; } //opt,指纹读卡器状态：0-失败，1-成功，2-该指纹模组不在线，3-重试或指纹质量差，4-内存已满，5-已存在该指纹，6-已存在该指纹ID，7-非法指纹ID，8-该指纹模组无需配置，10-指纹读卡器版本过低（无法支持工号）
        public string errorMsg { get; set; } //opt
    }
}