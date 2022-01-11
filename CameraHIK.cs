﻿using DeviceService.DeviceModel;
using DeviceService.Model.ExceptionModels;
using DeviceService.SDK;
using System;
using System.IO;

namespace DeviceService
{
    public class CameraHIK : HIKVISION, ICamera
    {
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
            if (!CHCNetSDK.NET_DVR_CaptureJPEGPicture(UserID, lChannel, ref lpJpegPara, imgPath))
                throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());
        }

        public void Photograph(string path,out string imgPath)
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
            if (!CHCNetSDK.NET_DVR_CaptureJPEGPicture(UserID, lChannel, ref lpJpegPara, imgPath))
                throw HIKException.AbnormalJudgment(CHCNetSDK.NET_DVR_GetLastError());
        }
    }
}