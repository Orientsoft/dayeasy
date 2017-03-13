using DayEasy.Core.Domain;

namespace DayEasy.Contracts.Dtos.Download
{
    /// <summary> 下载日志查询类 </summary>
    public class DownloadLogSearchDto : DPage
    {
        /// <summary> 类型 </summary>
        public int Type { get; set; }
        public string AgencyId { get; set; }
        /// <summary> 关键字 </summary>
        public string Keyword { get; set; }
    }
}
