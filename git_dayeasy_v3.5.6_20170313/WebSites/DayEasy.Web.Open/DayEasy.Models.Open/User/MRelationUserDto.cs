
namespace DayEasy.Models.Open.User
{
    public class MRelationUserDto:MUserBaseDto
    {
        /// <summary> 邮箱/昵称(没有邮箱就显示昵称) </summary>
        public string Account { get; set; }
        public byte RelationType { get; set; }
    }
}
