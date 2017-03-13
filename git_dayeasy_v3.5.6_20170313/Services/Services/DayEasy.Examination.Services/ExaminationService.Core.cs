using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Examination;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain;
using DayEasy.Services;
using DayEasy.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.Examination.Services
{
    public partial class ExaminationService
    {
        #region 注入

        public IDayEasyRepository<TP_Paper> PaperRepository { private get; set; }
        public IDayEasyRepository<TC_Usage> UsageRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingResult> MarkingResultRepository { private get; set; }

        public IUserContract UserContract { private get; set; }
        public IGroupContract GroupContract { private get; set; }

        #endregion

        private DResults<ExamSubjectDto> ConvertToJointStatisticDto(Expression<Func<TP_JointMarking, bool>> condition,
            DPage page = null, int subjectId = -1, JointStatus status = JointStatus.Finished, ICollection<string> classList = null)
        {
            var jointList = new List<ExamSubjectDto>();
            var list = JointMarkingRepository.Where(condition)
                .Join(PaperRepository.Table, j => j.PaperId, p => p.Id, (j, p) => new
                {
                    JointBatch = j.Id,
                    PaperId = p.Id,
                    p.PaperType,
                    p.PaperTitle,
                    p.PaperNo,
                    p.SubjectID,
                    j.FinishedTime,
                    j.AddedBy,
                    j.GroupId,
                    j.AddedAt,
                    j.PaperACount,
                    j.PaperBCount
                });
            if (subjectId > 0)
            {
                list = list.Where(t => t.SubjectID == subjectId);
            }
            var count = list.Count();
            if (page == null)
            {
                list = list.OrderBy(t => t.SubjectID);
            }
            else
            {
                list = list.OrderByDescending(t => t.AddedAt)
                    .Skip(page.Page * page.Size)
                    .Take(page.Size);
            }
            if (!list.Any())
                return DResult.Succ(jointList, count);

            var userIds = list.Select(t => t.AddedBy).Distinct().ToList();
            var userDict = UserContract.LoadListDictUser(userIds);

            if (status == JointStatus.Finished)
            {
                //发布信息
                var batches = list.Select(t => t.JointBatch).ToList();
                var models = UsageRepository.Where(t => batches.Contains(t.JointBatch));
                if (classList != null)
                    models = models.Where(t => classList.Contains(t.ClassId));
                var usageDict = models.Select(t => new { t.JointBatch, t.Id, t.ClassId })
                    .GroupBy(t => t.JointBatch)
                    .ToDictionary(k => k.Key, v => new
                    {
                        batches = v.Select(t => new
                        {
                            t.Id,
                            t.ClassId
                        })
                    });
                var batchList = usageDict.Values.SelectMany(t => t.batches.Select(v => v.Id)).Distinct().ToList();

                var classIds = usageDict.Values.SelectMany(t => t.batches.Select(v => v.ClassId)).Distinct().ToList();
                classIds = classIds.Union(list.Select(t => t.GroupId).Distinct().ToList()).ToList();
                //每个批次下有多少学生
                var studentDict = MarkingResultRepository.Where(t => batchList.Contains(t.Batch))
                    .Select(t => t.Batch).ToList()
                    .GroupBy(t => t)
                    .ToDictionary(k => k.Key, v => v.Count());

                var groupDict = GroupContract.GroupDtoDict(classIds);
                list.ToList().ForEach(t =>
                {
                    var item = new ExamSubjectDto
                    {
                        JointBatch = t.JointBatch,
                        CreationTime = t.AddedAt,
                        FinishedTime = t.FinishedTime ?? DateTime.MinValue,
                        PaperId = t.PaperId,
                        PaperType = t.PaperType,
                        PaperTitle = t.PaperTitle,
                        PaperNo = t.PaperNo,
                        SubjectId = t.SubjectID,
                        Subject = SystemCache.Instance.SubjectName(t.SubjectID),
                        PaperACount=t.PaperACount,
                        PaperBCount=t.PaperBCount
                    };
                    if (usageDict.ContainsKey(item.JointBatch))
                    {
                        var dict = usageDict[item.JointBatch];
                        var ids = dict.batches.Select(b => b.Id);
                        item.StudentCount = studentDict.Where(s => ids.Contains(s.Key)).Sum(s => s.Value);
                        item.ClassCount = dict.batches.Count();
                        item.JointClasses = dict.batches.Select(c => new JointClass
                        {
                            ClassId = c.ClassId,
                            ClassName = groupDict.ContainsKey(c.ClassId) ? groupDict[c.ClassId].Name : string.Empty,
                            StudentCount = studentDict.ContainsKey(c.Id) ? studentDict[c.Id] : 0
                        }).OrderBy(c => c.ClassName.ClassIndex()).ToList();
                    }
                    if (userDict.ContainsKey(t.AddedBy))
                    {
                        item.Creator = userDict[t.AddedBy];
                    }
                    if (groupDict.ContainsKey(t.GroupId))
                    {
                        item.Group = groupDict[t.GroupId];
                    }
                    jointList.Add(item);
                });
                return DResult.Succ(jointList, count);
            }
            var colleagueDict = GroupContract.GroupDtoDict(list.Select(t => t.GroupId).Distinct().ToList());
            list.ToList().ForEach(t =>
            {
                var item = new ExamSubjectDto
                {
                    JointBatch = t.JointBatch,
                    CreationTime = t.AddedAt,
                    FinishedTime = t.FinishedTime ?? DateTime.MinValue,
                    PaperId = t.PaperId,
                    PaperType = t.PaperType,
                    PaperTitle = t.PaperTitle,
                    PaperNo = t.PaperNo,
                    SubjectId = t.SubjectID,
                    Subject = SystemCache.Instance.SubjectName(t.SubjectID),
                    PaperACount=t.PaperACount,
                    PaperBCount=t.PaperBCount
                };
                if (colleagueDict.ContainsKey(t.GroupId))
                {
                    item.Group = colleagueDict[t.GroupId];
                }
                if (userDict.ContainsKey(t.AddedBy))
                {
                    item.Creator = userDict[t.AddedBy];
                }
                jointList.Add(item);
            });
            return DResult.Succ(jointList, count);
        }
    }
}
