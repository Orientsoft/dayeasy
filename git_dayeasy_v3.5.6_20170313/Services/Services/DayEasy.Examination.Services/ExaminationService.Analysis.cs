
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Examination;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Examination.Services.Helper;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using StudentRankDto = DayEasy.Contracts.Dtos.Examination.StudentRankDto;

namespace DayEasy.Examination.Services
{
    /// <summary> 统计分析相关实现 </summary>
    public partial class ExaminationService
    {
        public IDayEasyRepository<TS_ClassScoreStatistics> ClassScoreRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingDetail> MarkingDetailRepository { private get; set; }
        public IDayEasyRepository<TP_JointDistribution> JointDistributionRepository { private get; set; }
        public IPaperContract PaperContract { private get; set; }

        /// <summary> 学生排名信息 </summary>
        public DResult<ExamRanksDto> Rankings(string examId)
        {
            var exam = ExaminationRepository.Load(examId);
            if (exam == null)
                return DResult.Error<ExamRanksDto>("考试不存在！");
            if ((exam.Status & (byte)ExamStatus.Sended) == 0)
                return DResult.Error<ExamRanksDto>("考试还没有生成统计信息！");
            var dto = new ExamRanksDto
            {
                Name = exam.Name,
                StudentCount = exam.StudentCount ?? 0,
                AverageScore = exam.AverageScore ?? 0M
            };
            dto.Students = StudentScoreRepository.Where(t => t.ExamId == examId)
                .Select(t => new ExamStudentDto
                {
                    StudentId = t.StudentId,
                    ClassId = t.ClassId,
                    Score = t.TotalScore,
                    ClassRank = t.ClassRank,
                    Rank = t.Rank
                }).OrderBy(t => t.Rank < 0 ? int.MaxValue : t.Rank).ToList();
            var subjects = SubjectScoreRepository.Where(t => t.ExamId == examId)
                .OrderBy(s => s.SubjectId)
                .Select(t => new { t.Id, t.SubjectId, t.Subject, t.PaperType })
                .ToList();
            foreach (var subject in subjects)
            {
                var titles = new List<string>();
                if (subject.PaperType == (byte)PaperType.Normal)
                {
                    titles.Add(subject.Subject);
                }
                else
                {
                    titles.Add(subject.Subject + "A");
                    titles.Add(subject.Subject + "B");
                }
                if (subject.SubjectId.In(4, 5))
                {
                    titles.Add(subject.Subject + "折");
                }
                else
                {
                    titles.Add(subject.Subject + "合");
                }
                titles.Add("排名");
                dto.Subjects.Add(subject.Id, titles);
            }
            var subjectScores = StudentSubjectScoreRepository.Where(t => t.ExamId == examId)
                .Select(s => new
                {
                    s.ExamSubjectId,
                    s.StudentId,
                    s.ClassId,
                    s.ScoreA,
                    s.ScoreB,
                    s.Score,
                    s.Rank,
                    s.ClassRank
                }).ToList();
            var userIds = dto.Students.Select(t => t.StudentId).Distinct().ToList();
            var classIds = dto.Students.Select(t => t.ClassId).Distinct().ToList();
            var classDict = GroupContract.GroupDict(classIds);
            var userDict = UserRepository.Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.TrueName, u.StudentNum, u.UserCode })
                .ToList()
                .ToDictionary(k => k.Id, v => v);
            foreach (var student in dto.Students)
            {
                //班级
                if (classDict.ContainsKey(student.ClassId))
                    student.ClassName = classDict[student.ClassId];
                //用户
                if (userDict.ContainsKey(student.StudentId))
                {
                    var dict = userDict[student.StudentId];
                    student.Name = dict.TrueName;
                    student.StudentNo = dict.StudentNum;
                    student.UserCode = dict.UserCode;
                }
                student.ScoreDetails = subjectScores.Where(
                    t => t.StudentId == student.StudentId && t.ClassId == student.ClassId)
                    .ToDictionary(k => k.ExamSubjectId, v => new StudentRankDto
                    {
                        ScoreA = v.ScoreA,
                        ScoreB = v.ScoreB,
                        Score = v.Score,
                        Rank = v.Rank,
                        ClassRank = v.ClassRank
                    });
            }
            return DResult.Succ(dto);
        }

        /// <summary> 班级分析 - 重点率 </summary>
        public List<ClassAnalysisKeyDto> ClassAnalysisKey(AnalysisInputDto inputDto)
        {
            var rankResult = Rankings(inputDto.ExamId);
            if (!rankResult.Status)
                return new List<ClassAnalysisKeyDto>();
            var classList = rankResult.Data.Students.GroupBy(t => new { t.ClassId, t.ClassName })
                .OrderBy(t => t.Key.ClassName.ClassIndex()).ToList();
            var list = new List<ClassAnalysisKeyDto>();
            var studentScores = rankResult.Data.Students.Where(s => s.Score >= 0)
                .Select(s => s.Score)
                .OrderByDescending(s => s)
                .ToList();
            //重点率分数线
            decimal keyScore;
            if (inputDto.KeyType == 0)
            {
                keyScore = inputDto.KeyScore;
            }
            else
            {
                //比例
                var count =
                    (int)Math.Ceiling(studentScores.Count() * (inputDto.KeyScore / 100M));
                keyScore = studentScores.Skip(count - 1).First();
            }
            var classIds = classList.Select(t => t.Key.ClassId).Distinct().ToList();
            var teachers = GroupContract.SearchGroups(classIds).Data.ToDictionary(k => k.Id, v => v.Owner);

            foreach (var cls in classList)
            {
                var hasScores = cls.Where(t => t.Score >= 0).Select(s => s.Score).ToList();
                var item = new ClassAnalysisKeyDto
                {
                    ClassId = cls.Key.ClassId,
                    ClassName = cls.Key.ClassName,
                    TotalCount = cls.Count(),
                    StudentCount = hasScores.Count(),
                    AverageScore = Math.Round(hasScores.Average(s => s), 2)
                };
                item.AverageScoreDiff = Math.Round(item.AverageScore - rankResult.Data.AverageScore, 2);
                if (teachers.ContainsKey(cls.Key.ClassId))
                    item.Teacher = teachers[cls.Key.ClassId];
                //重点数
                item.KeyCount = hasScores.Count(s => s >= keyScore);
                item.AverageRatio = Math.Round(item.AverageScore / rankResult.Data.AverageScore, 5);
                //A卷平均合格
                item.ACount =
                    cls.Count(
                        s =>
                            s.Score >= 0 &&
                            s.ScoreDetails.Values.Where(t => t.ScoreA >= 0).Average(t => t.ScoreA) >= inputDto.ScoreA);
                item.UnACount =
                    cls.Count(
                        s =>
                            s.Score >= 0 &&
                            s.ScoreDetails.Values.Where(t => t.ScoreA >= 0).Average(t => t.ScoreA) < inputDto.UnScoreA);
                list.Add(item);
            }
            var grade = new ClassAnalysisKeyDto
            {
                ClassName = "全年级",
                TotalCount = rankResult.Data.StudentCount,
                StudentCount = list.Sum(t => t.StudentCount),
                AverageScore = rankResult.Data.AverageScore,
                KeyCount = list.Sum(t => t.KeyCount),
                ACount = list.Sum(t => t.ACount),
                UnACount = list.Sum(t => t.UnACount)
            };
            list.ForEach(t =>
            {
                t.KeyScaleDiff = Math.Round(t.KeyScale - grade.KeyScale, 4);
                t.AScaleDiff = Math.Round(t.AScale - grade.AScale, 4);
                t.UnAScaleDiff = Math.Round(t.UnAScale - grade.UnAScale, 4);
            });
            //全年级
            list.Add(grade);
            return list;
        }

        /// <summary> 班级分析 - 分层统计 </summary>
        public List<ClassAnalysisLayerDto> ClassAnalysisLayer(ClassAnalysisLayerInputDto inputDto)
        {
            if (inputDto.LayerA + inputDto.LayerB + inputDto.LayerC + inputDto.LayerD + inputDto.LayerE != 100M)
                return new List<ClassAnalysisLayerDto>();
            var rankResult = Rankings(inputDto.ExamId);
            var students = rankResult.Data.Students;
            var classList = students.GroupBy(t => new { t.ClassId, t.ClassName })
                .OrderBy(t => t.Key.ClassName.ClassIndex()).ToList();
            var studentScores =
                students.Where(t => t.Score >= 0).Select(t => t.Score).OrderByDescending(t => t).ToList();
            var list = new List<ClassAnalysisLayerDto>();
            //分层分数划分
            int studentCount = studentScores.Count();
            var skip = inputDto.LayerA;
            var scoreA = studentScores.Skip((int)Math.Ceiling(studentCount * (skip / 100M)) - 1).First();
            skip += inputDto.LayerB;
            var scoreB = studentScores.Skip((int)Math.Ceiling(studentCount * (skip / 100M)) - 1).First();
            skip += inputDto.LayerC;
            var scoreC = studentScores.Skip((int)Math.Ceiling(studentCount * (skip / 100M)) - 1).First();
            skip += inputDto.LayerD;
            var scoreD = studentScores.Skip((int)Math.Ceiling(studentCount * (skip / 100M)) - 1).First();

            var classIds = classList.Select(t => t.Key.ClassId).Distinct().ToList();
            var teachers = GroupContract.SearchGroups(classIds).Data.ToDictionary(k => k.Id, v => v.Owner);
            foreach (var cls in classList)
            {
                var hasScores = cls.Where(t => t.Score >= 0).Select(s => s.Score).ToList();
                var item = new ClassAnalysisLayerDto
                {
                    ClassId = cls.Key.ClassId,
                    ClassName = cls.Key.ClassName,
                    TotalCount = cls.Count(),
                    StudentCount = hasScores.Count(),
                    LayerA = hasScores.Count(s => s >= scoreA),
                    LayerB = hasScores.Count(s => s >= scoreB && s < scoreA),
                    LayerC = hasScores.Count(s => s >= scoreC && s < scoreB),
                    LayerD = hasScores.Count(s => s >= scoreD && s < scoreC),
                    LayerE = hasScores.Count(s => s < scoreD)
                };
                if (teachers.ContainsKey(cls.Key.ClassId))
                    item.Teacher = teachers[cls.Key.ClassId];
                list.Add(item);
            }
            var grade = new ClassAnalysisLayerDto
            {
                ClassName = "全年级",
                TotalCount = list.Sum(t => t.TotalCount),
                StudentCount = list.Sum(t => t.StudentCount),
                LayerA = list.Sum(t => t.LayerA),
                LayerB = list.Sum(t => t.LayerB),
                LayerC = list.Sum(t => t.LayerC),
                LayerD = list.Sum(t => t.LayerD),
                LayerE = list.Sum(t => t.LayerE)
            };
            list.Add(grade);
            return list;
        }

        /// <summary> 科目分析 </summary>
        public List<SubjectAnalysisDto> SubjectAnalysis(SubjectAnalysisInputDto inputDto)
        {
            if (inputDto.ExamSubjectId.IsNullOrEmpty())
                return new List<SubjectAnalysisDto>();
            var subject = SubjectScoreRepository.Load(inputDto.ExamSubjectId);
            if (subject == null)
                return new List<SubjectAnalysisDto>();
            var classList = StudentSubjectScoreRepository.Where(t => t.ExamSubjectId == inputDto.ExamSubjectId)
                .GroupBy(t => new { t.ClassId, t.Batch })
                .ToList();
            var studentScores =
                classList.SelectMany(c => c.Where(t => t.Score >= 0).Select(t => t.ScoreA + t.ScoreB))
                    .OrderByDescending(s => s)
                    .ToList();
            var list = new List<SubjectAnalysisDto>();
            subject.AverageScore = Math.Round(subject.AverageScoreA + subject.AverageScoreB ?? 0M, 2);
            //重点率分数线
            decimal keyScore;
            if (inputDto.KeyType == 0)
            {
                keyScore = inputDto.KeyScore;
            }
            else
            {
                //比例
                var count =
                    (int)Math.Ceiling(studentScores.Count * (inputDto.KeyScore / 100M));
                keyScore = studentScores.Skip(count - 1).First();
            }
            var classIds = classList.Select(t => t.Key.ClassId).Distinct().ToList();
            var teachers = GroupContract.SubjectTeachers(classIds, subject.SubjectId);
            var classDict = GroupContract.GroupDict(classIds);
            var batches = classList.Select(t => t.Key.Batch).Distinct().ToList();
            var segments = ClassScoreRepository.Where(t => batches.Contains(t.Batch))
                .Select(t => new
                {
                    t.Batch,
                    t.ScoreGroups,
                    t.SectionScoreGroups
                }).ToList()
                .ToDictionary(k => k.Batch, v => new
                {
                    segment = JsonHelper.JsonList<ScoreGroupsDto>(v.ScoreGroups),
                    sections =
                        string.IsNullOrWhiteSpace(v.SectionScoreGroups)
                            ? null
                            : JsonHelper.JsonList<List<ScoreGroupsDto>>(v.SectionScoreGroups).ToList()
                });
            foreach (var cls in classList)
            {
                var item = new SubjectAnalysisDto
                {
                    ClassId = cls.Key.ClassId,
                    TotalCount = cls.Count()
                };
                if (classDict.ContainsKey(cls.Key.ClassId))
                    item.ClassName = classDict[cls.Key.ClassId];
                if (teachers.ContainsKey(cls.Key.ClassId))
                    item.Teacher = teachers[cls.Key.ClassId].Name;
                var hasScores = cls.Where(s => s.Score >= 0).ToList();
                if (!hasScores.Any())
                {
                    list.Add(item);
                    continue;
                }
                item.StudentCount = hasScores.Count();
                item.AverageScore = Math.Round(hasScores.Average(s => s.ScoreA + s.ScoreB), 2);
                item.AverageScoreDiff = Math.Round(item.AverageScore - subject.AverageScore, 2);

                //重点数
                item.KeyCount = hasScores.Count(s => s.ScoreA + s.ScoreB >= keyScore);
                item.AverageRatio = Math.Round(item.AverageScore / subject.AverageScore, 5);
                item.AverageScoreA = Math.Round(hasScores.Average(s => s.ScoreA), 2);
                item.AverageScoreB = Math.Round(hasScores.Average(s => s.ScoreB), 2);
                //A卷平均合格
                item.ACount = hasScores.Count(s => s.ScoreA >= inputDto.ScoreA);
                item.UnACount = hasScores.Count(s => s.ScoreA < inputDto.UnScoreA);
                if (segments.ContainsKey(cls.Key.Batch) && segments[cls.Key.Batch] != null)
                {
                    var segment = segments[cls.Key.Batch];
                    item.Segment = segment.segment.OrderByDescending(
                        t => ConvertHelper.StrToInt(t.ScoreInfo.Split('-')[0], 0))
                        .ToDictionary(k => k.ScoreInfo, v => v.Count);
                    if (segment.sections != null && segment.sections.Count() == 2)
                    {
                        item.SegmentA = segment.sections[0].OrderByDescending(
                            t => ConvertHelper.StrToInt(t.ScoreInfo.Split('-')[0], 0))
                            .ToDictionary(k => k.ScoreInfo, v => v.Count);
                        item.SegmentB = segment.sections[1].OrderByDescending(
                            t => ConvertHelper.StrToInt(t.ScoreInfo.Split('-')[0], 0))
                            .ToDictionary(k => k.ScoreInfo, v => v.Count);
                    }
                }
                list.Add(item);
            }
            list = list.OrderBy(t => t.ClassName.ClassIndex()).ToList();
            //全年级
            var grade = new SubjectAnalysisDto
            {
                ClassName = "全年级",
                Teacher = string.Empty,
                TotalCount = list.Sum(t => t.TotalCount),
                StudentCount = list.Sum(t => t.StudentCount),
                AverageScore = subject.AverageScore,
                AverageScoreA = subject.AverageScoreA,
                AverageScoreB = subject.AverageScoreB ?? -1M,
                KeyCount = list.Sum(t => t.KeyCount),
                ACount = list.Sum(t => t.ACount),
                UnACount = list.Sum(t => t.UnACount)
            };
            list.ForEach(t =>
            {
                t.KeyScaleDiff = Math.Round(t.KeyScale - grade.KeyScale, 4);
                t.AScaleDiff = Math.Round(t.AScale - grade.AScale, 4);
                t.UnAScaleDiff = Math.Round(t.UnAScale - grade.UnAScale, 4);
            });
            grade.Segment = new Dictionary<string, int>();
            var seg = list.FirstOrDefault(t => t.Segment != null);
            if (seg != null)
            {
                foreach (var key in seg.Segment.Keys)
                {
                    grade.Segment.Add(key, list.Where(t => t.Segment != null).Sum(t => t.Segment[key]));
                }
            }
            var sectionSegment = list.FirstOrDefault(t => t.SegmentA != null && t.SegmentB != null);
            if (sectionSegment != null)
            {
                grade.SegmentA = new Dictionary<string, int>();
                foreach (var key in sectionSegment.SegmentA.Keys)
                {
                    grade.SegmentA.Add(key, list.Where(t => t.SegmentA != null).Sum(t => t.SegmentA[key]));
                }
                grade.SegmentB = new Dictionary<string, int>();
                foreach (var key in sectionSegment.SegmentB.Keys)
                {
                    grade.SegmentB.Add(key, list.Where(t => t.SegmentB != null).Sum(t => t.SegmentB[key]));
                }
            }
            list.Add(grade);
            return list;
        }

        /// <summary> 题目得分率统计 </summary>
        public DResult<ScoreRateDto> SubjectScoreRates(string id)
        {
            var model = ExaminationTask.Instance.QuestionScores(id);
            if (model == null)
                return DResult.Error<ScoreRateDto>("考试科目得分率异常");
            return DResult.Succ(model.Scores);
        }
    }
}
