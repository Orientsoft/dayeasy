using DayEasy.AsyncMission;
using DayEasy.AsyncMission.Models;
using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.Marking;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.EntityFramework;
using DayEasy.Marking.Services.Helper;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DayEasy.Marking.Services
{
    public partial class MarkingService : DayEasyService, IMarkingContract
    {
        #region 注入

        public MarkingService(IDbContextProvider<DayEasyDbContext> context)
            : base(context)
        { }
        private readonly ILogger _logger = LogManager.Logger<MarkingService>();
        public IGroupContract GroupContract { private get; set; }
        public IPaperContract PaperContract { private get; set; }
        public IUserContract UserContract { get; set; }
        public IDayEasyRepository<TP_MarkingResult, string> MarkingResultRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingDetail, string> MarkingDetailRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingPicture, string> MarkingPictureRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingMark, string> MarkingMarkRepository { private get; set; }
        public IDayEasyRepository<TP_AnswerShare, string> AnswerShareRepository { private get; set; }
        public IDayEasyRepository<TC_Usage, string> UsageRepository { private get; set; }
        public IDayEasyRepository<TP_Paper, string> PaperRepository { private get; set; }
        public IDayEasyRepository<TP_PaperSection, string> PaperSectionRepository { private get; set; }
        public IDayEasyRepository<TP_PaperContent, string> PaperContentRepository { private get; set; }
        public IDayEasyRepository<TU_User, long> UserRepository { private get; set; }
        public IDayEasyRepository<TQ_Question, string> QuestionRepository { private get; set; }
        public IDayEasyRepository<TQ_SmallQuestion, string> SmallQuestionRepository { private get; set; }
        public IDayEasyRepository<TQ_Answer, string> AnswerRepository { private get; set; }
        public IDayEasyRepository<TP_PaperAnswer, string> PaperAnswerRepository { private get; set; }
        public IDayEasyRepository<TS_ClassScoreStatistics, string> ClassScoreStatisticsRepository { private get; set; }
        public IDayEasyRepository<TS_StuScoreStatistics, string> StuScoreStatisticsRepository { private get; set; }
        public IDayEasyRepository<TP_ErrorQuestion, string> ErrorQuestionRepository { private get; set; }

        #endregion

        #region 阅卷标记区域

        /// <summary> 进入阅卷区域标记界面验证 </summary>
        public DResult<MarkingAreaDto> MkAreaCheck(string batch, int type, long userId, bool isJoint = false)
        {
            var batchs = new List<string>();
            if (isJoint)
            {
                if (!JointMarkingRepository.Exists(j => j.Id == batch))
                    return DResult.Error<MarkingAreaDto>("没有查询到协同记录！");
                batchs = UsageRepository.Where(u => u.JointBatch == batch).Select(u => u.Id).ToList();
            }
            else
            {
                batchs.Add(batch);
            }
            if (!batchs.Any())
                return DResult.Error<MarkingAreaDto>("没有查询到发布记录");
            var usage = UsageRepository.FirstOrDefault(u => batchs.Contains(u.Id));
            if (usage == null) return DResult.Error<MarkingAreaDto>("没有查询到发布记录");

            var paperResult = PaperContract.PaperDetailById(usage.SourceID, false);
            if (!paperResult.Status || paperResult.Data == null)
                return DResult.Error<MarkingAreaDto>("没有查询到试卷资料");

            if (!paperResult.Data.PaperBaseInfo.IsAb)
                type = 0;
            else
            {
                if (type != (byte)PaperSectionType.PaperB)
                    type = (byte)PaperSectionType.PaperA;
            }

            var paperType = (type <= 0 || type > 2) ? 1 : type;
            var questionsResult = MkAreaQuestions(usage.SourceID, paperType);
            if (!questionsResult.Status)
                return DResult.Error<MarkingAreaDto>(questionsResult.Message);

            var picture = MarkingPictureRepository
                .FirstOrDefault(p => batchs.Contains(p.BatchNo) && p.AnswerImgType == type);
            if (picture == null)
                return DResult.Error<MarkingAreaDto>("没有任何答卷");
            var result = new MarkingAreaDto
            {
                ImageUrl = picture.AnswerImgUrl,
                PaperId = picture.PaperID,
                Type = type,
                Questions = questionsResult.Data.ToList()
            };
            var area = MarkingMarkRepository.FirstOrDefault(t => t.BatchNo == batch && t.PaperType == type);
            if (area != null)
                result.Areas = area.Mark;
            return DResult.Succ(result);
        }

        /// <summary> 答卷答题区域标记 - 切换图片 </summary>
        public string MkAreaChangePicture(string batch, int type, bool isJoint = false)
        {
            var random = RandomHelper.Random();
            string imageUrl;
            if (isJoint)
            {
                if (!JointMarkingRepository.Exists(j => j.Id == batch)) return null;
                var usages = UsageRepository.Where(t => t.JointBatch == batch);
                imageUrl = MarkingPictureRepository.Where(p => p.AnswerImgType == type)
                    .Join(usages, p => p.BatchNo, u => u.Id, (p, u) => p.AnswerImgUrl)
                    .ToList().OrderBy(i => random.Next()).FirstOrDefault();
            }
            else
            {
                imageUrl = MarkingPictureRepository
                    .Where(r => r.BatchNo == batch && r.AnswerImgType == type)
                    .Select(r => r.AnswerImgUrl).ToList()
                    .OrderBy(r => random.Next()).FirstOrDefault();
            }
            return imageUrl;
        }

        /// <summary> 答卷答题区域标记 - 问题序号列表 </summary>
        public DResults<MarkingAreaQuestion> MkAreaQuestions(string paperId, int type)
        {
            if (type == 0) type = 1;
            var detailResult = PaperContract.PaperDetailById(paperId);
            if (!detailResult.Status)
                return DResult.Errors<MarkingAreaQuestion>("没有查询到试卷详情");
            //主观题序号
            var dict = PaperContract.PaperSorts(detailResult.Data);
            var result = detailResult.Data.PaperSections.Where(s => s.PaperSectionType == type)
                .SelectMany(s => s.Questions.Where(q => !q.Question.IsObjective))
                .Select(t => new MarkingAreaQuestion
                {
                    Index = dict[t.Question.Id],
                    Id = t.Question.Id,
                    Type = t.Question.Type
                }).ToList();
            return DResult.Succ(result, result.Count);
        }

        /// <summary> 答卷答题区域标记 - 保存 </summary>
        public DResult MkAreaSave(string batch, int type, string areas)
        {
            if (string.IsNullOrWhiteSpace(batch))
                return DResult.Error("批次号不能为空");
            if (string.IsNullOrWhiteSpace(areas))
                return DResult.Error("阅卷区域不能为空");
            if (type < 0 || type > 2)
                return DResult.Error("试卷类型异常");
            var usage = UsageRepository.Exists(t => t.Id == batch);
            var isJoint = false;
            if (!usage)
            {
                var joint = JointMarkingRepository.Exists(t => t.Id == batch);
                if (!joint)
                    return DResult.Error("阅卷批次不正确！");
                isJoint = true;
            }
            int result;
            var mark = MarkingMarkRepository.FirstOrDefault(m => m.BatchNo == batch && m.PaperType == type);
            if (mark != null)
            {
                mark.Mark = areas;
                result = MarkingMarkRepository.Update(t => new { t.Mark }, mark);
            }
            else
            {
                var id = MarkingMarkRepository.Insert(new TP_MarkingMark
                {
                    Id = IdHelper.Instance.Guid32,
                    BatchNo = batch,
                    PaperType = (byte)type,
                    Mark = areas
                });
                result = string.IsNullOrWhiteSpace(id) ? 0 : 1;
            }
            if (result > 0 && isJoint)
            {
                MarkingHelper.UpdateRegionAsync(batch, (byte)type, areas);
            }
            return DResult.FromResult(result);
        }

        #endregion

        #region 提交批阅、结束阅卷

        /// <summary> 首次提交，生成必要数据 </summary>
        public DResult Commit(string pictureId)
        {
            return AutoCommit(pictureId);
        }

        /// <summary> 提交批阅 </summary>
        public DResult Submit(MkSubmitDto item)
        {
            //验证
            if (item == null) return DResult.Error("参数错误,请刷新重试");
            var picture = MarkingPictureRepository.FirstOrDefault(p => p.Id == item.PictureId);
            if (picture == null) return DResult.Error("没有查询到答卷资料");
            var usage = UsageRepository.FirstOrDefault(u => u.Id == picture.BatchNo);
            if (usage == null || usage.SourceType != (byte)PublishType.Print)
                return DResult.Error("没有查询到发布记录");
            if (usage.MarkingStatus == (byte)MarkingStatus.AllFinished)
                return DResult.Error("此试卷已批阅完成");

            //批阅痕迹
            picture.LastMarkingTime = Clock.Now;
            SetPictureIcons(picture, item);

            if (item.Details == null || !item.Details.Any())
                return PrintMarking(null, item.UserId, picture);

            //批阅数据
            var resultModel = MarkingResultRepository.FirstOrDefault(r =>
                r.Batch == picture.BatchNo &&
                r.PaperID == picture.PaperID &&
                r.StudentID == picture.StudentID);
            if (resultModel == null)
                return DResult.Error("没有查询到阅卷结果记录");
            if (resultModel.IsFinished)
                return DResult.Error("此试卷已经批阅完成");

            var details = item.Details.Select(d => new MkDetailDto
            {
                QuestionId = d.QuestionId,
                SmallQuestionId = d.SmallQuestionId,
                IsCorrect = d.IsCorrect,
                CurrentScore = d.Score // ??怎么回事
            }).ToList();

            return PrintMarking(details, item.UserId, picture);
        }

        /// <summary> 设置批阅图标 </summary>
        void SetPictureIcons(TP_MarkingPicture picture, MkSubmitDto item)
        {
            if (picture == null) return;
            var picIcons = new List<MkSymbol>();
            var nIcons = new List<MkSymbol>();
            var picMarks = new List<MkComment>();
            var nMarks = new List<MkComment>();
            var removeIcons = new List<MkSymbolBase>();

            if (picture.RightAndWrong.IsNotNullOrEmpty())
                picIcons = picture.RightAndWrong.Replace("'", "\"").JsonToObject2<List<MkSymbol>>();
            if (item.Icons.IsNotNullOrEmpty())
                nIcons = item.Icons.Replace("'", "\"").JsonToObject2<List<MkSymbol>>();
            if (item.Marks.IsNotNullOrEmpty())
                nMarks = item.Marks.Replace("'", "\"").JsonToObject2<List<MkComment>>();
            if (picture.Marks.IsNotNullOrEmpty())
                picMarks = picture.Marks.Replace("'", "\"").JsonToObject2<List<MkComment>>();
            if (item.RemoveIcons.IsNotNullOrEmpty())
                removeIcons = item.RemoveIcons.Replace("'", "\"").JsonToObject2<List<MkSymbolBase>>();

            if (removeIcons.Any())
            {
                removeIcons.ForEach(r =>
                {
                    var icon = picIcons.FirstOrDefault(i => i.X == r.X && i.Y == r.Y);
                    if (icon != null)
                        picIcons.Remove(icon);
                    var mark = picMarks.FirstOrDefault(i => i.X == r.X && i.Y == r.Y);
                    if (mark != null)
                        picMarks.Remove(mark);
                });
            }

            picIcons.AddRange(nIcons);
            picMarks.AddRange(nMarks);
            picture.RightAndWrong = picIcons.Any() ? picIcons.ToJson2() : null;
            picture.Marks = picMarks.Any() ? picMarks.ToJson2() : null;
        }

        ///// <summary> 完成阅卷 </summary>
        //[Obsolete("即将过期,CompleteMarking替代")]
        //public DResult Finished(string batch, string paperId, MarkingStatus type, bool autoSetIcon, bool autoSetScore, long userId)
        //{
        //    if (string.IsNullOrWhiteSpace(batch) || string.IsNullOrWhiteSpace(paperId))
        //        return DResult.Error(MarkingConsts.MsgBatchNotFind);
        //    var usage = UsageRepository.FirstOrDefault(u => u.Id == batch);
        //    if (usage == null)
        //        return DResult.Error("没有查询到发布记录");
        //    if (usage.MarkingStatus == (byte)MarkingStatus.AllFinished || usage.MarkingStatus == (byte)type)
        //        return DResult.Succ(new DResult(true, "试卷已完成批阅"));

        //    //            UpdateRightIconAndErrorObjMission(batch, paperId, type, autoSetIcon, autoSetScore);
        //    //            return DResult.Success;

        //    var status = usage.MarkingStatus + (byte)type;
        //    //阅卷完成
        //    if (status == (byte)MarkingStatus.AllFinished)
        //    {
        //        //更新正确答题默认勾勾 - 批阅符号
        //        MarkingTask.UpdateRightIconAndErrorObjMission(batch, paperId, autoSetIcon, autoSetScore).Wait();

        //        return FinishedMarking(usage, userId, type);
        //    }

        //    //更新阅卷状态
        //    usage.MarkingStatus = (byte)status;
        //    return UsageRepository.Update(u => new { u.MarkingStatus }, usage) > 0
        //        ? DResult.Success
        //        : DResult.Error("操作失败，请稍后重试");
        //}

        /// <summary> 更新阅卷状态(普通阅卷) </summary>
        public DResult UpdateMarkingStatus(string batch, byte status)
        {
            if (string.IsNullOrWhiteSpace(batch))
                return DResult.Error(MarkingConsts.MsgBatchNotFind);
            if (!Enum.IsDefined(typeof(MarkingStatus), status))
                return DResult.Error("批阅状态异常");
            var usage = UsageRepository.Load(batch);
            if (usage == null)
                return DResult.Error("没有查询到发布记录");
            if (usage.MarkingStatus == (byte)MarkingStatus.AllFinished || usage.MarkingStatus == status)
                return DResult.Succ(new DResult(true, "试卷已完成批阅"));
            //更新阅卷状态
            usage.MarkingStatus += status;
            if (usage.MarkingStatus == (byte)MarkingStatus.AllFinished)
                return DResult.Error("该试卷已 完成批阅");
            return UsageRepository.Update(u => new { u.MarkingStatus }, usage) > 0
                ? DResult.Success
                : DResult.Error("操作失败，请稍后重试");
        }

        /// <summary> 完成阅卷(普通 & 协同) </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        public DResult CompleteMarking(CompleteMarkingInputDto inputDto)
        {
            if (inputDto == null || string.IsNullOrWhiteSpace(inputDto.Batch))
                return DResult.Error("参数错误，请刷新重试！");
            DResult result;
            try
            {
                result = CompleteMarking(inputDto.Batch, inputDto.IsJoint);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                result = DResult.Error("数据更新异常");
            }
            if (result != null && result.Status)
            {
                //推送完成阅卷任务
                MissionHelper.PushMission(MissionType.FinishMarking, new FinishMarkingParam
                {
                    Batch = inputDto.Batch,
                    IsJoint = inputDto.IsJoint,
                    SetIcon = inputDto.SetIcon,
                    SetMarks = inputDto.SetMarks
                }, inputDto.UserId);
            }
            return result;
        }

        #endregion

        #region 待批阅数据 - 非协同

        /// <summary> 进入批阅验证 </summary>
        /// <returns> bool：正常、需要标记区域 </returns>
        public DResult<bool> MkCheck(string batch, int type)
        {
            var usage = UsageRepository.FirstOrDefault(u => u.Id == batch);
            if (usage == null || usage.SourceType != (byte)PublishType.Print)
                return DResult.Error<bool>("没有查询到该批次号的阅卷资料");
            if (usage.MarkingStatus == (byte)MarkingStatus.AllFinished)
                return DResult.Error<bool>("该阅卷已结束批阅");
            var paperResult = PaperContract.PaperDetailById(usage.SourceID);
            if (!paperResult.Status)
                return DResult.Error<bool>("没有查询到试卷资料");
            var hasPicture = MarkingPictureRepository.Exists(p => p.BatchNo == batch && p.AnswerImgType == type);

            if (!hasPicture)
                return DResult.Succ(false);

            //type 类型
            var sectionType = type;
            if (type == (int)MarkingPaperType.Normal)
                sectionType = (int)PaperSectionType.PaperA;

            //是否全客观题
            var allObjective = (sectionType == (byte)PaperSectionType.PaperA && paperResult.Data.AllObjectiveA) ||
                               (sectionType == (byte)PaperSectionType.PaperB && paperResult.Data.AllObjectiveB);
            if (allObjective)
                return DResult.Succ(false);

            //是否标记区域
            var exist =
                MarkingMarkRepository.Exists(
                    m =>
                        m.BatchNo == batch &&
                        (m.PaperType == type || (m.PaperType == 0 && type == (byte)PaperSectionType.PaperA)));

            return DResult.Succ(!exist);
        }

        /// <summary> 答卷图片列表(非协同) </summary>
        /// <returns></returns>
        public DResult<MarkingDataDto> MkPictureList(string batch, int type)
        {
            //发布记录
            var usage = UsageRepository.FirstOrDefault(u => u.Id == batch);
            if (usage == null || usage.SourceType != (byte)PublishType.Print)
                return DResult.Error<MarkingDataDto>("没有查询到发布记录");
            if (usage.MarkingStatus == (byte)MarkingStatus.AllFinished)
                return DResult.Error<MarkingDataDto>("此试卷已完成批阅");

            //试卷详细
            var paperResult = PaperContract.PaperDetailById(usage.SourceID);

            if (!paperResult.Status || paperResult.Data == null)
                return DResult.Error<MarkingDataDto>("没有查询到试卷资料");
            var paper = paperResult.Data.PaperBaseInfo;

            //type 类型
            var sectionType = type;
            if (type == (byte)MarkingPaperType.Normal)
                sectionType = (byte)PaperSectionType.PaperA;

            //是否全客观题
            var allObjective = (sectionType == (byte)PaperSectionType.PaperA && paperResult.Data.AllObjectiveA) ||
                               (sectionType == (byte)PaperSectionType.PaperB && paperResult.Data.AllObjectiveB);

            if (paper.IsAb)
            {
                //是否已结束阅卷
                if ((type == (byte)MarkingPaperType.PaperA && usage.MarkingStatus == (byte)MarkingStatus.FinishedA) ||
                    (type == (byte)MarkingPaperType.PaperB && usage.MarkingStatus == (byte)MarkingStatus.FinishedB))
                    return DResult.Error<MarkingDataDto>("此试卷已完成批阅");
            }

            //是否已上传答卷
            var existPicture = MarkingPictureRepository.Exists(p => p.BatchNo == batch && p.AnswerImgType == type);

            //已标记题号区域的坐标 - 全客观题无须标记
            var mark = string.Empty;
            if (!allObjective && existPicture)
            {
                var area = MarkingMarkRepository.FirstOrDefault(m => m.BatchNo == batch && m.PaperType == (byte)type);
                if (area == null)
                    return DResult.Error<MarkingDataDto>("请先标记题号区域");
                mark = area.Mark;
            }

            //试卷题号、分数
            var sorts = PaperContract.PaperSorts(paperResult.Data, sectionType: sectionType);
            var questions = new List<QuestionSortDto>();
            var scoreDict = paperResult.Data.PaperSections.Where(s => s.PaperSectionType == sectionType)
                .SelectMany(s => s.Questions)
                .ToDictionary(k => k.Question.Id, v => v.Score);
            foreach (var sort in sorts)
            {
                var item = new QuestionSortDto
                {
                    Id = sort.Key,
                    Sort = sort.Value,
                    Objective = false,
                    Score = 0M
                };
                if (scoreDict.ContainsKey(sort.Key))
                    item.Score = scoreDict[sort.Key];
                questions.Add(item);
            }

            //圈子详细
            var groupResult = GroupContract.LoadById(usage.ClassId);
            if (!groupResult.Status)
                return DResult.Error<MarkingDataDto>("未查询到圈子资料");
            var group = groupResult.Data;

            //圈子中的学生
            var studentResult = GroupContract.GroupMembers(group.Id, UserRole.Student);
            var students = studentResult.Status ? studentResult.Data.ToList() : new List<MemberDto>();

            //答案分享列表
            var shares =
                AnswerShareRepository.Where(s => s.Batch == batch && s.Status == (byte)AnswerShareStatus.PreShare);

            //阅卷图片列表
            var pictures = MarkingPictureRepository
                .Where(p => p.AnswerImgType == (byte)type && p.BatchNo == batch)
                .OrderBy(p => p.AddedAt)
                .ThenBy(p => p.SubmitSort)
                .MapTo<List<MkPictureDto>>();
            var lastPicture = string.Empty;
            if (pictures.Any())
            {
                if (pictures.Any(p => p.MarkingTime.HasValue))
                {
                    lastPicture =
                        pictures.Where(p => p.MarkingTime.HasValue).OrderByDescending(p => p.MarkingTime.Value)
                            .Select(p => p.PictureId).FirstOrDefault();
                }
                else
                {
                    lastPicture = pictures.First().PictureId;
                }
            }

            //未提交的学生名单
            var submitIds = pictures.Where(p => p.StudentId > 0).Select(p => p.StudentId).ToList();

            return DResult.Succ(new MarkingDataDto
            {
                Batch = batch,
                PaperId = paper.Id,
                PaperTitle = paper.PaperTitle,
                SectionType = type,
                MarkingStatus = usage.MarkingStatus,
                IsAb = paper.IsAb,
                IsBag = false,
                Areas = mark,
                Questions = questions,
                Pictures = pictures,
                LastPicture = lastPicture,
                AllObjective = allObjective,
                DeyiGroupId = group.Id,
                DeyiGroupName = group.Name,
                UnSubmits = students.Where(u => !submitIds.Contains(u.Id)).Select(u => u.Name).ToList(),
                Shares = shares.Select(s => new MarkingDataAnswerShareDto
                {
                    Id = s.Id,
                    QuestionId = s.QuestionId,
                    StudentId = s.AddedBy
                }).ToList()
            });
        }

        /// <summary> 答卷图片详细 </summary>
        /// <param name="pictureId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public DResult<MkPictureDto> MkPictureDetail(string pictureId, int type)
        {
            var item = MarkingPictureRepository.Load(pictureId);
            if (item == null)
                return DResult.Error<MkPictureDto>("没有查询到答卷资料");

            //试卷详细
            var paperResult = PaperContract.PaperDetailById(item.PaperID);
            if (!paperResult.Status || paperResult.Data == null)
                return DResult.Error<MkPictureDto>("没有查询到试卷资料");

            //返回数据
            var result = item.MapTo<MkPictureDto>();
            if (result == null)
                return DResult.Error<MkPictureDto>("没有查询到答卷资料");
            var sectionType = type == (byte)MarkingPaperType.Normal
                ? (byte)PaperSectionType.PaperA
                : type;
            var sectionQuestions = paperResult.Data.PaperSections.Where(s => s.PaperSectionType == sectionType)
                .SelectMany(s => s.Questions.Select(q => new { q.Question.Id, q.Question.IsObjective }))
                .ToList();
            var details =
                MarkingDetailRepository.Where(
                    d =>
                        d.Batch == item.BatchNo && d.PaperID == item.PaperID && d.StudentID == item.StudentID)
                    .Select(d => new MkDetailMinDto
                    {
                        Id = d.Id,
                        QuestionId = d.QuestionID,
                        Score = d.CurrentScore,
                        IsCorrect = d.IsCorrect
                    })
                    .ToList();
            var subjectiveIds = sectionQuestions.Where(s => !s.IsObjective).Select(s => s.Id);
            result.Details = details.Where(d => subjectiveIds.Contains(d.QuestionId)).ToList();
            var ids = sectionQuestions.Select(s => s.Id);
            result.TotalScore = details.Where(d => ids.Contains(d.QuestionId)).Sum(d => d.Score);
            return DResult.Succ(result);
        }

        #endregion

        #region 阅卷结束后 - 相关查询

        /// <summary> 查询试卷批改详情 </summary>
        public DResults<MarkedDetailDto> GetMarkedDetail(string batch, string paperId, long userId)
        {
            if (string.IsNullOrEmpty(batch) || string.IsNullOrEmpty(paperId) || userId < 1)
                return DResult.Errors<MarkedDetailDto>("参数错误！");

            var result = MarkingDetailRepository
                .Where(u => u.Batch == batch && u.PaperID == paperId && u.StudentID == userId)
                .Select(m => new MarkedDetailDto
                {
                    AnswerContent = m.AnswerContent,
                    AnswerImages = m.AnswerImages,
                    Batch = m.Batch,
                    PaperId = m.PaperID,
                    QuestionId = m.QuestionID,
                    SmallQId = m.SmallQID,
                    CurrentScore = m.CurrentScore,
                    IsCorrect = m.IsCorrect,
                    IsFinished = m.IsFinished
                });

            return DResult.Succ<MarkedDetailDto>(result);
        }

        /// <summary> 查询答卷图片 </summary>
        public DResult<MkPictureDto> LoadPictureDto(string batch, string paperId, long studentId, MarkingPaperType type = MarkingPaperType.Normal)
        {
            Expression<Func<TP_MarkingPicture, bool>> condition = p =>
                p.BatchNo == batch && p.PaperID == paperId && p.StudentID == studentId;
            condition = type == MarkingPaperType.PaperB
                ? condition.And(p => p.AnswerImgType == (byte)MarkingPaperType.PaperB)
                : condition.And(p => (p.AnswerImgType == (byte)MarkingPaperType.PaperA ||
                                      p.AnswerImgType == (byte)MarkingPaperType.Normal));
            var picture = MarkingPictureRepository.FirstOrDefault(condition).MapTo<MkPictureDto>();
            return DResult.Succ(picture);
        }

        /// <summary> 查询阅卷结果 </summary>
        public DResult<MarkingResultDto> LoadResultDto(string batch, string paperId, long studentId)
        {
            var result = MarkingResultRepository
                .FirstOrDefault(r => r.Batch == batch && r.PaperID == paperId && r.StudentID == studentId)
                .MapTo<MarkingResultDto>();
            return DResult.Succ(result);
        }

        #endregion

        //#region 阅卷结束后 - 更新成绩统计

        /// <summary> 编辑统计分数 </summary>
        public DResult UpdateScoreStatistics(string batch, string paperId, long teacherId, List<StudentRankInfoDto> data)
        {
            var usage = UsageRepository.FirstOrDefault(u => u.Id == batch);
            if (usage == null)
                return DResult.Error("没有查询到发布记录");
            if (usage.UserId != teacherId)
                return DResult.Error("没有编辑权限");
            var paperResult = PaperContract.PaperDetailById(paperId, false);
            if (!paperResult.Status)
                return DResult.Error("没有查询到试卷资料");
            var paper = paperResult.Data.PaperBaseInfo;

            // update by epc 16/03/28
            // 先干一件事：将 data.id 等于空的数据 验证并添加到数据库，后续逻辑无须改变
            var studentIds = data.Where(d => d.Id.IsNullOrEmpty()).Select(d => d.StudentId).ToList();
            if (studentIds.Any())
            {
                if (!ScoreStatistics(usage, paperId, studentIds))
                    return DResult.Error("录入学生分数失败，请重试");
            }

            var classScore = ClassScoreStatisticsRepository
                .FirstOrDefault(c => c.Batch == batch && c.PaperId == paperId && c.ClassId == usage.ClassId);
            var stuScores = StuScoreStatisticsRepository
                .Where(s => s.Batch == batch && s.PaperId == paperId && s.ClassId == usage.ClassId).ToList();
            if (classScore == null || !stuScores.Any())
                return DResult.Error("没有查询到分数统计资料");


            decimal scoreA = 0M, scoreB = 0M;
            if (paper.IsAb)
            {
                scoreA = paper.PaperScores.TScoreA;
                scoreB = paper.PaperScores.TScoreB;
            }

            #region TS_StuScoreStatistics

            data.ForEach(i =>
            {
                var item = stuScores.FirstOrDefault(s => s.StudentId == i.StudentId);
                if (item == null) return;

                if (i.TotalScore < 0) i.TotalScore = 0;
                if (i.SectionAScore < 0) i.SectionAScore = 0;
                if (i.SectionBScore < 0) i.SectionBScore = 0;
                if (paper.IsAb)
                {
                    if (i.SectionAScore > paper.PaperScores.TScoreA)
                        i.SectionAScore = paper.PaperScores.TScoreA;
                    if (i.SectionBScore > paper.PaperScores.TScoreB)
                        i.SectionBScore = paper.PaperScores.TScoreB;
                    i.TotalScore = i.SectionAScore + i.SectionBScore;
                }
                if (i.TotalScore > paper.PaperScores.TScore)
                    i.TotalScore = paper.PaperScores.TScore;

                item.CurrentScore = i.TotalScore ?? 0M;
                item.SectionAScore = i.SectionAScore ?? 0M;
                item.SectionBScore = i.SectionBScore ?? 0M;
            });
            stuScores.ForEach(s =>
            {
                s.CurrentSort = stuScores.Count(t => t.CurrentScore > s.CurrentScore) + 1;
            });

            #endregion

            #region TS_ClassScoreStatistics

            List<decimal> currentScores = stuScores.Select(m => m.CurrentScore).ToList(),
                aScores = new List<decimal>(),
                bScores = new List<decimal>();

            if (paper.IsAb)
            {
                aScores = stuScores.Select(s => s.SectionAScore).ToList();
                bScores = stuScores.Select(s => s.SectionBScore).ToList();
            }
            //班级统计数据
            InitClassScoreStatistics(classScore,
                 currentScores, aScores, bScores,
                 paper.PaperScores.TScore, scoreA, scoreB,
                 paper.IsAb);

            #endregion

            #region 事务处理

            var result = UnitOfWork.Transaction(() =>
            {
                StuScoreStatisticsRepository.Update(s => new
                {
                    s.CurrentScore,
                    s.CurrentSort,
                    s.SectionAScore,
                    s.SectionBScore
                }, stuScores.ToArray());

                ClassScoreStatisticsRepository.Update(c => new
                {
                    c.AverageScore,
                    c.TheHighestScore,
                    c.TheLowestScore,
                    c.ScoreGroups,
                    c.SectionScores,
                    c.SectionScoreGroups
                }, classScore);
            });

            #endregion

            return result > 0 ? DResult.Success : DResult.Error("操作失败，请重试");
        }

        /// <summary>
        /// 教师录入分数后，初始化统计数据
        /// add by epc 16/03/28
        /// </summary>
        /// <returns></returns>
        bool ScoreStatistics(TC_Usage usage, string paperId, List<long> studentIds)
        {
            if (usage.SourceType != (byte)PublishType.Test) return true;
            if (!studentIds.Any()) return true;
            //不属于该班级的学生 不新增
            var studentsResult = GroupContract.GroupMembers(usage.ClassId, UserRole.Student);
            if (!studentsResult.Status || !studentsResult.Data.Any()) return false;
            var classes = studentsResult.Data.Select(stu => stu.Id).ToList();
            studentIds = studentIds.Where(classes.Contains).ToList();
            if (!studentIds.Any()) return true;

            //已存在的 不新增
            var addeds = StuScoreStatisticsRepository.Where(s =>
                s.Batch == usage.Id && s.PaperId == paperId && s.ClassId == usage.ClassId)
                .Select(s => s.StudentId).ToList();
            studentIds = studentIds.Where(id => !addeds.Contains(id)).ToList();
            if (!studentIds.Any()) return true;

            var sort = addeds.Count + 1;

            return UnitOfWork.Transaction(() =>
            {
                #region TS_StuScoreStatistics

                var list = new List<TS_StuScoreStatistics>();
                studentIds.ForEach(id => list.Add(new TS_StuScoreStatistics
                {
                    Id = IdHelper.Instance.GetGuid32(),
                    Batch = usage.Id,
                    PaperId = paperId,
                    ClassId = usage.ClassId,
                    AddedAt = usage.AddedAt,
                    AddedBy = usage.UserId,
                    CurrentScore = 0M,
                    CurrentSort = sort,
                    ErrorQuCount = 0,
                    SectionAScore = 0M,
                    SectionBScore = 0M,
                    Status = (byte)StuStatisticsStatus.Normal,
                    StudentId = id,
                    SubjectId = usage.SubjectId
                }));
                StuScoreStatisticsRepository.Insert(list.ToArray());

                #endregion

                #region TS_ClassScoreStatistics

                var exists = ClassScoreStatisticsRepository.Exists(
                    c => c.Batch == usage.Id && c.PaperId == paperId && c.ClassId == usage.ClassId);
                if (!exists)
                {
                    ClassScoreStatisticsRepository.Insert(new TS_ClassScoreStatistics
                    {
                        Id = IdHelper.Instance.GetGuid32(),
                        Batch = usage.Id,
                        PaperId = paperId,
                        ClassId = usage.ClassId,
                        AddedBy = usage.UserId,
                        AddedAt = usage.AddedAt,
                        AverageScore = 0M,
                        ScoreGroups = string.Empty,
                        SectionScoreGroups = string.Empty,
                        SectionScores = string.Empty,
                        SubjectId = usage.SubjectId,
                        Status = (byte)ClassStatisticsStatus.Normal,
                        TheHighestScore = 0M,
                        TheLowestScore = 0M
                    });
                }

                #endregion
            }) > 0;
        }

        //#endregion

        //#region 更新批阅结果 - 未完成阅卷时，更改选择题答案

        ///// <summary> 更新批阅结果 - 未完成阅卷时，更改选择题答案 </summary>
        //public void MkUpdateDetailByQuestionChange(string questionId, string paperId, string smallId = null)
        //{
        //    if (string.IsNullOrEmpty(paperId))
        //    {
        //        var ids =
        //            PaperContentRepository.Where(t => t.QuestionID == questionId).Select(t => t.PaperID).Distinct();
        //        foreach (var id in ids)
        //        {
        //            MkUpdateDetailByQuestionChange(questionId, id, smallId);
        //        }
        //        return;
        //    }
        //    var sender = QuestionRepository.Load(questionId);
        //    //跳过非主观题或者没有用过的题目
        //    if (!sender.IsObjective || !sender.IsUsed.HasValue || !sender.IsUsed.Value)
        //        return;
        //    var sb = new StringBuilder();
        //    sb.AppendLine(string.Empty);
        //    var watch = new Stopwatch();
        //    watch.Start();
        //    //获取客观题答案
        //    var answerDict = PaperContract.QuestionAnswer(questionId, smallId, paperId);

        //    try
        //    {
        //        sb.AppendLine(string.Format("开始更新题目[{0}，{1}]的阅卷结果", sender.Id, paperId));
        //        var results = MarkingResultRepository.Where(t => t.PaperID == paperId && !t.IsFinished);
        //        Expression<Func<TP_MarkingDetail, bool>> detailCondition =
        //            d => d.PaperID == paperId && d.QuestionID == questionId;
        //        if (!string.IsNullOrWhiteSpace(smallId))
        //            detailCondition = detailCondition.And(d => d.SmallQID == smallId);

        //        var details = MarkingDetailRepository.Where(detailCondition)
        //            .Join(results, s => s.MarkingID, d => d.Id, (s, d) => s)
        //            .ToList();
        //        sb.AppendLine(string.Format("找到[{0}]条MarkingDetail记录", details.Count()));
        //        foreach (var detail in details)
        //        {
        //            var id = string.IsNullOrWhiteSpace(detail.SmallQID) ? detail.QuestionID : detail.SmallQID;
        //            if (!answerDict.ContainsKey(id))
        //                continue;
        //            var answers = answerDict[id];
        //            var studentAnswers = (detail.AnswerIDs ?? "[]").JsonToObject<string[]>();
        //            var correct = studentAnswers.ArrayEquals(answers);
        //            detail.IsCorrect = correct;
        //            if (!correct)
        //            {
        //                //不定项扣一半
        //                if (sender.QType == 3 && studentAnswers.All(a => answers.Contains(a)))
        //                    detail.CurrentScore = detail.Score / 2M;
        //                else
        //                    detail.CurrentScore = 0;
        //            }
        //            else
        //            {
        //                detail.CurrentScore = detail.Score;
        //            }
        //        }
        //        MarkingDetailRepository.Update(d => new
        //        {
        //            d.IsCorrect,
        //            d.CurrentScore
        //        }, details.ToArray());
        //        sb.AppendLine("阅卷结果更新完成！");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error("阅卷结果更新失败:" + ex.Message, ex);
        //    }
        //    finally
        //    {
        //        watch.Stop();
        //        sb.AppendLine(string.Format("耗时：{0}ms", watch.ElapsedMilliseconds));
        //        _logger.Info(sb.ToString());
        //    }
        //}
        //#endregion

        /// <summary> 异步任务提交试卷图片 </summary>
        /// <param name="paperId"></param>
        /// <param name="pictureIds"></param>
        /// <param name="userId"></param>
        /// <param name="jointBatch"></param>
        /// <returns></returns>
        public Task CommitPictureAsync(long userId, string paperId, List<string> pictureIds, string jointBatch = null)
        {
            return Task.Run(() =>
            {
                MissionHelper.PushMission(MissionType.CommitPicture, new CommitPictureParam
                {
                    PaperId = paperId,
                    PictureIds = pictureIds,
                    JointBatch = jointBatch
                }, userId);
                //var count = 0;
                //var helper = new MarkingPictureHelper(paperId, pictureIds, jointBatch);
                //while (count < 5)
                //{
                //    _logger.Info("试卷：{0}，协同：{1}，第{2}次MarkingPictureHelper.Commit", paperId, jointBatch, count + 1);
                //    var result = helper.Commit();
                //    if (result)
                //        break;
                //    count++;
                //    Thread.Sleep(TimeSpan.FromMinutes(2 * count));
                //}
            });
        }

        /// <summary> 是否已完成阅卷 </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        public bool IsFinished(string batch)
        {
            if (UsageRepository.Exists(t => t.MarkingStatus == (byte)MarkingStatus.AllFinished && t.Id == batch))
                return true;
            if (JointMarkingRepository.Exists(t => t.Status == (byte)JointStatus.Finished && t.Id == batch))
                return true;
            return false;
        }

        #region 按照批次号、问题、当前答卷、获取对应图片
        /// <summary> 按照批次号、问题、当前答卷、获取对应图片 </summary>
        public DResult InterceptPicture(string batch, string paperId, long studentNo, MarkingPaperType paperType, string questionId)
        {
            Dictionary<string, MkQuestionAreaDto> dic = new Dictionary<string, MkQuestionAreaDto>();
            if (string.IsNullOrEmpty(questionId))
            {
                return DResult.Error("未找到该问题");
            }
            if (string.IsNullOrEmpty(batch))
            {
                return DResult.Error("未找到该批次试卷");
            }
            var joint = UsageRepository.Where(w => w.Id == batch).Select(s => s.JointBatch).FirstOrDefault();
            //如果当前批次号不是协同则为普通批次
            if (string.IsNullOrEmpty(joint))
                dic = JointHelper.QuestionAreaCache(batch);
            else
                dic = JointHelper.QuestionAreaCache(joint);
            if (!dic.Any() || !dic.ContainsKey(questionId))
            {
                return DResult.Error("未找到题目区域");
            }
            var questionArea = dic[questionId];//取得当前题目区域
            var answerPic = LoadPictureDto(batch, paperId, studentNo, paperType);//获得当前答题试卷
            if (!answerPic.Status || answerPic.Data == null || string.IsNullOrEmpty(answerPic.Data.PictureUrl))
            {
                return DResult.Error("未找到该试卷");
            }
            var resultPicUrl = answerPic.Data.PictureUrl;
            var x = Math.Max((int)Math.Ceiling(questionArea.X - 20), 0);
            var y = Math.Max((int)Math.Ceiling(questionArea.Y - 20), 0);
            var width = Math.Min((int)Math.Ceiling(questionArea.Width + 40), 780);
            var height = (int)Math.Ceiling(questionArea.Height) + 40;
            resultPicUrl = resultPicUrl.PaperImage(x, y, width, height);
            return DResult.Succ(resultPicUrl);
        }
        #endregion
    }
}
