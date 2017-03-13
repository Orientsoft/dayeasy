using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain;
using DayEasy.EntityFramework;
using DayEasy.Group.Services.Helper;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
namespace DayEasy.Group.Services
{
    /// <summary> 圈子基础业务 </summary>
    public partial class GroupService : Version3Service, IGroupContract
    {
        public GroupService(IDbContextProvider<Version3DbContext> context)
            : base(context)
        { }

        public IVersion3Repository<TG_Group> GroupRepository { private get; set; }
        public IVersion3Repository<TG_Member> MemberRepository { private get; set; }
        public IVersion3Repository<TG_Class> ClassGroupRepository { private get; set; }
        public IVersion3Repository<TG_Colleague> ColleagueGroupRepository { private get; set; }
        public IVersion3Repository<TG_Share> ShareGroupRepository { private get; set; }
        public IDayEasyRepository<TU_User, long> UserRepository { private get; set; }
        public IDayEasyRepository<TU_UserAgency> UserAgencyRepository { private get; set; }

        public IDayEasyRepository<TU_UserApplication, string> UserApplicationRepository { private get; set; }

        #region 创建圈子

        /// <summary> 创建圈子 </summary>
        /// <param name="groupDto"></param>
        /// <param name="creatorRole"></param>
        /// <param name="trueName"></param>
        /// <param name="isManage"></param>
        /// <returns></returns>
        public DResult<GroupDto> CreateGroup(GroupDto groupDto, byte creatorRole = (byte)UserRole.Teacher, string trueName = null, bool isManage = false)
        {
            if (groupDto == null)
                return DResult.Error<GroupDto>("圈子信息不完整！");
            if (string.IsNullOrEmpty(groupDto.Name))
                return DResult.Error<GroupDto>("圈子名称不能为空！");
            if (groupDto.Name.Length > 20)
                return DResult.Error<GroupDto>("圈子名称太长了！");

            groupDto.Id = IdHelper.Instance.Guid32;
            //生成圈号，成功之后保存到缓存
            groupDto.Code = GroupCodeManager.Instance(this).GroupCode((GroupType)groupDto.Type);
            if (string.IsNullOrEmpty(groupDto.Code))
                return DResult.Error<GroupDto>("圈号生成失败，请稍后重试！");

            var group = groupDto.MapTo<TG_Group>();
            if (group == null)
                return DResult.Error<GroupDto>("创建失败，请稍后重试！");
            group.Status = (byte)NormalStatus.Normal;
            group.UnCheckedCount = 0;
            group.MemberCount = 1;
            group.AddedBy = group.ManagerId;
            group.AddedAt = Clock.Now;
            group.GroupAvatar = GroupAvatarHelper.Avatar();
            TG_Class classGroup = null;
            TG_Colleague colleagueGroup = null;
            TG_Share shareGroup = null;
            if (isManage)
            {
                group.CertificationLevel = (int)GroupCertificationLevel.Normal;
                group.ManagerId = 0;
                group.MemberCount = 0;
            }

            //圈子的特殊信息
            switch (groupDto.Type)
            {
                case (byte)GroupType.Class:
                    var classDto = groupDto as ClassGroupDto;

                    if (classDto == null)
                        return DResult.Error<GroupDto>("创建失败，请稍后重试！");

                    if (string.IsNullOrEmpty(classDto.AgencyId))
                        return DResult.Error<GroupDto>("学校信息不能为空！");
                    classGroup = new TG_Class
                    {
                        AgencyId = classDto.AgencyId,
                        GradeYear = classDto.GradeYear,
                        Id = groupDto.Id,
                        Stage = classDto.Stage
                    };
                    break;
                case (byte)GroupType.Colleague:
                    var colleagueDto = groupDto as ColleagueGroupDto;

                    if (colleagueDto == null)
                        return DResult.Error<GroupDto>("创建失败，请稍后重试！");

                    if (string.IsNullOrEmpty(colleagueDto.AgencyId))
                        return DResult.Error<GroupDto>("学校信息不能为空！");
                    colleagueGroup = new TG_Colleague
                    {
                        AgencyId = colleagueDto.AgencyId,
                        Id = groupDto.Id,
                        Stage = colleagueDto.Stage,
                        SubjectId = colleagueDto.SubjectId
                    };
                    break;
                case (byte)GroupType.Share:
                    var shareDto = groupDto as ShareGroupDto;

                    if (shareDto == null)
                        return DResult.Error<GroupDto>("创建失败，请稍后重试！");
                    if (shareDto.ChannelId <= 0)
                        return DResult.Error<GroupDto>("请选择分享圈的频道！");

                    //                    if (string.IsNullOrEmpty(shareGroup.AgencyId))
                    //                        return DResult.Error<GroupDto>("学校信息不能为空！");
                    shareGroup = new TG_Share
                    {
                        Id = groupDto.Id,
                        ClassType = shareDto.ChannelId,
                        Tags = shareDto.Tags,
                        PostAuth = shareDto.PostAuth,
                        JoinAuth = shareDto.JoinAuth
                    };
                    break;
            }
            var resultId = UnitOfWork.Transaction(() =>
            {
                //圈子基础信息插入
                GroupRepository.Insert(group);

                //圈子的特殊信息插入
                if (classGroup != null)
                {
                    //班级圈
                    ClassGroupRepository.Insert(classGroup);
                }
                if (colleagueGroup != null)
                {
                    //同事圈
                    ColleagueGroupRepository.Insert(colleagueGroup);
                }
                if (shareGroup != null)
                {
                    //分享圈
                    ShareGroupRepository.Insert(shareGroup);
                }
                if (!isManage)
                {
                    //圈主自动成为圈成员插入
                    MemberRepository.Insert(new TG_Member
                    {
                        AddedAt = Clock.Now,
                        BusinessCard = string.Empty,
                        GroupId = groupDto.Id,
                        Id = IdHelper.Instance.Guid32,
                        MemberId = groupDto.ManagerId,
                        MemberRole = creatorRole,
                        Status = (byte)NormalStatus.Normal
                    });
                }
                if (!string.IsNullOrWhiteSpace(trueName))
                {
                    UserContract.Update(new UserDto { Id = groupDto.ManagerId, Name = trueName });
                }
            });

            return resultId < 0 ? DResult.Error<GroupDto>("创建圈子失败，请稍后重试！") : DResult.Succ(groupDto);
        }

