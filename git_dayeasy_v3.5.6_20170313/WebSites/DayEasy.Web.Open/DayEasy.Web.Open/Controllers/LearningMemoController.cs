using System;
using System.Web.Http;
using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Models.Open.Group;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Web.Api;
using DayEasy.Web.Api.Attributes;

namespace DayEasy.Web.Open.Controllers
{
    /// <summary> 学习笺相关接口 </summary>
    [DApi]
    public class LearningMemoController : DApiController
    {
        public LearningMemoController(IUserContract userContract)
            : base(userContract)
        {
        }

        /// <summary> 保存学习笺 </summary>
        [HttpPost]
        [DApiAuthorize(UserRole.Teacher)]
        public DResult<string> Save()
        {
            string memo = "memo".Form(string.Empty);
            throw new NotImplementedException();
        }

        #region 学习组接口
        /// <summary> 学生组列表 </summary>
        /// <returns></returns>
        [HttpGet]
        [DApiAuthorize(UserRole.Teacher)]
        public DResults<MMemberDto> Groups()
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [DApiAuthorize(UserRole.Teacher)]
        public DResult<MMemberDto> Group(string groupId)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [DApiAuthorize(UserRole.Teacher)]
        public DResult CreateGroup(string userIds, string profile)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [DApiAuthorize(UserRole.Teacher)]
        public DResult EditGroupName(string groupId, string name)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [DApiAuthorize(UserRole.Teacher)]
        public DResult DeleteGroup(string groupId)
        {
            throw new NotImplementedException();
        }
        #endregion

        /// <summary> 学习笺列表 </summary>
        /// <returns></returns>
        [HttpGet]
        public DResults<string> List()
        {
            throw new NotImplementedException();
        }

        /// <summary> 学习笺详情 </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        public DResult<string> Detail(string batch)
        {
            throw new NotImplementedException();
        }

        /// <summary> 学习笺评论 </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        public DResults<string> Reviews(string batch)
        {
            throw new NotImplementedException();
        }

        /// <summary> 发布学习笺 </summary>
        /// <param name="memoId"></param>
        /// <param name="groupId"></param>
        /// <param name="userIds"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        public DResult Publish(string memoId, string groupId, string userIds, string profile)
        {
            throw new NotImplementedException();
        }

        /// <summary> 评价学习笺 </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        public DResult Evaluate(string batch)
        {
            throw new NotImplementedException();
        }

        /// <summary> 评价评论的内容 </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DResult ReviewEvaluate(string id)
        {
            throw new NotImplementedException();
        }

        /// <summary> 评论学习笺 </summary>
        /// <param name="content"></param>
        /// <param name="type"></param>
        /// <param name="batch"></param>
        /// <returns></returns>
        public DResult Review(string content, int type, string batch)
        {
            throw new NotImplementedException();
        }
    }
}
