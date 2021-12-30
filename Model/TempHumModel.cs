using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Model
{
    public class TempHumModel
    {
        public int DeviceID { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
    }
}
