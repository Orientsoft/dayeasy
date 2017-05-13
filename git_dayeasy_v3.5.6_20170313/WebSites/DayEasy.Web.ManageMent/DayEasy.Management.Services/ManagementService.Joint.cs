using DayEasy.AsyncMission;
using DayEasy.AsyncMission.Models;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Core.Domain;
using DayEasy.Management.Services.Helper;
using DayEasy.Office;
using DayEasy.Services;
using DayEasy.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using DayEasy.Contracts.Dtos.Marking.Joint;

namespace DayEasy.Management.Services
{
    public partial class ManagementService
    {
        public IDayEasyRepository<TP_JointMarking> JointMarkingRepository { private get; set; }
        public IDayEasyRepository<TC_Usage> UsageRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingPicture> MarkingPictureRepository { private get; set; }
        public IVersion3Repository<TM_GroupDynamic> GroupDynamicRepository { private get; set; }
        public IDayEasyRepository<TP_JointException> JointExceptionRepository { private get; set; }
        public IUserContract UserContract { private get; set; }
        public IStatisticContract StatisticContract { private get; set; }

        public DResults<JointMarkingDto> JointList(JointSearchDto searchDto)
        {
            var dtos = new List<JointMarkingDto>();

            var list = JointMarkingRepository.Table
                .Join(PaperRepository.Table, j => j.PaperId, p => p.Id,
                    (j, p) =>
                        new { j.Id, j.GroupId, j.AddedAt, j.AddedBy, j.Status, j.PaperACount, j.PaperBCount, j.PaperId, j.FinishedTime, p.SubjectID, p.PaperTitle, p.PaperNo });
            if (searchDto.Status >= 0)
                list = list.Where(t => t.Status == searchDto.Status);
            if (searchDto.SubjectId > 0)
                list = list.Where(t => t.SubjectID == searchDto.SubjectId);
            if (!string.IsNullOrWhiteSpace(searchDto.AgencyId))
            {
                var colleagues = ColleagueRepository.Where(t => t.AgencyId == searchDto.AgencyId)
                    .Select(t => t.Id).ToList();
                list = list.Where(t => colleagues.Contains(t.GroupId));
            }
            if (!string.IsNullOrWhiteSpace(searchDto.Keyword))
            {
                list = list.Where(t => t.PaperTitle.Contains(searchDto.Keyword) || t.PaperNo == searchDto.Keyword);
            }
            var count = list.Count();
            var models = list.OrderByDescending(t => t.AddedAt)
                .Skip(searchDto.Page * searchDto.Size)
                .Take(searchDto.Size).ToList();
            if (!models.Any())
                return DResult.Succ(dtos, count);

            var userIds = models.Select(t => t.AddedBy).Distinct();
            var userDict =
                UserRepository.Where(u => userIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.HeadPhoto, u.UserCode, u.TrueName })
                    .ToList()
                    .ToDictionary(k => k.Id, v => new UserDto
                    {
                        Id = v.Id,
                        Name = v.TrueName,
                        Avatar = v.HeadPhoto,
                        Code = v.UserCode
                    });
            var groupIds = models.Select(t => t.GroupId).Distinct().ToList();
            var groups = GroupContract.GroupDtoDict(groupIds);
            var subjects = SystemCache.Instance.Subjects();
            var batches = models.Select(t => t.Id).ToList();
            var countDict = JointExceptionCount(batches);
            foreach (var item in models)
            {
                var dto = new JointMarkingDto
                {
                    Id = item.Id,
                    Status = item.Status,
                    PaperACount = item.PaperACount,
                    PaperBCount = item.PaperBCount,
                    SubjectId = item.SubjectID,
                    PaperId = item.PaperId,
                    PaperNo = item.PaperNo,
                    PaperTitle = item.PaperTitle,
                    AddedAt = item.AddedAt,
                    FinishedTime = item.FinishedTime
                };
                if (countDict.ContainsKey(dto.Id))
                    dto.ExceptionCount = countDict[dto.Id];
                if (userDict.ContainsKey(item.AddedBy))
                    dto.User = userDict[item.AddedBy];
                if (subjects.ContainsKey(item.SubjectID))
                    dto.SubjectName = subjects[item.SubjectID];
                if (groups.ContainsKey(item.GroupId))
                    dto.Group = groups[item.GroupId];
                dtos.Add(dto);
            }
            return DResult.Succ(dtos, count);
        }

