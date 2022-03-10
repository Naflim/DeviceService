using DeviceService.DeviceModel;
using DeviceService.Model.ExceptionModels;
using DeviceService.SDK;
using System;
using System.IO;

namespace DeviceService
{
    public class CameraHIK : HIKVISION, ICamera
    {
        /// <summary>
        /// 指定路径拍照
        /// </summary>
        /// <param name="path">路径</param>
        public void Photograph(string path)
        {
            string cachePath = $"{path}/{Ip}/";
            string imgPath = $"{cachePath}{DateTime.Now:yyyy-MM-dd HH-mm-ss}.jpg";

            if (Directory.Exists(cachePath) == false)
                Directory.CreateDirectory(cachePath);
            int lChannel = 1; //通道号 Channel number

            CHCNetSDK.NET_DVR_JPEGPARA lpJpegPara = new CHCNetSDK.NET_DVR_JPEGPARA();
            lpJpegPara.wPicQuality = 0; //图像质量 Image quality
            lpJpegPara.wPicSize = 0xff; //抓图分辨率 Picture size: 2- 4CIF，0xff- Auto(使用当前码流分辨率)，抓图分辨率需要设备支持，更多取值请参考SDK文档

            //JPEG抓图 Capture a JPEG picture
            if (!CHCNetSDK.NET_DVR_CaptureJPEGPicture(userID, lChannel, ref lpJpegPara, imgPath))
                throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());
        }

        /// <summary>
        /// 指定路径拍照
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="imgPath">路径</param>
        public void Photograph(string path, out string imgPath)
        {
            string cachePath = $"{path}/{Ip}/";
            imgPath = $"{cachePath}{DateTime.Now:yyyy-MM-dd HH-mm-ss}.jpg";

            if (Directory.Exists(cachePath) == false)
                Directory.CreateDirectory(cachePath);
            int lChannel = 1; //通道号 Channel number

            CHCNetSDK.NET_DVR_JPEGPARA lpJpegPara = new CHCNetSDK.NET_DVR_JPEGPARA();
            lpJpegPara.wPicQuality = 0; //图像质量 Image quality
            lpJpegPara.wPicSize = 0xff; //抓图分辨率 Picture size: 2- 4CIF，0xff- Auto(使用当前码流分辨率)，抓图分辨率需要设备支持，更多取值请参考SDK文档

            //JPEG抓图 Capture a JPEG picture
            if (!CHCNetSDK.NET_DVR_CaptureJPEGPicture(userID, lChannel, ref lpJpegPara, imgPath))
                throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());
        }

        /// <summary>
        /// 拍照
        /// </summary>
        /// <returns>base64图片</returns>
        public string PhotographBase64()
        {
            int lChannel = 1; //通道号 Channel number

            CHCNetSDK.NET_DVR_JPEGPARA lpJpegPara = new CHCNetSDK.NET_DVR_JPEGPARA();
            lpJpegPara.wPicQuality = 0; //图像质量 Image quality
            lpJpegPara.wPicSize = 0xff; //抓图分辨率 Picture size: 2- 4CIF，0xff- Auto(使用当前码流分辨率)，抓图分辨率需要设备支持，更多取值请参考SDK文档

            uint len = 1024 * 1024 * 5;
            byte[] buffer = new byte[len];
            uint size = 0;

            //JPEG抓图 Capture a JPEG picture
            if (!CHCNetSDK.NET_DVR_CaptureJPEGPicture_NEW(userID, lChannel, ref lpJpegPara, buffer,len,ref size))
                throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());

            byte[] imgBuf = new byte[size];
            Array.Copy(buffer, imgBuf, size);
            return Convert.ToBase64String(imgBuf);
        }
    }
}