        /// <summary> 指定权限创建圈子 </summary>
        /// <param name="groupDto"></param>
        /// <param name="status">权限状态码</param>
        /// <param name="appId">应用编号</param>
        /// <returns></returns>
        public DResult<GroupDto> CreateGroupForManage(GroupDto groupDto, int status, int appId)
        {
            if (groupDto == null)
                return DResult.Error<GroupDto>("圈子信息不完整！");

            var groupList =
                GroupRepository.Where(
                    w =>
                        w.CertificationLevel == (int)GroupCertificationLevel.Normal && w.GroupName.Equals(groupDto.Name) &&
                        w.Status == (byte)NormalStatus.Normal && w.GroupType == groupDto.Type);
            if (groupList.Any())
            {
                return DResult.Error<GroupDto>("当前圈子已存在不能重复创建");
            }
            bool flag = CheckUserApplication(groupDto.ManagerId, status, appId);
            if (!flag)
                return DResult.Error<GroupDto>("当前人员无教务管理权限");

            if (string.IsNullOrEmpty(groupDto.Name))
                return DResult.Error<GroupDto>("圈子名称不能为空！");
            if (groupDto.Name.Length > 20)
                return DResult.Error<GroupDto>("圈子名称太长了！");

            groupDto.Id = IdHelper.Instance.Guid32;
            //生成圈号，成功之后保存到缓存
            groupDto.Code = GroupCodeManager.Instance(this).GroupCode((GroupType)groupDto.Type);
            if (string.IsNullOrEmpty(groupDto.Code))
                return DResult.Error<GroupDto>("圈号生成失败，请稍后重试！");

            var group = groupDto.MapTo<TG_Group>();
            if (group == null)
                return DResult.Error<GroupDto>("创建失败，请稍后重试！");
            group.Status = (byte)NormalStatus.Normal;
            group.UnCheckedCount = 0;
            group.AddedBy = group.ManagerId;
            group.AddedAt = Clock.Now;
            group.GroupAvatar = GroupAvatarHelper.Avatar();
            group.MemberCount = 0;
            group.CertificationLevel = (int)GroupCertificationLevel.Normal;
            group.ManagerId = 0;
            TG_Class classGroup = null;
            TG_Colleague colleagueGroup = null;
            //圈子的特殊信息
            switch (groupDto.Type)
            {
                case (byte)GroupType.Class:
                    var classDto = groupDto as ClassGroupDto;

                    if (classDto == null)
                        return DResult.Error<GroupDto>("创建失败，请稍后重试！");

                    if (string.IsNullOrEmpty(classDto.AgencyId))
                        return DResult.Error<GroupDto>("学校信息不能为空！");
                    if (classDto.GradeYear <= 0 || classDto.Stage <= 0)
                        return DResult.Error<GroupDto>("班级圈信息不完整！");
                    classGroup = new TG_Class
                    {
                        AgencyId = classDto.AgencyId,
                        GradeYear = classDto.GradeYear,
                        Id = groupDto.Id,
                        Stage = classDto.Stage
                    };
                    break;
                case (byte)GroupType.Colleague:
                    var colleagueDto = groupDto as ColleagueGroupDto;

                    if (colleagueDto == null)
                        return DResult.Error<GroupDto>("创建失败，请稍后重试！");

                    if (string.IsNullOrEmpty(colleagueDto.AgencyId))
                        return DResult.Error<GroupDto>("学校信息不能为空！");
                    if (colleagueDto.SubjectId <= 0)
                        return DResult.Error<GroupDto>("同事圈没有对应的科目！");
                    colleagueGroup = new TG_Colleague
                    {
                        AgencyId = colleagueDto.AgencyId,
                        Id = groupDto.Id,
                        Stage = colleagueDto.Stage,
                        SubjectId = colleagueDto.SubjectId
                    };
                    break;
            }
            var resultId = UnitOfWork.Transaction(() =>
            {
                //圈子基础信息插入
                GroupRepository.Insert(group);

                //圈子的特殊信息插入
                if (classGroup != null)
                {
                    //班级圈
                    ClassGroupRepository.Insert(classGroup);
                }
                if (colleagueGroup != null)
                {
                    //同事圈
                    ColleagueGroupRepository.Insert(colleagueGroup);
                }
            });

            return resultId < 0 ? DResult.Error<GroupDto>("创建圈子失败，请稍后重试！") : DResult.Succ(groupDto);
        }

