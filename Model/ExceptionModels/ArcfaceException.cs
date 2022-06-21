using System;

namespace DeviceService.Model.ExceptionModels
{
    public class ArcfaceException : Exception
    {
        public ArcfaceException(string message) : base(message)
        {

        }

        public static ArcfaceException AbnormalJudgment(int state)
        {
            switch (state)
            {
                case 2:
                    return new ArcfaceException("无效的参数");
                case 4:
                    return new ArcfaceException("内存不足");
                case 28676:
                    return new ArcfaceException("SDKKey和使用的SDK不匹配");
                case 90127:
                    return new ArcfaceException("图片宽高不支持，宽度需四字节对齐");
                case 98308:
                    return new ArcfaceException("ACTIVEKEY与APPID、SDKKEY不匹配");
                default:
                    return new ArcfaceException($"错误码：{state}");
            }
        }
    }
}
