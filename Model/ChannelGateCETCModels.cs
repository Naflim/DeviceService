using System;
using System.Xml.Serialization;

namespace DeviceService.Model.GpoControl
{

    // 注意: 生成的代码可能至少需要 .NET Framework 4.5 或 .NET Core/Standard 2.0。
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class RFIDMessage
    {

        private decimal versionField;

        private byte sequenceField;

        private string commandTypeField;

        private string commandField;

        private RFIDMessageGpo[] paramsField;

        /// <remarks/>
        public decimal Version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }

        /// <remarks/>
        public byte Sequence
        {
            get
            {
                return this.sequenceField;
            }
            set
            {
                this.sequenceField = value;
            }
        }

        /// <remarks/>
        public string CommandType
        {
            get
            {
                return this.commandTypeField;
            }
            set
            {
                this.commandTypeField = value;
            }
        }

        /// <remarks/>
        public string Command
        {
            get
            {
                return this.commandField;
            }
            set
            {
                this.commandField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Gpo", IsNullable = false)]
        public RFIDMessageGpo[] Params
        {
            get
            {
                return this.paramsField;
            }
            set
            {
                this.paramsField = value;
            }
        }

        public RFIDMessage() { }

        public RFIDMessage(RFIDMessageGpo[] gpo)
        {
            versionField = 0.1M;
            sequenceField = 1;
            commandTypeField = "REQUEST";
            commandField = "GPOCONTROL";
            paramsField = gpo;
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RFIDMessageGpo
    {

        private byte gpoPinField;

        private byte eleLevelField;

        /// <remarks/>
        public byte GpoPin
        {
            get
            {
                return this.gpoPinField;
            }
            set
            {
                this.gpoPinField = value;
            }
        }

        /// <remarks/>
        public byte EleLevel
        {
            get
            {
                return this.eleLevelField;
            }
            set
            {
                this.eleLevelField = value;
            }
        }
        public RFIDMessageGpo()
        {

        }

        public RFIDMessageGpo(byte pin,byte level)
        {
            gpoPinField = pin;
            eleLevelField = level;
        }
    }


}

namespace DeviceService.Model
{

    // 注意: 生成的代码可能至少需要 .NET Framework 4.5 或 .NET Core/Standard 2.0。
    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRoot(Namespace = "", IsNullable = false)]
    public partial class RFIDMessage
    {

        private decimal versionField;

        private uint sequenceField;

        private string commandTypeField;

        private string commandField;

        private string whichCommandField;

        private byte statusField;

        private string descriptionField;

        private object paramsField;

        public RFIDMessage() { }

        public RFIDMessage(string cmd)
        {
            versionField = 0.1M;
            sequenceField = 1;
            commandTypeField = "REQUEST";
            commandField = cmd;
        }

        /// <remarks/>
        public decimal Version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }

        /// <remarks/>
        public uint Sequence
        {
            get
            {
                return this.sequenceField;
            }
            set
            {
                this.sequenceField = value;
            }
        }

        /// <remarks/>
        public string CommandType
        {
            get
            {
                return this.commandTypeField;
            }
            set
            {
                this.commandTypeField = value;
            }
        }

        /// <remarks/>
        public string Command
        {
            get
            {
                return this.commandField;
            }
            set
            {
                this.commandField = value;
            }
        }


        /// <remarks/>
        public string WhichCommand
        {
            get
            {
                return this.whichCommandField;
            }
            set
            {
                this.whichCommandField = value;
            }
        }

        /// <remarks/>
        public byte Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        public object Params
        {
            get
            {
                return this.paramsField;
            }
            set
            {
                this.paramsField = value;
            }
        }
    }

    public partial class SendRFIDMessage
    {

        private decimal versionField;

        private byte sequenceField;

        private string commandTypeField;

        private string commandField;

        private dynamic paramsField;

        public SendRFIDMessage() { }

        public SendRFIDMessage(string cmd)
        {
            versionField = 0.1M;
            sequenceField = 1;
            commandTypeField = "REQUEST";
            commandField = cmd;
        }

        /// <remarks/>
        public decimal Version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }

        /// <remarks/>
        public byte Sequence
        {
            get
            {
                return this.sequenceField;
            }
            set
            {
                this.sequenceField = value;
            }
        }

        /// <remarks/>
        public string CommandType
        {
            get
            {
                return this.commandTypeField;
            }
            set
            {
                this.commandTypeField = value;
            }
        }

        /// <remarks/>
        public string Command
        {
            get
            {
                return this.commandField;
            }
            set
            {
                this.commandField = value;
            }
        }

        /// <remarks/>
        public dynamic Params
        {
            get
            {
                return this.paramsField;
            }
            set
            {
                this.paramsField = value;
            }
        }
    }


