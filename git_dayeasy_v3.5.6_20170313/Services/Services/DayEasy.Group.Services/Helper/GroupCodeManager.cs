
using System.Linq;
using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Utility;
using DayEasy.Utility.License;

namespace DayEasy.Group.Services.Helper
{
    /// <summary> 圈号生成器 </summary>
    internal class GroupCodeManager
    {
        private readonly LicenseHelper _helper;
        private GroupCodeManager(IGroupContract groupContract)
        {
            LicenseManager.InitCacheHandler += type =>
            {
                if (type == LicenseType.GroupCode)
                    return groupContract.GroupCodes().ToList();
                return null;
            };

            LicenseManager.SetGenerateRole(LicenseType.GroupCode, DCode.Code);
            _helper = LicenseManager.Instance(LicenseType.GroupCode);
        }

        public static GroupCodeManager Instance(IGroupContract groupContract)
        {
            return Singleton<GroupCodeManager>.Instance ??
                   (Singleton<GroupCodeManager>.Instance = new GroupCodeManager(groupContract));
        }

        private static string GetPrefix(GroupType type)
        {
            switch (type)
            {
                case GroupType.Class:
                    return "GC";
                case GroupType.Colleague:
                    return "GP";
                case GroupType.Share:
                    return "GS";
                default:
                    return string.Empty;
            }
        }

        public static string GetPrefix(int type)
        {
            return GetPrefix((GroupType)type);
        }

        public string GroupCode(GroupType type)
        {
            return _helper.Code(GetPrefix(type));
        }
    }
}
