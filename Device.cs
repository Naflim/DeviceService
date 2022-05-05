using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceService.Model;

namespace DeviceService
{
    /// <summary>
    /// 设备服务器
    /// </summary>
    public interface IDeviceServer
    {
        /// <summary>
        /// 开启服务
        /// </summary>
        void OpenService();
        /// <summary>
        /// 关闭服务
        /// </summary>
        void CloseService();
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
    /// 读写器
    /// </summary>
    public interface IReader : IDevice
    {
        /// <summary>
        /// 循查标签
        /// </summary>
        /// <param name="seconds">循查时间</param>
        /// <returns>标签</returns>
        Task<string[]> CyclicQueryTags(int seconds);

        /// <summary>
        /// 查询标签
        /// </summary>
        /// <param name="Ant">天线</param>
        /// <returns>标签</returns>
        string[] QueryTags(int Ant = 0);
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
        void StartChannelGateServer(Action<IChannelGate, ChannelGateModel> adoptTrigger);
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
    /// 指纹扫描仪
    /// </summary>
    public interface IFingerprintScanner
    {

    }
}
