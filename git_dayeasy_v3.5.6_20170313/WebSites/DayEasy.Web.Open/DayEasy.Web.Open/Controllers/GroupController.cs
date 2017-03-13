using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain;
using DayEasy.Models.Open.Group;
using DayEasy.Models.Open.User;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Timing;
using DayEasy.Web.Api;
using DayEasy.Web.Api.Attributes;

namespace DayEasy.Web.Open.Controllers
{
    /// <summary> 圈子相关接口 </summary>
    [DApiAuthorize(UserRole.Teacher | UserRole.Student)]
    public class GroupController : DApiController
    {
        private readonly IGroupContract _groupContract;
        private readonly IMessageContract _messageContract;


        public GroupController(IUserContract userContract, IGroupContract groupContract,
            IMessageContract messageContract)
            : base(userContract)
        {
            _groupContract = groupContract;
            _messageContract = messageContract;
        }

        #region 相关查询

        /// <summary> 搜索圈子 </summary>
        [HttpGet]
        public DResults<MGroupDto> Search(string keyword, int type = -1, int page = 0, int size = 15)
        {
            List<int> types;
            if (type < 0)
            {
                types = new List<int>
                {
                    (int) GroupType.Class,
                    //(int) GroupType.Share
                };
                if (CurrentUser.IsTeacher())
                {
                    types.Add((int)GroupType.Colleague);
                }
            }
            else
            {
                types = new List<int> { type };
            }
            if (string.IsNullOrWhiteSpace(keyword))
            {
                types.Remove((int)GroupType.Class);
                types.Remove((int)GroupType.Colleague);
            }
            if (types.IsNullOrEmpty())
                return DResult.Succ(new List<MGroupDto>(), 0);

            var dPage = DPage.NewPage(page - 1, size);
            var searchDto = new SearchGroupDto
            {
                Keyword = keyword,
                Page = dPage.Page,
                Size = dPage.Size,
                Types = types
            };
            var result = _groupContract.SearchGroups(searchDto);
            return (result.Status
                ? DResult.Succ(result.Data.MapTo<List<MGroupDto>>(), result.TotalCount)
                : DResult.Errors<MGroupDto>(result.Message));
        }

        /// <summary> 用户圈子列表 </summary>
        [HttpGet]
        public DResults<MGroupDto> Groups(int type = -1, bool loadMessage = false)
        {
            var result = _groupContract.Groups(ChildOrUserId, type, loadMessage);
            if (result.Status)
            {
                return DResult.Succ(result.Data.MapTo<List<MGroupDto>>(), result.TotalCount);
            }
            return DResult.Errors<MGroupDto>(result.Message);
        }

        /// <summary> 圈子成员列表 </summary>
        [HttpGet]
        public DResults<MMemberDto> Members(string id, int role = -1, bool includeParents = false)
        {
            var result = _groupContract.GroupMembers(id, (role == -1 ? UserRole.Caird : (UserRole)role), includeParents);
            if (result.Status)
            {
                return DResult.Succ(result.Data.MapTo<List<MMemberDto>>(), result.TotalCount);
            }
            return DResult.Errors<MMemberDto>(result.Message);
        }

        /// <summary> 是否是圈子成员 </summary>
        [HttpGet]
        public DResult<int> IsMember(string id)
        {
            var status = _groupContract.IsGroupMember(ChildOrUserId, id);
            return DResult.Succ((int)status);
        }

        /// <summary> 获取圈内学生列表 </summary>
        /// <param name="code">圈号</param>
        /// <returns></returns>
        [HttpGet]
        [DApiAuthorize(UserRole.Teacher)]
        public DResult<MStudentListDto> Students(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || !code.StartsWith("GC", StringComparison.CurrentCultureIgnoreCase))
                return DResult.Error<MStudentListDto>("圈号不正确！");
            var group = _groupContract.LoadByCode(code);
            if (!group.Status || group.Data == null)
                return DResult.Error<MStudentListDto>("圈号不存在！");

            var members = _groupContract.GroupMembers(group.Data.Id, UserRole.Student);
            if (!members.Status)
                return DResult.Error<MStudentListDto>(members.Message);

            return DResult.Succ(new MStudentListDto
            {
                GroupId = group.Data.Id,
                GroupName = group.Data.Name,
                Students = members.Data.Select(t => new MUserBaseDto
                {
                    Id = t.Id,
                    Name = t.Name
                }).ToList()
            });
        }

        /// <summary> 待审核列表 </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [DApiAuthorize(UserRole.Teacher)]
        public DResults<PendingUserDto> Pendings(string id)
        {
            return _groupContract.PendingList(id, UserId);
        }

        #endregion

        #region 成员操作

        /// <summary> 申请加入圈子 </summary>
        [HttpPost]
        public DResult Apply(MGroupApplyInputDto dto)
        {
            //家长角色，分享圈用家长ID、班级圈用关联的学生ID
            if (!CurrentUser.IsParents())
                return _groupContract.ApplyGroup(dto.GroupId, UserId, dto.Message, dto.Name);

            var groupTypeResult = _groupContract.GetGroupType(dto.GroupId);
            if (!groupTypeResult.Status) return groupTypeResult;

            var userId = groupTypeResult.Data == (byte) GroupType.Class ? ChildOrUserId : UserId;
            return _groupContract.ApplyGroup(dto.GroupId, userId, dto.Message, dto.Name);
        }

