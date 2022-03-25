using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Model
{
    public class PersonnelModel
    {
        /// <summary>
        /// 性别
        /// </summary>
        public enum PersonnelSex { man, woman, other }

        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 人员性别
        /// </summary>
        public PersonnelSex Sex { get; set; }

        /// <summary>
        /// 年龄
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// 卡号
        /// </summary>
        public string CardID { get; set; }

        /// <summary>
        /// 工号
        /// </summary>
        public uint EmployeeID { get; set; }

        /// <summary>
        /// 门权限
        /// </summary>
        public string DoorRight { get; set; }
    }
}
