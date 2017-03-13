using DayEasy.Core.Domain.Entities;

namespace DayEasy.Portal.Services.Dto
{
    public class CreateGroupDto : DDto
    {
        public byte Type { get; set; }
        public byte Stage { get; set; }
        public string AgencyId { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Summary { get; set; }
        public int Grade { get; set; }
        public int ChannelId { get; set; }
        public byte PostAuth { get; set; }
        public byte JoinAuth { get; set; }
        public string Tags { get; set; }
        public int SubjectId { get; set; }
        public bool IsManager { get; set; }
    }
}