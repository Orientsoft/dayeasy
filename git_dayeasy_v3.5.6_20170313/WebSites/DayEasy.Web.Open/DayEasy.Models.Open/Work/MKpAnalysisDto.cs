
namespace DayEasy.Models.Open.Work
{
    /// <summary>
    /// 成绩分析-知识点错题统计
    /// </summary>
    public class MKpAnalysisDto : DDto
    {
        public string KpName { get; set; }
        public string KpCode { get; set; }
        public int ErrorCount { get; set; }
    }
}
