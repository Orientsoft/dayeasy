
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Marking;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Models.Open.Group;
using DayEasy.Models.Open.Paper;
using DayEasy.Models.Open.Work;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;

namespace DayEasy.Contract.Open.Services
{
    public partial class OpenService
    {
        public IDayEasyRepository<TP_AnswerShare> AnswerShareRepository { private get; set; }
        public IDayEasyRepository<TP_PaperContent> PaperContentRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingMark> MarkingMarkRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingDetail> MarkingDetailRepository { private get; set; }
        public IDayEasyRepository<TP_ErrorQuestion> ErrorQuestionRepository { private get; set; }
        public IDayEasyRepository<TQ_Question> QuestionRepository { private get; set; }
        public IDayEasyRepository<TQ_SmallQuestion> SmallQuestionRepository { private get; set; }
        public IDayEasyRepository<TP_ErrorReason> ErrorReasonRepository { private get; set; }

        public IErrorBookContract ErrorBookContract { private get; set; }
        public IPublishContract PublishContract { private get; set; }
        public ISmsScoreNoticeContract SmsScoreNoticeContract { private get; set; }

        private string AnswerImage(string batch, string paperId, string questionId, long userId)
        {
            var cacheKey = string.Format("answer_image_{0}_{1}_{2}_{3}", batch, paperId, questionId, userId);
            if (CacheHelper.Exists(cacheKey))
                return CacheHelper.Get<string>(cacheKey);
            var usage = UsageRepository.Load(batch);
            if (usage == null)
                return string.Empty;
            var content = PaperContentRepository.FirstOrDefault(t => t.PaperID == paperId && t.QuestionID == questionId);
            if (content == null)
                return string.Empty;
            var type = content.PaperSectionType;
            var picture =
                MarkingPictureRepository.FirstOrDefault(
                    t => t.BatchNo == batch && t.PaperID == paperId && t.StudentID == userId
                         && (type == 2 ? t.AnswerImgType == type : t.AnswerImgType <= type));
            if (picture == null)
                return string.Empty;
            var no = usage.JointBatch.IsNullOrEmpty() ? batch : usage.JointBatch;
            var mark =
                MarkingMarkRepository.FirstOrDefault(
                    t => t.BatchNo == no && (type == 2 ? t.PaperType == type : t.PaperType <= type));
            if (mark == null)
                return string.Empty;
            var area = JsonHelper.JsonList<MkQuestionAreaDto>(mark.Mark).FirstOrDefault(t => t.Id == questionId);
            if (area == null)
                return string.Empty;
            using (var helper = new HttpHelper(picture.AnswerImgUrl))
            {
                var stream = helper.GetStream();
                var image = ImageHelper.MakeImage(stream, (int)area.X, (int)area.Y, (int)area.Width,
                    (int)area.Height);
                CacheHelper.Add(cacheKey, image, 60 * 24 * 30);
                return image;
            }
        }

        public DResult<MStudentAnswerDto> StudentAnswer(string batch, string paperId, string questionId, long userId)
        {
            var dto = new MStudentAnswerDto
            {
                Status = 0
            };
            var details =
                MarkingDetailRepository.Where(
                    t => t.Batch == batch && t.PaperID == paperId && t.QuestionID == questionId && t.StudentID == userId)
                    .Select(t => new
                    {
                        t.SmallQID,
                        t.IsFinished,
                        t.IsCorrect,
                        t.AnswerContent
                    });
            if (!details.Any()) return DResult.Succ(dto);

            var question = QuestionRepository.FirstOrDefault(q => q.Id == questionId);
            if (question == null) return DResult.Succ(dto);
            if (question.IsObjective)
            {
                dto.Status = details.Any(d => !d.IsFinished)
                    ? (byte) 1
                    : (byte) (details.All(d => d.IsCorrect.HasValue && d.IsCorrect.Value) ? 2 : 4);

                if (question.HasSmallQuestion)
                {
                    var smalls = SmallQuestionRepository.Where(q => q.QID == questionId)
                        .OrderBy(q => q.Sort).Select(q => new { q.Id, q.Sort }).ToList();
                    if (!smalls.Any())
                        dto.Answer = string.Join(",", details.Select(t => t.AnswerContent));
                    var answers = (from q in smalls
                        from d in details
                        where q.Id == d.SmallQID
                        orderby q.Sort
                        select d.AnswerContent);
                    dto.Answer = string.Join(",", answers);
                }
                else
                {
                    dto.Answer = string.Join(",", details.Select(t => t.AnswerContent));
                }
            }
            else
            {
                dto.Answer = AnswerImage(batch, paperId, questionId, userId);
            }
            return DResult.Succ(dto);
        }

