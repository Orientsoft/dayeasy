using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Models;
using DayEasy.Utility;

namespace DayEasy.Contracts.Management
{
    public partial interface IManagementContract
    {
        Dictionary<int, string> GetAreas(List<int> codes);

        DResults<AgencyDto> AgencySearch(string keyword);

        DResults<TS_Agency> AgencySearch(AgencySearchDto dto);
        AgencyDto Agency(string agencyId);

        /// <summary> 认证机构 </summary>
        DResult CertificateAgency(string id);

        DResult EditAgency(AgencyEditDto dto);

        DResult AddAgency(AgencyInputDto dto);
    }
}
