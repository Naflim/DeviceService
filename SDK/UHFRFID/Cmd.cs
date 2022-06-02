using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviceService.SDK.UHFRFID
{
    internal class Cmd
    {
        /* 宏定义区 */
        //帧头
        public const byte RFID_HEAD_FIRST = 0xA5;
        public const byte RFID_HEAD_SECOND = 0x5A;

        //帧尾
        public const byte RFID_END_MRK_FIRST = 0x0D;
        public const byte RFID_END_MRK_SECOND = 0x0A;

        public const byte RFID_HEAD_LEN = (2 + 2 + 1);
        public const byte RFID_HEADEND_LEN = (RFID_HEAD_LEN + 1 + 2);

        //帧类型

        //版本管理类帧
        //获取设备硬件版本号
        public const byte RFID_CMD_GET_HARDWARE_VERSION = 0x00;       
        public const byte RFID_CMD_GET_HARDWARE_VERSION_RSP = 0x01;     

        //获取设备固件版本号
        public const byte RFID_CMD_GET_FIRMWARE_VERSION = 0x02;
        public const byte RFID_CMD_GET_FIRMWARE_VERSION_RSP = 0x03;

        public const byte RFID_CMD_GET_MODULE_ID = 0x04;
        public const byte RFID_CMD_GET_MODULE_ID_RSP = 0x05;

        //END 版本管理类帧

        //参数设置类帧
        //发射功率
        public const byte RFID_CMD_SET_TXPOWER = 0x10;
        public const byte RFID_CMD_SET_TXPOWER_RSP = 0x11;
        public const byte RFID_CMD_GET_TXPOWER = 0x12;
        public const byte RFID_CMD_GET_TXPOWER_RSP = 0x13;

        //跳频
        public const byte RFID_CMD_SET_FREQ_TABLE = 0x14;
        public const byte RFID_CMD_SET_FREQ_TABLE_RSP = 0x15;
        public const byte RFID_CMD_GET_FREQ_TABLE = 0x16;
        public const byte RFID_CMD_GET_FREQ_TABLE_RSP = 0x17;

        //寄存器
        public const byte RFID_CMD_SET_REG = 0x18;
        public const byte RFID_CMD_SET_REG_RSP = 0x19;
        public const byte RFID_CMD_GET_REG = 0x1A;
        public const byte RFID_CMD_GET_REG_RSP = 0x1B;

        //接收灵敏度
        public const byte RFID_CMD_SET_SENSITIVITY = 0x1C;
        public const byte RFID_CMD_SET_SENSITIVITY_RSP = 0x1D;
        public const byte RFID_CMD_GET_SENSITIVITY = 0x1E;
        public const byte RFID_CMD_GET_SENSITIVITY_RSP = 0x1F;

        //GEN 2 参数
        public const byte RFID_CMD_SET_GEN2_PARA = 0x20;
        public const byte RFID_CMD_SET_GEN2_PARA_RSP = 0x21;
        public const byte RFID_CMD_GET_GEN2_PARA = 0x22;
        public const byte RFID_CMD_GET_GEN2_PARA_RSP = 0x23;

        //CW
        public const byte RFID_CMD_SET_CW_STATUE = 0x24;
        public const byte RFID_CMD_SET_CW_STATUE_RSP = 0x25;
        public const byte RFID_CMD_GET_CW_STATUE = 0x26;
        public const byte RFID_CMD_GET_CW_STATUE_RSP = 0x27;

        //天线
        public const byte RFID_CMD_SET_ANT = 0x28;
        public const byte RFID_CMD_SET_ANT_RSP = 0x29;
        public const byte RFID_CMD_GET_ANT = 0x2A;
        public const byte RFID_CMD_GET_ANT_RSP = 0x2B;

        //区域
        public const byte RFID_CMD_SET_REGION = 0x2C;
        public const byte RFID_CMD_SET_REGION_RSP = 0x2D;
        public const byte RFID_CMD_GET_REGION = 0x2E;
        public const byte RFID_CMD_GET_REGION_RSP = 0x2F;

        //固件在线升级
        public const byte RFID_CMD_SET_BOOT = 0x30;
        public const byte RFID_CMD_SET_BOOT_RSP = 0x31;

        //反射功率
        public const byte RFID_CMD_GET_REVPWR = 0x32;
        public const byte RFID_CMD_GET_REVPWR_RSP = 0x33;

        //读取设备当前温度
        public const byte RFID_CMD_GET_TEMPERATURE = 0x34;
        public const byte RFID_CMD_GET_TEMPERATURE_RSP = 0x35;

        //读取设备前向功率
        public const byte RFID_CMD_GET_FORWDPWR = 0x36;
        public const byte RFID_CMD_GET_FORWDPWR_RSP = 0x37;

        //温度保护
        public const byte RFID_CMD_SET_TEMPPROTECT = 0x38;
        public const byte RFID_CMD_SET_TEMPPROTECT_RSP = 0x39;
        public const byte RFID_CMD_GET_TEMPPROTECT = 0x3A;
        public const byte RFID_CMD_GET_TEMPPROTECT_RSP = 0x3B;

        //连续寻卡等待时间
        public const byte RFID_CMD_SET_MULWAITTIME = 0x3C;
        public const byte RFID_CMD_SET_MULWAITTIME_RSP = 0x3D;
        public const byte RFID_CMD_GET_MULWAITTIME = 0x3E;
        public const byte RFID_CMD_GET_MULWAITTIME_RSP = 0x3F;

        //错误标志
        public const byte RFID_CMD_GET_ERRORFLAG = 0x40;
        public const byte RFID_CMD_GET_ERRORFLAG_RSP = 0x41;
        public const byte RFID_CMD_CLEAR_ERRORFLAG = 0x42;
        public const byte RFID_CMD_CLEAR_ERRORFLAG_RSP = 0x43;

        //DC Offset校准
        public const byte RFID_CMD_DC_OFFSET_CAL = 0x44;
        public const byte RFID_CMD_DC_OFFSET_CAL_RSP = 0x45;

        //GPIO操作
        public const byte RFID_CMD_SET_GPIO = 0x46;
        public const byte RFID_CMD_SET_GPIO_RSP = 0x47;
        public const byte RFID_CMD_GET_GPIO = 0x48;
        public const byte RFID_CMD_GET_GPIO_RSP = 0x49;

        //天线工作时间
        public const byte RFID_CMD_SET_ANT_TIME = 0x4A;
        public const byte RFID_CMD_SET_ANT_TIME_RSP = 0x4B;
        public const byte RFID_CMD_GET_ANT_TIME = 0x4C;
        public const byte RFID_CMD_GET_ANT_TIME_RSP = 0x4D;

        //多天线工作间隔时间
        public const byte RFID_CMD_SET_MULANT_TIME = 0x4E;
        public const byte RFID_CMD_SET_MULANT_TIME_RSP = 0x4F;
        public const byte RFID_CMD_GET_MULANT_TIME = 0x50;
        public const byte RFID_CMD_GET_MULANT_TIME_RSP = 0x51;

        //RF链路设置
        public const byte RFID_CMD_SET_RF_LINK = 0x52;
        public const byte RFID_CMD_SET_RF_LINK_RSP = 0x53;
        public const byte RFID_CMD_GET_RF_LINK = 0x54;
        public const byte RFID_CMD_GET_RF_LINK_RSP = 0x55;

        //设置蜂鸣器
        public const byte RFID_CMD_SET_BUZZER = 0x56;
        public const byte RFID_CMD_SET_BUZZER_RSP = 0x57;

        // FastID功能                         
        public const byte RFID_CMD_SET_FASTID = 0x5C;
        public const byte RFID_CMD_SET_FASTID_RSP = 0x5D;
        public const byte RFID_CMD_GET_FASTID = 0x5E;
        public const byte RFID_CMD_GET_FASTID_RSP = 0x5F;

        // Tagfocus功能
        public const byte RFID_CMD_SET_TAGFOCUS = 0x60;
        public const byte RFID_CMD_SET_TAGFOCUS_RSP = 0x61;
        public const byte RFID_CMD_GET_TAGFOCUS = 0x62;
        public const byte RFID_CMD_GET_TAGFOCUS_RSP = 0x63;

        // 获取环境RSSI
        public const byte RFID_CMD_GET_RSSI = 0x64;
        public const byte RFID_CMD_GET_RSSI_RSP = 0x65;

        // 设置波特率                    
        public const byte RFID_CMD_SET_BAUD = 0x66;
        public const byte RFID_CMD_SET_BAUD_RSP = 0x67;

        // 软件复位
        public const byte RFID_CMD_SOFTRESET = 0x68;
        public const byte RFID_CMD_SOFTRESET_RSP = 0x69;

        // Dual和Signel模式
        public const byte RFID_CMD_SET_DUAL = 0x6A;
        public const byte RFID_CMD_SET_DUAL_RSP = 0x6B;
        public const byte RFID_CMD_GET_DUAL = 0x6C;
        public const byte RFID_CMD_GET_DUAL_RSP = 0x6D;

        // 寻标签过滤设置
        public const byte RFID_CMD_SET_SELECTMASKRULE = 0x6E;
        public const byte RFID_CMD_SET_SELECTMASKRULE_RSP = 0x6F;

        // 同时获取epc和tid模式
        public const byte RFID_CMD_SET_EPCTIDTOGETHER = 0x70;
        public const byte RFID_CMD_SET_EPCTIDTOGETHER_RSP = 0x71;
        public const byte RFID_CMD_GET_EPCTIDTOGETHER = 0x72;
        public const byte RFID_CMD_GET_EPCTIDTOGETHER_RSP = 0x73;

        // 恢复出厂设置
        public const byte RFID_CMD_RESTORY_FACTORY = 0x74;
        public const byte RFID_CMD_RESTORY_FACTORY_RSP = 0x75;

        //END 参数设置帧

        //标签操作类帧

        //单次寻标签
        public const byte RFID_CMD_INVENTORY_SINGLE = 0x80;
        public const byte RFID_CMD_INVENTORY_SINGLE_RSP = 0x81;

        //连续寻标签
        public const byte RFID_CMD_INVENTORY_MUL = 0x82;
        public const byte RFID_CMD_INVENTORY_MUL_RSP = 0x83;

        //读数据
        public const byte RFID_CMD_READ_DATA = 0x84;
        public const byte RFID_CMD_READ_DATA_RSP = 0x85;

        //写数据
        public const byte RFID_CMD_WRITE_DATA = 0x86;
        public const byte RFID_CMD_WRITE_DATA_RSP = 0x87;

        //锁标签
        public const byte RFID_CMD_LOCK_TAG = 0x88;
        public const byte RFID_CMD_LOCK_TAG_RSP = 0x89;

        //kill标签
        public const byte RFID_CMD_KILL_TAG = 0x8A;
        public const byte RFID_CMD_KILL_TAG_RSP = 0x8B;

        public const byte RFID_CMD_STOP_INVENTORY = 0x8C;
        public const byte RFID_CMD_STOP_INVENTORY_RSP = 0x8D;

        // BLOCK WRITE
        public const byte RFID_CMD_BLOCK_WRITE_DATA = 0x93;
        public const byte RFID_CMD_BLOCK_WRITE_DATA_RSP = 0x94;

        // BLOCK ERASE
        public const byte RFID_CMD_BLOCK_ERASE_DATA = 0x95;
        public const byte RFID_CMD_BLOCK_ERASE_DATA_RSP = 0x96;

        // QT命令参数设置
        public const byte RFID_CMD_SET_QT_COMMAND = 0x97;
        public const byte RFID_CMD_SET_QT_COMMAND_RSP = 0x98;
        public const byte RFID_CMD_GET_QT_COMMAND = 0x99;
        public const byte RFID_CMD_GET_QT_COMMAND_RSP = 0x9A;

        // QT Read
        public const byte RFID_CMD_QT_READ_DATA = 0x9B;
        public const byte RFID_CMD_QT_READ_DATA_RSP = 0x9C;

        // QT Write
        public const byte RFID_CMD_QT_WRITE_DATA = 0x9D;
        public const byte RFID_CMD_QT_WRITE_DATA_RSP = 0x9E;

        // Block Permalock
        public const byte RFID_CMD_BLOCKPERMALOCK = 0x9F;
        public const byte RFID_CMD_BLOCKPERMALOCK_RSP = 0xA0;

        //END 标签操作类帧

        /* 测试类帧 */
        //无功率补偿时，CW设置
        public const byte RFID_CMD_TEST_SET_CW_STATUE = 0xFB;
        public const byte RFID_CMD_TEST_SET_CW_STATUE_RSP = 0xFC;

        //操作失败帧
        public const byte RFID_CMD_FAILD_RSP = 0xFF;

        public const byte MAX_SIZE_OF_RFID = 128;

        //单次寻卡时，默认的操时时间为50ms
        public const byte SIGNAL_INV_TIMEOUT = 50;


        //错误类别
        public const byte HW_ERROR = 0x00;  //0x01 ---  0x21
        public const byte EB_ERROR = 0x30;  //0x31 ---  0x51
        public const byte HC_ERROR = 0x70;  //0x71 ---  0x91


    }
}
