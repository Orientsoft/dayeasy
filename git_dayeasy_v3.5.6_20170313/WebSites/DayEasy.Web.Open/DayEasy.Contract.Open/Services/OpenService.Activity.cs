using DayEasy.Contract.Open.Dtos;
using DayEasy.Contract.Open.Helper;
using DayEasy.Contracts.Models;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System.Collections.Generic;
using System.Linq;
using DayEasy.Core.Domain;

namespace DayEasy.Contract.Open.Services
{
    public partial class OpenService
    {
        public IVersion3Repository<TA_TeacherGod> TeacherGodRepository { private get; set; }
        public IDayEasyRepository<TS_Area, int> AreaRepository { private get; set; }

        private DResult CheckDto(VTeacherGodInputDto dto)
        {
            if (dto == null)
                return DResult.Error("生成失败！");
            if (dto.AreaCode <= 0)
                return DResult.Error("请选择地区！");
            if (string.IsNullOrEmpty(dto.School))
                return DResult.Error("请输入学校名称！");
            if (string.IsNullOrEmpty(dto.Name))
                return DResult.Error("请输入教师姓名或称呼！");
            if (string.IsNullOrEmpty(dto.Creator))
                return DResult.Error("请输入您的名字或称呼！");
            //重复数据判断
            var ip = Utils.GetRealIp();
            var now = Clock.Now;
            var item = TeacherGodRepository.FirstOrDefault(t => t.CreatorIp == ip && t.Name == dto.Name);
            if (item != null && (now - item.CreationTime).TotalSeconds < 10)
                return DResult.Error("提交过快，请稍后再试！");
            return DResult.Success;
        }

        private VTeacherGodDto ParseToDto(TA_TeacherGod model)
        {
            if (model == null)
                return null;
            var dto = new VTeacherGodDto
            {
                Id = model.Id,
                School = model.School,
                Name = model.Name,
                PosterUrl = model.PosterUrl,
                Rank = 0
            };
            dto.Index = TeacherGodRepository.Count(t => t.CreationTime < model.CreationTime) + 1;
            if (string.IsNullOrEmpty(model.Mobile))
                return dto;
            //当前排名
            var count = TeacherGodRepository.Count(t => t.Mobile == model.Mobile);
            var ranks = TeacherGodRepository.Where(t => t.Mobile != null && t.Mobile.Length > 0)
                .Select(t => t.Mobile)
                .GroupBy(t => t)
                .Count(t => t.Count() > count);
            dto.Rank = ranks + 1;
            return dto;
        }

        /// <summary> 制作名师大神海报 </summary>
        public DResult<VTeacherGodDto> MakeTeacherGod(VTeacherGodInputDto dto)
        {
            var checkResult = CheckDto(dto);
            if (!checkResult.Status)
                return DResult.Error<VTeacherGodDto>(checkResult.Message);
            var posterUrl = PosterHelper.MakePoster(dto);
            if (string.IsNullOrEmpty(posterUrl))
                return DResult.Error<VTeacherGodDto>("海报制作失败，请稍候再来！");
            var model = new TA_TeacherGod
            {
                Id = IdHelper.Instance.Guid32,
                AreaCode = dto.AreaCode,
                School = dto.School,
                Name = dto.Name,
                Creator = dto.Creator,
                Type = dto.Type,
                Word = dto.Word,
                PosterUrl = posterUrl,
                CreationTime = Clock.Now,
                CreatorIp = Utils.GetRealIp()
            };
            var result = TeacherGodRepository.Insert(model);
            return string.IsNullOrEmpty(result)
                ? DResult.Error<VTeacherGodDto>("海报制作失败，请稍候再来！")
                : DResult.Succ(ParseToDto(model));
        }

        /// <summary> 名师大神海报 </summary>
        public VTeacherGodDto TeacherGod(string id)
        {
            //            return new VTeacherGodDto();
            if (string.IsNullOrEmpty(id))
                return null;
            var model = TeacherGodRepository.Load(id);
            return ParseToDto(model);
        }

        /// <summary> 提交教师手机号码 </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public DResult TeacherMobile(VTeacherMobileInputDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Id))
                return DResult.Error("数据异常，提交失败！");
            if (!dto.Mobile.As<IRegex>().IsMobile())
                return DResult.Error("请输入正确的手机号码！");
            var model = TeacherGodRepository.Load(dto.Id);
            if (model == null)
                return DResult.Error("名师大神未找到！！");
            model.Mobile = dto.Mobile;
            var result = TeacherGodRepository.Update(model, "Mobile");
            return DResult.FromResult(result);
        }

        public List<DKeyValue<int, string>> Areas(int code = 0)
        {
            var models = AreaRepository.Where(t => t.ParentCode == code)
                .Select(t => new
                {
                    t.Id,
                    t.Name
                }).ToList();
            return models.Select(t => new DKeyValue<int, string>(t.Id, t.Name)).ToList();
        }
    }
}
