using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Model
{
    /// <summary>
    /// 方向
    /// </summary>
    public enum Direction 
    {
        /// <summary>
        /// 无方向
        /// </summary>
        Null,
        /// <summary>
        /// 出
        /// </summary>
        Out,
        /// <summary>
        /// 进
        /// </summary>
        In
    }

    /// <summary>
    /// 标签
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// 标签号
        /// </summary>
        public string EPC { get; set; }

        /// <summary>
        /// 查询次数
        /// </summary>
        public uint Frequency { get; set; }

        /// <summary>
        /// 查询时间
        /// </summary>
        public DateTime QueryTime { get; set; }

        public Tag(string epc)
        {
            EPC = epc;
            Frequency = 1;
            QueryTime = DateTime.Now;
        }

        public Tag(string epc,uint frequency)
        {
            EPC = epc;
            Frequency = frequency;
            QueryTime = DateTime.Now;
        }

        public Tag(string epc, uint frequency,DateTime queryTime)
        {
            EPC = epc;
            Frequency = frequency;
            QueryTime = queryTime;
        }
    }

    public class ChannelGateModel
    {
        /// <summary>
        /// 通道门标识
        /// </summary>
        public string ChannelGateId { get; set; }

        /// <summary>
        /// 进出方向
        /// </summary>

        public Direction Direction { get; set; }

        /// <summary>
        /// 扫描的标签
        /// </summary>
        public List<Tag> Tags { get; set; }

        public ChannelGateModel() { }

        public ChannelGateModel(Direction direction, List<Tag> tags) 
        {
            Direction = direction;
            Tags = tags;
        }

        public ChannelGateModel(Direction direction, List<Tag> tags, string id)
        {
            Direction = direction;
            Tags = tags;
            ChannelGateId = id;
        }
    }
}
