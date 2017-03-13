using DayEasy.Contract.Open.Contracts;
using DayEasy.Contract.Open.Dtos;
using DayEasy.Core.Domain;
using DayEasy.Utility;
using System.Collections.Generic;
using System.Web.Http;

namespace DayEasy.Web.Open.Controllers
{
    [RoutePrefix("activity")]
    public class ActivityController : ApiController
    {
        private readonly IOpenContract _openContract;
        public ActivityController(IOpenContract openContract)
        {
            _openContract = openContract;
        }

        [HttpPost]
        [Route("make-poster")]
        public DResult<VTeacherGodDto> MakeGodPoster(VTeacherGodInputDto dto)
        {
            return _openContract.MakeTeacherGod(dto);
        }

        [HttpPost]
        [Route("teacher-mobile")]
        public DResult TeacherMobile(VTeacherMobileInputDto dto)
        {
            return _openContract.TeacherMobile(dto);
        }

        [HttpGet]
        [Route("god-poster/{id}")]
        public VTeacherGodDto GodPoster(string id)
        {
            return _openContract.TeacherGod(id);
        }

        [HttpGet]
        [Route("areas")]
        public List<DKeyValue<int, string>> Areas(int code=0)
        {
            return _openContract.Areas(code);
        }
    }
}