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
            return state switch
            {
                1 => new HIKException("用户名密码错误。注册时输入的用户名或者密码错误"),
                5 => new HIKException("连接到设备的用户个数超过最大。"),
                7 => new HIKException("连接设备失败。设备不在线或网络原因引起的连接超时等。"),
                8 => new HIKException("向设备发送失败。"),
                9 => new HIKException("从设备接收数据失败。"),
                10 => new HIKException("从设备接收数据超时。"),
                12 => new HIKException("调用次序错误。"),
                17 => new HIKException("参数错误。SDK 接口中给入的输入或输出参数为空。"),
                34 => new HIKException("创建文件出错。本地录像、保存图片、获取配置文件和远程下载录像时创建文件失败。"),
                47 => new HIKException("用户不存在。注册的用户 ID 已注销或不可用。"),
                102 => new HIKException("用户还没登录成功。"),
                1924 => new HIKException("布防超过最大连接数。"),
                _ => new HIKException($"错误码：{state}"),
            };
        }
    }
}
