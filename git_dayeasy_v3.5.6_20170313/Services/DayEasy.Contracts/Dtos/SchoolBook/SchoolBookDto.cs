using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.SchoolBook
{

    [AutoMapFrom(typeof(TS_SchoolBook))]
    public class SchoolBookDto : DDto
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int SubjectId { get; set; }
        public byte Stage { get; set; }
        public byte Status { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