        /// <summary>
        /// 批量创建圈子
        /// </summary>
        /// <returns></returns>
        public DResult BatchCreateGroups(BatchCreateGroupsDto dto, string agencyId)
        {
            var outMessage = new OutGroupMessage();
            var time = Clock.Now;
            var codeManager = GroupCodeManager.Instance(this);
            Func<GroupType, string, long, string, TG_Group> createFunc = (type, id, manageId, name) =>
            {
                var group = new TG_Group
                {
                    AddedAt = time,
                    AddedBy = manageId,
                    UnCheckedCount = 0,
                    MemberCount = 0,
                    GroupAvatar = GroupAvatarHelper.Avatar(),
                    CertificationLevel = (int)GroupCertificationLevel.Normal,
                    ManagerId = 0,
                    GroupCode = codeManager.GroupCode(type),
                    Id = id,
                    GroupName = name,
                    GroupType = (int)type,
                    Status = (byte)NormalStatus.Normal,
                    Capacity = type == GroupType.Class ? 200 : 100
                };
                return group;
            };
            var groups = new List<TG_Group>();//基础圈子信息集合
            var classes = new List<TG_Class>();//班级圈信息集合
            var colleagues = new List<TG_Colleague>();//同事圈信息集合
            var classNames = dto.ClassGroups.Select(w => w.Name).ToArray();//班级名称集合
            var colleagueNames = dto.ColleagueGroups.Select(w => w.Name).ToArray();//同事圈名字集合
            if (classNames.Any())
            {
                classNames = CheckGroupName(classNames, GroupType.Class, false, agencyId);
            }
            if (colleagueNames.Any())
            {
                colleagueNames = CheckGroupName(colleagueNames, GroupType.Colleague, false, agencyId);
            }
            var classDtos = dto.ClassGroups.Where(w => classNames.Contains(w.Name));//排重后的班级圈集合
            var colleagueDtos = dto.ColleagueGroups.Where(w => colleagueNames.Contains(w.Name));//排重后的同事圈集合
            //组装班级圈数据
            foreach (var item in classDtos)
            {
                var id = IdHelper.Instance.Guid32;
                var @class = new TG_Class
                {
                    AgencyId = agencyId,
                    Id = id,
                    GradeYear = item.GradeYear,
                    Stage = item.Stage
                };
                var group = createFunc(GroupType.Class, id, item.ManagerId, item.Name);
                classes.Add(@class);
                groups.Add(group);
            }
            //组装同事圈数据
            foreach (var item in colleagueDtos)
            {
                var id = IdHelper.Instance.Guid32;
                var colleague = new TG_Colleague
                {
                    AgencyId = agencyId,
                    Id = id,
                    Stage = item.Stage,
                    SubjectId = item.SubjectId
                };
                var group = createFunc(GroupType.Colleague, id, item.ManagerId, item.Name);
                colleagues.Add(colleague);
                groups.Add(group);
            }
            var resultId = UnitOfWork.Transaction(() =>
            {
                GroupRepository.Insert(groups);
                ClassGroupRepository.Insert(classes);
                ColleagueGroupRepository.Insert(colleagues);
                outMessage.ClassCount = classes.Count;
                outMessage.ColleagueCount = colleagues.Count;
                outMessage.GroupCount = groups.Count;
                outMessage.GroupsSuccess.AddRange(classNames);
                outMessage.GroupsSuccess.AddRange(colleagueNames);
            });
            return resultId < 0
                   ? DResults<OutGroupMessage>.Succ(new OutGroupMessage
                   { ClassCount = 0, ColleagueCount = 0, GroupCount = 0, GroupsSuccess = null }) :
                   DResults<OutGroupMessage>.Succ(outMessage);
        }

        /// <summary>
        /// 圈子名称重复验证
        /// </summary>
        /// <param name="names"></param>
        /// <param name="groupType"></param>
        /// <param name="isRepeat"></param>
        /// <param name="agencyId"></param>
        /// <returns></returns>
        private string[] CheckGroupName(ICollection<string> names, GroupType groupType, bool isRepeat = false, string agencyId = null)
        {
            if (names.IsNullOrEmpty())
                return new string[] { };
            var groups =
                GroupRepository.Where(
                    g =>
                        names.Contains(g.GroupName) && g.Status == (byte)NormalStatus.Normal &&
                        g.GroupType == (byte)groupType);
            if (!string.IsNullOrWhiteSpace(agencyId))
            {
                switch (groupType)
                {
                    case GroupType.Class:
                        groups = groups.Join(ClassGroupRepository.Where(c => c.AgencyId == agencyId), g => g.Id,
                            c => c.Id, (g, c) => g);
                        break;
                    case GroupType.Colleague:
                        groups = groups.Join(ColleagueGroupRepository.Where(c => c.AgencyId == agencyId), g => g.Id,
                            c => c.Id, (g, c) => g);
                        break;
                }
            }
            var repeatNames = groups.Select(g => g.GroupName).ToArray();
            return isRepeat ? repeatNames.ToArray() : names.Except(repeatNames).ToArray();
        }

        /// <summary>
        /// 当前用户是否具有指定状态的应用权限
        /// </summary>
        /// <param name="userId">用户编号</param>
        /// <param name="status">应用状态码正常:0</param>
        /// <param name="appId">应用编号</param>
        /// <returns></returns>
        private bool CheckUserApplication(long userId, int status, int appId)
        {
            var userApp = UserApplicationRepository.Where(w => w.UserID == userId && w.Status == status && w.ApplicationID == appId).FirstOrDefault();
            var result = userApp != null ? true : false;
            return result;
        }
        #endregion

        #region 获取用户所有圈子

