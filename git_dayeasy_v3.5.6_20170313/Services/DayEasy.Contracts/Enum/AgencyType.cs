using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 机构类型 </summary>
    public enum AgencyType : byte
    {
        [Description("K12(小学、初中、高中)")]
        K12 = 1,
        //[Description("职业教育(中职、高职)")]
        //Occupation = 2,
        //[Description("高等教育")]
        //Higher = 3,
        //[Description("培训机构")]
        //Training = 4,
        //[Description("幼儿教育")]
        //Kindergarten = 5,
        [Description("其他")]
        Other = 6
    }
}
