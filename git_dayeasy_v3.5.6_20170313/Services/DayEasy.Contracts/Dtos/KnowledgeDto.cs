using System.Collections.Generic;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos
{
    [AutoMapFrom(typeof(TS_Knowledge))]
    public class KnowledgeDto : DDto
    {
        public int Id { get; set; }

        [MapFrom("PID")]
        public int ParentId { get; set; }
        public string Code { get; set; }

        public string ParentCode { get; set; }
        public string Name { get; set; }
        public int Sort { get; set; }
        public int ErrCount { get; set; }

        [MapFrom("HasChildren")]
        public bool IsParent { get; set; }

        public Dictionary<string, string> Parents { get; set; }

        public KnowledgeDto()
        {
            Parents = new Dictionary<string, string>();
        }
    }
}
