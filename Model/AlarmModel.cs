using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Model
{
    public class AlarmModel
    {

        /// <summary>
        /// 扫描的标签
        /// </summary>
        public List<string> EPC { get; set; }

        public AlarmModel() { }

        public AlarmModel(List<string> epc) 
        {
            EPC = epc;
        }
    }
}
