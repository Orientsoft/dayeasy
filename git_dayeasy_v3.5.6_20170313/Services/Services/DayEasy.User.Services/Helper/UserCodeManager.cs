using DayEasy.Contracts;
using DayEasy.Core.Dependency;
using DayEasy.Utility;
using DayEasy.Utility.License;

namespace DayEasy.User.Services.Helper
{
    internal class UserCodeManager
    {
        private readonly LicenseHelper _helper;
        private UserCodeManager()
        {
            LicenseManager.InitCacheHandler += type =>
            {
                var userContract = CurrentIocManager.Resolve<IUserContract>();
                if (type == LicenseType.DCode)
                    return userContract.DCodes();
                return null;
            };

            LicenseManager.SetGenerateRole(LicenseType.DCode, DCode.Code);

            _helper = LicenseManager.Instance(LicenseType.DCode);
        }

        public static UserCodeManager Instance
        {
            get
            {
                return Singleton<UserCodeManager>.Instance ??
                       (Singleton<UserCodeManager>.Instance = new UserCodeManager());
            }
        }

        public string Code()
        {
            return _helper.Code();
        }
    }
}
