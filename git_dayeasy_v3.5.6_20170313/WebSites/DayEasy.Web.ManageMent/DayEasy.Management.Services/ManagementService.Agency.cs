using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Models;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.Management.Services
{
    public partial class ManagementService
    {
        public Dictionary<int, string> GetAreas(List<int> codes)
        {
            var dict = new Dictionary<int, string>();
            var parentsDict = new Dictionary<int, List<int>>();
            foreach (var id in codes)
            {
                var keyString = id.ToString();
                var parentIds = new List<int>();
                while (keyString.Length > 2)
                {
                    keyString = keyString.Substring(0, keyString.Length - 2);
                    parentIds.Add(keyString.CastTo(0));
                }
                parentsDict.Add(id, parentIds);
            }
            var codeList = parentsDict.Values.SelectMany(t => t).Distinct().ToList();
            codeList.AddRange(codes);
            var areaDict = AreaRepository.Where(a => codeList.Contains(a.Id))
                .Select(a => new { a.Id, a.Name })
                .ToList()
                .ToDictionary(k => k.Id, v => v.Name);
            foreach (var item in parentsDict)
            {
                if (!areaDict.ContainsKey(item.Key))
                {
                    dict.Add(item.Key, "无区域");
                    continue;
                }
                var name = areaDict[item.Key];
                name = item.Value.Aggregate(name, (current, code) =>
                {
                    if (!areaDict.ContainsKey(code))
                        return current;
                    return areaDict[code] + " " + current;
                });
                dict.Add(item.Key, name);
            }
            return dict;
        }

        public DResults<AgencyDto> AgencySearch(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return DResult.Errors<AgencyDto>("关键字不能为空！");
            var list = AgencyRepository.Where(a => a.AgencyName.Contains(keyword))
                .Take(10)
                .Select(t => new
                {
                    t.Id,
                    Name = t.AgencyName,
                    t.Stage,
                    t.AreaCode
                }).ToList();
            if (!list.Any())
                return DResult.Errors<AgencyDto>("未找到机构！");
            var agencyList = new List<AgencyDto>();
            var areas = GetAreas(list.Select(t => t.AreaCode).Distinct().ToList());
            list.ForEach(t =>
            {
                var item = new AgencyDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    StageCode = t.Stage,
                    Stage = t.Stage.GetEnumText<StageEnum, byte>(),
                    Area = string.Empty
                };
                if (areas.ContainsKey(t.AreaCode))
                    item.Area = areas[t.AreaCode];
                agencyList.Add(item);
            });
            return DResult.Succ(agencyList, agencyList.Count);
        }

        public DResults<TS_Agency> AgencySearch(AgencySearchDto dto)
        {
            Expression<Func<TS_Agency, bool>> condition = a => a.Status == (byte)NormalStatus.Normal;
            if (dto.Stage > 0)
            {
                condition = condition.And(a => a.Stage == dto.Stage);
            }
            if (dto.Level >= 0)
            {
                condition = condition.And(a => a.CertificationLevel == dto.Level);
            }
            if (!string.IsNullOrWhiteSpace(dto.Keyword))
            {
                condition =
                    condition.And(
                        a => a.AgencyName.Contains(dto.Keyword) || a.AreaCode.ToString().StartsWith(dto.Keyword));
            }
            var list = AgencyRepository.Where(condition)
                .OrderByDescending(a => a.Sort)
                .Skip(dto.Page * dto.Size)
                .Take(dto.Size)
                .ToList();
            return DResult.Succ(list, AgencyRepository.Count(condition));
        }

        public AgencyDto Agency(string agencyId)
        {
            var item = AgencyRepository.Load(agencyId);
            if (item == null)
                return null;
            var agency = new AgencyDto
            {
                Id = item.Id,
                Name = item.AgencyName,
                StageCode = item.Stage,
                Stage = item.Stage.GetEnumText<StageEnum, byte>()
            };
            var parents = GetAreas(new List<int> { item.AreaCode });
            if (parents.ContainsKey(item.AreaCode))
                agency.Area = parents[item.AreaCode];
            return agency;
        }
        public DResult CertificateAgency(string id)
        {
            var result = AgencyRepository.Update(new TS_Agency
            {
                CertificationLevel = (byte)CertificationLevel.Official
            }, a => a.Id == id, "CertificationLevel");
            return DResult.FromResult(result);
        }

        public DResult EditAgency(AgencyEditDto dto)
        {
            var attrs = new List<string>();
            if (!string.IsNullOrWhiteSpace(dto.Name))
                attrs.Add("AgencyName");
            if (!string.IsNullOrWhiteSpace(dto.Summary))
                attrs.Add("Summary");
            if (!string.IsNullOrWhiteSpace(dto.Logo))
                attrs.Add("AgencyLogo");
            if (dto.Sort >= 0)
                attrs.Add("Sort");
            var result = AgencyRepository.Update(new TS_Agency
            {
                AgencyName = dto.Name,
                AgencyLogo = dto.Logo,
                Summary = dto.Summary,
                Sort = dto.Sort
            }, a => a.Id == dto.Id, attrs.ToArray());
            return DResult.FromResult(result);
        }

        public DResult AddAgency(AgencyInputDto dto)
        {
            if (dto == null)
                return DResult.Error("参数异常");
            if (string.IsNullOrWhiteSpace(dto.Name))
                return DResult.Error("机构名称不能为空");
            if (!Enum.IsDefined(typeof(AgencyType), dto.Type))
                return DResult.Error("机构类型异常");
            if (dto.Code <= 0)
                return DResult.Error("所属区域异常");
            if (dto.Stages == null || !dto.Stages.Any())
                return DResult.Error("机构所在学段异常");
            var exists = AgencyRepository.Exists(t => t.AgencyName == dto.Name && dto.Stages.Contains(t.Stage));
            if (exists)
                return DResult.Error("机构对应学段已存在");
            var idHelper = IdHelper.Instance;
            var models = dto.Stages.Select(stage => new TS_Agency
            {
                Id = idHelper.Guid32,
                AgencyName = dto.Name,
                AreaCode = dto.Code,
                Stage = (byte)stage,
                Status = (byte)NormalStatus.Normal,
                AgencyType = dto.Type,
                Sort = 0
            }).ToArray();
            var result = AgencyRepository.Insert(models);
            return DResult.FromResult(result);
        }
    }
}
