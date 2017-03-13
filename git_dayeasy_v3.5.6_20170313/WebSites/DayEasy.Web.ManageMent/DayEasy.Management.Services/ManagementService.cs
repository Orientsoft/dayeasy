using DayEasy.AutoMapper;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain;
using DayEasy.EntityFramework;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.Management.Services
{
    public partial class ManagementService : DayEasyService, IManagementContract
    {
        public ManagementService(IDbContextProvider<DayEasyDbContext> context)
            : base(context)
        {
        }

        public IDayEasyRepository<TS_Application, int> ApplicationRepository { private get; set; }
        public IDayEasyRepository<TU_UserApplication, string> UserApplicationRepository { private get; set; }

        public IDayEasyRepository<TS_SystemLog, string> SystemLogRepository { private get; set; }

        public IDayEasyRepository<TS_Subject, int> SubjectRepository { private get; set; }

        public IDayEasyRepository<TS_QuestionType, int> QuestionTypeRepository { private get; set; }
        public IDayEasyRepository<TS_DownloadLog> DownloadLogRepository { private get; set; }


        #region Application

        public List<TS_Application> Applications(bool hasDelete = false)
        {
            return ApplicationRepository.Where(a => (hasDelete || a.Status != (byte)NormalStatus.Delete))
                .OrderBy(a => a.Status).ThenBy(a => a.Sort).ToList();
        }

        public TS_Application Application(int id)
        {
            return ApplicationRepository.Load(id);
        }

        public DResult InsertOrUpdateApp(AppDto dto)
        {
            if (dto == null)
                return DResult.Error("参数错误，请稍后重试！");
            if (string.IsNullOrWhiteSpace(dto.Name))
                return DResult.Error("应用名称不能为空！");
            if (string.IsNullOrWhiteSpace(dto.Link))
                return DResult.Error("应用地址不能为空！");
            if (dto.Type < 0)
                return DResult.Error("请选择应用类型！");
            var app = dto.MapTo<TS_Application>();
            int result;
            if (app.Id > 0)
            {
                //更新
                result = ApplicationRepository.Update(a => new
                {
                    a.AppName,
                    a.AppURL,
                    a.AppIcon,
                    a.AppType,
                    a.Sort,
                    a.IsSLD,
                    a.AppRoles,
                    a.AppRemark
                }, app);
            }
            else
            {
                //新增
                app.Status = (byte)NormalStatus.Normal;
                result = ApplicationRepository.Insert(app);
            }
            return DResult.FromResult(result);
        }

        public DResult DeleteApplication(int id)
        {
            var app = ApplicationRepository.Load(id);
            if (app == null)
            {
                return DResult.Error("应用不存在！");
            }
            int result;
            if (app.Status == (byte)NormalStatus.Delete)
            {
                result = ApplicationRepository.Delete(app);
            }
            else
            {
                app.Status = (byte)NormalStatus.Delete;

                result = ApplicationRepository.Update(app, "Status");
            }
            return DResult.FromResult(result);
        }

        public DResults<UserAppDto> ApplicationUsers(int id, DPage page = null)
        {
            var app = ApplicationRepository.Load(id);
            if (app == null)
            {
                return DResult.Errors<UserAppDto>("应用不存在！");
            }
            if (app.AppType == (byte)ApplicationType.Normal)
                return DResult.Errors<UserAppDto>("普通应用不能分配用户！");
            if (page == null)
                page = DPage.NewPage();
            var apps =
                UserApplicationRepository.Where(t => t.ApplicationID == id && t.Status == (byte)NormalStatus.Normal);
            var count = apps.Count();
            var userApps = apps.Select(t => new
            {
                t.UserID,
                t.ApplicationID,
                t.AddedAt,
                t.AgencyId
            }).OrderByDescending(t => t.AddedAt)
                .Skip(page.Page * page.Size)
                .Take(page.Size)
                .ToList();
            var list = new List<UserAppDto>();
            if (userApps.IsNullOrEmpty())
                return DResult.Succ(list, count);
            //机构、用户信息
            var agencyIds = userApps.Select(t => t.AgencyId).Distinct().ToList();
            var agencys = new List<TS_Agency>();
            if (agencyIds.Any())
                agencys = AgencyRepository.Where(a => agencyIds.Contains(a.Id)).ToList();
            var userIds = userApps.Select(t => t.UserID).Distinct().ToList();
            var users = UserContract.LoadList(userIds);
            userApps.ForEach(t =>
            {
                var item = new UserAppDto
                {
                    UserId = t.UserID,
                    AgencyId = t.AgencyId,
                    AddedAt = t.AddedAt
                };
                var user = users.FirstOrDefault(u => u.Id == item.UserId);
                if (user != null)
                {
                    item.UserName = user.Name;
                    item.Avatar = user.Avatar;
                    item.Role = user.Role;
                    item.Account = (user.Email.IsNotNullOrEmpty()
                        ? user.Email
                        : (user.Nick.IsNotNullOrEmpty() ? user.Nick : user.Mobile));
                    item.UserCode = user.Code;
                }
                if (t.AgencyId.IsNotNullOrEmpty())
                {
                    var agency = agencys.FirstOrDefault(a => a.Id == t.AgencyId);
                    if (agency != null)
                    {
                        item.AgencyId = agency.Id;
                        item.AgencyName = agency.AgencyName;
                    }
                }
                list.Add(item);
            });
            return DResult.Succ(list, count);
        }

        public DResult RemoveUserApp(int appId, long userId)
        {
            var app = UserApplicationRepository.FirstOrDefault(t => t.ApplicationID == appId && t.UserID == userId);
            if (app == null)
                return DResult.Error("该用户还没有当前应用！");
            if (app.Status == (byte)NormalStatus.Delete)
                return DResult.Error("用户应用已被移除！");
            app.Status = (byte)NormalStatus.Delete;
            var result = UserApplicationRepository.Update(app, "Status");
            return DResult.FromResult(result);

        }

        #endregion

        #region Subject

        public TS_Subject Subject(int id)
        {
            return SubjectRepository.Load(id);
        }

        public List<TS_Subject> Subjects()
        {
            var list = SubjectRepository.Where(s => s.Status == (byte)TempStatus.Normal).ToList();
            return list;
        }

        public DResult DeleteSubject(int id)
        {
            var subject = SubjectRepository.Load(id);
            if (subject == null)
            {
                return DResult.Error("科目不存在！");
            }
            int result;
            if (subject.Status == (byte)TempStatus.Delete)
            {
                result = SubjectRepository.Delete(subject);
            }
            else
            {
                subject.Status = (byte)TempStatus.Delete;

                result = SubjectRepository.Update(subject, "Status");
            }
            return DResult.FromResult(result);
        }

        public DResult InsertOrUpdateSubject(SubjectDto subjectDto)
        {
            if (string.IsNullOrEmpty(subjectDto.SubName))
            {
                return DResult.Error("请填写名称！");
            }
            if (subjectDto.QType == null || !subjectDto.QType.Any())
            {
                return DResult.Error("请先选择题型！");
            }
            var subject = new TS_Subject
            {
                Id = subjectDto.Id,
                SubjectName = subjectDto.SubName,
                IsLoadFormula = subjectDto.LoadFormula,
                QTypeIDs = CommonExtension.ToJson(subjectDto.QType),
                Status = subjectDto.Status
            };
            int result;
            if (subject.Id > 0)
            {
                result = SubjectRepository.Update(s => new
                {
                    s.SubjectName,
                    s.QTypeIDs,
                    s.IsLoadFormula,
                    s.Status
                }, subject);
            }
            else
            {
                subject.Status = (byte)TempStatus.Normal;
                result = SubjectRepository.Insert(subject);
            }
            return DResult.FromResult(result);
        }

        #endregion

        #region QuestionType

        public List<TS_QuestionType> QuestionTypes()
        {
            return QuestionTypeRepository.Where(t => t.Status == (byte)TempStatus.Normal).ToList();
        }

        public TS_QuestionType QuestionType(int id)
        {
            return QuestionTypeRepository.Load(id);
        }

        public DResult InsertOrUpdateQuestionType(QuestionTypeDto dto)
        {
            if (dto == null)
                return DResult.Error("参数异常！");
            if (string.IsNullOrWhiteSpace(dto.TypeName))
                return DResult.Error("请填写题型名称！");
            if (dto.QStyle == null || !dto.QStyle.Any())
                return DResult.Error("请选择题型包含的内容！");
            TS_QuestionType questionType;
            if (dto.Id > 0)
            {
                questionType = QuestionTypeRepository.Load(dto.Id);
                if (questionType == null)
                    return DResult.Error("没有找到该题型！");
            }
            else
            {
                questionType = new TS_QuestionType
                {
                    QTypeRemark = null,
                    Status = (byte)TempStatus.Normal
                };
            }
            questionType.QTypeName = dto.TypeName;
            questionType.HasMultiAnswer = dto.MultiAnswer == 1;
            questionType.QTypeStyle = (byte)dto.QStyle.Aggregate(0, (c, t) => c + t);
            questionType.HasSmallQuestion = dto.QStyle.Contains((byte)QuestionTypeStyle.Detail);

            int result;
            if (dto.Id > 0)
            {
                result = QuestionTypeRepository.Update(t => new
                {
                    t.QTypeName,
                    t.QTypeStyle,
                    t.HasMultiAnswer,
                    t.HasSmallQuestion
                }, questionType);
            }
            else
            {
                result = QuestionTypeRepository.Insert(questionType);
            }
            return DResult.FromResult(result);
        }

        public DResult DeleteQuestionType(int id)
        {
            var item = QuestionTypeRepository.Load(id);
            if (item == null)
                return DResult.Error("没有找到该题型！");
            int result;
            if (item.Status == (byte)TempStatus.Normal)
            {
                item.Status = (byte)TempStatus.Delete;
                result = QuestionTypeRepository.Update(item, "Status");
            }
            else
            {
                result = QuestionTypeRepository.Delete(item);
            }
            return DResult.FromResult(result);
        }

        #endregion

        #region Log

        public DResults<TS_SystemLog> Logs(int status = -1, int page = 0, int size = 15)
        {
            var dPage = DPage.NewPage(page, size);
            Expression<Func<TS_SystemLog, bool>> condition = l => true;
            if (status >= 0)
                condition = condition.And(l => l.Status == status);
            var list = SystemLogRepository.Where(condition)
                .OrderByDescending(l => l.Time)
                .Skip(dPage.Page * dPage.Size)
                .Take(dPage.Size)
                .ToList();
            int count = SystemLogRepository.Count(condition);
            return DResult.Succ(list, count);
        }

        public DResult ResolveLog(string id)
        {
            var result = SystemLogRepository.Update(new TS_SystemLog
            {
                Status = 1,
                ResolutionTime = Clock.Now
            }, l => l.Id == id, "Status", "ResolutionTime");
            return DResult.FromResult(result);
        }

        public DResults<TS_DownloadLog> DownloadLogs(int type, string keyword, DPage page = null)
        {
            Expression<Func<TS_DownloadLog, bool>> condition = d => true;
            if (type >= 0)
                condition = condition.And(d => d.Type == type);
            var logs = DownloadLogRepository.Where(condition);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                logs = logs.Join(UserRepository.Where(u => u.UserCode == keyword || u.TrueName.Contains(keyword)), l => l.UserId, u => u.Id, (l, u) => l);
            }
            var count = logs.Count();
            var dtos = logs.OrderByDescending(t => t.AddedAt).Skip(page.Page * page.Size).Take(page.Size).ToList();
            return DResult.Succ(dtos, count);
        }

        #endregion
    }
}
