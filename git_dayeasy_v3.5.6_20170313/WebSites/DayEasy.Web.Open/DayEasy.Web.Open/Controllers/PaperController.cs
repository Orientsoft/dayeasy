using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.Core.Config;
using DayEasy.Models.Open.Paper;
using DayEasy.Utility;
using DayEasy.Web.Api;
using DayEasy.Web.Api.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using DayEasy.Services.Helper;

namespace DayEasy.Web.Open.Controllers
{
    /// <summary> 试卷相关接口 </summary>
    [DApiAuthorize(UserRole.Teacher | UserRole.Student)]
    public class PaperController : DApiController
    {
        private readonly IPaperContract _paperContract;

        public PaperController(IUserContract userContract, IPaperContract paperContract)
            : base(userContract)
        {
            _paperContract = paperContract;
        }

        private MPaperDto ConvertPaperDto(PaperDetailDto dto)
        {
            if (dto == null)
                return null;
            var paper = dto.PaperBaseInfo.MapTo<MPaperDto>();
            paper.Sections = dto.PaperSections.MapTo<List<MPaperSectionDto>>();
            var types =
                SystemCache.Instance.QuestionTypes().Where(t => t.Multi).Select(t => t.Id).ToList();
            paper.Sections.ForEach(s =>
            {
                var item = dto.PaperSections.FirstOrDefault(t => t.SectionID == s.SectionId);
                if (item != null)
                {
                    s.Questions = new List<MPaperQuestionDto>();
                    var questions = item.Questions;
                    foreach (var question in questions)
                    {
                        var paperQuestion = question.Question.MapTo<MPaperQuestionDto>();
                        paperQuestion.Score = question.Score;
                        paperQuestion.Sort = question.Sort;
                        paperQuestion.HasMulti = types.Contains(question.Question.Type);
                        s.Questions.Add(paperQuestion);
                    }
                }
            });
            return paper;
        }

        /// <summary> 问题信息 </summary>
        [HttpGet]
        public DResult<MQuestionDto> Question(string id)
        {
            var item = _paperContract.LoadQuestion(id);
            if (item == null)
                return DResult.Error<MQuestionDto>("问题不存在！");
            return DResult.Succ(item.MapTo<MQuestionDto>());
        }

        /// <summary> 试卷信息 </summary>
        [HttpGet]
        public DResult<MPaperDto> Info(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return DResult.Error<MPaperDto>("试卷Id或试卷编号不能为空！");
            var result = (keyword.Length == 32
                ? _paperContract.PaperDetailById(keyword)
                : _paperContract.PaperDetailByPaperNo(keyword));

            if (result.Status)
            {
                if (result.Data == null || result.Data.PaperBaseInfo.Status != (byte)PaperStatus.Normal)
                    return DResult.Error<MPaperDto>("试卷不存在~！");
                if (!CurrentUser.Code.HasSpecialAuth(SpecialAccountType.ScannerManager) && CurrentUser.IsTeacher() &&
                    result.Data.PaperBaseInfo.SubjectId != CurrentUser.SubjectId)
                    return DResult.Error<MPaperDto>("不能查看其他科目的试卷！");
                return DResult.Succ(ConvertPaperDto(result.Data));
            }
            return DResult.Error<MPaperDto>(result.Message);
        }
    }
}
