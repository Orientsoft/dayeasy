
using DayEasy.Core.Domain;

namespace DayEasy.Contracts.Dtos
{
    /// <summary> 知识点搜索传输对象 </summary>
    public class SearchKnowledgeDto : DPage
    {
        public byte Stage { get; set; }
        public int SubjectId { get; set; }

        public int ParentId { get; set; }

        public string ParentCode { get; set; }

        public string Keyword { get; set; }

        public byte? Version { get; set; }
        public bool LoadPath { get; set; }
        public bool IsLast { get; set; }
    }
}
