using System;
using System.Linq;
using System.Linq.Expressions;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Models;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;

namespace DayEasy.Management.Services
{
    public partial class ManagementService
    {
        public IDayEasyRepository<TP_Paper> PaperRepository { private get; set; }

        public DResults<TP_Paper> PaperSearch(PaperSearchDto searchDto)
        {
            Expression<Func<TP_Paper, bool>> condition = p => true;
            if (searchDto.Subject > 0)
            {
                condition = condition.And(p => p.SubjectID == searchDto.Subject);
            }
            if (searchDto.ShareRange >= 0)
            {
                condition = condition.And(p => p.ShareRange == searchDto.ShareRange);
            }
            if (searchDto.Status >= 0)
            {
                condition = condition.And(p => p.Status == searchDto.Status);
            }
            if (!string.IsNullOrWhiteSpace(searchDto.Keyword))
            {
                condition =
                    condition.And(p => p.PaperTitle.Contains(searchDto.Keyword) || p.PaperNo == searchDto.Keyword);
            }
            var list =
                PaperRepository.Where(condition)
                    .OrderByDescending(p => p.AddedAt)
                    .Skip(searchDto.Page*searchDto.Size)
                    .Take(searchDto.Size)
                    .ToList();
            var count = PaperRepository.Count(condition);
            return DResult.Succ(list, count);
        }
    }
}