        public DResult JointRecall(string jointBatch)
        {
            if (jointBatch.IsNullOrEmpty()) return DResult.Error("协同批次号不能为空");
            var joint = JointMarkingRepository.Load(jointBatch);
            if (joint == null) return DResult.Error("没有查询到协同记录");
            var result = 1;
            if (joint.Status != (byte)NormalStatus.Delete)
            {
                result = UnitOfWork.Transaction(() =>
                {
                    UsageRepository.Update(
                        new TC_Usage { Status = (byte)NormalStatus.Delete },
                        u => u.JointBatch == jointBatch, "Status");

                    joint.Status = (byte)NormalStatus.Delete;
                    JointMarkingRepository.Update(j => new { j.Status }, joint);
                });
            }
            if (result <= 0)
                return DResult.Error("操作失败，请重试");
            DynamicTasks.Delete(jointBatch);
            return DResult.Success;
        }

        public DResult<JointUnSubmitDto> JointUnsubmits(string jointBatch)
        {
            if (jointBatch.IsNullOrEmpty()) return DResult.Error<JointUnSubmitDto>("协同批次号不能为空");
            var joint = JointMarkingRepository.Load(jointBatch);
            if (joint == null) return DResult.Error<JointUnSubmitDto>("没有查询到协同记录");
            var paper = PaperRepository.FirstOrDefault(p => p.Id == joint.PaperId);
            if (paper == null) return DResult.Error<JointUnSubmitDto>("没有查询到试卷资料");
            var groupIds = UsageRepository.Where(u => u.JointBatch == jointBatch)
                .Select(u => u.ClassId).ToList();
            if (!groupIds.Any()) return DResult.Error<JointUnSubmitDto>("没有查询到参与协同考试的班级资料");
            var groups = GroupRepository.Where(g => groupIds.Contains(g.Id))
                .ToDictionary(g => g.Id, g => g.GroupName);
            if (!groups.Any()) return DResult.Error<JointUnSubmitDto>("没有查询到参与协同考试的班级资料");

            var result = new JointUnSubmitDto
            {
                PaperId = paper.Id,
                PaperTitle = paper.PaperTitle,
                IsAb = paper.PaperType == (byte)PaperType.AB,
                UnSubmits = groups.Select(g => new JointUnsGroup
                {
                    GroupId = g.Key,
                    GroupName = g.Value,
                    UnsA = new List<NameDto<long, string>>(),
                    UnsB = new List<NameDto<long, string>>()
                }).ToList()
            };

            //各班考试批次号
            var batchs = UsageRepository.Where(u => u.JointBatch == jointBatch).Select(u => u.Id);

            //已提交的学生
            List<long> submitsB = new List<long>();
            Expression<Func<TP_MarkingPicture, bool>> conditionMpa = p => batchs.Contains(p.BatchNo);
            if (result.IsAb)
            {
                conditionMpa = conditionMpa.And(p => p.AnswerImgType == (byte)MarkingPaperType.PaperA);
                submitsB = MarkingPictureRepository
                    .Where(p => batchs.Contains(p.BatchNo) && p.AnswerImgType == (byte)MarkingPaperType.PaperB)
                    .Select(p => p.StudentID).ToList();
            }
            else
            {
                conditionMpa = conditionMpa.And(p => p.AnswerImgType == (byte)MarkingPaperType.Normal);
            }
            var submitsA = MarkingPictureRepository.Where(conditionMpa)
                .Select(p => p.StudentID).ToList();

            GetJointUnsGroup(result.UnSubmits, submitsA, submitsB);

            return DResult.Succ(result);
        }