        /// <summary> 获取用户所有圈子 </summary>
        /// <param name="userId"></param>
        /// <param name="groupType"></param>
        /// <param name="loadMessageCount"></param>
        /// <param name="containsAll">包含已删除的？</param>
        /// <returns></returns>
        public DResults<GroupDto> Groups(long userId, int groupType = -1, bool loadMessageCount = false, bool containsAll = false)
        {
            Expression<Func<TG_Group, bool>> condition =
                u => containsAll || u.Status == (byte)NormalStatus.Normal;
            //圈子类型
            if (groupType >= 0)
            {
                condition = condition.And(u => u.GroupType == groupType);
            }

            //查询用户所在的圈子Id
            var groupList = MemberRepository.Where(
                u => u.MemberId == userId && (containsAll || u.Status == (byte)NormalStatus.Normal))
                .Join(GroupRepository.Where(condition), m => m.GroupId, g => g.Id, (m, g) => new
                {
                    m.AddedAt,
                    g
                })
                .OrderByDescending(t => t.AddedAt)
                .Select(t => t.g)
                .ToList();

            if (groupList.Count <= 0)
                return DResult.Succ<GroupDto>(null, 0);
            var groups = ParseGroupDtos(groupList).ToList();
            if (loadMessageCount)
            {
                // 最新动态数
                UserGroupMessageCount(groups, userId);
            }
            return DResult.Succ(groups, groupList.Count);
        }
        #endregion

        #region 获取班级圈子的毕业年级
        /// <summary>
        /// 获取班级圈子的毕业年级
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DResult<Dictionary<int, List<int>>> GroupGradeYears(long userId)
        {
            var result = new Dictionary<int, List<int>>();

            var classGroups = Groups(userId, (int)GroupType.Class);
            if (!classGroups.Status || classGroups.Data == null)
                return DResult.Succ(result);

            var classGroupList = classGroups.Data.Select(u => u as ClassGroupDto).ToList();
            classGroupList.GroupBy(u => u.Stage).ToList().Foreach(u =>
            {
                u.ToList().ForEach(g =>
                {
                    g.GradeYear += 3;
                    if (g.Stage == (byte)StageEnum.PrimarySchool)
                        g.GradeYear += 3;
                });

                result.Add(u.Key, u.Select(g => g.GradeYear).Distinct().ToList());
            });
            return DResult.Succ(result);
        }
        #endregion

        #region 根据圈号查询圈子

        /// <summary> 根据圈号查询圈子 </summary>
        /// <param name="groupCode"></param>
        /// <returns></returns>
        public DResult<GroupDto> LoadByCode(string groupCode)
        {
            if (string.IsNullOrEmpty(groupCode))
                return DResult.Error<GroupDto>("参数错误！");
            var dto = LoadGroupFromDbByCode(groupCode);
            if (dto == null)
                return DResult.Error<GroupDto>("圈子未找到！");
            return DResult.Succ(dto);
        }

        #endregion

        #region 根据圈子id 查询圈子

        /// <summary>
        /// 根据圈子id 查询圈子
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public DResult<GroupDto> LoadById(string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
                return DResult.Error<GroupDto>("参数错误！");
            var dto = LoadGroupFromDb(groupId);
            if (dto == null)
                return DResult.Error<GroupDto>("圈子未找到！");
            return DResult.Succ(dto);
        }

        #endregion

        #region 查询圈子

        /// <summary> 查询圈子 </summary>
        /// <param name="searchGroupDto"></param>
        /// <returns></returns>
        public DResults<GroupDto> SearchGroups(SearchGroupDto searchGroupDto)
        {
            Expression<Func<TG_Group, bool>> condition = u => u.Status == (byte)NormalStatus.Normal;
            //认证等级
            if (searchGroupDto.CertificationLevels != null)
            {
                condition = condition.And(g => searchGroupDto.CertificationLevels.Contains(g.CertificationLevel));
            }
            //关键词
            if (!string.IsNullOrEmpty(searchGroupDto.Keyword))
            {
                condition =
                    condition.And(
                        g =>
                            g.GroupCode == searchGroupDto.Keyword ||
                            g.GroupName.Contains(searchGroupDto.Keyword));
            }
            if (!searchGroupDto.Types.IsNullOrEmpty())
            {
                if (searchGroupDto.Types.Count == 1)
                {
                    var type = searchGroupDto.Types[0];
                    condition = condition.And(u => u.GroupType == type);
                }
                else
                {
                    condition = condition.And(u => searchGroupDto.Types.Contains(u.GroupType));
                }
            }

            if (!string.IsNullOrWhiteSpace(searchGroupDto.AgencyId))
            {
                var classIds = ClassGroupRepository.Where(t => t.AgencyId == searchGroupDto.AgencyId).Select(t => t.Id);
                var colleagueIds =
                    ColleagueGroupRepository.Where(t => t.AgencyId == searchGroupDto.AgencyId).Select(t => t.Id);
                condition =
                    condition.And(
                        u =>
                            (u.GroupType == (byte)GroupType.Class && classIds.Contains(u.Id)) ||
                            (u.GroupType == (byte)GroupType.Colleague && colleagueIds.Contains(u.Id)));
            }
            if (searchGroupDto.GradeYear > 0)
            {
                //入学年份
                var classIds = ClassGroupRepository.Where(t => t.GradeYear == searchGroupDto.GradeYear)
                    .Select(t => t.Id);
                condition = condition.And(t => t.GroupType == (byte)GroupType.Class && classIds.Contains(t.Id));
            }
            if (searchGroupDto.SubjectId > 0)
            {
                //入学年份
                var colleagueIds = ColleagueGroupRepository.Where(t => t.SubjectId == searchGroupDto.SubjectId)
                    .Select(t => t.Id);
                condition = condition.And(t => t.GroupType == (byte)GroupType.Colleague && colleagueIds.Contains(t.Id));
            }
            var count = GroupRepository.Count(condition);
            var groupList = GroupRepository.Where(condition)
                .OrderByDescending(u => u.AddedAt)
                .ThenBy(u => u.GroupName.Length)
                .ThenBy(u => u.GroupName)
                .Skip(searchGroupDto.Size * searchGroupDto.Page)
                .Take(searchGroupDto.Size)
                .ToList();
            var list = ParseGroupDtos(groupList);
            return DResult.Succ(list, count);
        }

