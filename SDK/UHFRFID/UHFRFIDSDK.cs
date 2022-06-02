using System;
using System.Runtime.InteropServices;

namespace DeviceService.SDK.UHFRFID
{
    internal class UHFRFIDSDK
    {
        //
        [DllImport("UHFRFID.dll", EntryPoint = "UHF_RFID_Open", CallingConvention = CallingConvention.Cdecl)]
        public extern static int UHF_RFID_Open(byte Port, int Baud);

        //
        [DllImport("UHFRFID.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int UHF_RFID_Close(byte Port);

        //
        [DllImport("UHFRFID.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int UHF_RFID_NetOpen(string ipaddr, int port);

        //
        [DllImport("UHFRFID.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int UHF_RFID_NetClose(int port);

        //命令处理函数 
        [DllImport("UHFRFID.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int UHF_RFID_Set(byte eCMD, byte[] pInBuf, int inlen, byte[] pOutBuf, ref uint outlen);

        //获取ERROR信息 
        [DllImport("UHFRFID.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static String UHF_RFID_GET_ERROR(int err);

        //判断是否接收到数据 
        [DllImport("UHFRFID.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static bool UHF_RFID_IsReceiveData();

        //接收一个字节数据 
        [DllImport("UHFRFID.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static bool UHF_RFID_GetOneByteRxData(byte[] ch);

        //解帧函数
        [DllImport("UHFRFID.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static void UHF_RFID_ParseMFrameData(byte ch);

        //解帧处理函数，函数返回true时，才能取unsigned char *epc
        [DllImport("UHFRFID.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static bool UHF_RFID_Handle_Uart_Command(byte[] epc, byte[] len);


        //解析连续寻卡应答帧的ID 
        [DllImport("UHFRFID.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static bool ParseMulReadFrameDataProcess(byte[] ch, byte[] dlen);

        // 
        [DllImport("UHFRFID.dll", EntryPoint = "UHF_RFID_Command", CallingConvention = CallingConvention.Cdecl)]
        public extern static void UHF_RFID_Command(string cmd);
    }
} 
