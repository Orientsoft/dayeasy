
using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Core.Dependency;
using DayEasy.Models.Open.Work;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.Contract.Open.Services
{
    public partial class OpenService
    {
        public IGroupContract GroupContract { private get; set; }
        public IMarkingContract MarkingContract { private get; set; }

        public IDayEasyRepository<TC_Usage> UsageRepository { private get; set; }

        public IDayEasyRepository<TP_MarkingResult> MarkingResultRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingPicture> MarkingPictureRepository { private get; set; }

        /// <summary> 试卷协同记录 </summary>
        /// <param name="paperId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DResults<MJointUsageDto> JointUsages(string paperId, long userId = -1)
        {
            var result = MarkingContract.JointList(paperId, userId);
            if (result.Status)
                return DResult.Succ(result.Data.MapTo<List<MJointUsageDto>>(), result.TotalCount);
            return DResult.Errors<MJointUsageDto>(result.Message);
        }

        /// <summary> 
        /// 上传扫描图片 
        /// 1、检测班级任课老师
        /// 2、检查发布记录
        /// 3、发布新的记录
        /// 4、保存阅卷图片
        /// 5、初始化协同阅卷区域
        /// </summary>
        /// <param name="creator"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public DResults<MHandinResult> HandinPictures(long creator, MPictureList result)
        {
            if (result == null || result.Pictures == null || !result.Pictures.Any())
                return DResult.Errors<MHandinResult>("批阅图片不能为空！");
            if (result.UserId <= 0 || string.IsNullOrWhiteSpace(result.PaperId))
                return DResult.Errors<MHandinResult>("提交信息不完整！");
            if (result.Pictures.Any(p => string.IsNullOrWhiteSpace(p.GroupId)))
                return DResult.Errors<MHandinResult>("存在没有班级的试卷！");

            var list = new List<TP_MarkingPicture>();
            var results = new List<MHandinResult>();
            var publishList = new List<TC_Usage>();
            var updateList = new List<TC_Usage>();
            var idHelper = IdHelper.Instance;
            var now = Clock.Now;

            //根据班级ID和试卷类型分组
            var batchList = result.Pictures.GroupBy(t => new { t.GroupId, t.SectionType });

            foreach (var batch in batchList)
            {
                //根据班级ID和科目 确认AddedBy 以及未识别的试卷分班
                //获取相应班级、科目的任课教师ID
                //没有提交人ID时，直接查询圈子响应科目教师Id，用户辅助上传
                var teachers = GroupContract.GroupMembers(batch.Key.GroupId, UserRole.Teacher);
                if (!teachers.Status || teachers.Data == null)
                {
                    results.AddRange(batch.Select(b => new MHandinResult(b.Id, "班级没有任教老师")));
                    continue;
                }
                var teacher = teachers.Data.FirstOrDefault(t => t.SubjectId == result.SubjectId);
                if (teacher == null)
                {
                    results.AddRange(batch.Select(b => new MHandinResult(b.Id, "班级没有该科目任教老师！")));
                    continue;
                }

                var userId = teacher.Id;
                var sectionType = batch.Key.SectionType;

                var no = CheckPicturePublish(result.PaperId, batch.Key.GroupId, userId, sectionType, updateList,
                    result.JointBatch);

                //A,B卷同时上传
                var usageModel =
                    publishList.SingleOrDefault(u => u.ClassId == batch.Key.GroupId && u.SourceID == result.PaperId);
                if (usageModel != null)
                {
                    no = usageModel.Id;
                }

                //发布新的记录
                if (string.IsNullOrWhiteSpace(no))
                {
                    var printType = PrintType.HomeWork;
                    if (batch.Key.SectionType == (byte)PaperSectionType.PaperA)
                        printType = PrintType.PaperAHomeWork;
                    else if (batch.Key.SectionType == (byte)PaperSectionType.PaperB)
                        printType = PrintType.PaperBHomeWork;
                    no = idHelper.Guid32;
                    var usage = new TC_Usage
                    {
                        Id = no,
                        AddedAt = now,
                        SourceID = result.PaperId,
                        AddedIP = Utils.GetRealIp(),
                        ApplyType = (byte)ApplyType.Print,
                        PrintType = (byte)printType,
                        ClassId = batch.Key.GroupId,
                        ExpireTime = now,
                        IsControlOrder = false,
                        SourceType = (byte)PublishType.Print,
                        StartTime = now,
                        SubjectId = result.SubjectId,
                        UserId = userId,
                        MarkingStatus = (byte)MarkingStatus.NotMarking,
                        Status = (byte)NormalStatus.Normal
                    };
                    if (!string.IsNullOrWhiteSpace(result.JointBatch))
                    {
                        usage.ColleagueGroupId = result.GroupId;
                        usage.JointBatch = result.JointBatch;
                    }
                    publishList.Add(usage);
                }
                foreach (var picture in batch.ToList())
                {
                    var item = ParseToMarkingPicture(result.UserId, result.PaperId, picture, now, no);
                    list.Add(item);
                    results.Add(new MHandinResult(picture.Id));
                }
            }
            if (!list.Any())
                return DResult.Succ(results, results.Count);
            var publishResult = PublishPrintUsage(updateList, publishList, list, result.JointBatch);

            if (!publishResult.Status)
                return DResult.Errors<MHandinResult>(publishResult.Message);

            //开始异步任务
            var pictureIds = list.Select(t => t.Id).ToList();
            var jointBatch = result.JointBatch;

            MarkingContract.CommitPictureAsync(creator, result.PaperId, pictureIds, jointBatch);
            return DResult.Succ(results, results.Count);
        }

        public DResults<MPrintInfo> PrintList(string paperId, long teacherId = -1)
        {
            if (string.IsNullOrWhiteSpace(paperId))
                return DResult.Errors<MPrintInfo>("没有试卷信息！");
            var usages =
                UsageRepository.Where(
                    t =>
                        (teacherId == -1 || t.UserId == teacherId) && t.SourceType == (byte)PublishType.Print &&
                        t.SourceID == paperId && t.Status != (byte)NormalStatus.Delete)
                    .OrderByDescending(t => t.StartTime);
            var list = usages.Select(u => new MPrintInfo
            {
                Batch = u.Id,
                GroupId = u.ClassId,
                MarkingStatus = u.MarkingStatus
            }).Take(30).ToList();
            var groupIds = list.Select(t => t.GroupId).Distinct().ToList();
            var groups = GroupContract.SearchGroups(groupIds);
            list.ForEach(t =>
            {
                var group = groups.Data.FirstOrDefault(g => g.Id == t.GroupId);
                if (group != null)
                {
                    t.GroupName = group.Name;
                    t.AgencyName = group.AgencyName;
                }
                var times =
                    MarkingResultRepository.Where(mr => mr.Batch == t.Batch).Select(mr => mr.MarkingTime).ToList();
                t.MarkingCount = times.Count();
                t.MarkingTime = (times.Max() ?? DateTime.Now).ToLong();
            });
            return DResult.Succ(list, list.Count);
        }

        public DResults<MPrintDetail> PrintDetails(string batch, byte sectionType, int skip = 0, int size = 100)
        {
            Expression<Func<TP_MarkingPicture, bool>> condition =
                t => t.BatchNo == batch && t.AnswerImgType == sectionType;
            var pictures = MarkingPictureRepository.Where(condition)
                .OrderBy(t => t.AddedAt).ThenBy(t => t.SubmitSort)
                .Skip(skip)
                .Take(size);
            var count = MarkingPictureRepository.Count(condition);
            if (!pictures.Any())
                return DResult.Succ(new List<MPrintDetail>(), 0);
            var list = ParsePictures(pictures, batch, sectionType);
            return DResult.Succ(list, count);
        }

        public DResults<MJointPrintInfo> JointPrintList(string paperId, long teacherId = -1)
        {
            var joints =
                JointMarkingRepository.Where(t => t.PaperId == paperId && t.Status != (byte)JointStatus.Delete);
            //权限
            if (teacherId > 0)
            {
                joints = joints.Where(t => t.AddedBy == teacherId);
            }
            if (!joints.Any())
                return DResult.Succ(new List<MJointPrintInfo>(), 0);
            var list = joints.Select(t => new MJointPrintInfo
            {
                Batch = t.Id,
                GroupId = t.GroupId,
                MarkingStatus = t.Status,
                MarkingDate = t.FinishedTime ?? DateTime.MinValue,
                ACount = t.PaperACount,
                BCount = t.PaperBCount,
                TeacherId = t.AddedBy
            }).OrderByDescending(t => t.MarkingDate).ToList();
            var groupIds = list.Select(t => t.GroupId).Distinct().ToList();
            var groups = GroupContract.SearchGroups(groupIds);
            var userIds = list.Select(t => t.TeacherId).Distinct().ToList();
            var userDict = UserContract.LoadListDictUser(userIds);
            foreach (var info in list)
            {
                var group = groups.Data.FirstOrDefault(g => g.Id == info.GroupId);
                if (group != null)
                {
                    info.GroupName = group.Name;
                    info.AgencyName = group.AgencyName;
                }
                if (userDict.ContainsKey(info.TeacherId))
                    info.TeacherName = userDict[info.TeacherId].Name;
                info.MarkingTime = info.MarkingDate.ToLong();
            }
            return DResult.Succ(list, -1);
        }

        public DResult<Dictionary<string, string>> JointAgencies(string joint)
        {
            var agencyDict = new Dictionary<string, string>();
            var classIds = UsageRepository.Where(t => t.JointBatch == joint)
                .Select(t => t.ClassId).Distinct().ToList();
            var groupResult = GroupContract.SearchGroups(classIds);
            if (!groupResult.Status || groupResult.Data == null || !groupResult.Data.Any())
                return DResult.Succ(agencyDict);
            foreach (var dto in groupResult.Data)
            {
                var cls = dto as ClassGroupDto;
                if (cls == null || string.IsNullOrWhiteSpace(cls.AgencyId) || agencyDict.ContainsKey(cls.AgencyId))
                    continue;
                agencyDict.Add(cls.AgencyId, cls.AgencyName);
            }
            return DResult.Succ(agencyDict);
        }

        public DResults<MPrintDetail> JointPrintDetails(string joint, byte sectionType, int skip = 0, int size = 50, string agencyId = null)
        {
            var usages = UsageRepository.Where(t => t.JointBatch == joint);
            if (!string.IsNullOrWhiteSpace(agencyId))
            {
                var classRepository = CurrentIocManager.Resolve<IVersion3Repository<TG_Class>>();
                var classIds = classRepository.Where(t => t.AgencyId == agencyId).Select(t => t.Id).ToList();
                usages = usages.Where(u => classIds.Contains(u.ClassId));
            }
            var batches = usages.Select(t => t.Id);
            Expression<Func<TP_MarkingPicture, bool>> condition =
                t => batches.Contains(t.BatchNo) && t.AnswerImgType == sectionType;
            var count = MarkingPictureRepository.Count(condition);
            var pictures =
                MarkingPictureRepository.Where(condition)
                    .OrderBy(t => t.AddedAt)
                    .ThenBy(t => t.SubmitSort)
                    .Skip(skip)
                    .Take(size);
            if (!pictures.Any())
                return DResult.Succ(new List<MPrintDetail>(), 0);
            var list = ParsePictures(pictures, joint, sectionType, true);
            return DResult.Succ(list, count);
        }

        private IEnumerable<MPrintDetail> ParsePictures(IQueryable<TP_MarkingPicture> pictures, string batch,
            byte sectionType, bool isJoint = false)
        {
            var classIds = pictures.Select(t => t.ClassID).Distinct().ToList();
            var classDto =
                GroupContract.SearchGroups(classIds)
                    .Data.Select(t => t as ClassGroupDto)
                    .ToDictionary(k => k.Id, v => new { v.Name, v.AgencyId, v.AgencyName });
            var list = pictures.Select(t => new MPrintDetail
            {
                Id = t.Id,
                StudentId = t.StudentID,
                StudentName = t.StudentName,
                ImagePath = t.AnswerImgUrl,
                ClassId = t.ClassID,
                Index = t.SubmitSort,
                IsSingle = t.IsSingleFace ?? false,
                PageCount = t.TotalPageNum,
                SectionType = t.AnswerImgType,
                Markings = t.RightAndWrong,
                Marks = t.Marks,
                ObjectiveErrorInfo = t.ObjectiveError,
                ObjectiveScore = t.ObjectiveScore
            }).ToList();

            var scores = StudentScores(batch, sectionType, isJoint);

            var i = 1;
            list.ForEach(t =>
            {
                t.Index = i++;
                if (classDto.ContainsKey(t.ClassId))
                {
                    var item = classDto[t.ClassId];
                    t.ClassName = item.Name;
                    t.AgencyId = item.AgencyId;
                    t.Agency = item.AgencyName;
                }
                if (!string.IsNullOrWhiteSpace(t.Markings))
                {
                    var symbols = JsonHelper.JsonList<SymbolTag>(t.Markings.Replace("'", "\""));
                    t.SymbolInfos = symbols.Select(s => new SymbolInfo
                    {
                        Point = s.Point,
                        SymbolType = s.T,
                        Score = s.W.CastTo(0F)
                    }).ToList();
                }
                if (!string.IsNullOrWhiteSpace(t.Marks))
                {
                    var marks = JsonHelper.JsonList<SymbolTag>(t.Marks.Replace("'", "\""));
                    t.CommentInfos = marks.Where(m => m.T != (int)SymbolType.Objective)
                        .Select(m => new CommentInfo
                        {
                            Point = m.Point,
                            Words = m.W,
                            SymbolType = m.T
                        }).ToList();
                }
                // A卷、B卷分数
                if (scores.ContainsKey(t.StudentId))
                    t.Score = scores[t.StudentId];
            });
            return list;
        }
    }
}
