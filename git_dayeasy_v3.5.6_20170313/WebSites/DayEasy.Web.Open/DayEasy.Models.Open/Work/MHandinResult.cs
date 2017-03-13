
namespace DayEasy.Models.Open.Work
{
    /// <summary> 提交扫描图片返回结果 </summary>
    public class MHandinResult : DDto
    {
        /// <summary> 提交时的ID </summary>
        public string Id { get; set; }

        /// <summary> 提交状态 </summary>
        public bool Status { get; set; }

        /// <summary> 错误信息 </summary>
        public string Message { get; set; }

        public MHandinResult()
            : this(string.Empty)
        {
        }

        public MHandinResult(string id)
        {
            Id = id;
            Status = true;
        }

        public MHandinResult(string id, string msg)
        {
            Id = id;
            Status = false;
            Message = msg;
        }
    }
}
