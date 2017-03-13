using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Examination;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Contracts.Models.Mongo;
using DayEasy.Core.Dependency;
using DayEasy.MongoDb;
using DayEasy.Services;
using DayEasy.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DayEasy.Examination.Services.Helper
{
    /// <summary> 发布试卷异步任务 </summary>
    public class ExaminationTask
    {
        #region 私有方法

        private readonly ILogger _logger = LogManager.Logger<ExaminationTask>();

        private ExaminationTask() { }

        public static ExaminationTask Instance
        {
            get
            {
                return (Singleton<ExaminationTask>.Instance ??
                        (Singleton<ExaminationTask>.Instance = new ExaminationTask()));
            }
        }

        /// <summary> 获取涵盖科目 </summary>
        /// <param name="examId"></param>
        /// <param name="jointBatches"></param>
        /// <returns></returns>
        private List<TE_SubjectScore> ExaminationSubjects(string examId, ICollection<string> jointBatches)
        {
            var jointMarkRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_JointMarking>>();
            var paperRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_Paper>>();
            var idHelper = IdHelper.Instance;

            var subjectList = jointMarkRepository.Where(
                t => jointBatches.Contains(t.Id) && t.Status == (byte)JointStatus.Finished)
                .Join(paperRepository.Table, j => j.PaperId, p => p.Id, (j, p) => new
                {
                    p.SubjectID,
                    j.Id,
                    j.GroupId,
                    j.PaperId,
                    p.PaperType,
                    p.PaperTitle
                }).ToList();

            return subjectList.Select(t => new TE_SubjectScore
            {
                Id = idHelper.Guid32,
                ExamId = examId,
                JointBatch = t.Id,
                SubjectId = t.SubjectID,
                Subject = SystemCache.Instance.SubjectName(t.SubjectID),
                GroupId = t.GroupId,
                PaperId = t.PaperId,
                PaperType = t.PaperType,
                PaperTitle = t.PaperTitle
            }).ToList();
        }

        /// <summary> 获取学生单科成绩列表 </summary>
        /// <param name="examId"></param>
        /// <param name="jointBatches"></param>
        /// <param name="subjectList"></param>
        /// <param name="classList"></param>
        /// <returns></returns>
        private List<TE_StudentSubjectScore> ExaminationStudentSubjects(string examId,
            ICollection<string> jointBatches, List<TE_SubjectScore> subjectList, ICollection<string> classList)
        {
            var usageRepository = CurrentIocManager.Resolve<IDayEasyRepository<TC_Usage>>();
            var scoreStatisticsRepository =
                CurrentIocManager.Resolve<IDayEasyRepository<TS_StuScoreStatistics>>();
            var models =
                usageRepository.Where(
                    u => jointBatches.Contains(u.JointBatch) && u.MarkingStatus == (byte)MarkingStatus.AllFinished);
            if (classList != null && classList.Any())
                models = models.Where(u => classList.Contains(u.ClassId));

            var batchList = models.Select(t => new { t.Id, t.ClassId, t.JointBatch }).ToList();

            var batches = batchList.Select(t => t.Id);

            var groupMemberRepository = CurrentIocManager.Resolve<IVersion3Repository<TG_Member>>();
            var classIds = batchList.Select(t => t.ClassId).Distinct().ToList();

            //所有学生
            var students = groupMemberRepository.Where(
                m => (m.MemberRole & (byte)UserRole.Student) > 0 && m.Status == (byte)NormalStatus.Normal
                     && classIds.Contains(m.GroupId))
                .Select(m => new { m.MemberId, m.GroupId })
                .ToList();

            //分数源
            var scoreList = scoreStatisticsRepository.Where(t => batches.Contains(t.Batch))
                .Select(t => new
                {
                    t.Batch,
                    t.StudentId,
                    t.SubjectId,
                    t.ClassId,
                    t.SectionAScore,
                    t.SectionBScore,
                    t.CurrentScore
                }).ToList();

            var studentSubjectList = new List<TE_StudentSubjectScore>();
            var idHelper = IdHelper.Instance;
            foreach (var student in students)
            {
                foreach (var subject in subjectList)
                {
                    var item = new TE_StudentSubjectScore
                    {
                        Id = idHelper.Guid32,
                        ExamId = examId,
                        ExamSubjectId = subject.Id,
                        StudentId = student.MemberId,
                        ClassId = student.GroupId
                    };
                    var score =
                        scoreList.FirstOrDefault(
                            t =>
                                t.StudentId == item.StudentId && t.ClassId == item.ClassId &&
                                t.SubjectId == subject.SubjectId);
                    if (score == null)
                    {
                        //缺考
                        var batch =
                            batchList.FirstOrDefault(
                                t => t.JointBatch == subject.JointBatch && t.ClassId == item.ClassId);
                        if (batch != null)
                            item.Batch = batch.Id;
                        item.ScoreA = -1M;
                        item.ScoreB = -1M;
                        item.Score = -1M;
                    }
                    else
                    {
                        item.Batch = score.Batch;
                        item.ScoreA = score.SectionAScore;
                        item.ScoreB = score.SectionBScore;
                        //折算
                        item.Score = (score.SubjectId.In(4, 5)
                            ? Math.Round((score.SectionAScore / 2M) + score.SectionBScore, 2)
                            : score.CurrentScore);
                    }
                    studentSubjectList.Add(item);
                }
            }
            return studentSubjectList;
        }

        private void InsertExamRanks(string examId, ICollection<TE_SubjectScore> subjects,
            ICollection<TE_StudentScore> students,
            ICollection<TE_StudentSubjectScore> studentSubjects)
        {
            if (subjects.IsNullOrEmpty() || students.IsNullOrEmpty() || studentSubjects.IsNullOrEmpty())
                return;
            var examinationRepository = CurrentIocManager.Resolve<IVersion3Repository<TE_Examination>>();
            var studentScoreRepository = CurrentIocManager.Resolve<IVersion3Repository<TE_StudentScore>>();
            var subjectScoreRepository = CurrentIocManager.Resolve<IVersion3Repository<TE_SubjectScore>>();
            var studentSubjectRepository = CurrentIocManager.Resolve<IVersion3Repository<TE_StudentSubjectScore>>();
            //总平均分
            var exam = new TE_Examination
            {
                StudentCount = students.Count,
                AverageScore = Math.Round(students.Where(t => t.TotalScore >= 0).Average(t => t.TotalScore), 2)
            };
            //学生总排名
            students.Foreach(s =>
            {
                if (s.TotalScore < 0)
                {
                    s.Rank = -1;
                    s.ClassRank = -1;
                }
                else
                {
                    //总排名
                    s.Rank = students.Count(t => t.TotalScore > s.TotalScore) + 1;
                    //班级排名
                    s.ClassRank = students.Count(t => t.ClassId == s.ClassId && t.TotalScore > s.TotalScore) + 1;
                }
            });
            //单科排名
            studentSubjects.Foreach(s =>
            {
                if (s.Score < 0)
                {
                    s.Rank = -1;
                    s.ClassRank = -1;
                }
                else
                {
                    //单科总排名
                    s.Rank = studentSubjects.Count(t => t.ExamSubjectId == s.ExamSubjectId && t.Score > s.Score) + 1;
                    //单科班级排名
                    s.ClassRank =
                        studentSubjects.Count(
                            t => t.ExamSubjectId == s.ExamSubjectId && t.ClassId == s.ClassId && t.Score > s.Score) + 1;
                }
            });
            //各科目平均分
            var subjectScores = studentSubjects.GroupBy(t => t.ExamSubjectId)
                .ToDictionary(k => k.Key, v => v.Select(t => new
                {
                    t.ScoreA,
                    t.ScoreB,
                    t.Score
                }).ToList());
            foreach (var subject in subjects)
            {
                if (!subjectScores.ContainsKey(subject.Id))
                    continue;
                var score = subjectScores[subject.Id];
                subject.AverageScoreA = Math.Round(score.Where(t => t.ScoreA >= 0).Average(t => t.ScoreA), 2);
                subject.AverageScoreB = Math.Round(score.Where(t => t.ScoreB >= 0).Average(t => t.ScoreB), 2);
                subject.AverageScore = Math.Round(score.Where(t => t.Score >= 0).Average(t => t.Score), 2);
            }
            subjectScoreRepository.UnitOfWork.Transaction(() =>
            {
                examinationRepository.Update(exam, e => e.Id == examId, "StudentCount", "AverageScore");
                studentScoreRepository.Insert(students);
                subjectScoreRepository.Insert(subjects);
                studentSubjectRepository.Insert(studentSubjects);
            });
        }
        /// <summary> 题目得分率 </summary>
        /// <param name="subjectScoreId"></param>
        /// <returns></returns>
        public MExaminationQuestionScores QuestionScores(string subjectScoreId)
        {
            var collection = new MongoManager().Collection<MExaminationQuestionScores>();
            var score = collection.FindOneById(subjectScoreId);
            if (score != null)
                return score;
            var subjectScoreRepository = CurrentIocManager.Resolve<IVersion3Repository<TE_SubjectScore>>();
            var examRepository = CurrentIocManager.Resolve<IVersion3Repository<TE_Examination>>();
            var model = subjectScoreRepository.Where(t => t.Id == subjectScoreId)
                .Join(examRepository.Table, s => s.ExamId, e => e.Id,
                    (s, e) => new { e.ExamType, e.ClassList, subject = s })
                .FirstOrDefault();
            if (model == null) return null;
            List<string> classList = null;
            if (model.ExamType == (byte)ExamType.Union && !string.IsNullOrWhiteSpace(model.ClassList))
                classList = JsonHelper.JsonList<string>(model.ClassList).ToList();
            score = QuestionScores(model.subject, classList);
            collection.Insert(score);
            return score;
        }

        private MExaminationQuestionScores QuestionScores(TE_SubjectScore model, ICollection<string> classList)
        {
            var paperContract = CurrentIocManager.Resolve<IPaperContract>();
            var groupContract = CurrentIocManager.Resolve<IGroupContract>();
            var usageRepository = CurrentIocManager.Resolve<IDayEasyRepository<TC_Usage>>();
            var markingDetailRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingDetail>>();

            var sorts = paperContract.QuestionSorts(model.PaperId);
            var isAb = model.PaperType == (byte)PaperType.AB;
            var dto = new ScoreRateDto
            {
                QuestionSorts = sorts.ToDictionary(k => k.Key, v =>
                {
                    var sort = v.Value;
                    if (isAb)
                    {
                        return (sort.Key == (byte)PaperSectionType.PaperA ? "A" : "B") + sort.Value;
                    }
                    return sort.Value.ToString();
                })
            };
            var usageModels = usageRepository.Where(t => t.JointBatch == model.JointBatch);
            if (classList != null && classList.Any())
                usageModels = usageModels.Where(u => classList.Contains(u.ClassId));
            var usages = usageModels.Select(t => new { t.Id, t.ClassId }).ToList().ToDictionary(k => k.Id, v => v.ClassId);

            var classIds = usages.Values.ToList();
            dto.ClassList = groupContract.GroupDict(classIds)
                .OrderBy(t => t.Value.ClassIndex())
                .ToDictionary(k => k.Key, v => v.Value);
            var batches = usages.Keys.ToList();
            var details = markingDetailRepository.Where(d => batches.Contains(d.Batch))
                .Select(d => new
                {
                    d.QuestionID,
                    d.SmallQID,
                    d.CurrentScore,
                    d.Score,
                    d.Batch
                }).ToList();
            var scores = details.GroupBy(d => (string.IsNullOrWhiteSpace(d.SmallQID) ? d.QuestionID : d.SmallQID))
                .ToDictionary(k => k.Key, v => v.GroupBy(t => t.Batch)
                    .ToDictionary(k => k.Key, s => s.Select(t => new { t.CurrentScore, t.Score }).ToList()));
            //年级得分率
            dto.ScoreRates = scores.ToDictionary(k => k.Key, v =>
            {
                var list = v.Value.SelectMany(t => t.Value).ToList();
                if (list.IsNullOrEmpty())
                    return 0M;
                var total = list.Sum(t => t.Score);
                if (total <= 0) return 0M;
                return Math.Round(list.Sum(t => t.CurrentScore) / total, 2, MidpointRounding.AwayFromZero) * 100;
            });
            dto.ClassScoreRates = scores.ToDictionary(k => k.Key, v =>
            {
                var dict = new Dictionary<string, decimal>();
                foreach (var item in v.Value)
                {
                    if (!usages.ContainsKey(item.Key))
                        continue;
                    var classId = usages[item.Key];
                    var rate = 0M;
                    if (!item.Value.IsNullOrEmpty())
                    {
                        var total = item.Value.Sum(t => t.Score);
                        if (total > 0)
                        {
                            rate = Math.Round(item.Value.Sum(t => t.CurrentScore) / total, 2,
                                MidpointRounding.AwayFromZero);
                        }
                    }
                    dict.Add(classId, rate * 100M);
                }
                return dict;
            });
            return new MExaminationQuestionScores
            {
                Id = model.Id,
                ExamId = model.ExamId,
                SubjectId = model.SubjectId,
                Scores = dto
            };
        }

        /// <summary> 题目得分率 </summary>
        public void InsertQuestionScores(IEnumerable<TE_SubjectScore> subjects, ICollection<string> classList)
        {
            var scoreModels = new List<MExaminationQuestionScores>();
            foreach (var model in subjects)
            {
                var score = QuestionScores(model, classList);
                if (score != null)
                    scoreModels.Add(score);
            }
            if (scoreModels.Any())
            {
                var collection = new MongoManager().Collection<MExaminationQuestionScores>();
                collection.InsertBatch(scoreModels);
            }
        }

        #endregion

        /// <summary> 生成考试排名统计 </summary>
        /// <param name="examId"></param>
        /// <returns></returns>
        public Task GenerateExaminatiomRanksAsync(string examId)
        {
            return Task.Factory.StartNew(() =>
            {
                if (string.IsNullOrWhiteSpace(examId))
                    return;
                Utils.WatchAction("异步任务[生成考试排名统计]", log =>
                {
                    var examinationRepository = CurrentIocManager.Resolve<IVersion3Repository<TE_Examination>>();
                    var exam = examinationRepository.Load(examId);
                    if (exam == null)
                    {
                        log($"考试[{examId}]不存在！");
                        return;
                    }
                    if (exam.StudentCount.HasValue)
                    {
                        log($"考试统计[{examId}]已经存在！");
                        return;
                    }
                    var jointBatches = JsonHelper.JsonList<string>(exam.JointBatches).ToList();
                    var idHelper = IdHelper.Instance;
                    List<string> classList = null;
                    if (exam.ExamType == (byte)ExamType.Union)
                        classList = JsonHelper.JsonList<string>(exam.ClassList ?? string.Empty).ToList();
                    var subjectList = ExaminationSubjects(examId, jointBatches);
                    if (subjectList.IsNullOrEmpty())
                    {
                        log($"考试[{examId}]不包含任何科目！");
                        return;
                    }

                    var studentSubjectList = ExaminationStudentSubjects(examId, jointBatches, subjectList, classList);

                    var studentScores = studentSubjectList.GroupBy(t => new { t.StudentId, t.ClassId })
                        .Select(t => new TE_StudentScore
                        {
                            Id = idHelper.Guid32,
                            ExamId = examId,
                            StudentId = t.Key.StudentId,
                            ClassId = t.Key.ClassId,
                            TotalScore =
                                t.Any(s => s.Score >= 0)
                                    ? Math.Round(t.Where(s => s.Score >= 0).Sum(s => s.Score), 2)
                                    : -1M
                        }).ToList();

                    InsertExamRanks(examId, subjectList, studentScores, studentSubjectList);
                    InsertQuestionScores(subjectList, classList);
                    //每题得分
                }, _logger.Info);
            });
        }

        /// <summary> 发布任务 </summary>
        /// <param name="examId"></param>
        /// <param name="teacherId"></param>
        /// <param name="name"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public Task PublishTaskAsync(string examId, long teacherId, string name, UserRole role)
        {
            return Task.Factory.StartNew(() =>
            {
                Utils.WatchAction("异步任务[大型考试发布]", () =>
                {
                    var studentSubjectRepository =
                        CurrentIocManager.Resolve<IVersion3Repository<TE_StudentSubjectScore>>();
                    var messageContract = CurrentIocManager.Resolve<IMessageContract>();
                    //班级圈
                    var classIds = studentSubjectRepository.Where(t => t.ExamId == examId)
                        .Select(t => t.ClassId)
                        .Distinct()
                        .ToList();
                    var dynamices = classIds.Select(c => new DynamicSendDto
                    {
                        DynamicType = GroupDynamicType.ExamNotice,
                        ContentType = (byte)ContentType.Publish,
                        ContentId = examId,
                        GroupId = c,
                        ReceivRole = role,
                        UserId = teacherId,
                        Title = name
                    });
                    messageContract.SendDynamics(dynamices);
                });
            });
        }
    }
}
