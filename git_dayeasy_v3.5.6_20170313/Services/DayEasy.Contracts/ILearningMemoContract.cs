
using DayEasy.Contracts.Dtos.LearningMemo;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    /// <summary> 学习笺相关契约 </summary>
    public interface ILearningMemoContract
    {
        /// <summary> 保存学习笺 </summary>
        /// <returns></returns>
        DResult<string> Save(LearningMemoDto memo);

        /// <summary> 教师 - 学生组列表 </summary>
        /// <param name="teacherId"></param>
        /// <param name="status"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        DResults<MemoGroupDto> Groups(long teacherId, int status, int page = 0, int size = 12);

        /// <summary> 教师 - 学生组详情 </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        DResult<MemoGroupDto> Group(string groupId);

        /// <summary> 创建学生组 </summary>
        /// <param name="teacherId">教师ID</param>
        /// <param name="userIds">用户IDs</param>
        /// <param name="name">组名</param>
        /// <param name="profile">头像</param>
        /// <returns></returns>
        DResult<string> CreateGroup(long teacherId, long[] userIds, string name = null, string profile = null);

        /// <summary> 编辑学生组名字 </summary>
        /// <param name="groupId">组ID</param>
        /// <param name="name">名字</param>
        /// <returns></returns>
        DResult EditGroupName(string groupId, string name);

        /// <summary> 删除学生组 </summary>
        /// <param name="groupId">组ID</param>
        /// <returns></returns>
        DResult DeleteGroup(string groupId);

        /// <summary> 学习笺列表 </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="subjectId">科目ID</param>
        /// <param name="groupId">分组ID</param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        DResults<MemoUsageDto> List(long userId, int subjectId = 0, string groupId = null, int page = 0, int size = 12);

        /// <summary> 学习笺详情 </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        DResult<MemoUsageDto> Detail(string batch);

        /// <summary> 评论列表 </summary>
        /// <param name="user"></param>
        /// <param name="batch">批次号</param>
        /// <param name="isTeacher">是否教师</param>
        /// <param name="desc">是否倒序</param>
        /// <param name="page">页码</param>
        /// <param name="size">显示数据</param>
        /// <returns></returns>
        DResults<UsageReviewDto> Reviews(UserDto user, string batch, bool isTeacher, bool desc = false, int page = 0,
            int size = 12);

        /// <summary>发布学习笺</summary>
        /// <param name="userIds">用户ID列表</param>
        /// <param name="user"></param>
        /// <param name="memoId">学习笺ID</param>
        /// <param name="profile">组头像</param>
        /// <returns></returns>
        DResult Publish(UserDto user, string memoId, long[] userIds, string profile);

        /// <summary> 发布学习笺 </summary>
        /// <param name="groupId">用户组ID</param>
        /// <param name="user"></param>
        /// <param name="memoId">学习笺ID</param>
        /// <returns></returns>
        DResult Publish(UserDto user, string memoId, string groupId);

        /// <summary> 评价 </summary>
        /// <param name="user">用户</param>
        /// <param name="batch">批次号</param>
        /// <param name="isGood">是否喜欢</param>
        /// <returns></returns>
        DResult Evaluate(UserDto user, string batch, bool isGood = true);

        /// <summary> 评价评论的内容 </summary>
        /// <param name="user">用户信息</param>
        /// <param name="id">评论ID</param>
        /// <param name="isGood">是否喜欢</param>
        /// <returns></returns>
        DResult ReviewEvaluate(UserDto user, string id, bool isGood = true);

        /// <summary> 评论 </summary>
        /// <param name="user">用户信息</param>
        /// <param name="type">评论类型</param>
        /// <param name="content">内容</param>
        /// <param name="batch">批次号</param>
        /// <param name="parentId">回复 父级ID</param>
        /// <returns></returns>
        DResult Review(UserDto user, string content, byte type, string batch, string parentId = null);
    }
}
