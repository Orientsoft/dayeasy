
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Core.Dependency;
using DayEasy.Core.Domain;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.User.Services
{
    /// <summary> 家长相关业务 </summary>
    public partial class UserService
    {
        public IVersion3Repository<TU_StudentParents> ParentsRepository { private get; set; }

        /// <summary> 用户关系 </summary>
        /// <param name="condition"></param>
        /// <param name="role">用户角色：学生，查父母；父母，查学生</param>
        /// <returns></returns>
        private Dictionary<long, List<RelationUserDto>> RelationUsers(
            Expression<Func<TU_StudentParents, bool>> condition, UserRole role = UserRole.Student)
        {
            var dict = new Dictionary<long, List<RelationUserDto>>();
            if (!role.In(UserRole.Student, UserRole.Parents))
                return dict;
            condition = condition.And(p => p.Status == (byte)NormalStatus.Normal);
            var parents = ParentsRepository.Where(condition)
                .Select(p => new
                {
                    p.StudentId,
                    p.ParentId,
                    p.RelationType
                }).ToList();
            if (parents.IsNullOrEmpty())
                return dict;
            List<UserDto> userList;
            if (role == UserRole.Student)
            {
                var userIds = parents.Select(t => t.ParentId).Distinct().ToList();
                userList = LoadList(userIds);
                dict = parents.GroupBy(p => p.StudentId)
                    .ToDictionary(k => k.Key, v => v.Select(t => new RelationUserDto
                    {
                        Id = t.ParentId,
                        RelationType = t.RelationType
                    }).ToList());
            }
            else
            {
                var userIds = parents.Select(t => t.StudentId).Distinct().ToList();
                userList = LoadList(userIds);
                dict = parents.GroupBy(p => p.ParentId)
                    .ToDictionary(k => k.Key, v => v.Select(t => new RelationUserDto
                    {
                        Id = t.StudentId,
                        RelationType = t.RelationType
                    }).ToList());
            }
            foreach (var dto in dict.Values.SelectMany(p => p))
            {
                var user = userList.FirstOrDefault(u => u.Id == dto.Id);
                if (user == null)
                    continue;
                dto.Avatar = user.Avatar;
                dto.Name = user.Name;
                dto.Nick = user.Nick;
                dto.Account = role == UserRole.Parents ? user.Name : UserAccount(user);
                dto.Mobile = user.Mobile.IsNotNullOrEmpty() &&
                             (user.ValidationType & (byte)ValidationType.Mobile) > 0
                    ? user.Mobile
                    : string.Empty;
            }
            return dict;
        }

        public DResult<DUserDto> LoadChild(string account, string password)
        {
            var loginResult = AccountLogin(account, password);
            if (!loginResult.Status)
                return DResult.Error<DUserDto>(loginResult.Message);
            var user = loginResult.Data;
            if ((user.Role & (byte)UserRole.Student) == 0)
                return DResult.Error<DUserDto>("请登录学生帐号！");
            var dto = new DUserDto
            {
                Id = user.Id,
                Name = user.TrueName,
                Avatar = user.HeadPhoto
            };
            if (dto.Avatar.IsNotNullOrEmpty())
                dto.Avatar = Consts.DefaultAvatar();
            return DResult.Succ(dto);
        }

        public DResult<DUserDto> LoadChildByPlatform(PlatformDto platformDto)
        {
            var platform =
                PlatformRepository.SingleOrDefault(p => p.PlatformId == platformDto.PlatformId && p.UserID > 0);
            if (platform == null)
                return DResult.Error<DUserDto>("帐号不存在或未激活！~");
            var user = UserRepository.Load(platform.UserID);
            if (user == null || (user.Role & (byte)UserRole.Student) == 0)
                return DResult.Error<DUserDto>("请登录学生帐号！");
            var dto = new DUserDto
            {
                Id = user.Id,
                Name = user.TrueName,
                Avatar = user.HeadPhoto
            };
            if (dto.Avatar.IsNotNullOrEmpty())
                dto.Avatar = Consts.DefaultAvatar();
            return DResult.Succ(dto);
        }

        public DResult BindChild(long parentsId, long studentId, FamilyRelationType relationType)
        {
            var parent = UserRepository.Load(parentsId);
            var child = UserRepository.Load(studentId);
            if (parent == null || child == null)
                return DResult.Error("用户信息异常~！");
            if ((parent.Role & (byte)UserRole.Parents) <= 0)
                return DResult.Error("只有家长才能绑定孩子！");
            if ((child.Role & (byte)UserRole.Student) <= 0)
                return DResult.Error("只有学生才能被家长绑定！");
            if (relationType != FamilyRelationType.Other)
            {
                var exists =
                    ParentsRepository.Exists(
                        p =>
                            p.StudentId == studentId && p.RelationType == (byte)relationType &&
                            p.Status == (byte)NormalStatus.Normal);
                if (exists)
                {
                    return DResult.Error("该学生关联关系[{0}]已存在！".FormatWith(relationType.GetText()));
                }
            }

            var relation = ParentsRepository.FirstOrDefault(p => p.ParentId == parentsId && p.StudentId == studentId);
            bool insert = true;
            if (relation != null)
            {
                //已有记录
                if (relation.Status == (byte)NormalStatus.Normal)
                    return DResult.Error("已经和学生建立关联，不能重复关联！");
                insert = false;
                relation.Status = (byte)NormalStatus.Normal;
                relation.RelationType = (byte)relationType;
            }
            else
            {
                relation = new TU_StudentParents
                {
                    Id = IdHelper.Instance.Guid32,
                    AddedAt = Clock.Now,
                    AddedBy = parentsId,
                    ParentId = parentsId,
                    StudentId = studentId,
                    RelationType = (byte)relationType,
                    Status = (byte)NormalStatus.Normal
                };
            }
            var result = UnitOfWork.Transaction(() =>
            {
                if (string.IsNullOrWhiteSpace(parent.TrueName) && child.TrueName.IsNotNullOrEmpty())
                {
                    parent.TrueName = string.Concat(child.TrueName, relationType.GetText());
                    UserRepository.Update(u => new { u.TrueName }, parent);
                    ResetCache(parentsId);
                }

                if (insert)
                {
                    ParentsRepository.Insert(relation).IsNotNullOrEmpty();
                }
                else
                {
                    ParentsRepository.Update(p => new { p.Status, p.RelationType }, relation);
                }
                //发送系统消息
                var title = "“{0}” 成功与你建立了家庭关联帐号".FormatWith(
                    string.IsNullOrWhiteSpace(parent.Email)
                        ? parent.NickName
                        : parent.Email);
                CurrentIocManager.Resolve<IMessageContract>().SendMessage(new SystemMessageSendDto(studentId)
                {
                    SenderId = parentsId,
                    MessageType = MessageType.AssociateAccount,
                    Title = title,
                    Content = new DKeyValue("", "得一号：" + parent.UserCode)
                });
            });
            return DResult.FromResult(result);
        }

        public DResult CancelBindRelation(long parentId, long studentId)
        {
            var relation =
                ParentsRepository.FirstOrDefault(
                    p => p.ParentId == parentId && p.StudentId == studentId && p.Status == (byte)NormalStatus.Normal);
            if (relation == null)
                return DResult.Error("没有找到关联信息~！");
            relation.Status = (byte)NormalStatus.Delete;
            var result = UnitOfWork.Transaction(() =>
            {
                ParentsRepository.Update(p => new { p.Status }, relation);

                var parent = Load(relation.ParentId);
                UserRepository.Update(u => new { u.TrueName }, new TU_User { Id = parentId, TrueName = null });
                ResetCache(parentId);

                var title = "“{0}” 与你解除了家庭关联帐号".FormatWith(
                    string.IsNullOrWhiteSpace(parent.Email)
                        ? parent.Nick
                        : parent.Email);
                CurrentIocManager.Resolve<IMessageContract>()
                    .SendMessage(new SystemMessageSendDto(relation.StudentId)
                    {
                        SenderId = relation.ParentId,
                        MessageType = MessageType.AssociateAccount,
                        Title = title,
                        Content = new DKeyValue("", "得一号：" + parent.Code)
                    });
            });
            return DResult.FromResult(result);
        }

        public List<RelationUserDto> Parents(long childId)
        {
            var dict = RelationUsers(p => p.StudentId == childId);
            return dict.ContainsKey(childId) ? dict[childId] : new List<RelationUserDto>();
        }

        public Dictionary<long, List<RelationUserDto>> ParentsDict(List<long> studentIds)
        {
            if (studentIds == null || !studentIds.Any())
                return new Dictionary<long, List<RelationUserDto>>();
            return RelationUsers(p => studentIds.Contains(p.StudentId));
        }

        public List<RelationUserDto> Children(long parentId)
        {
            var dict = RelationUsers(p => p.ParentId == parentId, UserRole.Parents);
            return dict.ContainsKey(parentId) ? dict[parentId] : new List<RelationUserDto>();
        }
    }
}
