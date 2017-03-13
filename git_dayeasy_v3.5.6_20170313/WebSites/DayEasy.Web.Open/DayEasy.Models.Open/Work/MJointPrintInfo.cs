
namespace DayEasy.Models.Open.Work
{
    public class MJointPrintInfo : MPrintInfo
    {
        public int ACount { get; set; }
        public int BCount { get; set; }

        public long TeacherId { get; set; }
        /// <summary> 发起人 </summary>
        public string TeacherName { get; set; }
    }
}
