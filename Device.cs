using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceService.Model;

namespace DeviceService
{
    /// <summary>
    /// 上传服务器
    /// </summary>
    public interface IUploadServer
    {
        /// <summary>
        /// 接口上传
        /// </summary>
        void RequestInterface();

        /// <summary>
        /// 数据库上传
        /// </summary>
        void UploadDatabase();
    }

    /// <summary>
    /// 设备
    /// </summary>
    public interface IDevice
    {
        /// <summary>
        /// 连接设备
        /// </summary>
        /// <param name="connect">连接参数</param>
        void Connect(ConnectModel connect);

        /// <summary>
        /// 断开连接
        /// </summary>
        void Disconnect();
    }

    /// <summary>
    /// 温湿度
    /// </summary>
    public interface ITempHumMeter : IDevice
    {
        /// <summary>
        /// 获取温湿度列表
        /// </summary>
        /// <returns>温度</returns>
        List<TempHumModel> GetTempHumList();
        /// <summary>
        /// 获取温湿度（通过设备编号）
        /// </summary>
        /// <returns>湿度</returns>
        TempHumModel GetTempHum(int deviceID);
    }

    /// <summary>
    /// 摄像头
    /// </summary>
    public interface ICamera : IDevice
    {
        /// <summary>
        /// 拍照
        /// </summary>
        void Photograph(string cachePath);
    }

    /// <summary>
    /// 警报器
    /// </summary>
    public interface IAlarm : IDevice
    {
        /// <summary>
        /// 触发警报
        /// </summary>
        void AlarmTrigger();
    }

    /// <summary>
    /// 柜子
    /// </summary>
    public interface ICabinet : IDevice
    {
        /// <summary>
        /// 开门
        /// </summary>
        void OpenDoor(int doorID);
        /// <summary>
        /// 开所有门
        /// </summary>
        void OpenAllDoor();
        /// <summary>
        /// 关门
        /// </summary>
        void CloseDoor();
    }

    /// <summary>
    /// 通道门
    /// </summary>
    public interface IChannelGate
    {
        /// <summary>
        /// 人员通过触发
        /// </summary>
        void AdoptTrigger(ChannelGateModel channelGate);
    }

    /// <summary>
    /// IC读卡器
    /// </summary>
    public interface IReaderIC
    {
        /// <summary>
        /// 获取IC卡ID
        /// </summary>
        /// <returns>卡ID</returns>
        string GetCardID();
    }

    /// <summary>
    /// 键盘模式发卡器
    /// </summary>
    public interface IReadOrWrite : IDevice
    {
        /// <summary>
        /// 自动串口连接
        /// </summary>
        void Connect();
        /// <summary>
        /// 查询标签
        /// </summary>
        /// <returns>标签组</returns>
        string[] SelTag();
        /// <summary>
        /// epc写入标签
        /// </summary>
        /// <param name="epc">写入的epc</param>
        void WriteTag(string epc);
        /// <summary>
        /// 设置功率
        /// </summary>
        /// <param name="power">功率值</param>
        void SetPower(byte power);
    }
}
