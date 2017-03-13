using System.Collections.Generic;
using System.Linq;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Examination;
using DayEasy.Utility;
using DayEasy.Utility.Extend;

namespace DayEasy.Examination.Services
{
    /// <summary> 学生统计 </summary>
    public partial class ExaminationService
    {
        public IStatisticContract StatisticContract { get; set; }

        public DResult<StudentSummaryDto> Summary(string id, long studentId)
        {
            if (id.IsNullOrEmpty() || studentId < 0) return DResult.Error<StudentSummaryDto>("考试ID不能为空");

            var em = ExaminationRepository.Load(id);
            if (em == null) return DResult.Error<StudentSummaryDto>("没有查询到考试资料");

            var subjects = ExamSubjects(id);
            var studentScore = StudentScoreRepository.FirstOrDefault(i => i.StudentId == studentId && i.ExamId == id);
            if (studentScore == null) return DResult.Error<StudentSummaryDto>("没有查询到学生成绩统计");

            var sections = StudentSubjectScoreRepository.Where(i => i.ExamId == id && i.StudentId == studentId).ToList();
            if (!subjects.Any() || !sections.Any()) return DResult.Error<StudentSummaryDto>("没有查询到学科成绩统计");

            var dto = new StudentSummaryDto
            {
                ExaminationId = id,
                ExaminationTitle = em.Name,
                TotalScore = studentScore.TotalScore,
                AvgGradeScore = em.AverageScore ?? 0M,
                GradeRank = studentScore.Rank,
                ClassRank = studentScore.ClassRank
            };
            dto.AvgClassScore = StudentScoreRepository.Where(i =>
                    i.ExamId == id && i.ClassId == studentScore.ClassId).Average(i => i.TotalScore);
            dto.Ranks = (from a in subjects
                from b in sections
                where a.Id == b.ExamSubjectId
                select new StudentSubjectRankDto
                {
                    Batch = b.Batch,
                    PaperId = a.PaperId,
                    PaperTitle = a.PaperTitle,
                    PaperType = a.PaperType,
                    SubjectName = a.Subject,
                    TotalScore = b.Score,
                    AScore = b.ScoreA,
                    BScore = b.ScoreB,
                    AvgGradeTotalScore = a.AverageScore,
                    AvgGradeAScore = a.AverageScoreA,
                    AvgGradeBScore = a.AverageScoreB ?? 0M,
                    GradeRank = b.Rank
                }).ToList();
            
            return DResult.Succ(dto);
        }

        public DResults<StudentSubjectSectionDto> ScoreSections(string id, long studentId)
        {
            var subjects = ExamSubjects(id);
            var scores = StudentSubjectScoreRepository
                .Where(i => i.ExamId == id && i.StudentId == studentId).ToList();
            if (!subjects.Any() || !scores.Any())
                return DResult.Errors<StudentSubjectSectionDto>("没有查询到学科成绩统计");

            var result = new List<StudentSubjectSectionDto>();
            var list = (from a in subjects
                from b in scores
                where a.Id == b.ExamSubjectId
                select new
                {
                    b.Batch,
                    a.PaperId,
                    a.PaperType,
                    a.Subject,
                    b.ClassRank
                }).ToList();
            if (!list.Any()) DResult.Succ(result, 0);

            list.ForEach(i =>
            {
                if (i.Batch.IsNullOrEmpty()) return;
                var rt = StatisticContract.GetStatisticsAvges(i.Batch, i.PaperId, false, true);
                if (!rt.Status) return;
                var section = rt.Data.FirstOrDefault();
                if (section == null) return;
                result.Add(new StudentSubjectSectionDto
                {
                    PaperId = i.PaperId,
                    PaperType = i.PaperType,
                    SubjectName = i.Subject,
                    Rank = i.ClassRank,
                    Section = section
                });
            });

            return DResult.Succ(result, result.Count);
        }
    }
}
