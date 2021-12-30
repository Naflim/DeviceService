using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Model
{
    public class ConnectModel
    {
        public string Ip { get; set; }
        public int Port { get; set; } = int.MinValue;
        public int Com { get; set; }
        public string UserID { get; set; }
        public string Pwd { get; set; }
    }
}