        public DResults<MShareAnswerDto> ShareAnswers(string questionId, string classId, int size = 10)
        {
            Expression<Func<TP_AnswerShare, bool>> condition = a =>
                a.Status == (byte)AnswerShareStatus.Normal
                && a.QuestionId == questionId;
            var classCondition = condition.And(a => a.ClassId == classId);

            if (AnswerShareRepository.Exists(classCondition))
            {
                condition = classCondition;
            }
            var shares = AnswerShareRepository.Where(condition).OrderByDescending(a => a.WorshipCount)
                .Select(a => new MShareAnswerDto
                {
                    Id = a.Id,
                    StudentId = a.AddedBy,
                    StudentName = a.AddedName,
                    LikeCount = a.WorshipCount
                }).Take(size).ToList();
            var userIds = shares.Select(t => t.StudentId).Distinct();
            var users = UserContract.LoadListDictUser(userIds);
            shares.ForEach(t =>
            {
                if (!users.ContainsKey(t.StudentId))
                    return;
                var student = users[t.StudentId];
                t.StudentName = student.Name;
                t.StudentAvatar = student.Avatar;
            });
            return DResult.Succ(shares, shares.Count);
        }

        public DResult<string> ShareAnswer(string shareId)
        {
            var share = AnswerShareRepository.Load(shareId);
            if (share == null)
                return DResult.Error<string>("分享的答案不存在！");
            var answerImage = AnswerImage(share.Batch, share.PaperId, share.QuestionId, share.AddedBy);
            return DResult.Succ(answerImage);
        }

        public int ErrorCount(string batch, string paperId, string questionId)
        {
            return ErrorQuestionRepository
                .Count(t => t.Batch == batch && t.PaperID == paperId && t.QuestionID == questionId);
        }

        public DResult<string> SetError(string batch, string paperId, string questionId, long studentId)
        {
            return ErrorBookContract.MarkErrorQuestion(batch, paperId, questionId, studentId);
        }

        public IEnumerable<MPaperErrorDto> PaperErrors(string batch, string paperId, long studentId)
        {
            var usage = UsageRepository.Load(batch);
            if (usage == null || usage.SourceType != (byte)PublishType.Test)
                return new List<MPaperErrorDto>();
            return
                ErrorQuestionRepository.Where(t => t.Batch == batch && t.PaperID == paperId && t.StudentID == studentId)
                    .Select(t => new MPaperErrorDto
                    {
                        QuestionId = t.QuestionID,
                        ErrorId = t.Id
                    }).ToList();
        }


        /// <summary>
        /// 查询推荐的错题标签
        /// </summary>
        public DResults<NameDto> RecommendErrorTags(string errorId)
        {
            return ErrorBookContract.RecomTags(errorId);
        }

        /// <summary>
        /// 添加错因分析
        /// </summary>
        public DResult SetAnalysis(MErrorAnalysisInputDto dto)
        {
            return ErrorBookContract.AddReason(dto.ErrorId, dto.Content, dto.TagList);
        }

        /// <summary>
        /// 查询错因分析
        /// </summary>
        public DResult<MErrorAnalysisDto> ErrorAnalysis(string errorId)
        {
            var dto = new MErrorAnalysisDto
            {
                ErrorId = errorId
            };
            var reason = ErrorReasonRepository.FirstOrDefault(t => t.ErrorId == errorId);
            if (reason != null)
            {
                dto.Content = reason.Content;
                if (reason.Tags.IsNotNullOrEmpty())
                    dto.TagList = JsonHelper.JsonList<NameDto>(reason.Tags).Select(r => r.Name).ToList();
            }
            return DResult.Succ(dto);
        }

        /// <summary>
        /// 查询错题ID
        /// </summary>
        public string GetErrorId(string batch, string questionId, long studentId)
        {
            if (batch.IsNullOrEmpty() || questionId.IsNullOrEmpty() || studentId < 1)
                return string.Empty;
            var item = ErrorQuestionRepository.FirstOrDefault(i =>
                i.Batch == batch && i.QuestionID == questionId && i.StudentID == studentId);
            return item == null ? string.Empty : item.Id;
        }