        /// <summary> 退出圈子 </summary>
        [HttpGet]
        public DResult Quit(MGroupInputDto dto)
        {
            if(!CurrentUser.IsParents())
            return _groupContract.QuitGroup(UserId, dto.GroupId);

            var groupTypeResult = _groupContract.GetGroupType(dto.GroupId);
            if (!groupTypeResult.Status) return groupTypeResult;

            var userId = groupTypeResult.Data == (byte)GroupType.Class ? ChildOrUserId : UserId;
            return _groupContract.QuitGroup(userId, dto.GroupId);
        }

        #endregion

        #region 圈子管理

        /// <summary> 创建圈子 </summary>
        [HttpPost]
        public DResult<GroupDto> Create(MGroupCreateInputDto createDto)
        {
            var group = new GroupDto
            {
                Type = (byte)createDto.Type,
                Name = createDto.Name,
                GroupSummary = createDto.Summary,
                Capacity = 200,
                ManagerId = UserId,
                CreationTime = Clock.Now
            };
            switch (createDto.Type)
            {
                case (int)GroupType.Class:
                    var classGroup = @group.MapTo<ClassGroupDto>();
                    classGroup.Capacity = 200;
                    classGroup.AgencyId = createDto.AgencyId;
                    classGroup.GradeYear = createDto.GradeYear;
                    classGroup.Stage = (byte)createDto.Stage;
                    return _groupContract.CreateGroup(classGroup, (int)UserRole.Teacher, createDto.UserName);
                case (int)GroupType.Colleague:
                    var colleagueGroup = @group.MapTo<ColleagueGroupDto>();
                    colleagueGroup.Capacity = 100;
                    colleagueGroup.AgencyId = createDto.AgencyId;
                    colleagueGroup.SubjectId = CurrentUser.SubjectId;
                    colleagueGroup.Stage = (byte)createDto.Stage;
                    return _groupContract.CreateGroup(colleagueGroup, (int)UserRole.Teacher, createDto.UserName);
                default:
                    return DResult.Error<GroupDto>("圈子类型异常！");
            }
        }

        /// <summary> 更新圈子信息 </summary>
        [HttpPost]
        public DResult Update(MGroupUpdateInputDto dto)
        {
            return _groupContract.Update(new UpdateGroupDto
            {
                Id = dto.GroupId,
                Avatar = dto.Logo,
                Banner = dto.Banner,
                Name = dto.Name,
                Notice = dto.Notice,
                Summary = dto.Summary
            });
        }

        /// <summary> 成员申请审核 </summary>
        [HttpPost]
        [DApiAuthorize(UserRole.Teacher)]
        public DResult Verify(MGroupVerifyInputDto dto)
        {
            return _groupContract.Verify(dto.RecordId, (CheckStatus)dto.Status, dto.Message);
        }

        /// <summary> 删除成员 </summary>
        [HttpPost]
        [DApiAuthorize(UserRole.Teacher)]
        public DResult DeleteMember(MGroupDeleteMemberInputDto dto)
        {
            return _groupContract.DeleteMember(dto.GroupId, dto.MemberId, UserId);
        }

        /// <summary> 解散圈子 </summary>
        [HttpPost]
        public DResult Dissolution(MGroupInputDto dto)
        {
            return _groupContract.DissolutionGroup(dto.GroupId, UserId);
        }

        #endregion

        #region 圈子动态

        /// <summary> 发送动态消息 </summary>
        [HttpPost]
        public DResult SendDynamic(MGroupSendNoticeInputDto dto)
        {
            var result = _messageContract.SendDynamic(new DynamicSendDto
            {
                DynamicType = (GroupDynamicType)dto.Type,
                GroupId = dto.GroupId,
                ReceivRole = UserRole.Student | UserRole.Teacher,
                UserId = UserId,
                Message = dto.Message,
                Title = dto.Title,
                RecieveIds = dto.ReceiverList
            });
            return result;
        }

        /// <summary> 圈子动态 </summary>
        [HttpGet]
        public DResult<DynamicMessageResultDto> Dynamics([FromUri] MDynamicSearchDto searchDto)
        {
            var dto = searchDto.MapTo<DynamicSearchDto>();
            if (CurrentUser.IsParents())
            {
                dto.UserId = ChildOrUserId;
                dto.ParentId = UserId;
                dto.Role = UserRole.Student;
            }
            else
            {
                dto.UserId = UserId;
                dto.Role = (UserRole)CurrentUser.Role;
            }
            var result = _messageContract.GetDynamics(dto);
            _groupContract.UpdateLastTime(dto.GroupId, dto.UserId);
            return result;
        }

        /// <summary> 动态点赞 </summary>
        [HttpPost]
        public DResult<int> SupportDynamic(MGroupDynamicInputDto dto)
        {
            return _messageContract.LikeDynamic(dto.DynamicId, ChildOrUserId);
        }

        #endregion
    }
}
