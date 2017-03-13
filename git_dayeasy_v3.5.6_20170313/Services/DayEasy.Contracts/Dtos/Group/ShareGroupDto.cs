
using DayEasy.AutoMapper.Attributes;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using Newtonsoft.Json;

namespace DayEasy.Contracts.Dtos.Group
{
    public class ShareGroupDto : GroupDto
    {
        /// <summary> 分类Id </summary>
        [MapFrom("ClassType")]
        public int ChannelId { get; set; }
        /// <summary> 标签 </summary>
        [JsonIgnore]
        public string Tags { get; set; }

        public string[] TagList
        {
            get
            {
                return string.IsNullOrWhiteSpace(Tags)
                    ? new string[] { }
                    : Tags.JsonToObject<string[]>();
            }
            set { Tags = (value == null ? null : JsonHelper.ToJson(value, NamingType.CamelCase)); }
        }

        /// <summary> 发帖权限 </summary>
        public byte PostAuth { get; set; }
        /// <summary> 加圈权限 </summary>
        public byte JoinAuth { get; set; }
        /// <summary> 帖子数量 </summary>
        public int TopicNum { get; set; }
        /// <summary> 公告 </summary>
        public string Notice { get; set; }
    }
}
