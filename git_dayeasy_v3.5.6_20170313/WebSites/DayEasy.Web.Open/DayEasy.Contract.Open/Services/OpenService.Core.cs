using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Models.Open.Work;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;

namespace DayEasy.Contract.Open.Services
{
    public partial class OpenService
    {
        public IUserContract UserContract { private get; set; }
        public IStatisticContract StatisticContract { private get; set; }
        public IDayEasyRepository<TS_StuScoreStatistics> StudentScoreRepository { private get; set; }
        public IDayEasyRepository<TS_ClassScoreStatistics> ClassScoreRepository { private get; set; }
        public IDayEasyRepository<TP_Paper> PaperRepository { private get; set; }

        public IDayEasyRepository<TP_JointMarking> JointMarkingRepository { private get; set; }

        /// <summary> 试卷打印发布检测 </summary>
        /// <param name="paperId"></param>
        /// <param name="classGroupId"></param>
        /// <param name="teacherId"></param>
        /// <param name="sectionType"></param>
        /// <param name="updateUsages"></param>
        /// <param name="jointBatch"></param>
        /// <returns></returns>
        private string CheckPicturePublish(string paperId, string classGroupId, long teacherId, byte sectionType,
            List<TC_Usage> updateUsages, string jointBatch = null)
        {
            Expression<Func<TC_Usage, bool>> condition = t =>
                t.SourceID == paperId
                && t.SourceType == (byte)PublishType.Print
                && t.ClassId == classGroupId
                && t.UserId == teacherId
                && t.Status != (byte)NormalStatus.Delete
                && t.MarkingStatus != (byte)MarkingStatus.AllFinished;
            //协同批次
            condition = (!string.IsNullOrWhiteSpace(jointBatch)
                ? condition.And(t => t.JointBatch == jointBatch)
                : condition.And(t => t.JointBatch == null || t.JointBatch == ""));
            var usage = UsageRepository.FirstOrDefault(condition);
            if (usage == null)
                return null;
            if (sectionType <= 0)
                return usage.Id;
            updateUsages = updateUsages ?? new List<TC_Usage>();
            //分卷
            switch (sectionType)
            {
                case (byte)PaperSectionType.PaperA:
                    if (usage.MarkingStatus == (byte)MarkingStatus.FinishedA)
                        return null;
                    if ((usage.PrintType & (byte)PrintType.PaperAHomeWork) == 0)
                    {
                        usage.PrintType |= (byte)PrintType.PaperAHomeWork;
                        updateUsages.Add(usage);
                    }
                    break;
                case (byte)PaperSectionType.PaperB:
                    if (usage.MarkingStatus == (byte)MarkingStatus.FinishedB)
                        return null;
                    if ((usage.PrintType & (byte)PrintType.PaperBHomeWork) == 0)
                    {
                        usage.PrintType |= (byte)PrintType.PaperBHomeWork;
                        updateUsages.Add(usage);
                    }
                    break;
            }
            return usage.Id;
        }

