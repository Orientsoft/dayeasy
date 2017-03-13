using DayEasy.Core.Domain;

namespace DayEasy.Contracts.Management.Dto
{
    public class UserSearchDto : DPage
    {
        public string Keyword { get; set; }
        public int Role { get; set; }
        public int ValidationType { get; set; }

        public UserSearchDto()
        {
            Role = -1;
            ValidationType = -1;
        }
    }
}