        internal void GetJointUnsGroup(List<JointUnsGroup> groups, List<long> submitsA, List<long> submitsB)
        {
            var groupIds = groups.Select(g => g.GroupId).ToList();
            var list = MemberRepository.Where(m =>
                m.Status == (byte)NormalStatus.Normal &&
                (m.MemberRole & (byte)UserRole.Student) > 0 &&
                groupIds.Contains(m.GroupId))
                .Select(m => new { m.MemberId, m.GroupId }).ToList();
            if (!list.Any()) return;

            var unsAIds = list.Where(m => !submitsA.Contains(m.MemberId)).Select(m => m.MemberId).ToList();
            var unsA = UserRepository.Where(u => unsAIds.Contains(u.Id)).Select(u => new { u.Id, u.TrueName }).ToList();

            var unsBIds = list.Where(m => !submitsB.Contains(m.MemberId)).Select(m => m.MemberId).ToList();
            var unsB = UserRepository.Where(u => unsBIds.Contains(u.Id)).Select(u => new { u.Id, u.TrueName }).ToList();

            groups.ForEach(g =>
            {
                g.UnsA =
                    (from i in list
                     from u in unsA
                     where i.MemberId == u.Id
                           && i.GroupId == g.GroupId
                     select
                         new NameDto<long, string>(u.Id, u.TrueName)
                        ).ToList();
                g.UnsB =
                    (from i in list
                     from u in unsB
                     where i.MemberId == u.Id
                           && i.GroupId == g.GroupId
                     select
                         new NameDto<long, string>(u.Id, u.TrueName)
                        ).ToList();
            });
        }

        public List<DKeyValue> JointPictures(string jointBatch, byte type)
        {
            var pictures = new List<DKeyValue>();
            if (string.IsNullOrWhiteSpace(jointBatch))
                return pictures;
            var batches = UsageRepository.Where(t => t.JointBatch == jointBatch);
            pictures = MarkingPictureRepository.Where(
                p => (type == 1 && p.AnswerImgType == 0) || p.AnswerImgType == type)
                .Join(batches, mp => mp.BatchNo, uc => uc.Id,
                    (mp, uc) => new { mp.StudentName, mp.AnswerImgUrl, mp.AddedAt, mp.SubmitSort })
                .OrderBy(t => t.AddedAt).ThenBy(t => t.SubmitSort)
                .ToList()
                .Select(t => new DKeyValue(t.StudentName, t.AnswerImgUrl))
                .ToList();
            return pictures;
        }

        public DKeyValue Picture(string id)
        {
            var item = MarkingPictureRepository.Load(id);
            return item == null ? null : new DKeyValue(item.StudentName, item.AnswerImgUrl);
        }

        public Dictionary<string, int> JointExceptionCount(ICollection<string> jointBatchs)
        {
            return JointExceptionRepository.Where(
                t => jointBatchs.Contains(t.JointBatch) && t.Status == (byte)NormalStatus.Normal)
                .Select(t => t.JointBatch)
                .GroupBy(t => t)
                .ToList()
                .ToDictionary(k => k.Key, v => v.Count());
        }

