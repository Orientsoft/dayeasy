using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking.Joint
{
    /// <summary> 区域 </summary>
    public class DArea : DDto
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float W { get; set; }
        public float H { get; set; }
    }

    public class DMark : DDto
    {
        public string Id { get; set; }
        public string W { get; set; }
        public int T { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }
}
