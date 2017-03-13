using DayEasy.Contract.Open.Contracts;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.Core.Config;
using DayEasy.Models.Open.Paper;
using DayEasy.Models.Open.Work;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Logging;
using DayEasy.Web.Api;
using DayEasy.Web.Api.Attributes;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace DayEasy.Web.Open.Controllers
{
    /// <summary> 作业/考试相关接口 </summary>
    [DApiAuthorize]
    public class WorkController : DApiController
    {
        private readonly ILogger _logger = LogManager.Logger<WorkController>();
        private readonly IOpenContract _openContract;
        private readonly IExaminationContract _examinationContract;
        private readonly IStatisticContract _statisticContract;
        private readonly IPublishContract _publishContract;

        public WorkController(
            IUserContract userContract,
            IOpenContract openContract,
            IExaminationContract examinationContract,
            IStatisticContract statisticContract,
            IPublishContract publishContract)
            : base(userContract)
        {
            _openContract = openContract;
            _examinationContract = examinationContract;
            _statisticContract = statisticContract;
            _publishContract = publishContract;
        }

        /// <summary> 协同记录 </summary>
        [HttpGet]
        [DApiAuthorize(UserRole.Teacher)]
        public DResults<MJointUsageDto> JointUsages(string paperId)
        {
            var userId = UserId;
            if (CurrentUser.Code.HasSpecialAuth(SpecialAccountType.ScannerManager))
                userId = -1;
            return _openContract.JointUsages(paperId, userId);
        }

        /// <summary> 提交扫描图片 </summary>
        [HttpPost]
        [DApiAuthorize(UserRole.Teacher)]
        public DResults<MHandinResult> HandinPictures()
        {
            try
            {
                var results = "results".Form(string.Empty);
                var resultData = results.JsonToObject<MPictureList>();
                _logger.Info(resultData);
                return _openContract.HandinPictures(UserId, resultData);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return DResult.Errors<MHandinResult>("上传异常~");
            }
        }

        /// <summary> 套打列表 </summary>
        [HttpGet]
        [DApiAuthorize(UserRole.Teacher)]
        public DResults<MPrintInfo> BatchPrint(string paper_id)
        {
            var id = UserId;
            if (CurrentUser.Code.HasSpecialAuth(SpecialAccountType.ScannerManager))
                id = -1;
            return _openContract.PrintList(paper_id, id);
        }

        /// <summary> 套打详情 </summary>
        [HttpGet]
        [DApiAuthorize(UserRole.Teacher)]
        public DResults<MPrintDetail> PrintDetails(string batch, byte type = 0, int skip = 0, int size = 100)
        {
            return _openContract.PrintDetails(batch, type, skip, size);
        }

        /// <summary> 套打列表 </summary>
        [HttpGet]
        [DApiAuthorize(UserRole.Teacher)]
        public DResults<MJointPrintInfo> JointBatchPrint(string paper_id)
        {
            var id = UserId;
            if (CurrentUser.Code.HasSpecialAuth(SpecialAccountType.ScannerManager))
                id = -1;
            return _openContract.JointPrintList(paper_id, id);
        }

        /// <summary> 协同机构 </summary>
        [HttpGet]
        [DApiAuthorize(UserRole.Teacher)]
        public DResult<Dictionary<string, string>> JointAgencies(string joint)
        {
            return _openContract.JointAgencies(joint);
        }

        /// <summary> 套打详情 </summary>
        [HttpGet]
        [DApiAuthorize(UserRole.Teacher)]
        public DResults<MPrintDetail> JointPrintDetails(string joint, byte type = 0, int skip = 0, int size = 50, string agencyId = null)
        {
            return _openContract.JointPrintDetails(joint, type, skip, size, agencyId);
        }



        /// <summary> 学生答题信息 </summary>
        [HttpGet]
        public DResult<MStudentAnswerDto> StudentAnswer(string batch, string paperId, string questionId,
            long studentId = 0)
        {
            return _openContract.StudentAnswer(batch, paperId, questionId, studentId <= 0 ? ChildOrUserId : studentId);
        }

        /// <summary> 同学分享答案 </summary>
        [HttpGet]
        public DResults<MShareAnswerDto> ShareAnswers(string questionId, string classId, int size = 10)
        {
            return _openContract.ShareAnswers(questionId, classId, size);
        }
        /// <summary> 同学分享答案详情 </summary>
        [HttpGet]
        public DResult<string> ShareAnswer(string shareId)
        {
            return _openContract.ShareAnswer(shareId);
        }

        /// <summary> 题目错误数 </summary>
        [HttpGet]
        public DResult<int> ErrorCount(string batch, string paperId, string questionId)
        {
            return DResult.Succ(_openContract.ErrorCount(batch, paperId, questionId));
        }

        /// <summary> 标记错题 </summary>
        /// <returns></returns>
        [HttpPost]
        public DResult<string> SetError(MSetErrorInputDto dto)
        {
            return _openContract.SetError(dto.Batch, dto.PaperId, dto.QuestionId, ChildOrUserId);
        }

        /// <summary> 试卷错题列表 </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <returns></returns>
        [HttpGet]
        public DResults<MPaperErrorDto> PaperErrors(string batch, string paperId)
        {
            return DResult.Succ(_openContract.PaperErrors(batch, paperId, ChildOrUserId), -1);
        }

        #region 错因分析

        /// <summary> 推荐的标签 </summary>
        [HttpGet]
        public DResults<NameDto> RecommendTags(string errorId)
        {
            return _openContract.RecommendErrorTags(errorId);
        }

        /// <summary> 分析 </summary>
        [HttpPost]
        public DResult SetAnalysis(MErrorAnalysisInputDto dto)
        {
            return _openContract.SetAnalysis(dto);
        }

        [HttpGet]
        public DResult<MErrorAnalysisDto> ErrorAnalysis(string errorId = "", string batch = "", string questionId = "")
        {
            if (errorId.IsNullOrEmpty())
                errorId = _openContract.GetErrorId(batch, questionId, ChildOrUserId);
            return _openContract.ErrorAnalysis(errorId);
        }

        #endregion

        /// <summary> 答题卡 </summary>
        [HttpGet]
        public DResults<MAnswerSheetDto> AnswerSheet(string batch, string paperId, long studentId = 0)
        {
            return _openContract.AnswerSheet(batch, paperId, studentId > 0 ? studentId : ChildOrUserId);
        }

        /// <summary> 成绩统计 </summary>
        [HttpGet]
        [DApiAuthorize(UserRole.Student | UserRole.Parents)]
        public DResult<MScoreStatisticsDto> ScoreStatistics(string batch, string paperId)
        {
            return _openContract.ScoreStatistics(batch, paperId, ChildOrUserId);
        }

        /// <summary> 分数段统计 </summary>
        [HttpGet]
        public DResult ScoreSections(string batch, string paperId)
        {
            return _statisticContract.GetStatisticsAvges(batch, paperId, single: true);
        }

        /// <summary> 排名统计 </summary>
        [HttpGet]
        [DApiAuthorize(UserRole.Teacher)]
        public DResult RankStatistics(string batch, string paperId)
        {
            return _statisticContract.GetStatisticsRank(batch, paperId);
        }

        /// <summary> 已发送成绩通知短信的手机号码 </summary>
        [HttpGet]
        [DApiAuthorize(UserRole.Teacher)]
        public DResult SendedSmsScore(string batch, string paperId)
        {
            return _openContract.SendedSmsScore(batch, paperId);
        }

        /// <summary> 发送成绩通知短信 </summary>
        [HttpPost]
        [DApiAuthorize(UserRole.Teacher)]
        public DResult SendSmsScore(MSendSmsScoreDto dto)
        {
            return _openContract.SendSmsScore(dto);
        }

        [HttpGet]
        public DResult<MQuestionDto> VariantQuestion(string questionId)
        {
            return DResult.Succ(new MQuestionDto());
        }

        /// <summary> 错题统计 </summary>
        [HttpGet]
        public DResult<MAnswerPaperDto> ErrorStatistics(string batch)
        {
            return _openContract.ErrorStatistics(batch);
        }

        /// <summary> 学生答题详细 </summary>
        [HttpGet]
        [DApiAuthorize(UserRole.Student | UserRole.Parents)]
        public DResult StudentDetails(string batch, string paperId)
        {
            return _openContract.StudentDetails(batch, paperId, ChildOrUserId);
        }

        /// <summary> 变式过关 </summary>
        [HttpGet]
        public DResult VariantPass(string batch)
        {
            return _openContract.VariantPass(batch, ChildOrUserId);
        }

        /// <summary> 大型考试成绩汇总 </summary>
        [HttpGet]
        public DResult EaSummary(string id)
        {
            return _examinationContract.Summary(id, ChildOrUserId);
        }

        /// <summary> 大型考试各学科成绩分数段 </summary>
        [HttpGet]
        public DResult EaScoreSections(string id)
        {
            return _examinationContract.ScoreSections(id, ChildOrUserId);
        }

        /// <summary> 客观题答题统计报表 </summary>
        [HttpGet]
        [DApiAuthorize(UserRole.Teacher)]
        public DResult ObjectiveStatistics(string batch, string paperId, string questionId, string smallQuestionId = "")
        {
            return _statisticContract.StatisticsQuestionDetail(batch, paperId, questionId, smallQuestionId);
        }

        [HttpGet]
        [DApiAuthorize(UserRole.Teacher)]
        public DResult UnSubmits(string batch, string paperId)
        {
            return _openContract.UnSubmits(batch, paperId);
        }

        [HttpPost]
        [DApiAuthorize(UserRole.Teacher)]
        public DResult PubPaper(MPubPaperDto dto)
        {
            return _publishContract.PulishPaper(dto.PaperId, dto.GroupIds, dto.SourceGroupId, CurrentUser.Id, dto.Message);
        }

        [HttpGet]
        [DApiAuthorize(UserRole.Teacher)]
        public DResult PaperVariant(string batch, string paperId)
        {
            return _openContract.PaperVariant(batch, paperId);
        }

        [HttpGet]
        [DApiAuthorize(UserRole.Teacher)]
        public DResult NewVariant(string questionId, int count, string excepts = null)
        {
            return _openContract.NewVariant(questionId, count, excepts);
        }

        [HttpPost]
        [DApiAuthorize(UserRole.Teacher)]
        public DResult SendVariant(MSendVariantDto dto)
        {
            dto.TeacherId = CurrentUser.Id;
            return _openContract.SendVariant(dto);
        }

    }
}