        /// <summary>
        /// 答题卡
        /// </summary>
        public DResults<MAnswerSheetDto> AnswerSheet(string batch, string paperId, long studentId)
        {

            var pictures =
                MarkingPictureRepository.Where(
                    t => t.BatchNo == batch && t.PaperID == paperId && t.StudentID == studentId)
                    .Select(t => new MAnswerSheetDto
                    {
                        Id = t.Id,
                        Type = t.AnswerImgType,
                        Picture = t.AnswerImgUrl,
                        ObjectError = t.ObjectiveError,
                        Marks = t.Marks,
                        Markings = t.RightAndWrong
                    }).ToList();
            if (pictures.IsNullOrEmpty())
                return DResult.Errors<MAnswerSheetDto>("没有找到相关的试卷信息！");
            var stu =
                StudentScoreRepository.FirstOrDefault(
                    t => t.Batch == batch && t.PaperId == paperId && t.StudentId == studentId);
            pictures.ForEach(t =>
            {
                t.Score = (t.Type == (byte)MarkingPaperType.PaperB ? stu.SectionBScore : stu.SectionAScore);
            });
            return DResult.Succ(pictures, -1);
        }

        /// <summary>
        /// 学生-成绩统计
        /// </summary>
        public DResult<MScoreStatisticsDto> ScoreStatistics(string batch, string paperId, long studentId)
        {
            var classScore = ClassScoreRepository.FirstOrDefault(t => t.Batch == batch && t.PaperId == paperId);
            if (classScore == null) return DResult.Error<MScoreStatisticsDto>("没有查询到考试记录");

            var dto = new MScoreStatisticsDto
            {
                Score = -1,
                AScore = -1,
                BScore = -1,
                Rank = -1,
                AverageScore = classScore.AverageScore,
                KpAnalysis = new List<MKpAnalysisDto>()
            };
            var stu = StudentScoreRepository.FirstOrDefault(
                t => t.Batch == batch && t.PaperId == paperId && t.StudentId == studentId);
            if (stu == null) return DResult.Succ(dto);

            dto.Score = stu.CurrentScore;
            dto.AScore = stu.SectionAScore;
            dto.BScore = stu.SectionBScore;
            dto.Rank = stu.CurrentSort;

            //查询错题
            var errorQIds = ErrorQuestionRepository
                .Where(i => i.Batch == batch && i.PaperID == paperId && i.StudentID == studentId)
                .Select(i => i.QuestionID).ToList();
            dto.KpAnalysis = KpAnalysis(errorQIds);

            return DResult.Succ(dto);
        }

        internal List<MKpAnalysisDto> KpAnalysis(List<string> errorQIds)
        {
            var kpList = new List<MKpAnalysisDto>();

            if (errorQIds == null || !errorQIds.Any()) return kpList;

            //查询问题知识点
            var questionKps = QuestionRepository.Where(u => errorQIds.Contains(u.Id))
                    .Select(q => new
                    {
                        QId = q.Id,
                        Kps = q.KnowledgeIDs
                    }).ToList();

            if (questionKps.Count < 1) return kpList;

            //所有的知识点
            var kpDicList = questionKps.Select(u => JsonHelper.Json<Dictionary<string, string>>(u.Kps)).ToList();
            if (!kpDicList.Any()) return kpList;

            kpList = kpDicList
                .SelectMany(u => u.Select(d => new MKpAnalysisDto { KpCode = d.Key, KpName = d.Value }))
                .GroupBy(u => u.KpCode)
                .Select(u => new MKpAnalysisDto
                {
                    KpCode = u.Key,
                    KpName = u.First().KpName,
                    ErrorCount = u.Count()
                }).ToList();

            return kpList;
        }

        /// <summary>
        /// 教师-已发送成绩通知短信的手机号码
        /// </summary>
        public DResults<string> SendedSmsScore(string batch, string paperId)
        {
            var list = SmsScoreNoticeContract.FindByBatch(batch, paperId)
                .Select(s => s.Mobile).ToList();
            return DResult.Succ(list, -1);
        }

        /// <summary>
        /// 教师-发送成绩通知短信
        /// </summary>
        public DResult SendSmsScore(MSendSmsScoreDto dto)
        {
            if (dto == null || dto.Batch.IsNullOrEmpty() || dto.PaperId.IsNullOrEmpty() || !dto.StudentIdList.Any())
                return DResult.Error("参数不正确");
            return StatisticContract.SendScoreSms(dto.Batch, dto.PaperId, dto.StudentIdList);
        }