        public DResults<GroupDto> SearchGroups(List<string> groupIds)
        {
            if (groupIds == null || groupIds.Count < 1)
                return DResult.Succ<GroupDto>(null, 0);
            var dtos = LoadGroupsFromDb(g => groupIds.Contains(g.Id));

            return !dtos.Any() ? DResult.Succ<GroupDto>(null, 0) : DResult.Succ(dtos, -1);
        }

        public Dictionary<string, DGroupDto> GroupDtoDict(ICollection<string> groupIds)
        {
            if (groupIds.IsNullOrEmpty())
                return new Dictionary<string, DGroupDto>();
            return GroupRepository.Where(g => groupIds.Contains(g.Id)).ToList()
                .MapTo<List<DGroupDto>>()
                .ToDictionary(k => k.Id, v => v);
        }

        public DResults<GroupDto> SearchGroupsByCode(List<string> codes)
        {
            if (codes == null || !codes.Any())
                return DResult.Errors<GroupDto>("圈号不能为空");
            var dtos = LoadGroupsFromDb(g => codes.Contains(g.GroupCode));
            return DResult.Succ(dtos, -1);
        }

        /// <summary> 查询分享圈子 </summary>
        /// <param name="channelId"></param>
        /// <param name="order"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public DResults<GroupDto> SearchShareGroups(int channelId, ShareGroupOrder order, DPage page)
        {
            var shares = ShareGroupRepository.Table;

            if (channelId > 0)
            {
                if (channelId % 100 == 0) //顶级
                {
                    var type = channelId / 100;

                    shares = shares.Where(u => u.ClassType >= type * 100 && u.ClassType < (type + 1) * 100);
                }
                else
                {
                    shares = shares.Where(u => u.ClassType == channelId);
                }
            }

            var groups =
                GroupRepository.Where(
                    u => u.GroupType == (byte)GroupType.Share && u.Status == (byte)NormalStatus.Normal)
                    .Join(shares, g => g.Id, s => s.Id, (g, s) => new
                    {
                        g.AddedAt,
                        g.Id,
                        s.TopicNum
                    });

            switch (order)
            {
                case ShareGroupOrder.AddAtAsc:
                    groups = groups.OrderBy(u => u.AddedAt);
                    break;
                case ShareGroupOrder.TopicNumDesc:
                    groups = groups.OrderByDescending(u => u.TopicNum);
                    break;
                default:
                    groups = groups.OrderByDescending(u => u.AddedAt);
                    break;
            }

            var count = groups.Count();
            var groupIds = groups.Skip(page.Size * page.Page).Take(page.Size).Select(u => u.Id).ToList();

            var result = SearchGroups(groupIds);
            result.TotalCount = count;

            return result;
        }

        public DResult<byte> GetGroupType(string groupId)
        {
            if (groupId.IsNullOrEmpty()) return DResult.Error<byte>("圈子ID不能为空");
            var groupInfo = GroupRepository
                .SingleOrDefault(u => u.Id == groupId && u.Status == (byte)NormalStatus.Normal);
            if (groupInfo == null) return DResult.Error<byte>("圈子不存在");
            return DResult.Succ((byte)groupInfo.GroupType);
        }

        #endregion

        public IEnumerable<string> GroupCodes()
        {
            return GroupRepository.Table.Select(g => g.GroupCode);
        }

        public DResult Update(UpdateGroupDto updateDto)
        {
            if (updateDto == null || string.IsNullOrWhiteSpace(updateDto.Id))
                return DResult.Error("圈子信息异常，请刷新重试！");
            var group = GroupRepository.Load(updateDto.Id);
            if (group == null)
                return DResult.Error("没有找到圈子信息！");
            if (group.Status == (byte)NormalStatus.Delete)
                return DResult.Error("圈子已经被解散！");
            var updateParams = new List<string>();
            if (updateDto.Name.IsNotNullOrEmpty())
            {
                group.GroupName = updateDto.Name;
                updateParams.Add("GroupName");
            }
            if (updateDto.Avatar.IsNotNullOrEmpty())
            {
                group.GroupAvatar = updateDto.Avatar;
                updateParams.Add("GroupAvatar");
            }
            if (updateDto.Banner.IsNotNullOrEmpty())
            {
                group.GroupBanner = updateDto.Banner;
                updateParams.Add("GroupBanner");
            }
            if (updateDto.Summary.IsNotNullOrEmpty())
            {
                group.GroupSummary = updateDto.Summary;
                updateParams.Add("GroupSummary");
            }
            var result = 0;
            if (updateParams.Any())
            {
                result = GroupRepository.Update(group, updateParams.ToArray());
                updateParams.Clear();
            }
            if (group.GroupType == (byte)GroupType.Share)
            {
                var shareGroup = ShareGroupRepository.Load(updateDto.Id);
                if (updateDto.PostAuth >= 0)
                {
                    shareGroup.PostAuth = (byte)updateDto.PostAuth;
                    updateParams.Add("PostAuth");
                }
                if (updateDto.Notice.IsNotNullOrEmpty())
                {
                    shareGroup.Notice = updateDto.Notice;
                    updateParams.Add("Notice");
                }
                if (updateDto.Tags.IsNotNullOrEmpty())
                {
                    shareGroup.Tags = updateDto.Tags;
                    updateParams.Add("Tags");
                }
                if (updateParams.Any())
                {
                    result += ShareGroupRepository.Update(shareGroup, updateParams.ToArray());
                }
            }
            return DResult.FromResult(result);
        }

