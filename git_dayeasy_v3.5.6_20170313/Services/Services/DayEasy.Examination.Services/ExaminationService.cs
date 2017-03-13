using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Examination;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain;
using DayEasy.EntityFramework;
using DayEasy.Examination.Services.Helper;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.Examination.Services
{

    public partial class ExaminationService : Version3Service, IExaminationContract
    {
        #region 注入

        public ExaminationService(IDbContextProvider<Version3DbContext> context)
            : base(context)
        {
        }

        public IVersion3Repository<TG_Group> GroupRepository { private get; set; }
        public IVersion3Repository<TG_Class> ClassRepository { private get; set; }
        public IVersion3Repository<TS_Agency> AgencyRepository { private get; set; }

        public IVersion3Repository<TE_Examination> ExaminationRepository { private get; set; }
        public IVersion3Repository<TE_StudentScore> StudentScoreRepository { private get; set; }
        public IVersion3Repository<TE_SubjectScore> SubjectScoreRepository { private get; set; }
        public IVersion3Repository<TE_StudentSubjectScore> StudentSubjectScoreRepository { private get; set; }
        public IDayEasyRepository<TP_JointMarking> JointMarkingRepository { private get; set; }
        public IDayEasyRepository<TU_User, long> UserRepository { private get; set; }

        #endregion

        public DResults<ExamSubjectDto> ExamJointList(string id)
        {

            var exam = ExaminationRepository.Load(id);
            if (exam == null)
                return DResult.Succ(new List<ExamSubjectDto>(), 0);
            var joints = JsonHelper.JsonList<string>(exam.JointBatches);
            List<string> classList = null;
            if (exam.ExamType == (byte)ExamType.Union)
                classList = JsonHelper.JsonList<string>(exam.ClassList ?? "[]").ToList();
            return ConvertToJointStatisticDto(t => joints.Contains(t.Id), classList: classList);
        }

        public DResults<ExamSubjectDto> JointList(JointSearchDto dto)
        {

            //按照机构下面的教师查询
            if (dto == null || string.IsNullOrWhiteSpace(dto.AgencyId))
                return ConvertToJointStatisticDto(t => t.Status == (byte)dto.Status, DPage.NewPage(dto.Page, dto.Size), dto.Subject, dto.Status);
            var users = UserContract.LoadUsersByAgencyId(dto.AgencyId, (byte)UserRole.Teacher).Data.Select(w => w.Id);
            if (users.Any())
            {
                var jointDis = JointDistributionRepository.Where(w => users.Contains(w.TeacherId)).Select(w => w.JointBatch).Distinct();
                var jointStatis = ConvertToJointStatisticDto(
                  t => t.Status == (byte)dto.Status && jointDis.Contains(t.Id),
                  DPage.NewPage(dto.Page, dto.Size), dto.Subject, dto.Status);
                return jointStatis;
            }
            else {
                return DResult.Succ(new List<ExamSubjectDto>(), 0);
            }
           
           
            //按照机构查询协同批次
            //var colleagueList = GroupContract.AgencyColleagueGroups(dto.AgencyId);
            //if (colleagueList.IsNullOrEmpty())
            //    return DResult.Succ(new List<ExamSubjectDto>(), 0);
            //return
            //    ConvertToJointStatisticDto(
            //        t => t.Status == (byte)dto.Status && colleagueList.Contains(t.GroupId),
            //        DPage.NewPage(dto.Page, dto.Size), dto.Subject, dto.Status);
        }

        public DResults<ExamDto> Examinations(int status = -1, string agencyId = null, DPage page = null, bool? isUnion = null)
        {
            Expression<Func<TE_Examination, bool>> condition = e => true;
            if (!string.IsNullOrWhiteSpace(agencyId))
            {
                condition =
                    condition.And(e => e.AgencyId == agencyId && (e.Status & (byte)ExamStatus.Sended) > 0);
            }
            if (status >= 0)
            {
                if (status == (byte)ExamStatus.Delete || status == (byte)ExamStatus.Normal)
                    condition = condition.And(e => e.Status == status);
                else
                    condition = condition.And(e => (e.Status & status) > 0);
            }
            if (isUnion.HasValue)
            {
                if (isUnion.Value)
                    condition = condition.And(e => e.UnionBatch != null && e.UnionBatch.Length == 32);
                else
                    condition = condition.And(e => e.UnionBatch == null || e.UnionBatch == "");
            }
            if (page == null)
            {
                page = DPage.NewPage();
            }
            var count = ExaminationRepository.Count(condition);
            var list = ExaminationRepository.Where(condition)
                .OrderByDescending(e => e.CreationTime)
                .Skip(page.Page * page.Size)
                .Take(page.Size)
                .ToList()
                .MapTo<List<ExamDto>>();
            var examIds = list.Select(t => t.Id).ToList();
            var classList = StudentSubjectScoreRepository.Where(t => examIds.Contains(t.ExamId))
                .Join(GroupRepository.Table, s => s.ClassId, g => g.Id, (s, g) => new
                {
                    s.ExamId,
                    g.Id,
                    g.GroupName
                })
                .ToList()
                .GroupBy(t => t.ExamId)
                .ToDictionary(
                    k => k.Key,
                    v => v.DistinctBy(s => s.Id).Select(s => s.GroupName).OrderBy(s => s.ClassIndex()).ToList());
            list.ForEach(s =>
            {
                if (classList.ContainsKey(s.Id))
                    s.ClassList = classList[s.Id];
            });
            return DResult.Succ(list, count);
        }

        public DResult CreateExamination(ExamDto dto)
        {
            if (dto == null)
                return DResult.Error("考试数据异常！");
            if (dto.AgencyId.IsNullOrEmpty() || dto.Stage <= 0)
                return DResult.Error("请选择机构！");
            if (dto.JointList.IsNullOrEmpty() || dto.JointList.Length < 2)
                return DResult.Error("请选择至少两次协同考试！");
            if (dto.Name.IsNullOrEmpty() || dto.Name.Length < 5)
                return DResult.Error("请输入考试名称，且不少于5个字符！");
            var date = JointMarkingRepository.Where(t => dto.JointList.Contains(t.Id))
                .Select(t => t.FinishedTime).Max();
            if (dto.Type == (byte)ExamType.Union)
            {
                var jointList =
                    UsageRepository.Where(t => dto.JointList.Contains(t.JointBatch))
                        .Select(t => new { t.JointBatch, t.ClassId })
                        .GroupBy(t => t.JointBatch)
                        .ToDictionary(k => k.Key, v => v.Select(t => t.ClassId).ToList());
                var classList = jointList.SelectMany(t => t.Value).Distinct().ToList();

                //var classList =
                //    UsageRepository.Where(t => t.JointBatch != null && dto.JointList.Contains(t.JointBatch))
                //        .Select(t => t.ClassId)
                //        .Distinct()
                //        .ToList();

                var agencies = ClassRepository.Where(t => classList.Contains(t.Id))
                    .Join(AgencyRepository.Table, c => c.AgencyId, a => a.Id,
                        (c, a) => new { a.Id, a.AgencyName, classId = c.Id })
                    .ToList()
                    .GroupBy(t => new { t.Id, t.AgencyName })
                    .Select(
                        t =>
                            new
                            {
                                id = t.Key.Id,
                                name = t.Key.AgencyName,
                                classList = t.Select(a => a.classId).Distinct().ToList()
                            });
                var list = new List<TE_Examination>();
                foreach (var agency in agencies)
                {
                    var item = dto.MapTo<TE_Examination>();
                    item.AgencyId = agency.id;
                    item.AgencyName = agency.name;
                    item.ClassList = JsonHelper.ToJson(agency.classList);
                    var joints =
                        jointList.Where(t => t.Value.Any(a => agency.classList.Contains(a))).Select(t => t.Key).ToList();
                    item.JointBatches = JsonHelper.ToJson(joints);
                    //item.JointBatches = JsonHelper.ToJson(dto.JointList);
                    item.Id = IdHelper.Instance.Guid32;
                    item.CreationTime = Clock.Now;
                    item.ExamTime = date ?? Clock.Now;
                    item.ExamType = (byte)dto.Type;
                    list.Add(item);
                }
                var count = ExaminationRepository.Insert(list.ToArray());
                return DResult.FromResult(count);
            }
            var exam = dto.MapTo<TE_Examination>();
            //:联考类型
            exam.JointBatches = JsonHelper.ToJson(dto.JointList);
            exam.Id = IdHelper.Instance.Guid32;
            exam.CreationTime = Clock.Now;
            exam.ExamTime = date ?? Clock.Now;
            exam.ExamType = (byte)dto.Type;
            var result = ExaminationRepository.Insert(exam);
            return DResult.FromResult(string.IsNullOrWhiteSpace(result) ? 0 : 1);
        }

        public DResult UpdateExamination(ExamUpdateDto updateDto)
        {
            if (updateDto == null || string.IsNullOrWhiteSpace(updateDto.Id))
                return DResult.Error("参数异常");
            if (string.IsNullOrWhiteSpace(updateDto.Name) || updateDto.Name.Length < 5)
                return DResult.Error("考试名称不能为空且长度不小于5个字符");
            var result = ExaminationRepository.Update(new TE_Examination { Name = updateDto.Name },
                t => t.Id == updateDto.Id, nameof(TE_Examination.Name));
            return DResult.FromResult(result);
        }

        public DResult SendExamination(string id, long userId)
        {
            var exam = ExaminationRepository.Load(id);
            if (exam == null)
                return DResult.Error("考试不存在！");
            if (exam.Status != (byte)ExamStatus.Normal)
                return DResult.Error("当前考试状态不能进行推送操作！");
            exam.CreationTime = Clock.Now;
            exam.CreatorId = userId;
            exam.Status = (byte)ExamStatus.Sended;
            var result = ExaminationRepository.Update(e => new
            {
                e.CreatorId,
                e.CreationTime,
                e.Status
            }, exam);
            if (result > 0)
            {
                ExaminationTask.Instance.GenerateExaminatiomRanksAsync(id);
            }
            return DResult.FromResult(result);
        }

        public DResult DeleteExamination(string id, long userId)
        {
            var exam = ExaminationRepository.Load(id);
            if (exam == null)
                return DResult.Error("考试不存在！");
            if (exam.Status != (byte)ExamStatus.Normal)
                return DResult.Error("当前考试状态不能进行删除操作！");
            exam.CreatorId = userId;
            exam.Status = (byte)ExamStatus.Delete;
            var result = ExaminationRepository.Update(e => new
            {
                e.CreatorId,
                e.Status
            }, exam);
            return DResult.FromResult(result);
        }

        public DResult PublishExamination(string id, long userId, UserRole role)
        {
            var exam = ExaminationRepository.Load(id);
            if (exam == null)
                return DResult.Error("考试不存在！");
            if ((exam.Status & (byte)ExamStatus.Sended) == 0)
                return DResult.Error($"当前考试状态不能推送或发布！");
            if (role == UserRole.Teacher && (exam.Status & (byte)ExamStatus.SendToTeacher) > 0)
                return DResult.Error("当前考试不能重复推送给教师！");
            if (role == UserRole.Student && (exam.Status & (byte)ExamStatus.Published) > 0)
                return DResult.Error("当前考试不能重复生成学生成绩！");
            int result;
            if (role == UserRole.Student)
            {
                exam.PublisherId = userId;
                exam.PublishTime = Clock.Now;
                exam.Status |= (byte)ExamStatus.Published;
                result = ExaminationRepository.Update(e => new
                {
                    e.PublisherId,
                    e.PublishTime,
                    e.Status
                }, exam);
            }
            else
            {
                exam.Status |= (byte)ExamStatus.SendToTeacher;
                result = ExaminationRepository.Update(e => new
                {
                    e.Status
                }, exam);
            }
            if (result > 0)
            {
                ExaminationTask.Instance.PublishTaskAsync(id, userId, exam.Name, role);
            }
            return DResult.FromResult(result);
        }

        public List<ExamSubjectDto> ExamSubjects(string examId)
        {
            var subjects = SubjectScoreRepository.Where(t => t.ExamId == examId)
                .OrderBy(t => t.SubjectId)
                .ToList()
                .MapTo<List<ExamSubjectDto>>();
            var joints = subjects.Select(t => t.JointBatch);
            var list = JointMarkingRepository.Where(t => joints.Contains(t.Id))
                .Select(t => new { t.Id, t.AddedBy, t.GroupId, t.FinishedTime })
                .ToList();
            var userIds = list.Select(t => t.AddedBy).Distinct().ToList();
            var users = UserContract.LoadListDictUser(userIds);
            var groupIds = list.Select(t => t.GroupId).Distinct().ToList();
            var groups = GroupContract.GroupDtoDict(groupIds);
            var paperIds = subjects.Select(t => t.PaperId).Distinct().ToList();
            var scores = PaperRepository.Where(p => paperIds.Contains(p.Id))
                .Select(p => new { p.Id, p.PaperScores })
                .ToList()
                .ToDictionary(k => k.Id, v => JsonHelper.Json<PaperScoresDto>(v.PaperScores));
            foreach (var subject in subjects)
            {
                var item = list.FirstOrDefault(t => t.Id == subject.JointBatch);
                if (item == null)
                    continue;
                subject.FinishedTime = item.FinishedTime.Value;
                if (users.ContainsKey(item.AddedBy))
                    subject.Creator = users[item.AddedBy];
                if (groups.ContainsKey(item.GroupId))
                    subject.Group = groups[item.GroupId];
                if (scores.ContainsKey(subject.PaperId))
                {
                    var score = scores[subject.PaperId];
                    subject.Score = score.TScore;
                    subject.ScoreA = score.TScoreA;
                    subject.ScoreB = score.TScoreB;
                }
            }
            return subjects;
        }
    }
}
