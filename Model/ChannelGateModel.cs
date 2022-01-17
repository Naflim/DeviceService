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
    public enum Direction { Out, In }
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
    }
}
