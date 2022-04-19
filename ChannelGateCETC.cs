using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceService.DeviceModel;
using DeviceService.Model;
using DeviceService.Model.ExceptionModels;
using DeviceService.SDK;
using NaflimHelperLibrary;

namespace DeviceService
{
    public class ChannelGateCETC : UHFReaderCETC, IChannelGate
    {
        public void StartChannelGateServer(Action<IChannelGate, ChannelGateModel> adoptTrigger)
        {
            client.StartPerioInventory();
        }
    }
}
