

namespace DayEasy.Contracts.Dtos.User
{
    public class RelationUserDto : DUserDto
    {
        /// <summary> 邮箱/昵称(没有邮箱就显示昵称) </summary>
        public string Account { get; set; }
        public byte RelationType { get; set; }
        public string Mobile { get; set; }
    }
}
