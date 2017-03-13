using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking
{
    public class JointPictureSearchDto : DDto
    {
        public string JointBatch { get; set; }
        public string PaperId { get; set; }
        public string PictureId { get; set; }
        public string JointPictureId { get; set; }
        public long TeacherId { get; set; }
        public ICollection<string> Questions { get; set; }
        public RegionSpotDto Region { get; set; }
        public int UnMarkingCount { get; set; }
    }
}
