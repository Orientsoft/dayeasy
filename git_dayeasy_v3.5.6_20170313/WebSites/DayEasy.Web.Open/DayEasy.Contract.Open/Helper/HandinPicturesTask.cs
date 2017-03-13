
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Contracts;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Services;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;

namespace DayEasy.Contract.Open.Helper
{
    /// <summary> 扫描提交异步线程任务 </summary>
    public class HandinPicturesTask
    {
        private readonly ILogger _logger = LogManager.Logger<HandinPicturesTask>();
        private readonly IDayEasyRepository<TP_JointMarking> _jointMarkingRepository;
        private readonly IDayEasyRepository<TP_JointQuestionGroup> _questionGroupRepository;
        private readonly IDayEasyRepository<TP_MarkingPicture> _markingPictureRepository;
        private readonly IDayEasyRepository<TP_JointPictureDistribution> _jointPictureDistributionRepository;

        private HandinPicturesTask()
        {
            _jointMarkingRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_JointMarking>>();
            _questionGroupRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_JointQuestionGroup>>();
            _markingPictureRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingPicture>>();
            _jointPictureDistributionRepository =
                CurrentIocManager.Resolve<IDayEasyRepository<TP_JointPictureDistribution>>();
        }

        public static HandinPicturesTask Instance
        {
            get { return new HandinPicturesTask(); }
        }

        public void HandinTask(List<string> pictureIds, string jointBatch = null)
        {
            if (pictureIds == null || !pictureIds.Any())
                return;
            var sb = new StringBuilder();
            try
            {
                var watch = new Stopwatch();
                watch.Start();
                sb.AppendLine("上传试卷异步任务开始...,共{0}张试卷".FormatWith(pictureIds.Count));
                //1.初始化Detail
                const int size = 10;
                var tasks = new List<Task>();
                var taskCount = (int) Math.Ceiling(pictureIds.Count/(float) size);
                for (var i = 0; i < taskCount; i++)
                {
                    var index = i;
                    tasks.Add(Task.Run(() =>
                    {
                        var contract = CurrentIocManager.Resolve<IMarkingContract>();
                        var list = pictureIds.Skip(index*size).Take(size);
                        foreach (var id in list)
                        {
                            contract.Commit(id);
                        }
                    }));
                }
                Task.WaitAll(tasks.ToArray());
                //                foreach (var pictureId in pictureIds)
                //                {
                //                    _markingContract.Commit(pictureId);
                //                }
                watch.Stop();
                sb.AppendLine("完成Detail初始化,耗时:{0}ms".FormatWith(watch.ElapsedMilliseconds));
                //2.协同阅卷，初始化PictureDistribution
                if (string.IsNullOrWhiteSpace(jointBatch))
                    return;
                watch.Restart();
                sb.AppendLine("开始初始化协同区域...");
                var count = InitJointPictures(jointBatch, pictureIds);
                watch.Stop();
                sb.AppendLine("完成{0}个协同区域初始化,耗时:{1}ms".FormatWith(count, watch.ElapsedMilliseconds));
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
            finally
            {
                _logger.Info(sb.ToString());
            }
        }

        /// <summary> 初始化协同试卷分配区域 </summary>
        /// <param name="jointBatch"></param>
        /// <param name="pictureIds"></param>
        public int InitJointPictures(string jointBatch, List<string> pictureIds)
        {
            if (string.IsNullOrWhiteSpace(jointBatch) || pictureIds == null || !pictureIds.Any())
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
            foreach (var pictureId in pictureIds)
            {
                var picture = _markingPictureRepository.Load(pictureId);
                if (picture == null)
                    continue;
                //普通卷/A卷
                var type = (byte)(picture.AnswerImgType == 0 ? 1 : picture.AnswerImgType);
                if (!groups.ContainsKey(type))
                    continue;
                var ids = groups[type];
                list.AddRange(ids.Select(t => new TP_JointPictureDistribution
                {
                    Id = IdHelper.Instance.Guid32,
                    PictureId = pictureId,
                    QuestionGroupId = t
                }));
            }
            return _jointPictureDistributionRepository.Insert(list);
        }
    }
}
