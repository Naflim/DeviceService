using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Model.ExceptionModels
{
    public class RSException:Exception
    {
        public RSException(string message) : base(message)
        {

        }
    }
}
