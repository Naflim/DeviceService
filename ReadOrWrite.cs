using DeviceService.DeviceModel;

namespace DeviceService
{
    public class ReadOrWriteUHF09 : UHFReader09, IReadOrWrite
    {
        public void SetPower(int Ant, byte power)
        {
            SetPower(power);
        }
    }

    public class ReadOrWriteUHF86 : UHFReader86, IReadOrWrite
    {
        public void SetPower(int Ant, byte power)
        {
            SetPower(power);
        }
    }

    public class ReadOrWriteCETC : CardWriteCETC, IReadOrWrite
    {

    }

    public class ReadOrWriteUHFRFID : UHFReaderRFID
    {

    }
}
