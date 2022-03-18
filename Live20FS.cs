using DeviceService.DeviceModel;
using libzkfpcsharp;
using System;
using System.Collections.Generic;
using System.Text;
using DeviceService.Model.ExceptionModels;
using System.Threading.Tasks;
using System.Threading;

namespace DeviceService
{
    public class Live20FS : ZKTECO
    {
        const int TMPSIZE = 2048;
        public void CollectFingerprints()
        {

            int cbCapTmp = TMPSIZE;
            byte[] FPBuffer, CapTmp = new byte[TMPSIZE];
            
            while (true)
            {
                devices.ForEach(x =>
                {
                    FPBuffer = new byte[x.imgSize];

                    int ret = zkfp2.AcquireFingerprint(x.handle, FPBuffer, CapTmp, ref cbCapTmp);
                    if (ret != 0)
                        Console.WriteLine(ZKTECOException.AbnormalJudgment(ret).Message);
                    else
                        Console.WriteLine("ok!");
                });
                Thread.Sleep(200);
            }
        }

        public void Test()
        {
            

        }
    }
}
