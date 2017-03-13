using System.Web;
using DayEasy.Utility.Extend;

namespace DayEasy.Utility.Helper
{
    public class SessionHelper
    {
        /// <summary> 添加session缓存 </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="exprise">过期时间(分钟)</param>
        public static void Add(string key, object value, int exprise = 0)
        {
            HttpContext.Current.Session.Add(key, value);
            if (exprise > 0)
                HttpContext.Current.Session.Timeout = exprise;
        }

        public static T Get<T>(string key)
        {
            var obj = HttpContext.Current.Session[key];
            return obj == null ? default(T) : obj.CastTo<T>();
        }

        public static void Remove(string key)
        {
            HttpContext.Current.Session.Remove(key);
        }
    }
}