        public DResult TransferOwner(string groupId, long userId, long operatorId)
        {
            var group = GroupRepository.Load(groupId);
            if (group == null)
                return DResult.Error("圈子不存在！");
            if (group.Status == (byte)NormalStatus.Delete)
                return DResult.Error("圈子已经被解散！");
            var isManager = IsManager(ParseGroupDto(group), operatorId);
            if (!isManager)
                return DResult.Error("只有圈主和管理员才能转让！");
            var status = IsGroupMember(userId, groupId);
            if (status != CheckStatus.Normal)
                return DResult.Error("该用户还不是圈子成员！");
            var user = UserContract.Load(userId);
            if (user == null)
                return DResult.Error("用户不存在！");
            if (!user.IsTeacher())
                return DResult.Error("只能转让给教师用户！");
            var result = UnitOfWork.Transaction(() =>
            {
                group.ManagerId = userId;
                GroupRepository.Update(g => new { g.ManagerId }, group);
                var title = "{0} 把“{1}”圈主权限转让给你了";
                var manager = UserContract.Load(operatorId);
                title = title.FormatWith(manager.Name, group.GroupName);
                MessageContract.SendMessage(new SystemMessageSendDto(userId)
                {
                    SenderId = operatorId,
                    Title = title
                });
            });
            return DResult.FromResult(result);
        }

        public DResult DissolutionGroup(string groupId, long userId)
        {
            var group = GroupRepository.Load(groupId);
            if (group == null)
                return DResult.Error("圈子不存在！");
            if (group.ManagerId != userId)
                return DResult.Error("只有圈主才能解散圈子！");
            group.Status = (byte)NormalStatus.Delete;
            var result = GroupRepository.Update(g => new { g.Status }, group);
            return DResult.FromResult(result);
        }

        public List<string> AgencyColleagueGroups(string agencyId)
        {
            var colleagues = ColleagueGroupRepository.Where(t => t.AgencyId == agencyId).Select(g => g.Id);
            if (!colleagues.Any())
                return new List<string>();
            return GroupRepository.Where(g =>
                g.GroupType == (byte)GroupType.Colleague && g.Status == (byte)NormalStatus.Normal &&
                g.CertificationLevel.HasValue)
                .Join(colleagues, s => s.Id, d => d, (s, d) => d).ToList();
        }

        public Dictionary<string, string> GroupDict(List<string> groupIds)
        {
            return GroupRepository.Where(g => groupIds.Contains(g.Id))
                .Select(t => new { t.Id, t.GroupName })
                .ToDictionary(k => k.Id, v => v.GroupName);
        }

        public bool IsManager(GroupDto @group, long userId)
        {
            if (group == null || userId <= 0)
                return false;
            if (group.ManagerId == userId)
                return true;
            Func<string, bool> isManager = agencyId =>
            {
                var result = UserContract.ApplicationAgency(userId, 1012);
                if (!result.Status)
                    return false;
                return result.Data.Id == agencyId;
            };
            switch (group.Type)
            {
                case (byte)GroupType.Share:
                    return false;
                case (byte)GroupType.Class:
                    var classGroup = group as ClassGroupDto;
                    if (classGroup == null)
                        return false;
                    //教务管理
                    return isManager(classGroup.AgencyId);
                case (byte)GroupType.Colleague:
                    var colleagueGroup = group as ColleagueGroupDto;
                    if (colleagueGroup == null)
                        return false;
                    //教务管理
                    return isManager(colleagueGroup.AgencyId);
            }
            return false;
        }

        #region 圈子认证
        /// <summary>
        /// 圈子认证
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="isAuth">是否认证</param>
        /// <returns></returns>
        public DResult GroupCertificate(string groupId, bool isAuth)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return DResult.Error("圈子ID无效！");
            var group = GroupRepository.Load(groupId);
            if (group == null)
                return DResult.Error("圈子不存在！");
            if (isAuth)
                group.CertificationLevel = (int)GroupCertificationLevel.Normal;
            else
                group.CertificationLevel = (int)GroupCertificationLevel.Refuse;
            var result = GroupRepository.Update(g => new { g.CertificationLevel }, group);
            return DResult.FromResult(result);
        }
        #endregion

        # region 圈子批量导入用户

        ///  <summary>
        /// 圈子批量导入学生
        ///  </summary>
        ///  <param name="students">用户列表</param>
        ///  <param name="groupId">圈子ID</param>
        ///  <param name="agencyId">机构ID</param>
        /// <param name="stage"></param>
        /// <returns></returns>
        public DResult BatchImportStudent(string[] students, string groupId, string agencyId, byte stage)
        {
            var result = new ImportMessageDto(); //返回数据
            string[] newArray; //排重后的数据列表
            var memberList = new List<TG_Member>(); //圈成员实体
            if (!students.Any())
            {
                return DResult.Error("导入用户不能为空");
            }
            var group = GroupRepository.Where(w => w.Id == groupId).FirstOrDefault();
            if (group == null)
            {
                return DResult.Error("没有找到该圈子");
            }
            var namesArray =
                GroupMembers(groupId, UserRole.Student)
                    .Data.Where(w => students.Contains(w.Name))
                    .Select(w => w.Name)
                    .ToArray(); //找到重复的学生
            if (namesArray.Any())
            {
                newArray = students.Where(w => !namesArray.Contains(w)).ToArray(); //排除导入列表中该圈子已经包含的学生

            }
            else
            {
                newArray = students;
            }
            result.MessageCount = newArray.Length;
            result.RepeatCount = namesArray.Length;
            result.RepeatUsers = namesArray;
            if (!newArray.Any())
                return DResult.Succ(result);
            var users = UserContract.BatchImportStudents(newArray, agencyId, stage);
            if (users == null || !users.Any())
            {
                result.MessageCount = 0;
                return DResult.Succ(result);
            }
            foreach (var user in users)
            {
                var id = user.Id;
                var newMember = new TG_Member
                {
                    AddedAt = Clock.Now,
                    BusinessCard = string.Empty,
                    GroupId = groupId,
                    Id = IdHelper.Instance.Guid32,
                    MemberId = id,
                    MemberRole = (byte)UserRole.Student,
                    Status = (byte)CheckStatus.Normal
                };
                memberList.Add(newMember);
            }
            var code = UnitOfWork.Transaction(() =>
            {
                MemberRepository.Insert(memberList);
                @group.MemberCount += users.Count;
                GroupRepository.Update(w => new { w.MemberCount }, @group);
            });
            return code > 0 ? DResult.Succ(result) : DResult.Error("导入失败！");
        }

