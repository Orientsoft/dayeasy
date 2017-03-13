using System.Drawing;

namespace DayEasy.Models.Open.Work
{
    public class SymbolBaseInfo
    {
        public PointF Point { get; set; }
        public int SymbolType { get; set; }

    }

    public class SymbolInfo : SymbolBaseInfo
    {
        public double Score { get; set; }
    }

    public class CommentInfo : SymbolBaseInfo
    {
        public string Words { get; set; }
    }


    public class SymbolTag : DDto
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int T { get; set; }
        public string W { get; set; }

        public PointF Point
        {
            get { return new PointF(X, Y); }
        }
    }
}
