
using System.Linq;
using System.Web.Http;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Utility.Extend;

namespace DayEasy.Web.Api
{
    public abstract class DApiController : ApiController
    {
        protected readonly IUserContract UserContract;

        protected DApiController(IUserContract userContract)
        {
            UserContract = userContract;
        }

        private UserDto _user;

        public UserDto CurrentUser
        {
            get
            {
                if (_user != null)
                    return _user;
                var token = "token".QueryOrForm(string.Empty);
                if (string.IsNullOrWhiteSpace(ToString()))
                    return null;
                var comefrom = "comefrom".QueryOrForm(-1);
                if (comefrom < 0)
                    comefrom = PartnerBusi.Instance.Comefrom;
                return _user = UserContract.Load(token, (Comefrom)comefrom);
            }
        }

        public long ChildOrUserId
        {
            get
            {
                if (_user == null || !_user.IsParents())
                    return UserId;
                var child = UserContract.Children(UserId);
                if (child != null && child.Any())
                    return child.First().Id;
                return UserId;
            }
        }

        public long UserId
        {
            get { return (_user == null ? 0 : _user.Id); }
        }
    }
}