        /// <summary>
        /// 批量导入教师
        /// </summary>
        /// <param name="tIds">教师ID</param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public DResult BatchImportTeacher(long[] tIds, string groupId)
        {
            ImportMessageDto result = new ImportMessageDto();
            List<BatchTeacherDto> newList = null;
            List<TG_Member> memberList = new List<TG_Member>();//圈成员实体
            List<string> updateList = new List<string>();
            if (tIds == null || !tIds.Any())
            {
                result.MessageCount = 0;
                result.RepeatCount = 0;
                return new DResult<ImportMessageDto>(false, result);
            }
            var group = GroupRepository.Where(w => w.Id == groupId).FirstOrDefault();
            if (group == null)
            {
                result.MessageCount = 0;
                result.RepeatCount = 0;
                return new DResult<ImportMessageDto>(false, result);
            }
            //取得传入的教师
            var teachers = UserRepository.
                Where(w => tIds.Contains(w.Id) && w.Status == (byte)NormalStatus.Normal).
               Select(w =>
               new
               {
                   NikeName = w.NickName,
                   Name = w.TrueName,
                   Id = w.Id,
                   SubjectId = w.SubjectID.Value
               });
            if (!teachers.Any())
            {
                result.MessageCount = 0;
                result.RepeatCount = 0;
                return new DResult<ImportMessageDto>(false, result);
            }

            //取得当前选择教师科目重复的项，不被添加
            int[] temp = (from l in teachers group l by l.SubjectId into g where g.Count() > 1 select g.Key).ToArray();
            //圈子已经包含科目的教师
            //if (temp.Length > 0)
            //{
            //    var t = teachers.Where(w => temp.Contains(w.SubjectId)&&!repeatUsers.Contains(w.Name)).Select(w => w.Name).ToList();
            //    if(t.Count>0)
            //    repeatUsers.AddRange(t);
            //    newList = newList.Where(w => !temp.Contains(w.SubjectId.Value));//去掉选择同种科目类型的教师
            //    newList = newList.Select(dto => new BatchTeacherDto
            //    {
            //        Id = dto.Id,
            //        SubjectId = dto.SubjectId
            //    }).ToList();
            //}
            if (temp.Any())
            {
                var t = teachers.Where(w => temp.Contains(w.SubjectId)).Select(w => w.Name).ToArray();
                result.RepeatUsers = t;
                result.RepeatCount = t.Count();
                result.MessageCount = 0;
                return new DResult<ImportMessageDto>(false, result);
            }
            //取得圈子已经包含的教师
            var groupTeachers = GroupMembers(groupId, UserRole.Teacher).Data;
            //已包含科目
            List<int> subList = new List<int>();
            if (groupTeachers.Any())
            {
                //通过教师取得圈子已经包含的科目
                subList = groupTeachers.Select(w => w.SubjectId).ToList();
                //获取排重后的教师集合
                newList = teachers.
                    Where(w => !subList.Contains(w.SubjectId)).
                    Select(dto => new BatchTeacherDto
                    {
                        Id = dto.Id,
                        SubjectId = dto.SubjectId
                    }).ToList();
            }
            else
            {
                newList = teachers.
                    Select(dto => new BatchTeacherDto
                    {
                        Id = dto.Id,
                        SubjectId = dto.SubjectId
                    }).ToList();
            }
            if (!newList.Any())
            {
                result.MessageCount = 0;
                return DResult.Succ(result);
            }
            //组装数据
            foreach (var item in newList)
            {
                var id = MemberRepository.Where(m => m.GroupId == groupId && m.MemberId == item.Id).Select(m => m.Id).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(id))
                {
                    updateList.Add(id);
                    continue;
                }
                var newMember = new TG_Member
                {
                    Id = IdHelper.Instance.Guid32,
                    AddedAt = Clock.Now,
                    BusinessCard = string.Empty,
                    GroupId = groupId,
                    MemberId = item.Id,
                    MemberRole = (byte)UserRole.Teacher,
                    Status = (byte)CheckStatus.Normal
                };
                memberList.Add(newMember);
            }
            result.MessageCount = newList.Count();
            var resultId = UnitOfWork.Transaction(() =>
            {
                if (group.ManagerId == 0)
                {
                    group.ManagerId = newList.First().Id;
                }
                group.MemberCount += newList.Count();
                GroupRepository.Update(w => new { w.ManagerId, w.MemberCount }, group);
                if (memberList != null && memberList.Any())
                    MemberRepository.Insert(memberList.ToArray());
                if (updateList != null && updateList.Any())
                    MemberRepository.Update(new TG_Member { Status = (byte)CheckStatus.Normal }, m => updateList.Contains(m.Id), nameof(TG_Member.Status));
            });
            return resultId < 0 ? DResult.Error("导入教师失败！") : DResult.Succ(result);
        }

        public DResult ColleagueBatchTeacher(long[] tIds, string groupId)
        {
            ImportMessageDto result = new ImportMessageDto();
            List<TG_Member> members = new List<TG_Member>();
            List<string> updateList = new List<string>();
            if (!tIds.Any())
            {
                return new DResult<ImportMessageDto>(false, result);
            }
            var group = GroupRepository.Load(groupId);
            if (group == null)
            {
                return new DResult<ImportMessageDto>(false, result);
            }
            var groupTeachers = GroupMembers(groupId, UserRole.Teacher).Data;//圈子已经包含的教师
            var groupTids = groupTeachers.Select(w => w.Id).ToArray();//圈子包含教师ID
            var oldTids = tIds.Where(w => groupTids.Contains(w));//传入教师重复IDS
            var newTids = tIds.Where(w => !groupTids.Contains(w));//没有重复的教师
            //教师
            var teachers = UserRepository.Where(w => newTids.Contains(w.Id) && w.Status == (byte)NormalStatus.Normal);
            //重复的教师
            var repeatTeachers = UserRepository.
                Where(w => oldTids.Contains(w.Id) && w.Status == (byte)NormalStatus.Normal).
                Select(w => w.TrueName);
            if (!repeatTeachers.Any())
            {
                result.RepeatCount = repeatTeachers.Count();
                result.RepeatUsers = repeatTeachers.ToArray();
            }
            else
            {
                result.RepeatCount = 0;
                result.RepeatUsers = null;
            }

            if (!teachers.Any())
            {
                result.MessageCount = 0;
                return new DResult<ImportMessageDto>(false, result);
            }
            result.MessageCount = teachers.Count();
            //组装数据
            foreach (var item in teachers)
            {
                var id = MemberRepository.Where(m => m.GroupId == groupId && m.MemberId == item.Id).Select(m => m.Id).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(id))
                {
                    updateList.Add(id);
                    continue;
                }
                var newMember = new TG_Member
                {
                    Id = IdHelper.Instance.Guid32,
                    AddedAt = Clock.Now,
                    BusinessCard = string.Empty,
                    GroupId = groupId,
                    MemberId = item.Id,
                    MemberRole = (byte)UserRole.Teacher,
                    Status = (byte)CheckStatus.Normal
                };
                members.Add(newMember);
            }
            var resultId = UnitOfWork.Transaction(() =>
            {
                if (group.ManagerId == 0)
                {
                    group.ManagerId = teachers.First().Id;
                }
                group.MemberCount += teachers.Count();
                GroupRepository.Update(w => new { w.MemberCount, w.ManagerId }, group);
                if (members != null && members.Any())
                    MemberRepository.Insert(members.ToArray());
                if (updateList != null && updateList.Any())
                    MemberRepository.Update(new TG_Member { Status = (byte)CheckStatus.Normal }, m => updateList.Contains(m.Id), nameof(TG_Member.Status));
            });

            return resultId < 0 ? DResult.Error("导入教师失败！") : DResult.Succ(result);
        }

        /// <summary> 获取重复的圈子名称 </summary>
        /// <param name="dto"></param>
        /// <param name="agencyId"></param>
        /// <returns></returns>
        public DResult<OutGroupMessage> GetGroupRepeatMsg(BatchCreateGroupsDto dto, string agencyId)
        {
            var outMsg = new OutGroupMessage();
            if (dto == null)
            {
                return DResult.Error<OutGroupMessage>("圈子不能为空");
            }
            string[] reperatClass = { };
            string[] reperatColleagueGroup = { };
            var classGroupNames = dto.ClassGroups.Select(w => w.Name).ToArray();
            var colleagueGroupNames = dto.ColleagueGroups.Select(w => w.Name).ToArray();
            if (classGroupNames.Any())
            {
                reperatClass = CheckGroupName(classGroupNames, GroupType.Class, true, agencyId);
            }
            if (colleagueGroupNames.Any())
            {
                reperatColleagueGroup = CheckGroupName(colleagueGroupNames, GroupType.Colleague, true, agencyId);
            }
            if (reperatClass.Length == 0 && reperatColleagueGroup.Length == 0)
            {
                return DResult.Error<OutGroupMessage>("没有重复的圈子");
            }
            outMsg.ClassCount = reperatClass.Length;
            outMsg.ColleagueCount = reperatColleagueGroup.Length;
            outMsg.GroupsSuccess.AddRange(reperatClass);
            outMsg.GroupsSuccess.AddRange(reperatColleagueGroup);
            return DResult.Succ(outMsg);
        }
        /// <summary>
        ///  Execel导入学生
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="groupId">圈子ID</param>
        /// <param name="agencyId">机构ID</param>
        /// <param name="stage">学段号</param>
        /// <returns></returns>
        public DResult<OutGroupMessage> ExecelImportSutdents(string path, string groupId, string agencyId, int stage)
        {
            //DataTable dtn = new DataTable();
            //string[] column = {"学生姓名"};
            //OutGroupMessage result = new OutGroupMessage();
            //DataSet ds = ExcelHelper.Read(path);
            //DataTable dt = ds.Tables[0];
            //dtn = dt.DefaultView.ToTable("newTable", true, column);
            //dt.Select()
            //if (ds.Tables == null)
            //{

            //    foreach (DataRow item in ds.Tables[0].Rows)
            //    {
            //        item[column].
            //    }
            //}
            return null;
        }
        #endregion
    }
}
