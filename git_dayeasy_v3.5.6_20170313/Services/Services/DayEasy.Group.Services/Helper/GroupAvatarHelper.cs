
using System.Linq;
using DayEasy.Core.Config;
using DayEasy.Utility.Config;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;

namespace DayEasy.Group.Services.Helper
{
    /// <summary> 默认圈子头像~ </summary>
    public static class GroupAvatarHelper
    {
        /// <summary> 获取默认Logo </summary>
        /// <returns></returns>
        public static string Avatar()
        {
            var config = ConfigUtils<RecommendImageConfig>.Config;
            if (config == null || config.Recommends.IsNullOrEmpty())
                return string.Empty;
            var recommend = config.Recommends.FirstOrDefault(t => t.Type == RecommendImageType.GroupLogo);
            if (recommend == null || recommend.Images.IsNullOrEmpty())
                return string.Empty;
            var item = recommend.Images[RandomHelper.Random().Next(recommend.Images.Count)];
            return item.ImageUrl;
        }
    }
}
