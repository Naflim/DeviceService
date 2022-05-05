using ArcFaceSDK;
using ArcFaceSDK.Entity;
using ArcFaceSDK.SDKModels;
using ArcFaceSDK.Utils;
using DeviceService.Model;
using DeviceService.Model.ExceptionModels;
using DeviceService.SDK.Arcface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace DeviceService.DeviceModel
{
    public class Arcface
    {
        #region 公开属性
        /// <summary>
        /// 显示异常
        /// </summary>
        public Action<Exception>? ErrorShow { get; set; }

        /// <summary>
        /// 抛出日志
        /// </summary>
        public Action<string>? ThrowLog { get; set; }

        /// <summary>
        /// 显示特征图片
        /// </summary>
        public Action<Image>? FeatureShow { get; set; }

        /// <summary>
        /// 显示比对人脸图片
        /// </summary>
        public Action<Image, int>? CompareShow { get; set; }
        #endregion

        #region 基础参数
        /// <summary>
        /// 图片最大大小限制
        /// </summary>
        protected const long MAX_SIZE = 1024 * 1024 * 2;

        /// <summary>
        /// 最大宽度
        /// </summary>
        protected const int MAX_WIDTH = 1536;

        /// <summary>
        /// 最大高度
        /// </summary>
        protected const int MAX_HEIGHT = 1536;

        /// <summary>
        /// 图像质量注册阈值
        /// </summary>
        protected const float THRESHOLD_IMG_REGISTER = 0.63f;

        /// <summary>
        /// 图像质量识别戴口罩阈值
        /// </summary>
        protected const float THRESHOLD_IMG_MASK = 0.29f;

        /// <summary>
        /// 图像处理引擎对象
        /// </summary>
        protected FaceEngine imageEngine = new();

        /// <summary>
        /// 人脸库人脸特征列表
        /// </summary>
        protected Dictionary<string, FaceFeature> faceDatabase = new();

        /// <summary>
        /// 比对模型
        /// </summary>
        private const ASF_CompareModel COMPARE_MODEL = ASF_CompareModel.ASF_ID_PHOTO;
        #endregion

        /// <summary>
        /// 在线激活
        /// </summary>
        /// <param name="appId">APPID</param>
        /// <param name="sdkKey">SDK对应</param>
        /// <param name="activeKey">付费激活码</param>
        public void OnlineActivation(string appId, string sdkKey, string activeKey)
        {
            int retCode = imageEngine.ASFOnlineActivation(appId, sdkKey, activeKey);
            if (retCode != 0 && retCode != 90114)
                throw ArcfaceException.AbnormalJudgment(retCode);
            InitEngine();
        }

        /// <summary>
        /// 离线激活
        /// </summary>
        /// <param name="offlineActiveFilePath">离线激活文件路径</param>
        public void OfflineActivation(string offlineActiveFilePath)
        {
            int retCode = imageEngine.ASFOfflineActivation(offlineActiveFilePath);
            if (retCode != 0 && retCode != 90114)
                throw ArcfaceException.AbnormalJudgment(retCode);
            InitEngine();
        }

        /// <summary>
        /// 初始化引擎
        /// </summary>
        void InitEngine()
        {
            //初始化引擎
            DetectionMode detectMode = DetectionMode.ASF_DETECT_MODE_IMAGE;
            //Image模式下检测脸部的角度优先值
            ASF_OrientPriority imageDetectFaceOrientPriority = ASF_OrientPriority.ASF_OP_ALL_OUT;
            //最大需要检测的人脸个数
            int detectFaceMaxNum = 6;
            //引擎初始化时需要初始化的检测功能组合
            int combinedMask = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_AGE | FaceEngineMask.ASF_GENDER | FaceEngineMask.ASF_FACE3DANGLE | FaceEngineMask.ASF_IMAGEQUALITY | FaceEngineMask.ASF_MASKDETECT;
            //初始化引擎，正常值为0，其他返回值请参考http://ai.arcsoft.com.cn/bbs/forum.php?mod=viewthread&tid=19&_dsign=dbad527e
            int retCode = imageEngine.ASFInitEngine(detectMode, imageDetectFaceOrientPriority, detectFaceMaxNum, combinedMask);

            if (retCode != 0)
                throw ArcfaceException.AbnormalJudgment(retCode);
        }

        /// <summary>
        /// 注册人脸
        /// </summary>
        /// <param name="id">人脸id</param>
        /// <param name="imgPath">人脸图片路径</param>
        /// <exception cref="ArcfaceException">人脸异常</exception>
        public void RegisteredFace(string id, string imgPath)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("仅支持windows平台使用");

            CheckImage(imgPath);

            var image = ScaleImage(imgPath);
            var feature = GetFaceFeature(ref image);

            faceDatabase.Add(id, feature);

            FeatureShow?.Invoke(image);
            if (image != null)
                image.Dispose();
        }

        /// <summary>
        /// 比对人脸
        /// </summary>
        /// <param name="imgPath">人脸图片路径</param>
        /// <exception cref="ArcfaceException">人脸异常</exception>
        public void CompareFaces(string imgPath)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                 throw new PlatformNotSupportedException("仅支持windows平台使用");

            if (!imageEngine.GetEngineStatus())
                throw new ArcfaceException("请先初始化引擎！");

            if (faceDatabase.Count == 0)
                throw new ArcfaceException("请注册人脸！");

            CheckImage(imgPath);

            var image = ScaleImage(imgPath);
            var feature = GetFaceFeature(ref image);

            float compareSimilarity = 0f;
            string compare = string.Empty;

            foreach (var face in faceDatabase)
            {
                imageEngine.ASFFaceFeatureCompare(feature, face.Value, out float similarity, COMPARE_MODEL);
                //增加异常值处理
                if (similarity.ToString().IndexOf("E") > -1)
                    similarity = 0f;

                ThrowLog?.Invoke(string.Format("与人脸库{0}号比对结果:{1}", face.Key, similarity));
                if (similarity > compareSimilarity)
                {
                    compareSimilarity = similarity;
                    compare = face.Key;
                }
            }
            if (compareSimilarity > 0)
                ThrowLog?.Invoke(string.Format("匹配结果:{0}号,比对结果:{1}", compare, compareSimilarity));

            if (image != null)
                image.Dispose();
        }

        /// <summary>
        /// 检查图片
        /// </summary>
        /// <param name="imagePath">人脸图片路径</param>
        /// <exception cref="ArcfaceException">人脸异常</exception>
        static void CheckImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) throw new ArcfaceException("非法路径");
            FileInfo fileCheck = new(imagePath);
            if (!fileCheck.Exists) throw new ArcfaceException("文件不存在");
            if (fileCheck.Length > MAX_SIZE) throw new ArcfaceException("图片大小超过2M，请压缩后再导入");
        }

        /// <summary>
        /// 调整图片
        /// </summary>
        /// <param name="imagePath">人脸图片路径</param>
        /// <returns>调整后图片</returns>
        /// <exception cref="ArcfaceException">人脸异常</exception>
        static Image ScaleImage(string imagePath)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                 throw new PlatformNotSupportedException("仅支持windows平台使用");

            var image = ImageUtil.ReadFromFile(imagePath);
            if (image == null) throw new ArcfaceException("图片读取失败");

            if (image.Width > MAX_HEIGHT || image.Height > MAX_HEIGHT)
                image = ImageUtil.ScaleImage(image, MAX_WIDTH, MAX_HEIGHT);

            if (image.Width % 4 != 0)
                image = ImageUtil.ScaleImage(image, image.Width - (image.Width % 4), image.Height);
            return image;
        }

        /// <summary>
        /// 获取人脸特征
        /// </summary>
        /// <param name="image">人脸图片</param>
        /// <param name="personnel">人员信息</param>
        /// <returns>人脸特征</returns>
        /// <exception cref="ArcfaceException">人脸异常</exception>
        FaceFeature GetFaceFeature(ref Image image,PersonnelModel? personnel = null)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                 throw new PlatformNotSupportedException("仅支持windows平台使用");

            int retCode = imageEngine.ASFDetectFacesEx(image, out MultiFaceInfo multiFaceInfo);

            if (retCode != 0)
                throw ArcfaceException.AbnormalJudgment(retCode);

            if (multiFaceInfo.faceNum == 0)
                throw new ArcfaceException("未检测出人脸");

            #region 获取人员信息
            if (personnel != null)
            {
                AgeInfo ageInfo = FaceUtil.AgeEstimation(imageEngine, image, multiFaceInfo, out int retCode_Age);
                if (retCode_Age == 0)
                    personnel.Age = ageInfo.ageArray[0];
                else
                {
                    personnel.Age = -1;
                    ThrowLog?.Invoke(ArcfaceException.AbnormalJudgment(retCode_Age).Message);
                }
                GenderInfo genderInfo = FaceUtil.GenderEstimation(imageEngine, image, multiFaceInfo, out int retCode_Gender);
                if (retCode_Gender == 0)
                {
                    personnel.Sex = genderInfo.genderArray[0] switch
                    {
                        0 => PersonnelModel.PersonnelSex.man,
                        1 => PersonnelModel.PersonnelSex.woman,
                        _ => PersonnelModel.PersonnelSex.other,
                    };
                }
                else
                {
                    personnel.Sex = PersonnelModel.PersonnelSex.other;
                    ThrowLog?.Invoke(ArcfaceException.AbnormalJudgment(retCode_Gender).Message);
                }
            }
            #endregion

            string featureResult = string.Empty;

            FaceFeature feature = FaceUtil.ExtractFeature(imageEngine, image, THRESHOLD_IMG_REGISTER, THRESHOLD_IMG_MASK, ASF_RegisterOrNot.ASF_REGISTER, out SingleFaceInfo singleFaceInfo, out bool isMask, ref featureResult, out _);
            if (!string.IsNullOrEmpty(featureResult))
            {
                if (image != null)
                    image.Dispose();
                throw new ArcfaceException(featureResult);
            }

            MRECT rect = multiFaceInfo.faceRects[0];
            image = ImageUtil.CutImage(image, rect.left, rect.top, rect.right, rect.bottom);

            ThrowLog?.Invoke($"已提取人脸特征值，[left:{singleFaceInfo.faceRect.left},right:{singleFaceInfo.faceRect.right}," +
                $"top:{singleFaceInfo.faceRect.top},bottom:{singleFaceInfo.faceRect.bottom}," +
                $"orient:{singleFaceInfo.faceOrient},mask:{isMask}]");


            return feature;
        }
    }
}
