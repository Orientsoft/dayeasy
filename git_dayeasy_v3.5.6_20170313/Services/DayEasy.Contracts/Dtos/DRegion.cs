using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos
{
    /// <summary> 区域类 </summary>
    public class DRegion : DDto
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public DRegion() { }

        public DRegion(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