        public DResults<JointExceptionDto> JointExceptions(string jointBatch, int status = -1, DPage page = null, byte? type = null)
        {
            Expression<Func<TP_JointException, bool>> condition = t => t.JointBatch == jointBatch;
            if (status >= 0)
                condition = condition.And(t => t.Status == status);
            if (type.HasValue)
                condition = condition.And(t => t.ExceptionType == type.Value);
            if (page == null)
                page = DPage.NewPage();
            int count = JointExceptionRepository.Count(condition);
            var list = JointExceptionRepository.Where(condition)
                .Select(t => new JointExceptionDto
                {
                    Id = t.Id,
                    CreationTime = t.AddedAt,
                    Status = t.Status,
                    Message = t.Message,
                    PictureId = t.PictureId,
                    SolveTime = t.SolveTime,
                    TeacherId = t.UserId,
                    Student = t.StudentName,
                    ExceptionType = t.ExceptionType
                })
                .OrderByDescending(t => t.CreationTime)
                .Skip(page.Page * page.Size)
                .Take(page.Size)
                .ToList();
            var ids = list.Select(t => t.TeacherId).Distinct().ToList();
            var users = UserRepository.Where(u => ids.Contains(u.Id))
                .Select(u => new { u.Id, u.TrueName, u.UserCode })
                .ToList()
                .ToDictionary(k => k.Id, v => new
                {
                    v.TrueName,
                    v.UserCode
                });

            var types = Enum.GetValues(typeof(MarkingExceptionType)).Cast<byte>().ToList();

            list.ForEach(ut =>
            {
                if (ut.ExceptionType.HasValue && types.Contains(ut.ExceptionType.Value))
                    ut.ExceptionTypeTitle = ((MarkingExceptionType)ut.ExceptionType).GetText();

                if (!users.ContainsKey(ut.TeacherId))
                    return;
                var user = users[ut.TeacherId];
                ut.Teacher = user.TrueName;
                ut.TeacherCode = user.UserCode;
            });
            return DResult.Succ(list, count);
        }

        public DResult SolveJointException(string id)
        {
            return CurrentIocManager.Resolve<IMarkingContract>().SolveException(id);
            //            var ex = JointExceptionRepository.Load(id);
            //            if (ex == null)
            //                return DResult.Error("异常已取消！");
            //            if (ex.Status == (byte)NormalStatus.Delete)
            //                return DResult.Error("异常已解决！");
            //            ex.Status = (byte)NormalStatus.Delete;
            //            ex.SolveTime = Clock.Now;
            //            var result = JointExceptionRepository.Update(t => new
            //            {
            //                t.Status,
            //                t.SolveTime
            //            }, ex);
            //            return DResult.FromResult(result);
        }

        public DResult ResetJoint(string jointBatch, long userId)
        {
            if (string.IsNullOrWhiteSpace(jointBatch) || userId <= 0)
                return DResult.Error("参数异常");
            var model = JointMarkingRepository.Load(jointBatch);
            if (model == null || model.Status != (byte)JointStatus.Finished || !model.FinishedTime.HasValue)
                return DResult.Error("协同状态异常！");
            if (model.FinishedTime.Value < Clock.Now.AddDays(-10))
                return DResult.Error("协同已超期，不能重置！");
            return MissionHelper.PushMission(MissionType.ResetJoint, new ResetJointParam
            {
                JointBatch = jointBatch
            }, userId);
        }

        public void ExportJoint(string jointBatch)
        {
            var model = JointMarkingRepository.Load(jointBatch);
            if (model == null || model.Status != (byte)JointStatus.Finished)
                return;
            var batches =
                UsageRepository.Where(t => t.JointBatch == jointBatch).Select(t => new { t.Id, t.ClassId }).ToList();
            if (!batches.Any())
                return;

            var dataSet = new DataSet("协同考试情况统计表_{0}".FormatWith(Clock.Now.ToString("yyyyMMddHHmmss")));
            DataTable rankTable = null;
            DataTable scoresTable = null;
            DataTable knowledgeTable = null;
            foreach (var batch in batches)
            {
                var questionScores = StatisticContract.QuestionScores(batch.Id);
                scoresTable = ExportHelper.ScoresTable(questionScores.Data, scoresTable);
                if (knowledgeTable == null)
                    knowledgeTable =
                        ExportHelper.KnowledgeTable(model.PaperId, questionScores.Data.QuestionSorts);

                var students = GroupContract.GroupMembers(batch.ClassId, UserRole.Student)
                    .Data.ToDictionary(k => k.Id, v => v);
                rankTable = ExportHelper.RankTable(batch.Id, students, rankTable);
            }
            if (rankTable != null)
                dataSet.Tables.Add(rankTable);
            if (scoresTable != null)
                dataSet.Tables.Add(scoresTable);
            if (knowledgeTable != null)
                dataSet.Tables.Add(knowledgeTable);
            ExcelHelper.Export(dataSet, dataSet.DataSetName);
        }

