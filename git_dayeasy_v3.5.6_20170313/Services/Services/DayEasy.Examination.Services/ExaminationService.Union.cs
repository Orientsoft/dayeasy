using DayEasy.Contracts.Dtos.Examination;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.Examination.Services
{
    public partial class ExaminationService
    {
        public IVersion3Repository<TE_UnionReport> UnionReportRepository { private get; set; }

        #region 关联 & 取消

        public DResult UnionReport(List<string> ids, long userId)
        {
            if (ids == null || ids.Count <= 1)
                return DResult.Error("至少选择两次大型考试");
            var exams =
                ExaminationRepository.Where(t => ids.Contains(t.Id))
                    .Select(t => new { t.Id, t.JointBatches, t.UnionBatch, t.Status, t.AgencyId })
                    .ToList();
            //考试数>1
            if (!exams.Any() || exams.Count <= 1)
                return DResult.Error("至少选择两次大型考试");
            var agencyCount = exams.Select(t => t.AgencyId).Distinct().Count();
            if (exams.Count != agencyCount)
                return DResult.Error("一个机构只能选择一次大型考试参与关联");
            //存在已关联的?
            if (exams.Any(t => t.UnionBatch != null && t.UnionBatch.Length == 32))
                return DResult.Error("存在已关联的大型考试");
            //已推送
            if (exams.Any(t => (t.Status & (byte)ExamStatus.Sended) == 0))
                return DResult.Error("存在未推送的大型考试");
            var jointDict = exams.Select(t => new { t.Id, t.JointBatches })
                .ToList()
                .ToDictionary(k => k.Id, v => JsonHelper.JsonList<string>(v.JointBatches ?? "[]"));
            var count = jointDict.Values.First().Count();
            //科目
            if (jointDict.Any(t => t.Value.Count() != count))
                return DResult.Error("大型考试涵盖科目数不匹配");
            //试卷
            var joints = jointDict.Values.SelectMany(t => t.ToList()).Distinct().ToList();
            var jointPapers =
                JointMarkingRepository.Where(t => joints.Contains(t.Id))
                    .Select(t => new { t.Id, t.PaperId })
                    .ToList()
                    .ToDictionary(k => k.Id, v => v.PaperId);
            var paperIds = jointPapers.Where(t => jointDict.First().Value.Contains(t.Key)).Select(t => t.Value).ToList();
            if (
                jointDict.Any(
                    t => !jointPapers.Where(p => t.Value.Contains(p.Key)).Select(p => p.Value).ArrayEquals(paperIds)))
                return DResult.Error("大型考试对应学科试卷不匹配");
            var union = new TE_UnionReport
            {
                Id = IdHelper.Instance.Guid32,
                AddedAt = Clock.Now,
                AddedBy = userId,
                Status = (byte)NormalStatus.Normal
            };
            var result = UnitOfWork.Transaction(() =>
            {
                UnionReportRepository.Insert(union);
                ExaminationRepository.Update(new TE_Examination
                {
                    UnionBatch = union.Id
                }, t => jointDict.Keys.Contains(t.Id), nameof(TE_Examination.UnionBatch));
            });
            return DResult.FromResult(result);
        }

        public DResult CancelUnion(string unionBatch)
        {
            if (string.IsNullOrWhiteSpace(unionBatch))
                return DResult.Error("关联批次不能为空");
            var model = UnionReportRepository.Load(unionBatch);
            if (model == null) return DResult.Error("关联批次不存在");
            if (model.Status == (byte)NormalStatus.Delete)
                return DResult.Error("关联批次已被取消");
            var result = UnitOfWork.Transaction(() =>
            {
                model.Status = (byte)NormalStatus.Delete;
                UnionReportRepository.Update(t => new { t.Status }, model);
                ExaminationRepository.Update(new TE_Examination
                {
                    UnionBatch = null
                }, t => t.UnionBatch == unionBatch, nameof(TE_Examination.UnionBatch));
            });
            return DResult.FromResult(result);
        }

        #endregion

        public DResults<UnionExamDto> UnionList(DPage page = null)
        {
            page = page ?? DPage.NewPage();
            var models = UnionReportRepository.Where(t => t.Status == (byte)NormalStatus.Normal);
            if (!models.Any())
                return DResult.Succ(new List<UnionExamDto>(), 0);
            var count = models.Count();
            var dtos = models.GroupJoin(ExaminationRepository.Table, u => u.Id, e => e.UnionBatch, (u, e) =>
                new UnionExamDto
                {
                    Batch = u.Id,
                    Time = u.AddedAt,
                    Exams = e.Select(t => new DExamDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        AgencyId = t.AgencyId,
                        AgencyName = t.AgencyName
                    }).ToList()
                })
                .OrderByDescending(t => t.Time)
                .Skip(page.Page * page.Size)
                .Take(page.Size)
                .ToList();
            return DResult.Succ(dtos, count);
        }

        public DResult<UnionSourceDto> UnionSource(string unionBatch)
        {
            if (string.IsNullOrWhiteSpace(unionBatch))
                return DResult.Error<UnionSourceDto>("关联批次不存在");

            var examList =
                ExaminationRepository.Where(t => t.UnionBatch == unionBatch)
                    .Select(t => new { t.Id, t.Name, t.Status, t.AgencyId, t.AgencyName });
            if (examList.Any(t => (t.Status & (byte)ExamStatus.Sended) == 0))
                return DResult.Error<UnionSourceDto>("考试还没有生成统计信息！");
            var dto = new UnionSourceDto();
            //科目
            dto.Subjects = SubjectScoreRepository.Table.Join(examList, s => s.ExamId, e => e.Id, (s, e) =>
                new { Id = s.SubjectId, s.Subject, s.PaperId, s.PaperTitle, s.PaperType })
                .DistinctBy(t => t.Id)
                .OrderBy(t => t.Id)
                .ToList()
                .ToDictionary(k => k.Id, v => new UnionSubjectDto
                {
                    Id = v.Id,
                    Subject = v.Subject,
                    PaperId = v.PaperId,
                    PaperTitle = v.PaperTitle,
                    IsAb = (v.PaperType == (byte)PaperType.AB)
                });
            var paperIds = dto.Subjects.Values.Select(t => t.PaperId).Distinct().ToList();
            var scores =
                PaperRepository.Where(p => paperIds.Contains(p.Id)).Select(p => new { p.Id, p.PaperScores })
                    .ToList()
                    .ToDictionary(k => k.Id, v => JsonHelper.Json<PaperScoresDto>(v.PaperScores));
            foreach (var subject in dto.Subjects)
            {
                if (!scores.ContainsKey(subject.Value.PaperId))
                    continue;
                var score = scores[subject.Value.PaperId];
                subject.Value.Score = score.TScore;
                subject.Value.ScoreA = score.TScoreA;
                subject.Value.ScoreB = score.TScoreB;
            }
            dto.Exams = examList.Select(e => new DExamDto
            {
                Id = e.Id,
                Name = e.Name,
                AgencyId = e.AgencyId,
                AgencyName = e.AgencyName
            }).ToList().ToDictionary(k => k.Id, v => v);
            //学生
            var studentSubjects = StudentSubjectScoreRepository.Table.Join(SubjectScoreRepository.Table,
                s => s.ExamSubjectId, t => t.Id, (s, t) => new
                {
                    s.ExamId,
                    s.StudentId,
                    t.SubjectId,
                    t.Subject,
                    s.Batch,
                    s.Score,
                    s.ScoreA,
                    s.ScoreB
                });
            var students =
                StudentScoreRepository.Table.Join(examList, s => s.ExamId, e => e.Id, (s, e) => s)
                    .GroupJoin(studentSubjects, s => new { s.ExamId, s.StudentId }, ss => new { ss.ExamId, ss.StudentId },
                        (s, ss) => new
                        {
                            Id = s.StudentId,
                            s.ExamId,
                            s.ClassId,
                            s.TotalScore,
                            Subjects = ss
                        }).ToList();
            dto.Students = students.Select(s => new UnionStudentDto
            {
                Id = s.Id,
                ExamId = s.ExamId,
                ClassId = s.ClassId,
                TotalScore = s.TotalScore,
                Subjects = s.Subjects.DistinctBy(k => k.SubjectId).ToDictionary(k => k.SubjectId, v => new UnionStudentSubjectDto
                {
                    Batch = v.Batch,
                    Score = v.Score,
                    ScoreA = v.ScoreA,
                    ScoreB = v.ScoreB
                })
            }).ToList();
            //学生信息 & 班级信息
            var userIds = dto.Students.Select(t => t.Id).Distinct().ToList();
            var userDict = UserRepository.Where(u => userIds.Contains(u.Id)).Select(u => new
            {
                u.Id,
                u.TrueName,
                u.UserCode,
                u.StudentNum
            }).ToList().ToDictionary(k => k.Id, v => v);
            var classIds = dto.Students.Select(t => t.ClassId).Distinct().ToList();
            var groupDict = GroupContract.GroupDict(classIds);
            foreach (var student in dto.Students)
            {
                if (userDict.ContainsKey(student.Id))
                {
                    var user = userDict[student.Id];
                    student.Name = user.TrueName;
                    student.Code = user.UserCode;
                    student.StudentNum = user.StudentNum;
                }
                if (groupDict.ContainsKey(student.ClassId))
                    student.ClassName = groupDict[student.ClassId];
                //排名
                if (student.TotalScore < 0)
                    student.Rank = -1;
                else
                    student.Rank = dto.Students.Count(t => t.TotalScore > student.TotalScore) + 1;
            }
            dto.Students = dto.Students.OrderBy(t => t.Rank < 0 ? int.MaxValue : t.Rank).ToList();
            return DResult.Succ(dto);
        }
    }
}
