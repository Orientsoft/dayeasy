using DayEasy.Contracts.Dtos.User;
using DayEasy.Core.Domain;
using System.Collections.Generic;

namespace DayEasy.Portal.Services.Dto
{
    public class VHotTeacherDto : DUserDto
    {
        public string Code { get; set; }
        public int SubjectId { get; set; }
        public string Subject { get; set; }
        public byte Level { get; set; }
        public List<DKeyValue<string, int>> Impressions { get; set; }

        public VHotTeacherDto()
        {
            Impressions = new List<DKeyValue<string, int>>();
        }
    }
}
