using DayEasy.Contracts.Dtos.Download;
using DayEasy.Contracts.Models;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    public partial interface ISystemContract
    {
        /// <summary> 创建下载日志 </summary>
        TS_DownloadLog CreateDownload(DownloadLogInputDto dto);
        /// <summary> 完成下载 </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        DResult CompleteDownload(TS_DownloadLog log);

        /// <summary> 查询下载日志 </summary>
        DResults<DownloadLogDto> DownloadLogs(DownloadLogSearchDto searchDto);
    }
}