        #region 普通阅卷
        public DResults<VMarkingDto> MarkingList(VMarkingInputDto inputDto)
        {
            var dtos = new List<VMarkingDto>();
            inputDto = inputDto ?? new VMarkingInputDto();
            var models = UsageRepository.Where(t => t.JointBatch == null || t.JointBatch == "");
            //科目
            if (inputDto.SubjectId > 0)
                models = models.Where(t => t.SubjectId == inputDto.SubjectId);
            if (inputDto.MarkingStatus >= 0)
                models = models.Where(t => t.MarkingStatus == (byte)inputDto.MarkingStatus);
            if (!inputDto.ShowAll)
                models = models.Where(t => t.Status == (byte)NormalStatus.Normal);
            if (!string.IsNullOrWhiteSpace(inputDto.AgencyId))
            {
                var groups = ClassRepository.Where(t => t.AgencyId == inputDto.AgencyId).Select(t => t.Id).ToList();
                models = models.Where(t => groups.Contains(t.ClassId));
            }
            if (!string.IsNullOrWhiteSpace(inputDto.Keyword))
            {
                var reg = inputDto.Keyword.As<IRegex>();
                if (reg.IsMatch("^\\d{5}$"))
                {
                    //得一号
                    models = models.Join(UserRepository.Table, uc => uc.UserId, u => u.Id, (uc, u) => new { uc, u })
                        .Where(t => t.u.UserCode == inputDto.Keyword).Select(t => t.uc);
                }
                else if (reg.IsMatch("^gc\\d+", RegexOptions.IgnoreCase))
                {
                    var group =
                        GroupRepository.Where(t => t.GroupCode == inputDto.Keyword).Select(t => t.Id).FirstOrDefault();
                    if (group == null)
                        return DResult.Succ(dtos, 0);
                    models = models.Where(t => t.ClassId == group);
                }
                else
                {
                    models = models.Join(PaperRepository.Table, uc => uc.SourceID, p => p.Id, (uc, p) => new { uc, p })
                        .Where(t => t.p.PaperNo == inputDto.Keyword || t.p.PaperTitle.Contains(inputDto.Keyword))
                        .Select(t => t.uc);
                }
            }
            var count = models.Count();
            var list = models.Join(PaperRepository.Table, uc => uc.SourceID, p => p.Id, (uc, p) => new
            {
                Batch = uc.Id,
                PaperId = p.Id,
                p.PaperType,
                p.PaperTitle,
                p.PaperNo,
                uc.SourceType,
                uc.Status,
                uc.MarkingStatus,
                uc.UserId,
                uc.SubjectId,
                uc.ClassId,
                uc.AddedAt
            })
                .OrderByDescending(t => t.AddedAt)
                .Skip(inputDto.Page * inputDto.Size)
                .Take(inputDto.Size)
                .ToList();
            if (!list.Any())
                return DResult.Succ(dtos, count);
            var groupIds = list.Select(t => t.ClassId).Distinct().ToList();
            var groupDict = GroupContract.GroupDtoDict(groupIds);
            var userIds = list.Select(t => t.UserId).Distinct().ToList();
            var userDict =
                UserRepository.Where(u => userIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.HeadPhoto, u.UserCode, u.TrueName })
                    .ToList()
                    .ToDictionary(k => k.Id, v => new UserDto
                    {
                        Id = v.Id,
                        Name = v.TrueName,
                        Avatar = v.HeadPhoto,
                        Code = v.UserCode
                    });
            var batches = list.Select(t => t.Batch).ToList();
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
            var subjects = SystemCache.Instance.Subjects();
            foreach (var item in list)
            {
                var dto = new VMarkingDto
                {
                    Batch = item.Batch,
                    PublishType = (PublishType)item.SourceType,
                    PaperId = item.PaperId,
                    PaperTitle = item.PaperTitle,
                    PaperType = (PaperType)item.PaperType,
                    PaperCode = item.PaperNo,
                    SubjectId = item.SubjectId,
                    Status = (byte)item.Status,
                    MarkingStatus = (MarkingStatus)item.MarkingStatus,
                    Time = item.AddedAt
                };
                if (subjects.ContainsKey(item.SubjectId))
                    dto.Subject = subjects[item.SubjectId];
                if (groupDict.ContainsKey(item.ClassId))
                    dto.Group = groupDict[item.ClassId];
                if (userDict.ContainsKey(item.UserId))
                    dto.User = userDict[item.UserId];
                if (countDict.ContainsKey(item.Batch))
                {
                    var countItem = countDict[item.Batch];
                    dto.CountA = countItem.countA;
                    dto.CountB = countItem.countB;
                }
                dtos.Add(dto);
            }
            return DResult.Succ(dtos, count);
        }

        public List<DKeyValue> MarkingPictures(string batch, byte type)
        {
            var pictures = new List<DKeyValue>();
            if (string.IsNullOrWhiteSpace(batch))
                return pictures;
            pictures = MarkingPictureRepository.Where(
                p => p.BatchNo == batch && ((type == 1 && p.AnswerImgType == 0) || p.AnswerImgType == type))
                .Select(mp => new { mp.StudentName, mp.AnswerImgUrl, mp.AddedAt, mp.SubmitSort })
                .OrderBy(t => t.AddedAt).ThenBy(t => t.SubmitSort)
                .ToList()
                .Select(t => new DKeyValue(t.StudentName, t.AnswerImgUrl))
                .ToList();
            return pictures;
        }

        public DResult Recall(string batch)
        {
            if (string.IsNullOrWhiteSpace(batch))
                return DResult.Error("批次不存在");
            var model = UsageRepository.Load(batch);
            if (model == null)
                return DResult.Error("批次不存在");
            if (model.Status == (byte)NormalStatus.Delete)
                return DResult.Success;
            //if (model.MarkingStatus != (byte)MarkingStatus.NotMarking)
            //    return DResult.Error("已批阅的批次不能撤回");
            model.Status = (byte)NormalStatus.Delete;
            var result = UsageRepository.Update(t => new { t.Status }, model);
            if (result > 0)
            {
                DynamicTasks.Delete(batch);
            }
            return DResult.FromResult(result);
        }

        public void ExportMarking(string batch)
        {
            var model = UsageRepository.Load(batch);
            if (model == null || model.MarkingStatus != (byte)MarkingStatus.AllFinished)
                return;
            var dataSet = new DataSet("考试情况统计表_{0}".FormatWith(Clock.Now.ToString("yyyyMMddHHmmss")));
            var students = GroupContract.GroupMembers(model.ClassId, UserRole.Student)
                .Data.ToDictionary(k => k.Id, v => v);
            var rankTable = ExportHelper.RankTable(batch, students);
            var segmentTable = ExportHelper.SegmentTable(batch);
            var questionScores = StatisticContract.QuestionScores(batch);
            var scoresTable = ExportHelper.ScoresTable(questionScores.Data);
            var knowledgeTable = ExportHelper.KnowledgeTable(model.SourceID, questionScores.Data.QuestionSorts);
            dataSet.Tables.Add(rankTable);
            dataSet.Tables.Add(segmentTable);
            dataSet.Tables.Add(scoresTable);
            dataSet.Tables.Add(knowledgeTable);
            ExcelHelper.Export(dataSet, dataSet.DataSetName);
        }

        #endregion
    }
}
