
using System.Collections.Generic;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Models;
using DayEasy.Utility;

namespace DayEasy.Contracts.Management
{
    public partial interface IManagementContract
    {
        DResults<TP_Paper> PaperSearch(PaperSearchDto searchDto);
    }
}
