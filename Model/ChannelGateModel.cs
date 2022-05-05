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
    public class ChannelGateModel
    {
        /// <summary>
        /// 进出方向
        /// </summary>
       
        public Direction Direction { get; set; }

        /// <summary>
        /// 扫描的标签
        /// </summary>
        public List<string> EPC { get; set; }

        public ChannelGateModel(Direction direction, List<string> epc) 
        {
            Direction = direction;
            EPC = epc;
        }
    }
}
