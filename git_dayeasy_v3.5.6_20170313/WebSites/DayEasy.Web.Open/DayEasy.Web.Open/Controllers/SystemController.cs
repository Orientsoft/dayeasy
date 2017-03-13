using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Enum;
using DayEasy.Models.Open.System;
using DayEasy.Utility;
using DayEasy.Web.Api;
using DayEasy.Web.Api.Attributes;
using DayEasy.Web.Api.Config;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using DayEasy.Services.Helper;

namespace DayEasy.Web.Open.Controllers
{
    /// <summary> 系统相关接口 </summary>
    [DApi]
    public class SystemController : DApiController
    {
        private readonly ISystemContract _systemContract;

        public SystemController(IUserContract userContract, ISystemContract systemContract)
            : base(userContract)
        {
            _systemContract = systemContract;
        }

        /// <summary> 科目列表 </summary>
        [HttpGet]
        [DApiAuthorize]
        public DResults<MSubjectDto> Subjects()
        {
            var subjects = SystemCache.Instance.Subjects().Select(t => new MSubjectDto
            {
                Id = t.Key,
                Name = t.Value
            }).ToList();
            return DResult.Succ(subjects, subjects.Count);
        }

        /// <summary> 知识点列表 </summary>
        [HttpGet]
        [DApiAuthorize]
        public DResults<MKnowledgeDto> KpList(byte stage_id, byte subject_id, int parent_id = 0)
        {
            var list = _systemContract.Knowledges(new SearchKnowledgeDto
            {
                Stage = stage_id,
                SubjectId = subject_id,
                ParentId = parent_id,
                Page = 0,
                Size = 50
            });
            return DResult.Succ(list.MapTo<List<MKnowledgeDto>>(), list.Count);
        }

        /// <summary> 版本信息 </summary>
        [HttpGet]
        public DResult<MManifestDto> Manifest(int type)
        {
            var manifest = ManifestInfo.Get(type).MapTo<MManifestDto>();
            if (manifest == null)
                return DResult.Error<MManifestDto>("没有该类型版本信息！");
            return DResult.Succ(manifest);
        }

        /// <summary> 区域列表 </summary>
        [HttpGet]
        public DResults<MAreaDto> Areas(int parentCode = 0)
        {
            var list = _systemContract.Areas(parentCode).MapTo<List<MAreaDto>>();
            return DResult.Succ(list, list.Count);
        }

        /// <summary> 机构列表 </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpGet]
        public DResults<AgencyDto> Agencies([FromUri]MAgencyInputDto dto)
        {
            var list =
                _systemContract.AgencyList((StageEnum)dto.Stage, dto.AreaCode, (AgencyType)dto.Type, dto.Keyword)
                    .ToList();
            return DResult.Succ(list, list.Count());
        }
    }
}
