using AForge.Video.DirectShow;
using ArcFaceSDK;
using ArcFaceSDK.Entity;
using ArcFaceSDK.SDKModels;
using DeviceService.DeviceModel;
using DeviceService.Model.ExceptionModels;
using DeviceService.SDK.Arcface;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DeviceService
{
    public class CameraArcface : Arcface
    {
        /// <summary>
        /// 比对模型
        /// </summary>
        const ASF_CompareModel COMPARE_MODEL = ASF_CompareModel.ASF_ID_PHOTO;

        /// <summary>
        /// 人脸比对阈值
        /// </summary>
        const float THRESHOLD = 0.8f;

        /// <summary>
        /// 最大人脸检测数
        /// </summary>
        public int DetectFaceMaxNum { get; set; } = 6;

        /// <summary>
        /// FR失败重试次数
        /// </summary>
        public int FrMatchTime { get; set; } = 30;

        /// <summary>
        /// 活体检测失败重试次数
        /// </summary>
        public int LiveMatchTime { get; set; } = 30;

        FilterInfoCollection filterInfoCollection;
        VideoCaptureDevice videoCapture;
        /// <summary>
        /// RGB视频帧图像
        /// </summary>
        private Bitmap rgbVideoBitmap;

        /// <summary>
        /// rgb专用FR引擎
        /// </summary>
        readonly FaceEngine rgbVideoEngine = new FaceEngine();

        /// <summary>
        /// 摄像头视频人脸追踪检测结果
        /// </summary>
        readonly Dictionary<int, FaceTrackUnit> rgbTrackUnitDict = new Dictionary<int, FaceTrackUnit>();

        /// <summary>
        /// RGB 特征搜索尝试次数字典
        /// </summary>
        readonly Dictionary<int, int> rgbFeatureTryDict = new Dictionary<int, int>();

        /// <summary>
        /// RGB 活体检测尝试次数字典
        /// </summary>
        readonly Dictionary<int, int> rgbLivenessTryDict = new Dictionary<int, int>();

        bool canRGBLiveness;
        bool canRGBFR;

        public CameraArcface()
        {
            Init();
        }

        protected override void InitEngine()
        {
            base.InitEngine();

            //Video模式下检测脸部的角度优先值
            ASF_OrientPriority videoDetectFaceOrientPriority = ASF_OrientPriority.ASF_OP_ALL_OUT;
            DetectionMode detectModeVideo = DetectionMode.ASF_DETECT_MODE_VIDEO;
            int combinedMaskVideo = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_FACELANDMARK;
            int retCode = rgbVideoEngine.ASFInitEngine(detectModeVideo, videoDetectFaceOrientPriority, DetectFaceMaxNum, combinedMaskVideo);
            if (retCode != 0)
                throw ArcfaceException.AbnormalJudgment(retCode);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        void Init()
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (filterInfoCollection.Count == 0)
                throw new DriveNotFoundException("未检测到摄像头，请确保已安装摄像头或驱动!");
        }

        /// <summary>
        /// 开启服务
        /// </summary>
        /// <returns></returns>
        public VideoCaptureDevice StartServer()
        {
            videoCapture = new VideoCaptureDevice(filterInfoCollection[0].MonikerString);
            VideoRGBLiveness();
            VideoRGBFR();
            return videoCapture;
        }

        /// <summary>
        /// 位图判断
        /// </summary>
        /// <param name="bitmap">位图</param>
        /// <param name="faceTrackUnit">结果</param>
        /// <returns></returns>
        public bool RGBBitmapDetection(Bitmap bitmap, out FaceTrackUnit faceTrackUnit)
        {
            faceTrackUnit = null;
            if (bitmap == null) return false;
            rgbVideoBitmap = bitmap;
            Bitmap bitmapClone = (Bitmap)bitmap.Clone();
            //检测人脸，得到Rect框
            MultiFaceInfo multiFaceInfo = FaceUtil.DetectFaceAndLandMark(rgbVideoEngine, bitmapClone);
            //未检测到人脸
            if (multiFaceInfo.faceNum <= 0)
            {
                rgbTrackUnitDict.Clear();
                return false;
            }

            List<int> tempIdList = new List<int>();
            for (int faceIndex = 0; faceIndex < multiFaceInfo.faceNum; faceIndex++)
            {
                MRECT mrect = multiFaceInfo.faceRects[faceIndex];
                int faceId = multiFaceInfo.faceID[faceIndex];
                FaceTrackUnit currentFaceTrack = rgbTrackUnitDict.ContainsKey(faceId) ? rgbTrackUnitDict[faceId] : null;
                if (faceId >= 0)
                {
                    //判断faceid是否加入待处理队列
                    if (!rgbFeatureTryDict.ContainsKey(faceId))
                        rgbFeatureTryDict.Add(faceId, 0);

                    if (!rgbLivenessTryDict.ContainsKey(faceId))
                        rgbLivenessTryDict.Add(faceId, 0);

                    if (rgbTrackUnitDict.ContainsKey(faceId))
                    {
                        rgbTrackUnitDict[faceId].Rect = mrect;
                        rgbTrackUnitDict[faceId].FaceOrient = multiFaceInfo.faceOrients[faceIndex];
                        rgbTrackUnitDict[faceId].FaceDataInfo = multiFaceInfo.faceDataInfoList[faceIndex];

                        if (!string.IsNullOrEmpty(rgbTrackUnitDict[faceId].FaceLocal) && rgbTrackUnitDict[faceId].RgbLive == Liveness.Reality)
                            faceTrackUnit = rgbTrackUnitDict[faceId];
                    }
                    else
                        rgbTrackUnitDict.Add(faceId, new FaceTrackUnit(faceId, mrect, multiFaceInfo.faceOrients[faceIndex], multiFaceInfo.faceDataInfoList[faceIndex]));

                    tempIdList.Add(faceId);
                }
            }

            //初始化及刷新待处理队列,移除出框的人脸
            tempIdList.ForEach(v =>
            {
                rgbFeatureTryDict.Remove(v);
                rgbLivenessTryDict.Remove(v);
                rgbTrackUnitDict.Remove(v);
            });

            return faceTrackUnit != null;
        }

        /// <summary>
        /// RGB活体检测线程
        /// </summary>
        void VideoRGBLiveness()
        {
            canRGBLiveness = true;
            Task.Factory.StartNew(() =>
            {
                while (canRGBLiveness)
                {
                    if (rgbLivenessTryDict.Count <= 0)
                        continue;

                    if (rgbVideoBitmap is null)
                        continue;

                    try
                    {
                        List<int> faceIdList = new List<int>();
                        faceIdList.AddRange(rgbLivenessTryDict.Keys);
                        //遍历人脸Id，进行活体检测
                        foreach (int tempFaceId in faceIdList)
                        {
                            //待处理队列中不存在，移除
                            if (!rgbLivenessTryDict.ContainsKey(tempFaceId))
                                continue;

                            //大于尝试次数，移除
                            int tryTime = rgbLivenessTryDict[tempFaceId];
                            if (tryTime >= LiveMatchTime)
                                continue;
                            tryTime += 1;

                            //无对应的人脸框信息
                            if (!rgbTrackUnitDict.ContainsKey(tempFaceId))
                                continue;
                            FaceTrackUnit tempFaceTrack = rgbTrackUnitDict[tempFaceId];

                            //RGB活体检测
                            ThrowLog?.Invoke($"faceId:{tempFaceId},活体检测第{tryTime}次");
                            SingleFaceInfo singleFaceInfo = new SingleFaceInfo
                            {
                                faceOrient = tempFaceTrack.FaceOrient,
                                faceRect = tempFaceTrack.Rect,
                                faceDataInfo = tempFaceTrack.FaceDataInfo
                            };
                            Bitmap bitmapClone = null;
                            try
                            {
                                if (rgbVideoBitmap == null)
                                    break;

                                bitmapClone = (Bitmap)rgbVideoBitmap.Clone();
                                int retCodeLiveness = -1;

                                LivenessInfo liveInfo = FaceUtil.LivenessInfo_RGB(rgbVideoEngine, bitmapClone, singleFaceInfo, out retCodeLiveness);
                                //更新活体检测结果
                                if (retCodeLiveness.Equals(0) && liveInfo.num > 0 && rgbTrackUnitDict.ContainsKey(tempFaceId))
                                {
                                    rgbTrackUnitDict[tempFaceId].RgbLiveness = liveInfo.isLive[0];
                                    if (liveInfo.isLive[0].Equals(1))
                                        tryTime = LiveMatchTime;
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
                            if (rgbLivenessTryDict.ContainsKey(tempFaceId))
                                rgbLivenessTryDict[tempFaceId] = tryTime;
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorShow?.Invoke(ex);
                    }

                }
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// RGB特征提取和搜索线程
        /// </summary>
        void VideoRGBFR()
        {
            canRGBFR = true;
            Task.Factory.StartNew(() =>
            {
                while (canRGBFR)
                {
                    if (rgbFeatureTryDict.Count <= 0)
                        continue;

                    if (faceDatabase.Count <= 0)
                        continue;

                    if (rgbVideoBitmap is null)
                        continue;

                   
                    try
                    {
                        List<int> faceIdList = new List<int>();
                        faceIdList.AddRange(rgbFeatureTryDict.Keys);
                        foreach (int tempFaceId in faceIdList)
                        {
                            Console.WriteLine($"{DateTime.Now}:{"go"}");

                            //待处理队列中不存在，移除
                            if (!rgbFeatureTryDict.ContainsKey(tempFaceId))
                                continue;

                            //大于尝试次数，移除
                            int tryTime = rgbFeatureTryDict[tempFaceId];

                           
                            if (tryTime >= FrMatchTime)
                                continue;

                            

                            //无对应的人脸框信息
                            if (!rgbTrackUnitDict.ContainsKey(tempFaceId))
                                continue;
                            FaceTrackUnit tempFaceTrack = rgbTrackUnitDict[tempFaceId];
                            tryTime += 1;

                           

                            //特征搜索
                            string faceLocal = string.Empty;
                            float similarity = 0f;
                            ThrowLog?.Invoke($"faceId:{tempFaceId},特征搜索第{tryTime}次");
                            //提取人脸特征
                            SingleFaceInfo singleFaceInfo = new SingleFaceInfo();
                            singleFaceInfo.faceOrient = tempFaceTrack.FaceOrient;
                            singleFaceInfo.faceRect = tempFaceTrack.Rect;
                            singleFaceInfo.faceDataInfo = tempFaceTrack.FaceDataInfo;
                            Bitmap bitmapClone = null;
                            try
                            {
                                if (rgbVideoBitmap == null)
                                    break;

                                bitmapClone = (Bitmap)rgbVideoBitmap.Clone();

                                FaceFeature feature = FaceUtil.ExtractFeature(rgbVideoEngine, bitmapClone, singleFaceInfo);
                                if (feature == null || feature.featureSize <= 0)
                                    break;

                                //特征搜索
                                faceLocal = CompareFeature(feature, out similarity);

                                //Console.WriteLine($"{DateTime.Now}:{"go"}");
                                //更新比对结果
                                if (rgbTrackUnitDict.ContainsKey(tempFaceId))
                                {
                                    rgbTrackUnitDict[tempFaceId].SetFaceIndexAndSimilarity(faceLocal, similarity.ToString("#0.00"));
                                    if (!string.IsNullOrEmpty(faceLocal))
                                    {
                                        ThrowLog?.Invoke(faceLocal);
                                        tryTime = FrMatchTime;
                                    }
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
                            if (rgbLivenessTryDict.ContainsKey(tempFaceId))
                                rgbLivenessTryDict[tempFaceId] = tryTime;
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorShow?.Invoke(ex);
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 得到feature比较结果
        /// </summary>
        /// <param name="feature">特征</param>
        /// <param name="similarity">相似值</param>
        /// <returns>人脸库Id</returns>
        private string CompareFeature(FaceFeature feature, out float similarity)
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
                ErrorShow.Invoke(ex);
            }
            return result;
        }

        /// <summary>
        /// 关闭线程检测
        /// </summary>
        public void CloseDetection()
        {
            canRGBFR = false;
            canRGBLiveness = false;
        }
    }
}
