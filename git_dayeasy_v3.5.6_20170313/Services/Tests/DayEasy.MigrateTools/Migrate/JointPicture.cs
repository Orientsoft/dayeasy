
using Autofac;
using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Marking.Services.Helper;
using DayEasy.Services;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DayEasy.MigrateTools.Migrate
{
    public class JointPicture : MigrateBase
    {
        private readonly ILogger _logger = LogManager.Logger<JointPicture>();
        private readonly IDayEasyRepository<TP_JointMarking, string> _jointMarkingRepository;
        private readonly IDayEasyRepository<TP_JointPictureDistribution, string> _jointPictureRepository;
        private readonly IDayEasyRepository<TP_JointQuestionGroup, string> _questionGroupRepository;
        private readonly IDayEasyRepository<TC_Usage, string> _usageRepository;
        private readonly IDayEasyRepository<TP_MarkingPicture, string> _pictureRepository;
        private readonly IMarkingContract _markingContract;
        private readonly IPaperContract _paperContract;

        public JointPicture()
        {
            _jointMarkingRepository = Container.Resolve<IDayEasyRepository<TP_JointMarking, string>>();
            _jointPictureRepository =
                Container.Resolve<IDayEasyRepository<TP_JointPictureDistribution, string>>();
            _usageRepository = Container.Resolve<IDayEasyRepository<TC_Usage, string>>();
            _pictureRepository = Container.Resolve<IDayEasyRepository<TP_MarkingPicture, string>>();
            _questionGroupRepository = Container.Resolve<IDayEasyRepository<TP_JointQuestionGroup, string>>();
            _markingContract = Container.Resolve<IMarkingContract>();
            _paperContract = Container.Resolve<IPaperContract>();
        }

        public void CheckPicture()
        {
            var jointBatches = TxtFileHelper.Joints();
            var joints =
                _jointMarkingRepository.Where(
                    t => t.Status == (byte)(JointStatus.Normal) && jointBatches.Contains(t.Id))
                    .Select(t => t.Id).ToList();
            Console.WriteLine("找到{0}条数据", joints.Count);
            if (!joints.Any())
            {
                return;
            }
            foreach (var joint in joints)
            {
                try
                {
                    var batch = joint;
                    var groups = _questionGroupRepository.Where(t => t.JointBatch == batch)
                        .GroupBy(t => t.SectionType).ToDictionary(k => k.Key, v => v.Select(t => t.Id).ToList());

                    var batches = _usageRepository.Where(t => t.JointBatch == batch)
                        .Select(t => t.Id);

                    var pictures = _pictureRepository.Where(t => batches.Contains(t.BatchNo))
                        .Select(t => new { t.Id, t.AnswerImgType, t.LastMarkingTime }).ToList();
                    Console.WriteLine("批次{0}，找到{1}条图片数据。", joint, pictures.Count);

                    var list = new List<TP_JointPictureDistribution>();

                    foreach (var picture in pictures)
                    {
                        var pictureId = picture.Id;
                        if (!picture.LastMarkingTime.HasValue)
                        {
                            Console.WriteLine("Commit Picture {0}", pictureId);
                            var result = _markingContract.Commit(pictureId);
                            Console.WriteLine(result);
                        }

                        if (_jointPictureRepository.Exists(t => t.PictureId == pictureId))
                            continue;
                        var type = (picture.AnswerImgType == 0 ? (byte)1 : picture.AnswerImgType);

                        var ids = groups[type];
                        list.AddRange(ids.Select(id => new TP_JointPictureDistribution
                        {
                            Id = IdHelper.Instance.Guid32,
                            PictureId = picture.Id,
                            QuestionGroupId = id
                        }).ToList());
                    }
                    _jointPictureRepository.Insert(list);
                    Console.WriteLine("成功生成[{0}]", batch);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("生成[{0}]异常,{1}", joint, ex.Message);
                    _logger.Error(ex.Message, ex);
                }
            }
            Console.WriteLine("完成");
        }

        /// <summary> 图片检测任务 </summary>
        public void PictureTask()
        {
            var jointBatches = TxtFileHelper.Joints();
            foreach (var joint in jointBatches)
            {
                var id = joint;
                Console.WriteLine("开始检测{0}的图片任务...", joint);
                var model = _jointMarkingRepository.Load(id);
                if (model == null)
                    continue;
                var batches = _usageRepository.Where(t => t.JointBatch == id)
                    .Select(t => t.Id).ToList();
                var paper = _paperContract.PaperDetailById(model.PaperId);
                var questionDict = paper.Data.PaperSections.GroupBy(t => t.PaperSectionType)
                    .ToDictionary(k => k.Key,
                        v =>
                            v.SelectMany(
                                t => t.Questions.Where(q => !q.Question.IsObjective).Select(q => q.Question.Id))
                                .ToList());
                var pictureDict = _pictureRepository.Where(t => batches.Contains(t.BatchNo))
                    .Select(t => new
                    {
                        type = (t.AnswerImgType == 0 ? 1 : t.AnswerImgType),
                        t.Id
                    })
                    .ToList()
                    .GroupBy(t => t.type)
                    .ToDictionary(k => (byte)k.Key, v => v.Select(t => t.Id).ToList());
                JointHelper.Instance(id).InitPictureTask(pictureDict, questionDict).Wait();
                Console.WriteLine("{0}的图片任务检测完成", joint);
            }
        }

        public void PictureSorts()
        {
            var start = DateTime.Parse("2016-11-06");
            var list = _pictureRepository.Where(t => t.AddedAt > start)
                .Join(_usageRepository.Table, s => s.BatchNo, d => d.Id, (s, d) => new { s, d })
                .GroupBy(t => new { t.d.JointBatch, t.s.AddedAt })
                .Select(t => new
                {
                    key = t.Key,
                    sorts = t.Select(d => d.s).ToList()
                }).ToList();
            int count = 0;
            //var regex = new Regex("(\\d{4}).jpg", RegexOptions.IgnoreCase);
            foreach (var item in list)
            {
                Console.WriteLine(JsonHelper.ToJson(item.key));
                var models = new List<TP_MarkingPicture>();
                var index = 0;
                foreach (var sort in item.sorts.OrderBy(t => t.AnswerImgUrl))
                {
                    sort.SubmitSort = index;
                    models.Add(sort);
                    index++;
                }
                _pictureRepository.Update(p => new
                {
                    p.SubmitSort
                }, models.ToArray());
                count += models.Count;
                Console.WriteLine(count);
            }
            Console.WriteLine("End");
        }
    }
}
