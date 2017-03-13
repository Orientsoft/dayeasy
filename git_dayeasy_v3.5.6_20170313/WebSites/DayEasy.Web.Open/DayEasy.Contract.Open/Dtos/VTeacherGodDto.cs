using System;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contract.Open.Dtos
{
    [Serializable]
    public class VTeacherGodDto : DDto
    {
        public string Id { get; set; }
        public string School { get; set; }
        public string Name { get; set; }
        public string PosterUrl { get; set; }
        public int Rank { get; set; }
        public int Index { get; set; }
    }
}
