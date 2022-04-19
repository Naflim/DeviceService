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
            return state switch
            {
                28676 => new ArcfaceException("SDKKey和使用的SDK不匹配"),
                90127 => new ArcfaceException("图片宽高不支持，宽度需四字节对齐"),
                98308 => new ArcfaceException("ACTIVEKEY与APPID、SDKKEY不匹配"),
                _ => new ArcfaceException($"错误码：{state}"),
            };
        }
    }
}
