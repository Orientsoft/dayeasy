using DayEasy.Application.Services.Dto;
using DayEasy.Application.Services.Helper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Core.Domain;
using DayEasy.EntityFramework;
using DayEasy.Services;
using DayEasy.Utility;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.Application.Services.Services
{
    public class ApplicationService : DayEasyService, IApplicationContract
    {
        public IDayEasyRepository<TC_Usage> UsageRepository { private get; set; }
        public IDayEasyRepository<TP_Paper> PaperRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingPicture> MarkingPictureRepository { private get; set; }
        public IDayEasyRepository<TP_JointMarking> JointMarkingRepository { private get; set; }
        public IDayEasyRepository<TP_JointDistribution> JointDistributionRepository { private get; set; }

        public IVersion3Repository<TE_Examination> ExaminationRepository { private get; set; }
        public IVersion3Repository<TE_SubjectScore> SubjectScoreRepository { private get; set; }

        public IGroupContract GroupContract { private get; set; }
        public ApplicationService(IDbContextProvider<DayEasyDbContext> context)
            : base(context)
        {
        }

        public DResult<int> NewMarking(long userId)
        {
            var date = LastMarkingDateHelper.Get(userId);
            var models = UsageRepository.Where(
                t =>
                    t.SourceType == (byte)PublishType.Print && t.UserId == userId && t.AddedAt >= date
                    && (t.JointBatch == null || t.JointBatch == "") &&
                    t.MarkingStatus != (byte)MarkingStatus.AllFinished)
                .Count();
            //协同阅卷
            var joints = JointMarkingRepository
                .Where(t => t.Status == (byte)JointStatus.Normal && t.AddedAt >= date)
                .GroupJoin(JointDistributionRepository.Table, t => t.Id, d => d.JointBatch,
                    (j, d) => new { j, d })
                .Count(t => t.j.AddedBy == userId || (t.d.Any(d => d.TeacherId == userId)));
            return DResult.Succ(models + joints);
        }

        /// <summary> 待批阅列表 </summary>
        /// <param name="userId"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public DResults<VMarkingDto> MarkingList(long userId, DPage page)
        {
            var dtos = new List<VMarkingDto>();
            //普通阅卷
            var models =
                UsageRepository.Where(
                    t => t.Status == (byte)NormalStatus.Normal && t.SourceType == (byte)PublishType.Print
                    && t.UserId == userId && (t.JointBatch == null || t.JointBatch == "")
                    && t.MarkingStatus != (byte)MarkingStatus.AllFinished)
                    .Join(PaperRepository.Table, uc => uc.SourceID, p => p.Id, (t, p) =>
                        new
                        {
                            Batch = t.Id,
                            PaperId = t.SourceID,
                            p.PaperTitle,
                            p.PaperType,
                            Status = t.MarkingStatus,
                            Time = t.StartTime,
                            GroupId = t.ClassId,
                            CountA = 0,
                            CountB = 0,
                            IsOwner = true,
                            Alloted = true,
                            IsJoint = false
                        });
            //协同阅卷
            var joints = JointMarkingRepository.Where(t => t.Status == (byte)JointStatus.Normal)
                .GroupJoin(JointDistributionRepository.Table, t => t.Id, d => d.JointBatch,
                    (j, d) => new { j, d })
                .Where(t => t.j.AddedBy == userId || (t.d.Any(d => d.TeacherId == userId)))
                .Join(PaperRepository.Table, t => t.j.PaperId, p => p.Id, (t, p) =>
                    new
                    {
                        Batch = t.j.Id,
                        t.j.PaperId,
                        p.PaperTitle,
                        p.PaperType,
                        t.j.Status,
                        Time = t.j.AddedAt,
                        t.j.GroupId,
                        CountA = t.j.PaperACount,
                        CountB = t.j.PaperBCount,
                        IsOwner = (t.j.AddedBy == userId),
                        Alloted = (t.d.Any()),
                        IsJoint = true
                    });
            models = models.Union(joints);
            var total = models.Select(t => t.Batch).Count();
            var list = models.OrderByDescending(t => t.Time).Skip(page.Page * page.Size).Take(page.Size).ToList();
            if (!list.Any())
                return DResult.Succ(dtos, total);
            //获取非协同下试卷数量
            var batches = list.Where(t => !t.IsJoint).Select(t => t.Batch);
            var countDict = MarkingPictureRepository.Where(t => batches.Contains(t.BatchNo))
                .Select(t => new { t.BatchNo, t.AnswerImgType })
                .ToList()
                .GroupBy(t => t.BatchNo)
                .ToDictionary(k => k.Key,
                    v =>
                        new
                        {
                            countA =
                                v.Count(
                                    t =>
                                        t.AnswerImgType == (byte)MarkingPaperType.Normal ||
                                        t.AnswerImgType == (byte)MarkingPaperType.PaperA),
                            countB = v.Count(t => t.AnswerImgType == (byte)MarkingPaperType.PaperB)
                        });
            //获取圈子信息
            var groupIds = list.Select(t => t.GroupId).Distinct().ToList();
            var groups = GroupContract.GroupDtoDict(groupIds);
            //组装数据
            foreach (var model in list)
            {
                var item = new VMarkingDto
                {
                    Batch = model.Batch,
                    PaperId = model.PaperId,
                    PaperType = model.PaperType,
                    PaperTitle = model.PaperTitle,
                    Status = model.Status,
                    Time = model.Time,
                    IsJoint = model.IsJoint,
                    IsOwner = model.IsOwner,
                    Alloted = model.Alloted,
                    ACount = model.CountA,
                    BCount = model.CountB,
                    Group = new DGroupDto
                    {
                        Id = model.GroupId
                    }
                };
                if (groups.ContainsKey(model.GroupId))
                    item.Group = groups[model.GroupId];
                if (!model.IsJoint && countDict.ContainsKey(model.Batch))
                {
                    var count = countDict[model.Batch];
                    item.ACount = count.countA;
                    item.BCount = count.countB;
                }
                dtos.Add(item);
            }
            return DResult.Succ(dtos, total);
        }

        /// <summary> 班级报表 </summary>
        /// <param name="userId"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public DResults<VReportDto> ClassReports(long userId, DPage page)
        {
            var dtos = new List<VReportDto>();
            var models =
                UsageRepository.Where(
                    u => u.Status == (byte)NormalStatus.Normal && u.UserId == userId
                    && (u.SourceType == (byte)PublishType.Test || u.MarkingStatus == (byte)MarkingStatus.AllFinished))
                    .Join(PaperRepository.Table, uc => uc.SourceID, p => p.Id, (uc, p) => new
                    {
                        batch = uc.Id,
                        paperId = p.Id,
                        p.PaperType,
                        p.PaperTitle,
                        uc.JointBatch,
                        uc.ClassId,
                        uc.SourceType,
                        date = uc.AddedAt,
                    });
            var count = models.Count();
            var list = models.OrderByDescending(t => t.date).Skip(page.Page * page.Size).Take(page.Size).ToList();
            if (!list.Any())
                return DResult.Succ(dtos, 0);
            //获取圈子信息
            var groupIds = list.Select(t => t.ClassId).Distinct().ToList();
            var groups = GroupContract.GroupDtoDict(groupIds);
            foreach (var item in list)
            {
                if (!groups.ContainsKey(item.ClassId))
                    continue;
                var dto = new VReportDto
                {
                    Batch = item.batch,
                    Date = item.date.Date,
                    IsJoint = !string.IsNullOrWhiteSpace(item.JointBatch),
                    JointBatch = item.JointBatch,
                    PaperId = item.paperId,
                    PaperType = (PaperType)item.PaperType,
                    PaperTitle = item.PaperTitle,
                    PublishType = (PublishType)item.SourceType
                };
                dto.Group = groups[item.ClassId];
                dtos.Add(dto);
            }
            return DResult.Succ(dtos, count);
        }

        /// <summary> 年级报表 </summary>
        /// <param name="userId"></param>
        /// <param name="role"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public DResults<VGradeReportDto> GradeReports(long userId, int role, DPage page)
        {
            var dtos = new List<VGradeReportDto>();
            var memberRepository = CurrentIocManager.Resolve<IVersion3Repository<TG_Member>>();
            var groupRepository = CurrentIocManager.Resolve<IVersion3Repository<TG_Group>>();
            var dynamicRepository = CurrentIocManager.Resolve<IVersion3Repository<TM_GroupDynamic>>();
            var examinationRepository = CurrentIocManager.Resolve<IVersion3Repository<TE_Examination>>();
            var groups =
                memberRepository.Where(t => t.MemberId == userId && t.Status == (byte)NormalStatus.Normal)
                    .Join(groupRepository.Where(g => g.GroupType == (byte)GroupType.Class), m => m.GroupId, g => g.Id,
                        (m, g) => m.GroupId);
            if (!groups.Any())
                return DResult.Succ(dtos, 0);
            var models =
                dynamicRepository.Where(
                    t =>
                        t.Status == (byte)NormalStatus.Normal && t.DynamicType == (byte)GroupDynamicType.ExamNotice &&
                        groups.Contains(t.GroupId) && (t.ReceiveRole & role) > 0)
                    .GroupBy(t => t.ContentId)
                    .Select(t => new { ContentId = t.Key, AddedAt = t.Min(c => c.AddedAt) });
            if (!models.Any())
                return DResult.Succ(dtos, 0);
            var count = models.Count();
            dtos = models.Join(examinationRepository.Table, d => d.ContentId, e => e.Id, (d, e) =>
                new VGradeReportDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    IsUnion = (e.UnionBatch != null && e.UnionBatch != ""),
                    Time = d.AddedAt
                }).OrderByDescending(t => t.Time).Skip(page.Page * page.Size).Take(page.Size).ToList();
            return DResult.Succ(dtos, count);
        }

        public DResult<VUnionChartsDto> UnionCharts(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return DResult.Error<VUnionChartsDto>("大型考试不存在");
            var model =
                ExaminationRepository.Where(e => e.Id == id)
                    .Select(e => new { e.Id, e.Name, e.Status, e.UnionBatch })
                    .FirstOrDefault();
            if (model == null)
                return DResult.Error<VUnionChartsDto>("大型考试不存在");
            if (string.IsNullOrWhiteSpace(model.UnionBatch))
                return DResult.Error<VUnionChartsDto>("大型考试未关联报表");
            var subjects =
                SubjectScoreRepository.Where(t => t.ExamId == id)
                    .Select(t => new { t.SubjectId, t.Subject })
                    .OrderBy(t => t.SubjectId)
                    .ToList();

            var dto = new VUnionChartsDto
            {
                Id = id,
                Batch = model.UnionBatch,
                Title = model.Name,
                Subjects = subjects.ToDictionary(k => k.SubjectId, v => v.Subject)
            };
            return DResult.Succ(dto);
        }
    }
}
