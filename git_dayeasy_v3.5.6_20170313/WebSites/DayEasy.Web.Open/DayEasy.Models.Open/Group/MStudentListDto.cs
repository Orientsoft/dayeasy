using System.Collections.Generic;
using DayEasy.Models.Open.User;

namespace DayEasy.Models.Open.Group
{
    public class MStudentListDto
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public IList<MUserBaseDto> Students { get; set; }

        public MStudentListDto()
        {
            Students = new List<MUserBaseDto>();
        }
    }
}
