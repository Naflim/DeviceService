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
        public List<Tag> Tags { get; set; }

        public AlarmModel() { }

        public AlarmModel(List<Tag> tags) 
        {
            Tags = tags;
        }
    }
}
