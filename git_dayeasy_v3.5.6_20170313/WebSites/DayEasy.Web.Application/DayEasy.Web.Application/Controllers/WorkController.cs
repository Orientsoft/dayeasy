using DayEasy.Application.Services.Dto;
using DayEasy.Contracts;
using DayEasy.Contracts.Enum;

namespace DayEasy.Web.Application.Controllers
{
    /// <summary> 作业中心/考试中心基础Controller </summary>
    public class WorkController : DController
    {
        protected readonly IPaperContract PaperContract;
        protected readonly IPublishContract PublishContract;
        protected readonly IGroupContract GroupContract;
        protected readonly IStatisticContract StatisticContract;
        public WorkController(
            IUserContract userContract,
            IPaperContract paperContract,
            IPublishContract publishContract,
            IGroupContract groupContract,
            IStatisticContract statisticContract)
            : base(userContract)
        {
            PaperContract = paperContract;
            PublishContract = publishContract;
            StatisticContract = statisticContract;
            GroupContract = groupContract;
            ViewBag.PageNav = 2;
        }

        protected VWorkDto GetWorkDto(string batch, string paperId, int step)
        {
            var dto = new VWorkDto(batch, paperId, step);
            var publishResult = PublishContract.GetUsageDetail(batch);
            if (publishResult.Status && publishResult.Data != null)
            {
                dto.PublishType = publishResult.Data.SourceType;
                dto.ColleagueId = publishResult.Data.ColleagueGroupId;
            }

            var groupResult = PublishContract.LoadByBatch(batch);
            if (groupResult.Status)
            {
                dto.ClassId = groupResult.Data.Id;
                dto.ClassName = groupResult.Data.Name;
            }

            var paperResult = PaperContract.PaperDetailById(paperId, false);
            if (paperResult.Status && paperResult.Data != null)
            {
                dto.Score = paperResult.Data.PaperBaseInfo.PaperScores.TScore;
                dto.ScoreA = paperResult.Data.PaperBaseInfo.PaperScores.TScoreA;
                dto.ScoreB = paperResult.Data.PaperBaseInfo.PaperScores.TScoreB;
                dto.IsAb = paperResult.Data.PaperBaseInfo.PaperType == (byte)PaperType.AB;
                dto.PaperTitle = paperResult.Data.PaperBaseInfo.PaperTitle;
            }
            return dto;
            }
    }
}