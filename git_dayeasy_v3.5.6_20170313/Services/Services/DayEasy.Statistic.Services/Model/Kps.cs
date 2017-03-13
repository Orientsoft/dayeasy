
namespace DayEasy.Statistic.Services.Model
{
    /// <summary>
    /// 用于数据组装
    /// </summary>
    public class Kps
    {
        public int KpID { get; set; }
        public string KpLayerCode { get; set; }
        public int AnswerCount { get; set; }
        public int ErrorCount { get; set; }
    }
}
