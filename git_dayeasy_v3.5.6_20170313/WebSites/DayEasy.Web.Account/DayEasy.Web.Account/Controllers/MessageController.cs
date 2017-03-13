using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain;
using DayEasy.Utility;
using DayEasy.Web.Filters;

namespace DayEasy.Web.Account.Controllers
{
    [DAuthorize(true)]
    [RoutePrefix("msg")]
    public class MessageController : DController
    {
        private readonly IMessageContract _messageContract;

        public MessageController(
            IUserContract userContract,
            IMessageContract messageContract)
            : base(userContract)
        {
            _messageContract = messageContract;
        }

        [Route("")]
        public ActionResult List(int pageindex = 1, int pagesize = 15)
        {
            var result = _messageContract.UserMessages(CurrentUser.Id, page: DPage.NewPage((pageindex - 1), pagesize));
            if (!result.Status || !result.Data.Any())
                return View();
            var ids = result.Data.Select(m => m.Id).ToList();
            if (ids.Any())
            {
                //更新已读状态
                _messageContract.UpdateMessageStatus(UserId, ids, MessageStatus.Read);
            }
            ViewBag.TotalCount = result.TotalCount;
            ViewBag.CurrentPage = pageindex;
            return View(result.Data);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("del")]
        public ActionResult Delete(string id)
        {
            var result = _messageContract.UpdateMessageStatus(CurrentUser.Id, new List<string> { id }, MessageStatus.Delete);
            return DeyiJson(result ? DResult.Success : DResult.Error("删除失败，请稍后重试！"), true);
        }

        [AjaxOnly]
        [Route("~/message/count")]
        public int MessageCount()
        {
            return _messageContract.UserMessageCount(UserId);
        }
    }
}