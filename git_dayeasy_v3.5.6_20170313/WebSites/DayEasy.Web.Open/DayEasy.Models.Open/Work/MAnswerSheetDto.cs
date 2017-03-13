
namespace DayEasy.Models.Open.Work
{
    /// <summary> 答题卡返回类 </summary>
    public class MAnswerSheetDto : DDto
    {
        public string Id { get; set; }
        public int Type { get; set; }
        public string Picture { get; set; }
        public decimal Score { get; set; }
        public string ObjectError { get; set; }

        public string Marks { get; set; }
        public string Markings { get; set; }
    }
}
