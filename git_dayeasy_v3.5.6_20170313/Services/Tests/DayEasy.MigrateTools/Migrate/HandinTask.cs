
using DayEasy.AsyncMission.Jobs.JobTasks;
using DayEasy.AsyncMission.Models;
using DayEasy.Contracts;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Services;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.MigrateTools.Migrate
{
    public class HandinTask : MigrateBase
    {
        private readonly IMarkingContract _markingContract;
        private readonly IDayEasyRepository<TP_MarkingPicture> _pictureRepository;
        private readonly IDayEasyRepository<TC_Usage> _usageRepository;
        private readonly IDayEasyRepository<TP_JointMarking> _jointMarkingRepository;
        private readonly IDayEasyRepository<TP_JointQuestionGroup> _questionGroupRepository;
        private readonly IDayEasyRepository<TP_JointPictureDistribution> _jointPictureDistributionRepository;

        public HandinTask()
        {
            _markingContract = CurrentIocManager.Resolve<IMarkingContract>();
            _pictureRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingPicture>>();
            _jointMarkingRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_JointMarking>>();
            _questionGroupRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_JointQuestionGroup>>();
            _jointPictureDistributionRepository =
                CurrentIocManager.Resolve<IDayEasyRepository<TP_JointPictureDistribution>>();
            _usageRepository = CurrentIocManager.Resolve<IDayEasyRepository<TC_Usage>>();
        }

        public void Main()
        {
            Console.WriteLine("任务开始！");
            var date = DateTime.Parse("2016-01-11 14:48:33.637");
            var pictures = _pictureRepository.Where(p => p.AddedAt >= date && !p.LastMarkingTime.HasValue)
                .ToList();
            var joints = new Dictionary<string, List<TP_MarkingPicture>>();
            Console.WriteLine("共找到{0}张待批阅的试卷！", pictures.Count);
            foreach (var picture in pictures)
            {
                Console.WriteLine("自动批阅[{0}]！", picture.Id);
                var result = _markingContract.Commit(picture.Id);
                if (!result.Status)
                {
                    Console.WriteLine("自动批阅[{0}]异常：{1}", picture.Id, result.Message);
                    continue;
                }
                var usage = _usageRepository.Load(picture.BatchNo);
                if (usage != null && !string.IsNullOrWhiteSpace(usage.JointBatch))
                {
                    if (joints.ContainsKey(usage.JointBatch))
                    {
                        joints[usage.JointBatch].Add(picture);
                    }
                    else
                    {
                        joints.Add(usage.JointBatch, new List<TP_MarkingPicture> { picture });
                    }
                }
                picture.LastMarkingTime = Clock.Now;
                _pictureRepository.Update(p => new
                {
                    p.LastMarkingTime
                }, picture);
                Console.WriteLine("完成自动批阅[{0}]！", picture.Id);
            }

            if (joints.Keys.Any())
            {
                Console.WriteLine("开始协同任务初始化！");
                foreach (var batch in joints.Keys)
                {
                    InitJointPictures(batch, joints[batch]);
                }
            }
            Console.WriteLine("任务完成！");
        }

        public void PictureDist()
        {
            var ids = TxtFileHelper.Pictures();
            var pictures = _pictureRepository.Where(p => ids.Contains(p.Id)).ToList();
            var joints = new Dictionary<string, List<TP_MarkingPicture>>();
            Console.WriteLine("共找到{0}张待分配的试卷！", pictures.Count);
            foreach (var picture in pictures)
            {
                var usage = _usageRepository.Load(picture.BatchNo);
                if (usage != null && !string.IsNullOrWhiteSpace(usage.JointBatch))
                {
                    if (joints.ContainsKey(usage.JointBatch))
                    {
                        joints[usage.JointBatch].Add(picture);
                    }
                    else
                    {
                        joints.Add(usage.JointBatch, new List<TP_MarkingPicture> { picture });
                    }
                }
            }

            if (joints.Keys.Any())
            {
                Console.WriteLine("开始协同任务初始化！");
                foreach (var batch in joints.Keys)
                {
                    InitJointPictures(batch, joints[batch]);
                }
            }
            Console.WriteLine("任务完成！");
        }

        /// <summary> 初始化协同试卷分配区域 </summary>
        /// <param name="jointBatch"></param>
        /// <param name="pictures"></param>
        private int InitJointPictures(string jointBatch, List<TP_MarkingPicture> pictures)
        {
            if (string.IsNullOrWhiteSpace(jointBatch) || pictures == null || !pictures.Any())
                return 0;
            var joint = _jointMarkingRepository.Load(jointBatch);
            if (joint == null)
                return 0;
            var list = new List<TP_JointPictureDistribution>();
            var groups = _questionGroupRepository.Where(t => t.JointBatch == jointBatch)
                .GroupBy(t => t.SectionType)
                .ToDictionary(k => k.Key, v => v.Select(t => t.Id).ToList());
            if (!groups.Any())
                return 0;
            foreach (var picture in pictures)
            {
                //普通卷/A卷
                var type = (byte)(picture.AnswerImgType == 0 ? 1 : picture.AnswerImgType);
                if (!groups.ContainsKey(type))
                    continue;
                var ids = groups[type];
                list.AddRange(ids.Select(t => new TP_JointPictureDistribution
                {
                    Id = IdHelper.Instance.Guid32,
                    PictureId = picture.Id,
                    QuestionGroupId = t
                }));
            }
            return _jointPictureDistributionRepository.Insert(list);
        }

        public void ReCommit()
        {
            var joints = TxtFileHelper.Joints();
            foreach (var joint in joints)
            {
                Console.WriteLine($"开始检查：{joint}");
                var model = _jointMarkingRepository.Load(joint);
                if (model == null)
                    continue;
                var batches = _usageRepository.Where(t => t.JointBatch == joint).Select(t => t.Id);
                //var studentIds = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingResult>>()
                //    .Where(t => batches.Contains(t.Batch))
                //    .Select(t => t.StudentID);
                var pictureIds = _pictureRepository.Where(
                    p => batches.Contains(p.BatchNo))
                    .Select(p => p.Id).ToList();
                if (!pictureIds.Any())
                    continue;
                Console.WriteLine($"共{pictureIds.Count}张试卷待处理");
                var helper = new CommitPictureTask(new CommitPictureParam
                {
                    PaperId = model.PaperId,
                    PictureIds = pictureIds,
                    JointBatch = joint
                }, Console.WriteLine);
                helper.Execute();
            }
            Console.WriteLine("End");
        }

        public void ChangeAnswers()
        {
            var list = "changeAnswers.txt".ReadConfig();
            var repository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingDetail>>();
            foreach (var item in list)
            {
                Console.WriteLine($"开始更改{item}");
                var arr = item.Split(',');
                if (arr.Length != 3)
                    continue;
                string qid = arr[0],
                    sid = arr[1],
                    answer = arr[2];
                if (string.IsNullOrWhiteSpace(qid) || string.IsNullOrWhiteSpace(answer))
                    continue;
                Expression<Func<TP_MarkingDetail, bool>> condition = d => d.QuestionID == qid;
                if (!string.IsNullOrWhiteSpace(sid))
                    condition = condition.And(d => d.SmallQID == sid);
                var details = repository.Where(condition).ToList();
                var updates = new List<TP_MarkingDetail>();
                foreach (var detail in details)
                {
                    if (string.IsNullOrWhiteSpace(detail.AnswerContent))
                        continue;
                    if (detail.AnswerContent == answer && !(detail.IsCorrect.HasValue && detail.IsCorrect.Value))
                    {
                        detail.IsCorrect = true;
                        detail.CurrentScore = detail.Score;
                        updates.Add(detail);
                    }
                    //else if (answer.Length > 1 && detail.AnswerContent.All(t => answer.Contains(t)))
                    //{
                    //    detail.IsCorrect = false;
                    //    detail.CurrentScore = Math.Round(detail.Score / 2M, 2);
                    //    updates.Add(detail);
                    //}
                    else if (detail.AnswerContent != answer && (detail.IsCorrect.HasValue && detail.IsCorrect.Value))
                    {
                        detail.IsCorrect = false;
                        detail.CurrentScore = 0;
                        updates.Add(detail);
                    }
                }
                Console.WriteLine($"更新{updates.Count}条记录");
                repository.Update(d => new
                {
                    d.IsCorrect,
                    d.CurrentScore
                }, updates.ToArray());
            }
            Console.WriteLine("End");
        }
    }
}
