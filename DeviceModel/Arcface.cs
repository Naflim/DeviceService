using ArcFaceSDK;
using DeviceService.Model.ExceptionModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.DeviceModel
{
    public class Arcface
    {
        /// <summary>
        /// 图像处理引擎对象
        /// </summary>
        protected FaceEngine imageEngine = new();

        /// <summary>
        /// 在线激活
        /// </summary>
        public void OnlineActivation(string appId,string sdkKey,string activeKey)
        {
            int activeFlag = imageEngine.ASFOnlineActivation(appId, sdkKey, activeKey);
            if (activeFlag != 0 && activeFlag != 90114)
                throw ArcfaceException.AbnormalJudgment(activeFlag);
        }

        public void OfflineActivation(string offlineActiveFilePath)
        {
            int activeFlag = imageEngine.ASFOfflineActivation(offlineActiveFilePath);
            if (activeFlag != 0 && activeFlag != 90114)
                throw ArcfaceException.AbnormalJudgment(activeFlag);
        }
    }
}
