using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.ErrorQuestion;
using DayEasy.Contracts.Dtos.Marking;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain;
using DayEasy.EntityFramework;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.Marking.Services
{
    public partial class ErrorBookService : DayEasyService, IErrorBookContract
    {
        public ErrorBookService(IDbContextProvider<DayEasyDbContext> context)
            : base(context)
        { }

        public ISystemContract SystemContract { private get; set; }
        public IPaperContract PaperContract { private get; set; }
        public IGroupContract GroupContract { private get; set; }
        public IDayEasyRepository<TC_Usage, string> UsageRepository { private get; set; }
        public IDayEasyRepository<TP_ErrorQuestion, string> ErrorQuestionRepository { private get; set; }
        public IDayEasyRepository<TP_ErrorReason, string> ErrorReasonRepository { private get; set; }
        public IDayEasyRepository<TQ_Question, string> QuestionRepository { private get; set; }
        public IDayEasyRepository<TQ_SmallQuestion, string> SmallQuestionRepository { private get; set; }

        public IDayEasyRepository<TP_ErrorTag, string> ErrorTagRepository { private get; set; }
        public IDayEasyRepository<TS_Knowledge, int> KnowledgeRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingResult, string> MarkingResultRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingDetail, string> MarkingDetailRepository { private get; set; }
        public IDayEasyRepository<TP_PaperContent, string> PaperContentRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingPicture, string> MarkingPictureRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingMark, string> MarkingMarkRepository { private get; set; }

        /// <summary>
        /// 错题错因分析
        /// </summary>
        /// <param name="reasons"></param>
        /// <param name="errorId"></param>
        /// <returns></returns>
        internal ReasonDto FirstReason(IEnumerable<TP_ErrorReason> reasons, string errorId)
        {
            var reason = reasons.FirstOrDefault(r => r.ErrorId == errorId);
            if (reason == null) return null;
            var item = new ReasonDto
            {
                Id = reason.Id,
                ErrorId = reason.ErrorId,
                Content = reason.Content,
                Count = reason.CommentCount,
                AddedAt = reason.AddedAt
            };
            if (!reason.Tags.IsNotNullOrEmpty()) return item;
            item.TagList = JsonHelper.JsonList<NameDto>(reason.Tags).ToList();
            item.Tag = string.Join(",", item.TagList.Select(t => t.Name));
            return item;
        }

        /// <summary> 更改错题状态 </summary>
        public DResult UpdateErrorQuestionStatus(string errorId, ErrorQuestionStatus status, long studentId)
        {
            if (errorId.IsNullOrEmpty()) return DResult.Error("错题ID不能为空");
            var item = ErrorQuestionRepository.Load(errorId);
            if (item == null) return DResult.Error("没有查询到错题详细");
            if (item.StudentID != studentId) return DResult.Error("不能操作他人的错题");
            if (item.Status == (byte)status) return DResult.Success;
            item.Status = (byte)status;
            return ErrorQuestionRepository.Update(e => new { e.Status }, item) > 0
                ? DResult.Success
                : DResult.Error("操作失败");
        }

        /// <summary> 设置过关、取消过关 </summary>
        public DResult SetPass(string errorId, long studentId, bool pass)
        {
            if (errorId.IsNullOrEmpty()) return DResult.Error("错题ID不能为空");
            var item = ErrorQuestionRepository.Load(errorId);
            if (item == null) return DResult.Error("没有查询到错题详细");
            if (item.StudentID != studentId) return DResult.Error("不能操作他人的错题");

            var passed = (item.Status & (byte)ErrorQuestionStatus.Pass) > 0;
            if ((passed && pass) || (!passed && !pass))
                return DResult.Success;

            if (pass)
            {
                item.Status |= (byte)ErrorQuestionStatus.Pass;
            }
            else
            {
                item.Status ^= (byte)ErrorQuestionStatus.Pass;
            }

            return ErrorQuestionRepository.Update(e => new { e.Status }, item) > 0
                ? DResult.Success
                : DResult.Error("操作失败");
        }

        /// <summary>
        /// 错题列表
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public DResults<ErrorQuestionDto> ErrorQuestions(ErrorQuestionSearchDto search)
        {
            if (search == null || search.StudentId < 10)
                return DResult.Errors<ErrorQuestionDto>("参数错误");
            //查询条件
            Expression<Func<TP_ErrorQuestion, bool>> condition = q =>
                q.StudentID == search.StudentId
                && q.Status != (byte)ErrorQuestionStatus.Delete
                && q.SourceType == (byte)ErrorQuestionSourceType.Paper;
            if (search.IsPass.HasValue)
            {

                condition = search.IsPass.Value
                    ? condition.And(c => (c.Status & (byte)ErrorQuestionStatus.Pass) > 0)
                    : condition.And(c => (c.Status & (byte)ErrorQuestionStatus.Pass) == 0);
            }

            if (search.QType > 0)
                condition = condition.And(q => q.QType == search.QType);
            if (search.SubjectId > 0)
                condition = condition.And(q => q.SubjectID == search.SubjectId);
            if (search.StartTime != null && search.StartTime > DateTime.MinValue)
                condition = condition.And(q => q.AddedAt >= search.StartTime);
            if (search.EndTime != null && search.EndTime < DateTime.MaxValue)
                condition = condition.And(q => q.AddedAt < search.EndTime);
            if (search.Key.IsNotNullOrEmpty())
            {
                //知识点、题干搜索
                var questionIds = QuestionRepository
                    .Where(q => q.KnowledgeIDs.Contains(search.Key) || q.QContent.Contains(search.Key))
                    .Select(q => q.Id);
                //标签搜索
                var errorId = ErrorReasonRepository.Where(r => r.Tags.Contains(search.Key)).Select(r => r.ErrorId);
                condition = condition.And(q => questionIds.Contains(q.QuestionID) || errorId.Contains(q.Id));
            }
            if (search.HasReason != null)
            {
                //错因分析筛选
                var errorId = ErrorReasonRepository.Where(r => r.StudentId == search.StudentId).Select(r => r.ErrorId);
                condition = search.HasReason.Value
                    ? condition.And(q => errorId.Contains(q.Id))
                    : condition.And(q => !errorId.Contains(q.Id));
            }

            var ordered = ErrorQuestionRepository.Where(condition).OrderByDescending(q => q.AddedAt);
            var eqResult = ErrorQuestionRepository.PageList(ordered, search.Page);
            if (!eqResult.Status || !eqResult.Data.Any())
                return DResult.Succ(new List<ErrorQuestionDto>(), 0);

            //错题对应科目
            var subjectIds = eqResult.Data.Select(e => e.SubjectID).Distinct().ToList();
            var subjects = SystemContract.SubjectDict(subjectIds);

            //错因分析
            var errorIds = eqResult.Data.Select(e => e.Id).ToList();
            var reasons = ErrorReasonRepository.Where(r => errorIds.Contains(r.ErrorId)).ToList();

            //试卷
            var questions = PaperContract.LoadQuestions(eqResult.Data.Select(e => e.QuestionID).ToArray());
            if (questions == null || !questions.Any())
                return DResult.Succ(new List<ErrorQuestionDto>(), 0);


            return DResult.Succ((from e in eqResult.Data
                                 from q in questions
                                 where e.QuestionID == q.Id
                                 select new ErrorQuestionDto
                                 {
                                     Id = e.Id,
                                     Batch = e.Batch,
                                     PaperId = e.PaperID,
                                     PaperTitle = e.PaperTitle,
                                     Status = e.Status,
                                     Time = Utils.GetTime(e.AddedAt),
                                     SubjectId = e.SubjectID,
                                     SubjectName = subjects[e.SubjectID],
                                     Question = q,
                                     Reason = FirstReason(reasons, e.Id)
                                 }).ToList(), eqResult.TotalCount);
        }
        /// <summary>
        /// 错题医院错题列表
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public DResults<DErrorQuestionDto> ErrorQuestions(SearchErrorQuestionDto dto)
        {
            List<DErrorQuestionDto> list = null;
            List<QuestionDto> questions = null;
            if (string.IsNullOrEmpty(dto.GroupId))
                return DResult.Errors<DErrorQuestionDto>("圈子ID不能为空");
            DPage page;
            if (dto.pageIndex < 0 && dto.pageSize <= 0)
                page = DPage.NewPage();
            else
                page = DPage.NewPage(dto.pageIndex, dto.pageSize);
            Expression<Func<TP_ErrorQuestion, bool>> condition =
            u => u.SubjectID == dto.SubjectId;
            if (dto.DateRange > 0)
            {
                var dateNow = Clock.Now;
                var pastTimes = dateNow.AddDays(-dto.DateRange);
                condition = condition.And(w => w.AddedAt >= pastTimes && w.AddedAt <= dateNow);
            }
            if (dto.QuestionType > 0)
            {
                condition = condition.And(w => w.QType == dto.QuestionType);
            }
            if (dto.UserId > 0)
            {
                condition = condition.And(w => w.StudentID == dto.UserId);
            }
            else
            {
                var groupMembers = GroupContract.GroupMembers(dto.GroupId, UserRole.Student);
                if (!groupMembers.Status || !groupMembers.Data.Any())
                    return DResult.Errors<DErrorQuestionDto>("该班级没有学生");
                var memberIds = groupMembers.Data.Select(w => w.Id).ToList();
                if (memberIds.Any())
                    condition = condition.And(w => memberIds.Contains(w.StudentID));
            }
            var errorQuestions = ErrorQuestionRepository.Where(condition);
            if (!errorQuestions.Any())
                return DResult.Errors<DErrorQuestionDto>("没有错题信息");
            var groups = errorQuestions.GroupBy(w => w.QuestionID)
                .Select(g => new DErrorQuestionDto
                {
                    Id = g.Max(w => w.Id),
                    ErrUserCount = g.Count(),
                    PaperTitle = g.Max(w => w.PaperTitle),
                    Batch = g.Max(w => w.Batch),
                    PaperId = g.Max(w => w.PaperID),
                    QuestionId = g.Max(w => w.QuestionID),
                    CreateTime = g.Max(w => w.AddedAt),
                });
            if (dto.OrderOfArr == 1)
                list = groups.OrderByDescending(w => w.ErrUserCount).ToList();
            else if (dto.OrderOfArr == 2)
                list = groups.OrderBy(w => w.ErrUserCount).ToList();
            else
                list = groups.OrderByDescending(w => w.CreateTime).ToList();
            var ids = groups.Select(w => w.QuestionId).Distinct().ToList();
           
            questions = PaperContract.LoadQuestions(ids);
            string[] errorids;
            if (!string.IsNullOrEmpty(dto.KnowledgeCode) && dto.KnowledgeCode != "0")
            {
                questions = questions.Where(w => w.KnowledgeIDs.Contains(dto.KnowledgeCode)).ToList();
                if (questions.Any())
                    errorids = questions.Select(w => w.Id).ToArray();
                else
                    errorids = null;
            }
            else
            {
                errorids = null;
            }
            var count = questions.Count;
            var dic = questions.ToDictionary(k => k.Id, v => v.Body);
            list = list.Where(w => dic.Keys.Contains(w.QuestionId)).ToList();
            if (errorids != null)
                list = list.Where(w => errorids.Contains(w.QuestionId)).ToList();
            var l = list.DistinctBy(w => w.QuestionId).Skip(page.Page * page.Size).Take(page.Size).ToList();
            int sort = page.Page * page.Size;
            l.Foreach(q =>
            {
                sort++;
                q.Sort = sort;
                if (dic.ContainsKey(q.QuestionId))
                {
                    var question = questions.Single(w => w.Id == q.QuestionId);
                    q.QuestionContent = dic[q.QuestionId];
                    q.Question= question;
                }
                else
                    q.QuestionContent = "此题目不存在";
            });
            return DResult.Succ(l, count);
        }

        /// <summary>
        /// 错题详细 - 学生
        /// </summary>
        /// <param name="errorId"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        public DResult<ErrorQuestionDto> ErrorQuestion(string errorId, long studentId)
        {
            var item = ErrorQuestionRepository.FirstOrDefault(e =>
                e.Id == errorId &&
                e.Status != (byte)ErrorQuestionStatus.Delete &&
                e.SourceType == (byte)ErrorQuestionSourceType.Paper);
            if (item == null)
                return DResult.Error<ErrorQuestionDto>("没有查询到错题详细");
            if (item.StudentID != studentId)
                return DResult.Error<ErrorQuestionDto>("只能查看自己的错题");
            var usage = UsageRepository.FirstOrDefault(u => u.Id == item.Batch);
            if (usage == null)
                return DResult.Error<ErrorQuestionDto>("没有查询到发布记录");

            var result = new ErrorQuestionDto
            {
                Id = item.Id,
                Batch = item.Batch,
                PaperId = item.PaperID,
                PaperTitle = item.PaperTitle,
                GroupId = usage.ClassId,
                Time = Utils.GetTime(item.AddedAt),
                SubjectId = item.SubjectID,
                SourceType = item.SourceType,
                Status = item.Status,
                Question = PaperContract.LoadQuestion(item.QuestionID)
            };

            //标签
            var reason = ErrorReasonRepository.FirstOrDefault(r => r.ErrorId == item.Id);
            if (reason == null) return DResult.Succ(result);

            var ver = reason.MapTo<ReasonDto>();
            if (reason.Tags != null)
            {
                ver.TagList = JsonHelper.Json<List<NameDto>>(reason.Tags);
                ver.Tag = string.Join(",", ver.TagList.Select(t => t.Name));
            }
            ver.IsFinished = ver.Content.IsNotNullOrEmpty() && ver.TagList != null && ver.TagList.Any();
            if (ver.TagList != null && ver.TagList.Any())
                result.Reason = ver;
            return DResult.Succ(result);
        }

        /// <summary>
        /// 错题详细 - 教师
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="questionId"></param>
        /// <returns></returns>
        public DResult<ErrorQuestionDto> ErrorQuestion(string batch, string paperId, string questionId)
        {
            var usage = UsageRepository.FirstOrDefault(u => u.Id == batch);
            if (usage == null)
                return DResult.Error<ErrorQuestionDto>("没有查询到发布记录");
            var paperResult = PaperContract.PaperDetailById(paperId, false);
            if (!paperResult.Status)
                return DResult.Error<ErrorQuestionDto>("没有查询到试卷资料");
            var question = PaperContract.LoadQuestion(questionId);
            if (question == null)
                return DResult.Error<ErrorQuestionDto>("没有查询到问题资料");

            return DResult.Succ(new ErrorQuestionDto
            {
                Id = string.Empty,
                Batch = batch,
                PaperId = paperId,
                PaperTitle = paperResult.Data.PaperBaseInfo.PaperTitle,
                GroupId = usage.ClassId,
                Time = Utils.GetTime(usage.ExpireTime),
                SourceType = usage.SourceType,
                Question = question
            });
        }

        /// <summary>
        /// 标记推送试卷中的错题
        /// </summary>
        /// <returns></returns>
        public DResult<string> MarkErrorQuestion(string batch, string paperId, string qId, long studentId)
        {
            if (string.IsNullOrEmpty(batch) || string.IsNullOrEmpty(paperId) || string.IsNullOrEmpty(qId) ||
                studentId < 1)
                return DResult.Error<string>("参数错误，请稍后重试！");

            var publishModel =
                UsageRepository.SingleOrDefault(u => u.Id == batch && u.SourceType != (byte)PublishType.Print);
            if (publishModel == null) return DResult.Error<string>("没有找到该试卷！");

            //查询试卷
            var paperResult = PaperContract.PaperDetailById(paperId, false);
            if (!paperResult.Status || paperResult.Data == null)
                return DResult.Error<string>("加入失败，请稍后重试！");
            var paper = paperResult.Data.PaperBaseInfo;

            var question = PaperContract.LoadQuestion(qId);
            if (question == null)
                return DResult.Error<string>("该问题没有找到！");

            var existId = ErrorQuestionRepository.Where(i =>
                i.Batch == batch && i.PaperID == paperId && i.QuestionID == qId && i.StudentID == studentId)
                .Select(t => t.Id).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(existId))
                return DResult.Succ(existId);

            var errorQu = new TP_ErrorQuestion
            {
                Id = IdHelper.Instance.Guid32,
                PaperID = paperId,
                Batch = batch,
                QuestionID = qId,
                StudentID = studentId,
                PaperTitle = paper.PaperTitle,
                SubjectID = paper.SubjectId,
                Stage = paper.Stage,
                QType = question.Type,
                AddedAt = Clock.Now,
                Status = (byte)ErrorQuestionStatus.Marked,
                VariantCount = 0
            };

            var result = ErrorQuestionRepository.Insert(errorQu);
            return string.IsNullOrEmpty(result)
                ? DResult.Error<string>("加入失败，请稍后重试！")
                : DResult.Succ(errorQu.Id);
        }

        /// <summary> 学生答案 </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="questionId"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        public DResult<dynamic> ErrorAnswer(string batch, string paperId, string questionId, long studentId)
        {
            if (string.IsNullOrWhiteSpace(batch))
                return DResult.Error<dynamic>("批次号不能为空！");
            if (string.IsNullOrWhiteSpace(paperId))
                return DResult.Error<dynamic>("试卷ID不能为空！");
            if (string.IsNullOrWhiteSpace(questionId))
                return DResult.Error<dynamic>("问题ID不能为空！");
            if (studentId <= 0)
                return DResult.Error<dynamic>("学生ID不正确！");
            var usage = UsageRepository.FirstOrDefault(t => t.Id == batch && t.SourceID == paperId);
            if (usage == null)
                return DResult.Error<dynamic>("未找到发布批次！！");
            //学生作答详情
            var details = MarkingDetailRepository.Where(a =>
                a.Batch == batch && a.PaperID == paperId
                && a.QuestionID == questionId && a.StudentID == studentId)
                .Join(SmallQuestionRepository.Table, d => d.SmallQID, s => s.Id, (d, s) => new
                {
                    d.Id,
                    d.AnswerContent,
                    d.AnswerImages,
                    s.Sort
                }).OrderBy(t => t.Sort).ToList();

            if (usage.ApplyType != (byte)ApplyType.Print)
            {
                //非打印试卷
                var answers = details.Select(a => new
                {
                    AnswerId = a.Id,
                    Body = a.AnswerContent,
                    Images =
                        (a.AnswerImages.IsNotNullOrEmpty() ? new string[] { } : a.AnswerImages.JsonToObject<string[]>())
                });
                return DResult.Succ<dynamic>(new { HasAnswer = true, IsPrint = false, answers });
            }

            var content = PaperContentRepository
                .FirstOrDefault(c => c.PaperID == paperId && c.QuestionID == questionId);
            //是否B卷
            var isB = (content != null && content.PaperSectionType == (byte)PaperSectionType.PaperB);

            var picture = MarkingPictureRepository.FirstOrDefault(p =>
                p.BatchNo == batch && p.StudentID == studentId);
            if (picture == null || picture.AnswerImgUrl.IsNullOrEmpty())
                return DResult.Succ<dynamic>(new
                {
                    HasAnswer = false,
                    IsPrint = true,
                    IsB = isB
                });

            //答题内容
            if (!details.Any())
                return DResult.Succ<dynamic>(new
                {
                    HasAnswer = false,
                    IsPrint = true,
                    IsB = isB
                });

            var answerContent = details.Aggregate(string.Empty, (c, t) => c + t.AnswerContent + ",").TrimEnd(',');

            //问题所在试卷区域
            var areas = MarkingMarkRepository.Where(m => m.BatchNo == batch).ToList();
            if (!areas.Any())
                return DResult.Succ<dynamic>(new
                {
                    HasAnswer = false,
                    IsPrint = true,
                    IsB = isB
                });

            var marks = new List<MkQuestionAreaDto>();
            areas.ForEach(a => marks.AddRange(JsonHelper.JsonList<MkQuestionAreaDto>(a.Mark).ToList()));
            var mark = marks.FirstOrDefault(m => m.Id == questionId);
            if (mark == null)
            {
                return DResult.Succ<dynamic>(new
                {
                    HasAnswer = true,
                    IsPrint = true,
                    IsB = isB,
                    img = string.Empty,
                    Body = answerContent
                });
            }

            var img = string.Empty;
            try
            {
                img = ImageHelper.MakeImage((new HttpHelper(picture.AnswerImgUrl)).GetStream(),
                    (int)mark.X, (int)mark.Y, (int)mark.Width, (int)mark.Height);
            }
            catch { }

            return DResult.Succ<dynamic>(new
            {
                HasAnswer = false,
                IsPrint = true,
                IsB = isB,
                img,
                body = answerContent
            });
        }

        /// <summary>
        /// 试卷错题对应答错的学生
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="questionId"></param>
        /// <param name="loadUserInfo"></param>
        /// <returns></returns>
        public DResults<ErrorQuestionStudentDto> ErrorStudents(string batch, string paperId, string questionId = "", bool loadUserInfo = false)
        {
            Expression<Func<TP_ErrorQuestion, bool>> condition = i => i.Batch == batch && i.PaperID == paperId;
            if (questionId.IsNotNullOrEmpty())
                condition = condition.And(i => i.QuestionID == questionId);
            var list = ErrorQuestionRepository.Where(condition)
                .Select(i => new
                {
                    i.QuestionID,
                    i.StudentID
                }).ToList()
                .GroupBy(i => i.QuestionID)
                .Select(g => new ErrorQuestionStudentDto
                {
                    QuestionId = g.Key,
                    Students = g.Select(i => i.StudentID).Distinct().Select(id => new DUserDto { Id = id }).ToList()
                }).ToList();

            if (!loadUserInfo) return DResult.Succ(list, -1);

            var studentIds = list.SelectMany(i => i.Students.Select(s => s.Id));
            var students = UserContract.LoadList(studentIds);
            if (!students.Any()) return DResult.Succ(list, -1);

            list.ForEach(i => i.Students.ForEach(s =>
            {
                var student = students.FirstOrDefault(ss => ss.Id == s.Id);
                if (student == null) return;
                s.Name = student.Name;
                s.Avatar = student.Avatar;
                s.Nick = student.Nick;
            }));

            return DResult.Succ(list, -1);
        }

        /// <summary>
        /// 学生错题答案(非打印试卷)
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="questionId"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        private List<MkDetailDto> ErrorAnswers(string batch, string paperId, string questionId, long studentId)
        {
            return MarkingDetailRepository.Where(a =>
                a.Batch == batch
                && a.PaperID == paperId
                && a.QuestionID == questionId
                && a.StudentID == studentId)
                .MapTo<List<MkDetailDto>>();
        }

        /// <summary>
        /// 模拟智能匹配搜索关键字
        /// </summary>
        /// <param name="key"></param>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public DResults<string> SearchKeys(string key, int subjectId)
        {
            //标签、知识点各3个
            var tags = ErrorTagRepository
                .Where(t => t.TagName.Contains(key) && (subjectId < 1 || t.SubjectId == subjectId))
                .OrderByDescending(t => t.UseCount).Take(6)
                .Select(t => t.TagName).ToList();
            var kps = KnowledgeRepository
                .Where(k => k.Name.Contains(key) && (subjectId < 1 || k.SubjectID == subjectId))
                .Take(6).Select(k => k.Name).ToList();
            var list = tags.Take(3).ToList();
            list.AddRange(kps.Take(3));
            if (list.Count < 6)
                list.AddRange(tags.Skip(3).Take(6 - list.Count));
            if (list.Count < 6)
                list.AddRange(kps.Skip(3).Take(6 - list.Count));
            return DResult.Succ(list, list.Count);
        }

        /// <summary>
        /// 有错题的科目
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns></returns>
        public IDictionary<int, string> ErrorQuestionSubjects(long studentId)
        {
            var subjectIds = ErrorQuestionRepository.Where(e =>
                e.StudentID == studentId &&
                e.Status != (byte)ErrorQuestionStatus.Delete &&
                e.SourceType == (byte)ErrorQuestionSourceType.Paper)
                .Select(e => e.SubjectID).Distinct().ToList();
            return SystemContract.SubjectDict(subjectIds);
        }

        /// <summary>
        /// 有错题的题型
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public List<QuestionTypeDto> ErrorQuestionTypes(long studentId, int subjectId)
        {
            var idList = ErrorQuestionRepository.Where(e =>
                e.StudentID == studentId &&
                e.Status != (byte)ErrorQuestionStatus.Delete &&
                e.SourceType == (byte)ErrorQuestionSourceType.Paper &&
                e.SubjectID == subjectId)
                .Select(e => e.QType).Distinct().ToList();
            return SystemContract.GetQuestionTypes(idList);
        }

        /// <summary>
        /// 问题错误率
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="questionId"></param>
        /// <returns></returns>
        public DResult<dynamic> ErrorQuestionRate(string batch, string paperId, string questionId)
        {
            int groupTotal, groupCount, total = 0, count = 0;
            var usage = UsageRepository.FirstOrDefault(u => u.Id == batch);

            if (usage != null && usage.SourceType == (byte)PublishType.Test)
            {
                //圈内错误率 - 推送试卷查错题库
                groupCount = ErrorQuestionRepository.Count(e =>
                    e.Batch == batch && e.PaperID == paperId && e.QuestionID == questionId);
                groupTotal = GroupContract.GroupMemberCount(usage.ClassId, UserRole.Student);
            }
            else
            {
                //圈内错误率 - 其他试卷是阅卷结果
                var groupDetails = MarkingDetailRepository.Where(d =>
                    d.Batch == batch && d.PaperID == paperId && d.QuestionID == questionId);
                groupTotal = groupDetails.GroupBy(d => d.StudentID).Count();
                groupCount = groupDetails
                    .Where(d => d.IsFinished && !(d.IsCorrect.HasValue && d.IsCorrect.Value))
                    .GroupBy(d => d.StudentID).Count();

                //全网错误率
                var details = MarkingDetailRepository.Where(d => d.QuestionID == questionId);
                total = details.GroupBy(d => d.StudentID).Count();
                count = details
                    .Where(d => d.IsFinished && !(d.IsCorrect.HasValue && d.IsCorrect.Value))
                    .GroupBy(d => d.StudentID).Count();
            }

            return DResult.Succ<dynamic>(new
            {
                count_g = groupCount,
                total_g = groupTotal,
                rate_g = (groupCount > 0 && groupTotal > 0
                    ? ((groupCount * 100M) / groupTotal).ToString("0.0")
                    : "0"),
                count,
                total,
                rate = (count > 0 && total > 0
                    ? ((count * 100M) / total).ToString("0.0")
                    : "0")
            });
        }

        /// <summary>
        /// 获取试卷里面的错题Ids
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DResults<string> GetPaperErrorQIds(string batch, string paperId, long userId)
        {
            var result = ErrorQuestionRepository.Where(u => u.Batch == batch && u.PaperID == paperId && u.StudentID == userId)
                .Select(u => u.QuestionID).ToList();
            return DResult.Succ<string>(result);
        }

        /// <summary>
        /// 根据错题ID集查询问题ID集
        /// </summary>
        /// <param name="eids"></param>
        /// <returns></returns>
        public DResults<string> GetErrorQIdsByEIds(List<string> eids)
        {
            if (eids == null || !eids.Any())
                return DResult.Errors<string>("参数错误");
            var result = ErrorQuestionRepository.Where(e => eids.Contains(e.Id)).Select(e => e.QuestionID).ToList();
            return DResult.Succ<string>(result);
        }

        /// <summary>
        /// 指定试卷问题有答错的学生
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="questionId"></param>
        /// <returns></returns>
        public List<long> HasErrorStudentIds(string batch, string paperId, string questionId)
        {
            //已交卷且答错的学生
            return MarkingDetailRepository.Where(d =>
                d.Batch == batch && d.PaperID == paperId &&
                d.QuestionID == questionId && d.IsCorrect.HasValue && !d.IsCorrect.Value)
                .Select(d => d.StudentID).Distinct().ToList();
        }
        /// <summary>
        /// 已提交的学生ID
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <returns></returns>
        public List<long> IsSubmitStudentIds(string batch, string paperId)
        {
            var usage = UsageRepository.FirstOrDefault(u => u.Id == batch);
            if (usage == null) return new List<long>();

            //推送试卷，查错题
            if (usage.SourceType == (byte)PublishType.Test)
            {
                return ErrorQuestionRepository.Where(e => e.Batch == batch && e.PaperID == paperId)
                    .Select(e => e.StudentID).Distinct().ToList();
            }
            //普通试卷查阅卷结果
            return MarkingResultRepository.Where(r => r.Batch == batch && r.PaperID == paperId)
                .Select(r => r.StudentID).Distinct().ToList();
        }

    }
}
