using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;

namespace DayEasy.Contracts.Dtos.SchoolBook
{
    [AutoMapFrom(typeof(TS_SchoolBookChapter))]
    public class SchoolBookChapterDto : DDto
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Knowledge { get; set; }

        private List<NameDto> _KnowledgeList;

        public List<NameDto> KnowledgeList
        {
            get
            {
                if (_KnowledgeList != null) return _KnowledgeList;
                if (Knowledge.IsNullOrEmpty()) return null;
                return JsonHelper.JsonList<NameDto>(Knowledge).ToList();
            }
            set { _KnowledgeList = value; }
        }

        public byte Status { get; set; }
        public int Sort { get; set; }
        public bool HasChild { get; set; }
        public bool IsLast { get; set; }
    }
}
