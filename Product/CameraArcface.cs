using AForge.Controls;
using AForge.Video.DirectShow;
using ArcFaceSDK;
using ArcFaceSDK.Entity;
using ArcFaceSDK.SDKModels;
using DeviceService.DeviceModel;
using DeviceService.Model.ExceptionModels;
using DeviceService.SDK.Arcface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace DeviceService
{
    public enum IdentifyResults { None, NotLiving, NotIdentify, Ok }
    public class CameraArcface : Arcface
    {
        #region 公开属性
        /// <summary>
        /// 最大人脸检测数
        /// </summary>
        public int DetectFaceMaxNum { get; set; } = 6;

        /// <summary>
        /// FR失败重试次数
        /// </summary>
        public int FrMatchTime { get; set; } = 100;

        /// <summary>
        /// 活体检测失败重试次数
        /// </summary>
        public int LiveMatchTime { get; set; } = 100;
        #endregion

        #region 私有变量
        /// <summary>
        /// 摄像头设备
        /// </summary>
        VideoCaptureDevice videoCapture;

        /// <summary>
        /// 比对模型
        /// </summary>
        const ASF_CompareModel COMPARE_MODEL = ASF_CompareModel.ASF_ID_PHOTO;

        /// <summary>
        /// 人脸比对阈值
        /// </summary>
        const float THRESHOLD = 0.8f;

        /// <summary>
        /// 可见光（RGB）活体阈值
        /// </summary>
        const float THRESHOLD_RGB = 0.5f;

        /// <summary>
        /// RGB视频帧图像
        /// </summary>
        Bitmap rgbVideoBitmap;

        /// <summary>
        /// 视频引擎对象
        /// </summary>
        readonly FaceEngine videoEngine = new FaceEngine();

        /// <summary>
        /// rgb专用FR引擎
        /// </summary>
        readonly FaceEngine rgbVideoEngine = new FaceEngine();

        /// <summary>
        /// 摄像头视频人脸追踪检测结果
        /// </summary>
        readonly DictionaryUnit<int, FaceTrackUnit> rgbTrackUnitDict = new DictionaryUnit<int, FaceTrackUnit>();

        /// <summary>
        /// RGB 特征搜索尝试次数字典
        /// </summary>
        readonly DictionaryUnit<int, int> rgbFeatureTryDict = new DictionaryUnit<int, int>();

        /// <summary>
        /// RGB 活体检测尝试次数字典
        /// </summary>
        readonly DictionaryUnit<int, int> rgbLivenessTryDict = new DictionaryUnit<int, int>();
        bool canRGBLiveness;
        bool canRGBFR;
        #endregion

        #region 初始化
        public CameraArcface()
        {
            Init();
        }

        protected override void InitEngine()
        {
            base.InitEngine();

            int retCode;
            //初始化引擎
            DetectionMode detectMode = DetectionMode.ASF_DETECT_MODE_IMAGE;

            //初始化视频模式下人脸检测引擎
            DetectionMode detectModeVideo = DetectionMode.ASF_DETECT_MODE_VIDEO;

            //Video模式下检测脸部的角度优先值
            ASF_OrientPriority videoDetectFaceOrientPriority = ASF_OrientPriority.ASF_OP_ALL_OUT;
            //引擎初始化时需要初始化的检测功能组合
            int combinedMaskVideo = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_FACELANDMARK;
            retCode = videoEngine.ASFInitEngine(detectModeVideo, videoDetectFaceOrientPriority, DetectFaceMaxNum, combinedMaskVideo);
            if (retCode != 0)
                throw ArcfaceException.AbnormalJudgment(retCode);

            int combinedMask = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_LIVENESS | FaceEngineMask.ASF_MASKDETECT;
            retCode = rgbVideoEngine.ASFInitEngine(detectMode, videoDetectFaceOrientPriority, DetectFaceMaxNum, combinedMask);

            if (retCode != 0)
                throw ArcfaceException.AbnormalJudgment(retCode);

            rgbVideoEngine.ASFSetLivenessParam(THRESHOLD_RGB);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        void Init()
        {
            FilterInfoCollection filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (filterInfoCollection.Count == 0)
                throw new DriveNotFoundException("未检测到摄像头，请确保已安装摄像头或驱动!");

            videoCapture = new VideoCaptureDevice(filterInfoCollection[0].MonikerString);
        }
        #endregion

        #region 外部方法
        /// <summary>
        /// 开启服务
        /// </summary>
        /// <param name="sourcePlayer">视频播放控件</param>
        public void StartServer(VideoSourcePlayer sourcePlayer)
        {

            sourcePlayer.VideoSource = videoCapture;
            videoCapture.Start();
            VideoRGBLiveness();
            VideoRGBFR();
        }

        /// <summary>
        /// 位图判断
        /// </summary>
        /// <param name="bitmap">位图</param>
        /// <param name="faceTrackUnit">结果</param>
        /// <returns></returns>
        public IdentifyResults RGBBitmapDetection(Bitmap bitmap, out FaceTrackUnit faceTrackUnit)
        {
            Bitmap bitmapClone = null;
            IdentifyResults results = IdentifyResults.None;
            try
            {
                faceTrackUnit = null;
                if (bitmap == null) return results;
                rgbVideoBitmap = bitmap;
                bitmapClone = (Bitmap)bitmap.Clone();
                //检测人脸，得到Rect框
                MultiFaceInfo multiFaceInfo = FaceUtil.DetectFaceAndLandMark(videoEngine, bitmapClone);
                //未检测到人脸
                if (multiFaceInfo.faceNum <= 0)
                {
                    rgbTrackUnitDict.ClearAllElement();
                    return results;
                }

                List<int> tempIdList = new List<int>();
                for (int faceIndex = 0; faceIndex < multiFaceInfo.faceNum; faceIndex++)
                {
                    MRECT mrect = multiFaceInfo.faceRects[faceIndex];
                    int faceId = multiFaceInfo.faceID[faceIndex];
                    if (faceId >= 0)
                    {
                        if (!rgbLivenessTryDict.ContainsKey(faceId))
                            rgbLivenessTryDict.AddDictionaryElement(faceId, 0);
                        else
                        {
                            if (rgbLivenessTryDict.GetElementByKey(faceId) == LiveMatchTime)
                                results = IdentifyResults.NotLiving;
                        }

                        if (!rgbFeatureTryDict.ContainsKey(faceId))
                            rgbFeatureTryDict.AddDictionaryElement(faceId, 0);
                        else
                        {
                            if (rgbFeatureTryDict.GetElementByKey(faceId) == FrMatchTime)
                                results = IdentifyResults.NotIdentify;
                        }

                        if (rgbTrackUnitDict.ContainsKey(faceId))
                        {
                            var rgbTrackUnit = rgbTrackUnitDict.GetElementByKey(faceId);
                            rgbTrackUnit.Rect = mrect;
                            rgbTrackUnit.FaceOrient = multiFaceInfo.faceOrients[faceIndex];
                            rgbTrackUnit.FaceDataInfo = multiFaceInfo.faceDataInfoList[faceIndex];
                            faceTrackUnit = rgbTrackUnitDict.GetElementByKey(faceId);
                            if (!string.IsNullOrEmpty(rgbTrackUnit.FaceLocal) && rgbTrackUnit.RgbLive == Liveness.Reality)
                                results = IdentifyResults.Ok;
                        }
                        else
                            rgbTrackUnitDict.AddDictionaryElement(faceId, new FaceTrackUnit(faceId, mrect, multiFaceInfo.faceOrients[faceIndex], multiFaceInfo.faceDataInfoList[faceIndex]));

                        tempIdList.Add(faceId);
                    }
                }

                //初始化及刷新待处理队列,移除出框的人脸
                //初始化及刷新待处理队列,移除出框的人脸
                rgbFeatureTryDict.RefershElements(tempIdList);
                rgbLivenessTryDict.RefershElements(tempIdList);
                rgbTrackUnitDict.RefershElements(tempIdList);

                return results;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (bitmapClone != null)
                    bitmapClone.Dispose();
            }
        }

        /// <summary>
        /// 关闭服务
        /// </summary>
        public void CloseServer()
        {
            videoCapture.SignalToStop();
            canRGBFR = false;
            canRGBLiveness = false;
        }
        #endregion

        #region 内部方法
        /// <summary>
        /// RGB活体检测线程
        /// </summary>
        void VideoRGBLiveness()
        {
            canRGBLiveness = true;
            Task.Run(() =>
            {
                while (canRGBLiveness)
                {
                    if (rgbLivenessTryDict.GetDictCount() <= 0)
                        continue;

                    try
                    {
                        if (rgbVideoBitmap == null)
                            continue;

                        List<int> faceIdList = new List<int>();
                        faceIdList.AddRange(rgbLivenessTryDict.GetAllElement().Keys);
                        //遍历人脸Id，进行活体检测
                        foreach (int tempFaceId in faceIdList)
                        {
                            //待处理队列中不存在，移除
                            if (!rgbLivenessTryDict.ContainsKey(tempFaceId))
                                continue;

                            //大于尝试次数，移除
                            int tryTime = rgbLivenessTryDict.GetElementByKey(tempFaceId);
                            if (tryTime >= LiveMatchTime)
                                continue;

                            tryTime += 1;
                            //无对应的人脸框信息
                            if (!rgbTrackUnitDict.ContainsKey(tempFaceId))
                                continue;

                            FaceTrackUnit tempFaceTrack = rgbTrackUnitDict.GetElementByKey(tempFaceId);

                            //RGB活体检测
                            ThrowLog?.Invoke(string.Format("faceId:{0},活体检测第{1}次\r\n", tempFaceId, tryTime));
                            SingleFaceInfo singleFaceInfo = new SingleFaceInfo
                            {
                                faceOrient = tempFaceTrack.FaceOrient,
                                faceRect = tempFaceTrack.Rect,
                                faceDataInfo = tempFaceTrack.FaceDataInfo
                            };
                            Bitmap bitmapClone = null;
                            try
                            {
                                lock (rgbVideoBitmap)
                                {
                                    if (rgbVideoBitmap == null)
                                        break;

                                    bitmapClone = (Bitmap)rgbVideoBitmap.Clone();
                                }
                                int retCodeLiveness = -1;
                                LivenessInfo liveInfo = FaceUtil.LivenessInfo_RGB(rgbVideoEngine, bitmapClone, singleFaceInfo, out retCodeLiveness);
                                //更新活体检测结果
                                if (retCodeLiveness.Equals(0) && liveInfo.num > 0 && rgbTrackUnitDict.ContainsKey(tempFaceId))
                                {
                                    rgbTrackUnitDict.GetElementByKey(tempFaceId).RgbLiveness = liveInfo.isLive[0];
                                    if (liveInfo.isLive[0].Equals(1))
                                        tryTime = int.MaxValue;
                                }
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                            finally
                            {
                                if (bitmapClone != null)
                                    bitmapClone.Dispose();
                            }
                            rgbLivenessTryDict.UpdateDictionaryElement(tempFaceId, tryTime);
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorShow?.Invoke(ex);
                    }
                }
            });
        }

        /// <summary>
        /// RGB特征提取和搜索线程
        /// </summary>
        void VideoRGBFR()
        {
            canRGBFR = true;
            Task.Run(() =>
            {
                while (canRGBFR)
                {
                    if (rgbFeatureTryDict.GetDictCount() <= 0)
                        continue;

                    //左侧人脸库为空时，不用进行特征搜索
                    if (faceDatabase.Count <= 0)
                        continue;

                    try
                    {
                        if (rgbVideoBitmap == null)
                            continue;

                        List<int> faceIdList = new List<int>();
                        faceIdList.AddRange(rgbFeatureTryDict.GetAllElement().Keys);
                        foreach (int tempFaceId in faceIdList)
                        {
                            //待处理队列中不存在，移除
                            if (!rgbFeatureTryDict.ContainsKey(tempFaceId))
                                continue;
                            //大于尝试次数，移除
                            int tryTime = rgbFeatureTryDict.GetElementByKey(tempFaceId);
                            if (tryTime >= FrMatchTime)
                                continue;

                            //无对应的人脸框信息
                            if (!rgbTrackUnitDict.ContainsKey(tempFaceId))
                                continue;

                            FaceTrackUnit tempFaceTrack = rgbTrackUnitDict.GetElementByKey(tempFaceId);
                            tryTime += 1;
                            //特征搜索
                            string faceIndex = string.Empty;
                            float similarity = 0f;
                            ThrowLog?.Invoke(string.Format("faceId:{0},特征搜索第{1}次\r\n", tempFaceId, tryTime));
                            //提取人脸特征
                            SingleFaceInfo singleFaceInfo = new SingleFaceInfo
                            {
                                faceOrient = tempFaceTrack.FaceOrient,
                                faceRect = tempFaceTrack.Rect,
                                faceDataInfo = tempFaceTrack.FaceDataInfo
                            };
                            Bitmap bitmapClone = null;
                            try
                            {
                                lock (rgbVideoBitmap)
                                {
                                    if (rgbVideoBitmap == null)
                                        break;
                                    bitmapClone = (Bitmap)rgbVideoBitmap.Clone();
                                }
                                FaceFeature feature = FaceUtil.ExtractFeature(rgbVideoEngine, bitmapClone, singleFaceInfo);
                                if (feature == null || feature.featureSize <= 0)
                                    break;
                                //特征搜索
                                faceIndex = CompareFeature(feature, out similarity);

                                //更新比对结果
                                if (rgbTrackUnitDict.ContainsKey(tempFaceId))
                                {
                                    rgbTrackUnitDict.GetElementByKey(tempFaceId).SetFaceIndexAndSimilarity(faceIndex, similarity.ToString("#0.00"));
                                    if (!string.IsNullOrEmpty(faceIndex))
                                        tryTime = int.MaxValue;
                                }
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                            finally
                            {
                                if (bitmapClone != null)
                                {
                                    bitmapClone.Dispose();
                                }
                            }
                            rgbFeatureTryDict.UpdateDictionaryElement(tempFaceId, tryTime);
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorShow?.Invoke(ex);
                    }
                }
            });
        }

        /// <summary>
        /// 得到feature比较结果
        /// </summary>
        /// <param name="feature">特征</param>
        /// <param name="similarity">相似值</param>
        /// <returns>人脸库Id</returns>
        string CompareFeature(FaceFeature feature, out float similarity)
        {
            string result = string.Empty;
            similarity = 0f;
            try
            {
                //如果人脸库不为空，则进行人脸匹配
                if (faceDatabase != null && faceDatabase.Count > 0)
                {
                    foreach (var item in faceDatabase)
                    {
                        rgbVideoEngine.ASFFaceFeatureCompare(feature, item.Value, out similarity, COMPARE_MODEL);
                        if (similarity >= THRESHOLD)
                        {
                            result = item.Key;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorShow?.Invoke(ex);
            }
            return result;
        }
        #endregion

    }
}