        /// <summary>
        /// 试卷错题统计
        /// </summary>
        public DResult<MAnswerPaperDto> ErrorStatistics(string batch)
        {
            var publishModel = PublishContract.GetUsageDetail(batch);
            if (!publishModel.Status || publishModel.Data == null)
                return DResult.Error<MAnswerPaperDto>(publishModel.Message);

            var paperId = publishModel.Data.SourceId;
            var result = new MAnswerPaperDto
            {
                Batch = batch,
                PaperId = paperId,
                GroupId = publishModel.Data.ClassId,
                IsPrint = publishModel.Data.SourceType == (byte)PublishType.Print
            };

            //错题
            result.Errors = ErrorQuestionRepository
                .Where(i => i.Batch == batch && i.PaperID == paperId)
                .Select(i => new
                {
                    i.QuestionID,
                    i.StudentID
                }).ToList()
                .GroupBy(i => i.QuestionID)
                .ToDictionary(g => g.Key, v => v.Select(s => s.StudentID).ToList());

            return DResult.Succ(result);
        }

        /// <summary>
        /// 学生答题明细
        /// </summary>
        public DResults<MMarkingDetailDto> StudentDetails(string batch, string paperId, long studentId)
        {
            //答题明细 todo epc 后期如果需要显示我的答案再更改
            var list = MarkingDetailRepository
                .Where(m => m.Batch == batch && m.PaperID == paperId && m.StudentID == studentId)
                .GroupBy(m => m.QuestionID)
                .Select(g => new MMarkingDetailDto
                {
                    QuestionId = g.Key,
                    IsCorrect = g.All(i => i.IsCorrect.HasValue && i.IsCorrect.Value),
                    Score = g.Sum(i => i.CurrentScore)
                }).ToList();
            return DResult.Succ(list, -1);
        }

        /// <summary>
        /// 相关变式
        /// </summary>
        public DResult VariantPass(string batch, long studentId)
        {
            return PublishContract.VariantQuestions(batch, studentId);
        }

        /// <summary>
        /// 未提交学生名单
        /// </summary>
        public DResults<MMemberDto> UnSubmits(string batch, string paperId)
        {
            var usage = UsageRepository.Load(batch);
            if (usage == null) return DResult.Errors<MMemberDto>("批次号不存在");
            var studentsResult = GroupContract.GroupMembers(usage.ClassId, UserRole.Student);
            if (!studentsResult.Status) return DResult.Errors<MMemberDto>(studentsResult.Message);
            var submitIds = ErrorBookContract.IsSubmitStudentIds(batch, paperId);
            var list = studentsResult.Data.Where(i => !submitIds.Contains(i.Id)).MapTo<List<MMemberDto>>();
            return DResult.Succ(list, -1);
        }

        /// <summary>
        /// 试卷错题变式推送
        /// </summary>
        public DResult<MPaperVariantDto> PaperVariant(string batch, string paperId)
        {
            var result = new MPaperVariantDto
            {
                IsSendVariant = PublishContract.IsSendVariant(batch, paperId)
            };

            var weakResult = PublishContract.PaperWeak(batch, paperId);
            if (weakResult.Status)
            {
                result.Knowledges = weakResult.Data.Knowledges;
                result.ErrorTags = weakResult.Data.ErrorTags;
            }

            var variantResult = result.IsSendVariant
                ? PublishContract.VariantList(batch, paperId)
                : PublishContract.VariantListFromSystem(batch, paperId, max: 10, pre: 0);
            if (variantResult.Status)
            {
                result.Variants = variantResult.Data.Variants.MapTo<List<MQuestionVariantDto>>();
                result.Questions = variantResult.Data.Questions
                    .ToDictionary(q => q.Key, v => v.Value.MapTo<MQuestionDto>());
            }

            return DResult.Succ(result);
        }

        /// <summary>
        /// 获取变式题
        /// </summary>
        public DResults<MQuestionDto> NewVariant(string questionId, int count = 1, string excepts = null)
        {
            var exceptList = (excepts ?? string.Empty).Split(',').ToList();
            var result = PublishContract.Variant(questionId, count, exceptList);
            if (!result.Status) return DResult.Errors<MQuestionDto>(result.Message);
            return DResult.Succ(result.Data.MapTo<List<MQuestionDto>>(), -1);
        }

        /// <summary>
        /// 推送变式
        /// </summary>
        public DResult SendVariant(MSendVariantDto dto)
        {
            var classList = dto.GroupIds.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var vlist = JsonHelper.Json<Dictionary<string, List<string>>>(dto.Variants);
            if (!classList.Any()) return DResult.Error("请选择要推送的班级！");
            if (vlist == null || !vlist.Any()) return DResult.Error("请选择要推送变式题！");

            return PublishContract.SendVariant(dto.TeacherId, dto.PaperId, vlist, classList);
        }

    }
}
