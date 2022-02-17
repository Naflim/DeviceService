using System;

namespace DeviceService.Model.ExceptionModels
{
    public class HIKException : Exception
    {
        public HIKException(string message) : base(message)
        {

        }

        public static HIKException AbnormalJudgment(uint state)
        {
            switch (state)
            {
                case 1:
                    return new HIKException("用户名密码错误。注册时输入的用户名或者密码错误");
                case 5:
                    return new HIKException("连接到设备的用户个数超过最大。");
                case 7:
                    return new HIKException("连接设备失败。设备不在线或网络原因引起的连接超时等。");
                case 8:
                    return new HIKException("向设备发送失败。");
                case 9:
                    return new HIKException("从设备接收数据失败。");
                case 10:
                    return new HIKException("从设备接收数据超时。");
                case 12:
                    return new HIKException("调用次序错误。");
                case 17:
                    return new HIKException("参数错误。SDK 接口中给入的输入或输出参数为空。");
                case 34:
                    return new HIKException("创建文件出错。本地录像、保存图片、获取配置文件和远程下载录像时创建文件失败。");
                case 47:
                    return new HIKException("用户不存在。注册的用户 ID 已注销或不可用。");
                case 102:
                    return new HIKException("用户还没登录成功。");
                default:
                    return new HIKException($"错误码：{state}");
            }
        }
    }
}
