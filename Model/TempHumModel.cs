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
        public float Temperature { get; set; }
        public float Humidity { get; set; }

        public TempHumModel() { }

        public TempHumModel(int deviceID, float temperature, float humidity)
        {
            DeviceID = deviceID;
            Temperature = temperature;
            Humidity = humidity;
        }
    }
}
