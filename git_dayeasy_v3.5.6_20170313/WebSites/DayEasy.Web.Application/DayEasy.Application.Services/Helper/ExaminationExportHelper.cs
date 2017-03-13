using DayEasy.Contracts.Dtos.Examination;
using DayEasy.Office;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DayEasy.Application.Services.Helper
{
    /// <summary> 大型考试Excel导出辅助 </summary>
    public static class ExaminationExportHelper
    {
        private static string ToPercent(this decimal scale)
        {
            return string.Concat(Math.Round(scale * 100M, 2), "%");
        }

        private static List<string> LayerRanks(params int[] counts)
        {
            var ranks = new List<string>();
            var prefix = 1;
            foreach (var count in counts)
            {
                if (count > 0)
                {
                    ranks.Add("({0}-{1}名)".FormatWith(prefix, prefix + count - 1));
                    prefix += count;
                }
                else
                {
                    ranks.Add("(-)");
                }
            }
            return ranks;
        }

        #region 年级排名报表
        /// <summary> 年级排名导出 </summary>
        /// <param name="dto"></param>
        public static void Rankings(ExamRanksDto dto, Action callback)
        {
            var dt = new DataTable { TableName = "年级排名" };
            var header = new List<object>
            {
                "姓名",
                "得一号",
                "学号",
                "班级",
                "排名",
                "总分"
            };
            var subjects = dto.Subjects;
            foreach (var subject in subjects)
            {
                if (subject.Value.IsNullOrEmpty())
                    continue;
                var name = subject.Value.First().TrimEnd('A');
                foreach (var title in subject.Value)
                {
                    header.Add(title == "排名" ? (name + title) : title);
                }
            }
            foreach (var key in header)
            {
                dt.Columns.Add(key + IdHelper.Instance.Guid32, typeof(string));
            }
            dt.Rows.Add(header.ToArray());
            foreach (var rank in dto.Students)
            {
                var cols = new List<object>
                {
                    rank.Name,
                    rank.UserCode,
                    rank.StudentNo??string.Empty,
                    rank.ClassName,
                    rank.Rank,
                    rank.Score
                };
                foreach (var subject in subjects)
                {
                    if (!rank.ScoreDetails.ContainsKey(subject.Key))
                        continue;
                    var detail = rank.ScoreDetails[subject.Key];
                    cols.Add(detail.ScoreA);
                    if (subject.Value.Count == 4)
                        cols.Add(detail.ScoreB);
                    cols.Add(detail.Score);
                    cols.Add(detail.Rank);
                }
                dt.Rows.Add(cols.ToArray());
            }
            callback();
            ExcelHelper.Export(new DataSet { Tables = { dt } }, dto.Name + "年级排名信息表");
        }
        #endregion

        #region 班级分析报表
        /// <summary> 班级分析导出 </summary>
        /// <param name="name"></param>
        /// <param name="keys"></param>
        /// <param name="layers"></param>
        public static void ClassAnalysis(string name, List<ClassAnalysisKeyDto> keys, List<ClassAnalysisLayerDto> layers, Action callback)
        {
            var keyTable = new DataTable("重点率分析");
            var keyHeaders = new object[]
            {
                "班级", "圈主", "应查人数", "实际人数", "总平均分", "总平均分差值", "均值比", "均值比差值", "重点率上线人数", "班级重点率", "重点率差值", "A卷合格人数",
                "A卷合格率", "A卷合格率差值", "A卷不合格人数", "A卷不合格率", "A卷不合格率差值"
            };
            foreach (var head in keyHeaders)
            {
                keyTable.Columns.Add(head.ToString(), typeof(string));
            }
            keyTable.Rows.Add(keyHeaders);
            foreach (var key in keys)
            {
                if (string.IsNullOrWhiteSpace(key.Teacher))
                {
                    //年级
                    keyTable.Rows.Add(key.ClassName, string.Empty, key.TotalCount, key.StudentCount, key.AverageScore,
                        string.Empty, string.Empty, string.Empty, key.KeyCount, key.KeyScale.ToPercent(), string.Empty,
                        key.ACount, key.AScale.ToPercent(), string.Empty, key.UnACount, key.UnAScale.ToPercent(),
                        string.Empty);
                }
                else
                {
                    keyTable.Rows.Add(key.ClassName, key.Teacher, key.TotalCount, key.StudentCount, key.AverageScore,
                        key.AverageScoreDiff, key.AverageRatio, key.AverageRatioDiff, key.KeyCount,
                        key.KeyScale.ToPercent(), key.KeyScaleDiff.ToPercent(), key.ACount, key.AScale.ToPercent(),
                        key.AScaleDiff.ToPercent(), key.UnACount, key.UnAScale.ToPercent(), key.UnAScaleDiff.ToPercent());
                }
            }

            var layerTable = new DataTable("学生分层分析");
            //分层人数
            var grade = layers.First(t => string.IsNullOrWhiteSpace(t.ClassId));
            var tips = LayerRanks(grade.LayerA, grade.LayerB, grade.LayerC, grade.LayerD, grade.LayerE);

            var layerHeaders = new object[]
            {
                "班级", "圈主", "应查人数", "实际人数", "A层人数" + tips[0], "B层人数" + tips[1], "C层人数" + tips[2], "D层人数" + tips[3],
                "E层人数" + tips[4]
            };
            foreach (var head in layerHeaders)
            {
                layerTable.Columns.Add(head.ToString(), typeof(string));
            }
            layerTable.Rows.Add(layerHeaders);
            foreach (var layer in layers)
            {
                layerTable.Rows.Add(layer.ClassName, layer.Teacher ?? string.Empty, layer.TotalCount, layer.StudentCount, layer.LayerA,
                    layer.LayerB, layer.LayerC, layer.LayerD, layer.LayerE);
            }
            callback();
            ExcelHelper.Export(new DataSet { Tables = { keyTable, layerTable } }, name + "-班级分析信息表");
        }
        #endregion

        #region 学科分析报表
        /// <summary> 学科分析导出 </summary>
        /// <param name="name"></param>
        /// <param name="subjects"></param>
        public static void SubjectAnalysis(string name, List<SubjectAnalysisDto> subjects, Action callback)
        {
            var keyTable = new DataTable("学科重点率分析");
            var keyHeaders = new object[]
            {
                "班级", "任课教师", "应查人数", "实际人数", "总平均分", "总平均分差值", "均值比", "均值比差值", "A卷平均分", "B卷平均分", "重点率上线人数", "重点率",
                "重点率差值", "A卷合格人数", "A卷合格率", "A卷合格率差值", "A卷不合格人数", "A卷不合格率", "A卷不合格率差值"
            };
            foreach (var key in keyHeaders)
            {
                keyTable.Columns.Add(key.ToString(), typeof(string));
            }
            keyTable.Rows.Add(keyHeaders);

            var segmentTable = new DataTable("总分分数段分析");
            var segmentHeaders = new List<object>
            {
                "班级",
                "任课教师",
                "应查人数",
                "实际人数"
            };
            var grade = subjects[subjects.Count - 1];
            if (grade.Segment == null || !grade.Segment.Any())
                return;

            #region AB卷分数段统计

            DataTable segmentTableA = null, segmentTableB = null;
            if (grade.SegmentA != null && grade.SegmentB != null)
            {
                segmentTableA = new DataTable("A卷分数段分析");
                segmentTableB = new DataTable("B卷分数段分析");
                var segmentA = new List<object>(segmentHeaders.ToArray());
                var segmentB = new List<object>(segmentHeaders.ToArray());
                segmentA.AddRange(grade.SegmentA.Keys);
                segmentB.AddRange(grade.SegmentB.Keys);
                foreach (var key in segmentA)
                {
                    segmentTableA.Columns.Add(key.ToString(), typeof(string));
                }
                segmentTableA.Rows.Add(segmentA.ToArray());
                foreach (var key in segmentB)
                {
                    segmentTableB.Columns.Add(key.ToString(), typeof(string));
                }
                segmentTableB.Rows.Add(segmentB.ToArray());
            }

            #endregion

            segmentHeaders.AddRange(grade.Segment.Keys);

            foreach (var segment in segmentHeaders)
            {
                segmentTable.Columns.Add(segment.ToString(), typeof(string));
            }
            segmentTable.Rows.Add(segmentHeaders.ToArray());
            foreach (var subject in subjects)
            {
                if (subject.Teacher.IsNullOrEmpty())
                {
                    keyTable.Rows.Add(subject.ClassName, string.Empty, subject.TotalCount, subject.StudentCount,
                        subject.AverageScore, string.Empty, string.Empty, string.Empty, subject.AverageScoreA,
                        subject.AverageScoreB, subject.KeyCount, subject.KeyScale.ToPercent(), string.Empty,
                        subject.ACount, subject.AScale.ToPercent(), string.Empty, subject.UnACount,
                        subject.UnAScale.ToPercent(), string.Empty);
                }
                else
                {
                    keyTable.Rows.Add(subject.ClassName, subject.Teacher, subject.TotalCount, subject.StudentCount,
                        subject.AverageScore, subject.AverageScoreDiff, subject.AverageRatio, subject.AverageRatioDiff,
                        subject.AverageScoreA, subject.AverageScoreB, subject.KeyCount, subject.KeyScale.ToPercent(),
                        subject.KeyScaleDiff.ToPercent(), subject.ACount, subject.AScale.ToPercent(),
                        subject.AScaleDiff.ToPercent(), subject.UnACount, subject.UnAScale.ToPercent(),
                        subject.UnAScaleDiff.ToPercent());
                }
                var cols = new List<object>
                {
                    subject.ClassName,
                    subject.Teacher ?? string.Empty,
                    subject.TotalCount,
                    subject.StudentCount
                };
                if (segmentTableA != null)
                {
                    var colsA = new List<object>(cols.ToArray());
                    var colsB = new List<object>(cols.ToArray());
                    if (subject.SegmentA != null && subject.SegmentA.Any())
                    {
                        colsA.AddRange(subject.SegmentA.Values.Cast<object>());
                    }
                    else
                    {
                        for (var i = 0; i < grade.SegmentA.Keys.Count; i++)
                        {
                            colsA.Add(0);
                        }
                    }
                    segmentTableA.Rows.Add(colsA.ToArray());
                    if (subject.SegmentB != null && subject.SegmentB.Any())
                    {
                        colsB.AddRange(subject.SegmentB.Values.Cast<object>());
                    }
                    else
                    {
                        for (var i = 0; i < grade.SegmentB.Keys.Count; i++)
                        {
                            colsB.Add(0);
                        }
                    }
                    segmentTableB.Rows.Add(colsB.ToArray());
                }

                if (subject.Segment != null && subject.Segment.Any())
                {
                    cols.AddRange(subject.Segment.Values.Cast<object>());
                }
                else
                {
                    for (var i = 0; i < grade.Segment.Keys.Count; i++)
                    {
                        cols.Add(0);
                    }
                }
                segmentTable.Rows.Add(cols.ToArray());
            }
            var ds = new DataSet { Tables = { keyTable, segmentTable } };
            if (segmentTableA != null)
                ds.Tables.Add(segmentTableA);
            if (segmentTableB != null)
                ds.Tables.Add(segmentTableB);
            callback();
            ExcelHelper.Export(ds, name + "-学科分析信息表");
        }
        #endregion

        public static void UnionRanksExport(UnionSourceDto dto, Action callback)
        {
            var dt = new DataTable { TableName = "联考排名" };
            var header = new List<object>
            {
                "姓名",
                "得一号",
                "学号",
                "班级",
                "学校",
                "排名",
                "总分"
            };
            var subjects = dto.Subjects;
            foreach (var subject in subjects)
            {
                var name = subject.Value.Subject;
                header.Add(name);
                if (!subject.Value.IsAb)
                    continue;
                header.Add(string.Format("{0}A",name));
                header.Add(string.Format("{0}B", name));
            }
            foreach (var key in header)
            {
                dt.Columns.Add(key + IdHelper.Instance.Guid32, typeof(string));
            }
            dt.Rows.Add(header.ToArray());
            foreach (var rank in dto.Students)
            {
                var exam = dto.Exams[rank.ExamId];
                var cols = new List<object>
                {
                    rank.Name,
                    rank.Code,//得一号
                    rank.StudentNum??string.Empty,//学号
                    rank.ClassName,
                    exam.AgencyName,
                    rank.Rank,
                    rank.TotalScore
                };
                foreach (var subject in subjects)
                {
                    var detail = rank.Subjects[subject.Key];
                    var score = 0M;
                    if (detail.ScoreA < 0 && detail.ScoreB < 0)
                        score = -1M;
                    else
                    {
                        if (detail.ScoreA > 0) score += detail.ScoreA;
                        if (detail.ScoreB > 0) score += detail.ScoreB;
                    }
                    cols.Add(score);
                    if (!subject.Value.IsAb)
                        continue;
                    cols.Add(detail.ScoreA);
                    cols.Add(detail.ScoreB);
                }
                dt.Rows.Add(cols.ToArray());
            }
            callback();
            ExcelHelper.Export(new DataSet { Tables = { dt } }, dto.Exams.First().Value.Name + "_关联报表.xls");
        }
    }
}