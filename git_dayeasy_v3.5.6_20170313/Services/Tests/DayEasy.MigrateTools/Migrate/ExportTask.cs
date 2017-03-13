using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Office;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace DayEasy.MigrateTools.Migrate
{
    public class ExportTask : MigrateBase
    {
        public void ClassExaminations()
        {
            var codes = "group_codes.txt".ReadConfig();
            if (!codes.Any())
                return;
            foreach (var code in codes)
            {
                ClassExamination(code);
                Thread.Sleep(200);
            }
            Log("成绩导出完成");
        }

        public void JointStatistics()
        {
            var joints = TxtFileHelper.Joints();
            if (!joints.Any())
                return;
            var groupContract = CurrentIocManager.Resolve<IGroupContract>();
            var usageRepository = CurrentIocManager.Resolve<IDayEasyRepository<TC_Usage>>();
            var statisticContract = CurrentIocManager.Resolve<IStatisticContract>();
            int index = 1;
            foreach (var joint in joints)
            {
                Log($"开始查询:{joint}");
                var batches = usageRepository.Where(
                    t => t.JointBatch == joint && t.MarkingStatus == (byte)MarkingStatus.AllFinished)
                    .Select(t => new { t.Id, t.ClassId }).ToList();
                Console.WriteLine($"总{batches.Count}条考试批次");
                if (!batches.Any())
                    continue;
                var dataSet = new DataSet($"{joint}考试情况统计表");

                var classIds = batches.Select(t => t.ClassId).ToList();
                var groupDict = groupContract.GroupDict(classIds);
                var scoresTable = new DataTable("每题得分");
                scoresTable.Columns.Add("className", typeof(string));
                scoresTable.Columns.Add("name", typeof(string));
                scoresTable.Columns.Add("code", typeof(string));
                var headers = new List<object> { "班级", "姓名", "得一号" };
                var questionScores = statisticContract.QuestionScores(batches.Select(t => t.Id).First());
                foreach (var sort in questionScores.Data.QuestionSorts)
                {
                    scoresTable.Columns.Add(sort.Key, typeof(string));
                    headers.Add(sort.Value);
                }
                scoresTable.Rows.Add(headers.ToArray());

                var knowledgeTable = KnowledgeTable(questionScores.Data.QuestionSorts, index++);
                dataSet.Tables.Add(knowledgeTable);
                foreach (var item in batches)
                {
                    Log($"正在执行{item.Id}");
                    var groupName = string.Empty;
                    if (groupDict.ContainsKey(item.ClassId))
                        groupName = groupDict[item.ClassId];
                    var result = statisticContract.QuestionScores(item.Id);
                    var dto = result.Data;
                    foreach (var student in dto.Students)
                    {
                        var row = new List<object> { groupName, student.Name, student.Code };
                        foreach (var sort in dto.QuestionSorts)
                        {
                            var score = -1M;
                            if (student.Scores.ContainsKey(sort.Key))
                                score = student.Scores[sort.Key];
                            row.Add(score);
                        }
                        scoresTable.Rows.Add(row.ToArray());
                    }
                }
                dataSet.Tables.Add(scoresTable);
                ExcelHelper.Export(dataSet, dataSet.DataSetName + ".xls", Utils.GetMapPath("exports"));
                Console.WriteLine($"{joint}成绩导出成功");
            }
        }

        private void ClassExamination(string code)
        {
            Log($"开始查询:{code}");
            var groupContract = CurrentIocManager.Resolve<IGroupContract>();
            var groupResult = groupContract.LoadByCode(code);
            if (!groupResult.Status)
            {
                Console.WriteLine($"error:{groupResult.Message}");
                return;
            }
            var group = groupResult.Data;
            var usageRepository = CurrentIocManager.Resolve<IDayEasyRepository<TC_Usage>>();
            var batches = usageRepository.Where(
                t =>
                    t.ClassId == group.Id && t.MarkingStatus == (byte)MarkingStatus.AllFinished &&
                    t.ApplyType == (byte)ApplyType.Print && t.SubjectId == 2)
                .OrderBy(t => t.AddedAt)
                .Select(t => t.Id).ToList();
            Console.WriteLine($"总{batches.Count}条考试批次");
            if (!batches.Any())
                return;

            var students = groupContract.GroupMembers(group.Id, UserRole.Student).Data.ToDictionary(k => k.Id, v => v);
            var statisticContract = CurrentIocManager.Resolve<IStatisticContract>();
            var dataSet = new DataSet($"{group.Name}考试情况统计表");
            var index = 1;
            foreach (var item in batches)
            {
                Console.WriteLine($"正在执行{item}:{index}");
                var rankTable = RankTable(item, index, students);
                var segmentTable = SegmentTable(item, index);
                var questionScores = statisticContract.QuestionScores(item);
                var scoresTable = ScoresTable(questionScores.Data, index);
                var knowledgeTable = KnowledgeTable(questionScores.Data.QuestionSorts, index);
                dataSet.Tables.Add(rankTable);
                dataSet.Tables.Add(segmentTable);
                dataSet.Tables.Add(scoresTable);
                dataSet.Tables.Add(knowledgeTable);
                index++;
            }
            ExcelHelper.Export(dataSet, dataSet.DataSetName + ".xls", Utils.GetMapPath("exports"));
            Console.WriteLine($"{code}成绩导出成功");
        }

        /// <summary> 班级排名表 </summary>
        /// <param name="batch"></param>
        /// <param name="index"></param>
        /// <param name="students"></param>
        /// <returns></returns>
        private DataTable RankTable(string batch, int index, IDictionary<long, MemberDto> students)
        {
            var rankTable = new DataTable($"班内排名{index}");
            rankTable.Columns.Add("rank", typeof(string));
            rankTable.Columns.Add("number", typeof(string));
            rankTable.Columns.Add("name", typeof(string));
            rankTable.Columns.Add("score", typeof(string));
            rankTable.Columns.Add("scoreA", typeof(string));
            rankTable.Columns.Add("scoreB", typeof(string));
            rankTable.Rows.Add("序号", "学号", "姓名", "总分", "A卷总分", "B卷总分");
            var stuScoreRepository = CurrentIocManager.Resolve<IDayEasyRepository<TS_StuScoreStatistics>>();
            var studentScores =
                    stuScoreRepository.Where(t => t.Batch == batch)
                        .Select(t => new { t.StudentId, t.CurrentScore, t.CurrentSort, t.SectionAScore, t.SectionBScore })
                        .OrderBy(t => t.CurrentSort)
                        .ToList();
            foreach (var score in studentScores)
            {
                string name = string.Empty, number = string.Empty;
                if (students.ContainsKey(score.StudentId))
                {
                    var student = students[score.StudentId];
                    name = student.Name ?? string.Empty;
                    number = student.StudentNum ?? string.Empty;
                }
                rankTable.Rows.Add(score.CurrentSort, number, name, score.CurrentScore, score.SectionAScore,
                    score.SectionBScore);
            }
            return rankTable;
        }

        private DataTable SegmentTable(string batch, int index)
        {
            var segmentTable = new DataTable($"分数段统计{index}");
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

        private DataTable ScoresTable(QuestionScoresDto dto, int index)
        {
            var scoresTable = new DataTable($"每题得分{index}");
            scoresTable.Columns.Add("name", typeof(string));
            scoresTable.Columns.Add("code", typeof(string));
            var headers = new List<object> { "姓名", "得一号" };
            foreach (var sort in dto.QuestionSorts)
            {
                scoresTable.Columns.Add(sort.Key, typeof(string));
                headers.Add(sort.Value);
            }
            scoresTable.Rows.Add(headers.ToArray());
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

        private DataTable KnowledgeTable(Dictionary<string, string> sorts, int index)
        {
            var knowledgeTable = new DataTable($"题目知识点{index}");
            knowledgeTable.Columns.Add("sort", typeof(string));

            var questionRepository = CurrentIocManager.Resolve<IDayEasyRepository<TQ_Question>>();
            var qids = sorts.Keys;
            var knowledges =
                questionRepository.Where(q => qids.Contains(q.Id)).Select(q => new { q.Id, q.KnowledgeIDs })
                    .ToList()
                    .ToDictionary(k => k.Id, v => JsonHelper.Json<Dictionary<string, string>>(v.KnowledgeIDs ?? "{}"));
            var max = knowledges.Max(t => t.Value.Count);
            var headers = new List<object> { "题号" };
            for (var i = 0; i < max; i++)
            {
                knowledgeTable.Columns.Add($"knowledge_{i}", typeof(string));
                headers.Add($"知识点{i}");
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
    }
}
