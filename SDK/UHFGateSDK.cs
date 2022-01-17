using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace DeviceService.SDK
{
    public static class UHFGateSDK
    {
         private const string DLLNAME = @"UHFGate.dll";
         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int AutoOpenComPort(ref int Port,
                                                  ref byte ComAddr,
                                                  ref int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int OpenComPort(int Port,
                                              ref byte ComAddr,
                                              ref int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int CloseComPort();

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int CloseSpecComPort(int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int OpenNetPort(int Port,
                                              string iPaddr,
                                              ref byte ComAddr,
                                              ref int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int CloseNetPort(int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int GetControllerInfo(ref byte ConAddr,
                                                    ref byte ProductCode,
                                                    byte[] Version,
                                                    byte[] DevSN,
                                                    int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int GetChannelMessage(byte ConAddr,
                                                    ref byte PacketNo,
                                                    byte[] Msg, 
                                                    ref byte MsgLength , 
                                                    ref byte MsgType,
                                                    int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int Acknowledge(byte ConAddr,
                                              int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int ConfigureController(byte ConAddr,
                                                      byte CFGNo,
                                                      byte CFGLen,
                                                      byte[] CFGData,
                                                      int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int GetControllerConfig(byte ConAddr,
                                                      byte CFGNo,
                                                      ref byte CFGLen,
                                                      byte[] CFGData,
                                                      int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int SetRelay(byte ConAddr,
                                           byte num,
                                           byte status,
                                           byte time,
                                           int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int SetBuzzer(byte ConAddr,
                                            byte onTime,
                                            byte offTime,
                                            byte Count,
                                            byte vol,
                                            int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int SetLED(byte ConAddr,
                                         byte onTime,
                                         byte offTime,
                                         byte Count,
                                         byte color,
                                         int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int SetClock(byte ConAddr,
                                           byte[] SetTime,
                                           int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int GetClock(byte ConAddr,
                                           byte[] CurrentTime,
                                           int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int ClearBuffer(byte ConAddr,
                                              int PortHandle);

        
         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int GetPersionAndAlarmInfo(byte ConAddr,
                                                         byte[] positive,
                                                         byte[] reverse,
                                                         byte[] Online,
                                                         byte[] Alarm,
                                                         int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int SetPersionAndAlarmInfo(byte ConAddr,
                                                         byte[] positive,
                                                         byte[] reverse,
                                                         byte[] Online,
                                                         byte[] Alarm,
                                                         int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int SetGPIO(byte ConAddr,
                                          byte GPIO_OUT,
                                          int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int GetGPIO(byte ConAddr,
                                          ref byte GPI,
                                          int PortHandle);

       

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int GetControllerReaderConnectionStatus(byte ConAddr,
                                                                      ref byte ConnectStatus,
                                                                      int PortHandle);
       

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int ControllerRestart(byte ConAddr,
                                                    int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int LoadDefault(byte ConAddr,
                                              int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int SetDeviceMAC(byte ConAddr,
                                               byte[] MAC,
                                               int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int GetDeviceMAC(byte ConAddr,
                                               byte[] MAC,
                                               int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int GetRerurnLoss(byte ConAddr,
                                                byte antenna,
                                                ref byte returnloss,
                                                int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int ChangeToFlashMode(byte ConAddr,
                                              int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int WriteBinData(byte ConAddr,
                                               byte Index,
                                               byte len,
                                               byte[]data,
                                              int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int CheckCRC32(byte ConAddr,
                                             byte[]crc32,
                                              int PortHandle);

         [DllImport(DLLNAME, CallingConvention = CallingConvention.StdCall)]
         public static extern int WriteMark(byte ConAddr,
                                              int PortHandle);


    }
}
