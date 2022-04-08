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
                _ => new ArcfaceException($"错误码：{state}"),
            };
        }
    }
}