        private DResult PublishPrintUsage(List<TC_Usage> updateUsages, List<TC_Usage> publishUsages,
            List<TP_MarkingPicture> pictures, string jointBatch = null)
        {
            if (publishUsages == null || pictures == null)
                return DResult.Error("参数错误！");
            int paperA = 0, paperB = 0;

            var result = UnitOfWork.Transaction(() =>
            {
                //更新打印类型
                if (updateUsages != null && updateUsages.Any())
                {
                    UsageRepository.Update(u => new
                    {
                        u.PrintType
                    }, updateUsages.ToArray());
                }
                //更新试卷使用次数
                var papers = publishUsages.GroupBy(t => t.SourceID);
                foreach (var paperGroup in papers)
                {
                    var paper = PaperRepository.Load(paperGroup.Key);
                    paper.UseCount += paperGroup.Count();
                    paper.IsUsed = true;
                    PaperRepository.Update(p => new
                    {
                        p.UseCount,
                        p.IsUsed
                    }, paper);
                }

                //更新教师发布统计
                var publishGroups = publishUsages.GroupBy(t => t.UserId);
                foreach (var publishGroup in publishGroups)
                {
                    StatisticContract.UpdateStatistic(new TeacherStatisticDto //更新教师统计数据
                    {
                        PublishPaperCount = publishGroup.Count(),
                        UserID = publishGroup.Key
                    });
                }

                //发布试卷
                UsageRepository.Insert(publishUsages);

                //新增或更新MarkingPicture
                foreach (var picture in pictures)
                {
                    if (!picture.IsSuccess)
                    {
                        MarkingPictureRepository.Insert(picture);
                    }
                    else
                    {
                        var item = picture;
                        var mp =
                            MarkingPictureRepository.FirstOrDefault(
                                t =>
                                    t.BatchNo == item.BatchNo && t.StudentID == item.StudentID &&
                                    t.PaperID == item.PaperID && t.AnswerImgType == item.AnswerImgType);
                        if (mp != null)
                        {
                            picture.Id = mp.Id;

                            //更新
                            mp.AnswerImgUrl = picture.AnswerImgUrl;
                            mp.SheetAnswers = picture.SheetAnswers;
                            mp.AddedAt = picture.AddedAt;
                            mp.SubmitSort = picture.SubmitSort;
                            mp.TotalPageNum = picture.TotalPageNum;
                            mp.IsSingleFace = picture.IsSingleFace;
                            MarkingPictureRepository.Update(p => new
                            {
                                p.AnswerImgUrl,
                                p.SheetAnswers,
                                p.AddedAt,
                                p.SubmitSort,
                                p.TotalPageNum,
                                p.IsSingleFace
                            }, mp);
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(jointBatch))
                            {
                                if (picture.AnswerImgType == (byte)MarkingPaperType.PaperB)
                                {
                                    paperB++;
                                }
                                else
                                {
                                    paperA++;
                                }
                            }
                            MarkingPictureRepository.Insert(picture);
                        }
                    }
                }
                //更新协同试卷数
                if (string.IsNullOrWhiteSpace(jointBatch))
                    return;
                var jointMarking = JointMarkingRepository.Load(jointBatch);
                jointMarking.PaperACount += paperA;
                jointMarking.PaperBCount += paperB;

                JointMarkingRepository.Update(j => new
                {
                    j.PaperACount,
                    j.PaperBCount
                }, jointMarking);
            });

            return DResult.FromResult(result);
        }

        private TP_MarkingPicture ParseToMarkingPicture(long addedBy, string paperId, MPictureInfo picture,
            DateTime addedTime, string no = null)
        {
            return new TP_MarkingPicture
            {
                Id = IdHelper.Instance.Guid32,
                AddedBy = addedBy,
                AddedAt = addedTime,
                BatchNo = no ?? string.Empty,
                PaperID = paperId,
                StudentID = picture.StudentId,
                StudentName = picture.StudentName ?? string.Empty,
                ClassID = picture.GroupId ?? string.Empty,
                AnswerImgUrl = picture.ImagePath ?? string.Empty,
                AnswerImgType = picture.SectionType,
                IsSuccess = picture.IsSuccess,
                Marks = null,
                SubmitSort = picture.Index,
                RightAndWrong = null,
                TotalPageNum = picture.PageCount,
                IsSingleFace = picture.IsSingle,
                SheetAnswers = picture.SheetAnwers.ToJson(),
                Status = 0
            };
        }

        /// <summary> 学生分数 </summary>
        /// <param name="batch"></param>
        /// <param name="sectionType"></param>
        /// <param name="isJoint"></param>
        /// <returns></returns>
        private Dictionary<long, decimal> StudentScores(string batch, byte sectionType, bool isJoint = false)
        {
            Expression<Func<TS_StuScoreStatistics, bool>> condition;
            if (isJoint)
            {
                var batches = UsageRepository.Where(u => u.JointBatch == batch)
                    .Select(u => u.Id);
                condition = s => batches.Contains(s.Batch);
            }
            else
            {
                condition = s => s.Batch == batch;
            }
            var scores = StudentScoreRepository.Where(condition)
                .Select(t => new
                {
                    t.StudentId,
                    t.CurrentScore,
                    t.SectionAScore,
                    t.SectionBScore
                }).ToList();
            Dictionary<long, decimal> dict;
            switch (sectionType)
            {
                case (byte)PaperSectionType.PaperA:
                    dict = scores.ToDictionary(k => k.StudentId, v => v.SectionAScore);
                    break;
                case (byte)PaperSectionType.PaperB:
                    dict = scores.ToDictionary(k => k.StudentId, v => v.SectionBScore);
                    break;
                default:
                    dict = scores.ToDictionary(k => k.StudentId, v => v.CurrentScore);
                    break;
            }
            return dict;
        }
    }
}