    // 注意: 生成的代码可能至少需要 .NET Framework 4.5 或 .NET Core/Standard 2.0。
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class ResultRFIDMessage
    {

        private decimal versionField;

        private byte sequenceField;

        private string commandTypeField;

        private string whichCommandField;

        private byte statusField;

        private string descriptionField;

        private dynamic paramsField;

        /// <remarks/>
        public decimal Version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }

        /// <remarks/>
        public byte Sequence
        {
            get
            {
                return this.sequenceField;
            }
            set
            {
                this.sequenceField = value;
            }
        }

        /// <remarks/>
        public string CommandType
        {
            get
            {
                return this.commandTypeField;
            }
            set
            {
                this.commandTypeField = value;
            }
        }

        /// <remarks/>
        public string WhichCommand
        {
            get
            {
                return this.whichCommandField;
            }
            set
            {
                this.whichCommandField = value;
            }
        }

        /// <remarks/>
        public byte Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        public dynamic Params
        {
            get
            {
                return this.paramsField;
            }
            set
            {
                this.paramsField = value;
            }
        }

        public ResultRFIDMessage() { }

        public ResultRFIDMessage(string cmd)
        {
            versionField = 0.1M;
            sequenceField = 1;
            commandTypeField = "REQUEST";
            whichCommandField = cmd;
            statusField = 200;
            descriptionField = "Successful!";
        }
    }

    // 注意: 生成的代码可能至少需要 .NET Framework 4.5 或 .NET Core/Standard 2.0。
    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRoot(Namespace = "", IsNullable = false)]
    public partial class InventoryReport
    {

        private ulong deviceIDField;

        private string timeStampField;

        private OpResultItem[] opResultItemField;

        /// <remarks/>
        public ulong DeviceID
        {
            get
            {
                return this.deviceIDField;
            }
            set
            {
                this.deviceIDField = value;
            }
        }

        /// <remarks/>
        public string TimeStamp
        {
            get
            {
                return this.timeStampField;
            }
            set
            {
                this.timeStampField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("OpResultItem")]
        public OpResultItem[] OpResultItem
        {
            get
            {
                return this.opResultItemField;
            }
            set
            {
                this.opResultItemField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    public partial class OpResultItem
    {

        private byte antennaIDField;

        private string ePCField;

        private sbyte rSSIField;

        private decimal frequencyField;

        private object tIDField;

        private byte inOutTypeField;

        /// <remarks/>
        public byte AntennaID
        {
            get
            {
                return this.antennaIDField;
            }
            set
            {
                this.antennaIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string EPC
        {
            get
            {
                return this.ePCField;
            }
            set
            {
                this.ePCField = value;
            }
        }

        /// <remarks/>
        public sbyte RSSI
        {
            get
            {
                return this.rSSIField;
            }
            set
            {
                this.rSSIField = value;
            }
        }

        /// <remarks/>
        public decimal Frequency
        {
            get
            {
                return this.frequencyField;
            }
            set
            {
                this.frequencyField = value;
            }
        }

        /// <remarks/>
        public object TID
        {
            get
            {
                return this.tIDField;
            }
            set
            {
                this.tIDField = value;
            }
        }

        /// <remarks/>
        public byte InOutType
        {
            get
            {
                return this.inOutTypeField;
            }
            set
            {
                this.inOutTypeField = value;
            }
        }
    }

}
