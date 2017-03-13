using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Management;

namespace DayEasy.Web.ManageMent.Filters
{
    public class AdminController : DController
    {
        protected IManagementContract ManagementContract { get; private set; }
        private long _managerRole = -1;

        public long Role
        {
            get
            {
                if (_managerRole >= 0)
                    return _managerRole;
                _managerRole = (UserId <= 0 ? 0 : ManagementContract.AdminRole(UserId));
                return _managerRole;
            }
        }

        public AdminController(IUserContract userContract, IManagementContract managementContract)
            : base(userContract)
        {
            ManagementContract = managementContract;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewData["AdminRole"] = Role;
        }
    }
}