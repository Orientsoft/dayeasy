using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Services;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DayEasy.Management.Services.Helper
{
    public class ExportHelper
    {
        /// <summary> 班级排名表 </summary>
        /// <param name="batch"></param>
        /// <param name="students"></param>
        /// <param name="rankTable"></param>
        /// <returns></returns>
        public static DataTable RankTable(string batch, IDictionary<long, MemberDto> students, DataTable rankTable = null)
        {
            if (rankTable == null)
            {
                rankTable = new DataTable("班内排名");
                rankTable.Columns.Add("rank", typeof(string));
                rankTable.Columns.Add("className", typeof(string));
                rankTable.Columns.Add("number", typeof(string));
                rankTable.Columns.Add("name", typeof(string));
                rankTable.Columns.Add("score", typeof(string));
                rankTable.Columns.Add("scoreA", typeof(string));
                rankTable.Columns.Add("scoreB", typeof(string));
                rankTable.Rows.Add("班内排名", "班级", "学号", "姓名", "总分", "A卷总分", "B卷总分");
            }
            var stuScoreRepository = CurrentIocManager.Resolve<IDayEasyRepository<TS_StuScoreStatistics>>();
            var groupRepository = CurrentIocManager.Resolve<IVersion3Repository<TG_Group>>();
            var classId = stuScoreRepository.Where(t => t.Batch == batch).Select(t => t.ClassId).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(classId))
                return rankTable;
            var className = groupRepository.Where(g => g.Id == classId).Select(g => g.GroupName).FirstOrDefault();
            var studentScores =
                stuScoreRepository.Where(t => t.Batch == batch)
                    .Select(
                        t =>
                            new
                            {
                                t.StudentId,
                                t.CurrentScore,
                                t.CurrentSort,
                                t.SectionAScore,
                                t.SectionBScore
                            })
                    .OrderBy(t => t.CurrentSort)
                    .ToList();
            if (!studentScores.Any())
                return rankTable;
            foreach (var score in studentScores)
            {
                string name = string.Empty, number = string.Empty;
                if (students.ContainsKey(score.StudentId))
                {
                    var student = students[score.StudentId];
                    name = student.Name ?? string.Empty;
                    number = student.StudentNum ?? string.Empty;
                }
                rankTable.Rows.Add(score.CurrentSort, className, number, name, score.CurrentScore,
                    score.SectionAScore, score.SectionBScore);
            }
            return rankTable;
        }

        /// <summary> 分数段统计 </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        public static DataTable SegmentTable(string batch)
        {
            var segmentTable = new DataTable("分数段统计");
            segmentTable.Columns.Add("segment", typeof(string));
            segmentTable.Columns.Add("count", typeof(string));
            segmentTable.Rows.Add("分数段", "人数");
            var classScoreRepository = CurrentIocManager.Resolve<IDayEasyRepository<TS_ClassScoreStatistics>>();
            var classScore =
                classScoreRepository.Where(t => t.Batch == batch)
                    .Select(t => new { t.AverageScore, t.TheHighestScore, t.TheLowestScore, t.ScoreGroups })
                    .FirstOrDefault();
            if (classScore == null)
                return segmentTable;
            var segments = JsonHelper.JsonList<ScoreGroupsDto>(classScore.ScoreGroups).ToList();

            foreach (var segment in segments.OrderByDescending(t => t.ScoreInfo.Split('-')[0].To(0)))
            {
                segmentTable.Rows.Add(segment.ScoreInfo, segment.Count);
            }
            segmentTable.Rows.Add("最高分", classScore.TheHighestScore);
            segmentTable.Rows.Add("最低分", classScore.TheLowestScore);
            segmentTable.Rows.Add("平均分", classScore.AverageScore);
            return segmentTable;
        }

        /// <summary> 每题得分 </summary>
        /// <param name="dto"></param>
        /// <param name="scoresTable"></param>
        /// <returns></returns>
        public static DataTable ScoresTable(QuestionScoresDto dto, DataTable scoresTable = null)
        {
            if (scoresTable == null)
            {
                scoresTable = new DataTable("每题得分");
                scoresTable.Columns.Add("name", typeof(string));
                scoresTable.Columns.Add("code", typeof(string));
                var headers = new List<object> { "姓名", "得一号" };
                foreach (var sort in dto.QuestionSorts)
                {
                    scoresTable.Columns.Add(sort.Key, typeof(string));
                    headers.Add(sort.Value);
                }
                scoresTable.Rows.Add(headers.ToArray());
            }
            foreach (var student in dto.Students)
            {
                var row = new List<object> { student.Name, student.Code };
                foreach (var sort in dto.QuestionSorts)
                {
                    var score = -1M;
                    if (student.Scores.ContainsKey(sort.Key))
                        score = student.Scores[sort.Key];
                    row.Add(score);
                }
                scoresTable.Rows.Add(row.ToArray());
            }
            return scoresTable;
        }

        /// <summary> 知识点 </summary>
        /// <param name="paperId"></param>
        /// <param name="sorts"></param>
        /// <returns></returns>
        public static DataTable KnowledgeTable(string paperId, IDictionary<string, string> sorts)
        {
            var knowledgeTable = new DataTable("题目知识点");
            knowledgeTable.Columns.Add("sort", typeof(string));

            var paperContract = CurrentIocManager.Resolve<IPaperContract>();
            var paperResult = paperContract.PaperDetailById(paperId);
            if (!paperResult.Status)
                return knowledgeTable;
            var knowledges = paperResult.Data.PaperSections.SelectMany(s => s.Questions.Select(q => q.Question))
                .Select(q => new { q.Id, q.KnowledgeIDs })
                .ToList()
                .ToDictionary(k => k.Id,
                    v => JsonHelper.Json<Dictionary<string, string>>(v.KnowledgeIDs ?? "{}"));
            var max = knowledges.Max(t => t.Value.Count);
            var headers = new List<object> { "题号" };
            for (var i = 0; i < max; i++)
            {
                knowledgeTable.Columns.Add("knowledge_{0}".FormatWith(i), typeof(string));
                headers.Add("知识点{0}".FormatWith((i + 1)));
            }
            knowledgeTable.Rows.Add(headers.ToArray());
            foreach (var sort in sorts)
            {
                var row = new List<object> { sort.Value };
                var list = new List<string>();
                if (knowledges.ContainsKey(sort.Key))
                    list = knowledges[sort.Key].Values.ToList();
                for (var i = 0; i < max; i++)
                {
                    var knowledge = string.Empty;
                    if (list.Count > i)
                        knowledge = list[i];
                    row.Add(knowledge);
                }
                knowledgeTable.Rows.Add(row.ToArray());
            }
            return knowledgeTable;
        }

        public static DataTable PaperSorts(string paperId)
        {
            var dt = new DataTable("试卷模板");
            if (string.IsNullOrWhiteSpace(paperId))
                return dt;
            var paperContract = CurrentIocManager.Resolve<IPaperContract>();

            var sorts = paperContract.PaperSorts(paperId, null);
            var headers = new List<object>
            {
                "姓名",
                "班级ID"
            };
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("groupCode", typeof(string));
            headers.AddRange(sorts.Values);
            foreach (var sort in sorts)
            {
                dt.Columns.Add(sort.Key, typeof(string));
            }
            dt.Rows.Add(headers.ToArray());
            for (var j = 0; j < 50; j++)
            {
                var row = new object[headers.Count];
                for (var i = 0; i < headers.Count; i++)
                {
                    row[i] = string.Empty;
                }
                dt.Rows.Add(row);
            }
            return dt;
        }
    }
}
