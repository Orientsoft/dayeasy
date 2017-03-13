
namespace DayEasy.Models.Open.System
{
    /// <summary> 知识点实体 </summary>
    public class MKnowledgeDto : DDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Sort { get; set; }
        public int ParentId { get; set; }
        public bool IsParent { get; set; }
    }
}
