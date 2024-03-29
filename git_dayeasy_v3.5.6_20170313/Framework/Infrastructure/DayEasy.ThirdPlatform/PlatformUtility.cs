﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using DayEasy.Utility;
using DayEasy.Utility.Extend;

namespace DayEasy.ThirdPlatform
{
    internal static class PlatformUtility
    {
        internal static NameValueCollection GetContext(string str)
        {
            var val = new NameValueCollection();
            try
            {
                if (str.IsNullOrEmpty()) return val;
                if (str.As<IRegex>().IsMatch("^\\{.*\\}$"))
                {
                    var qs = str.Trim('{', '}').Split(',');
                    foreach (var s in qs)
                    {
                        var q = s.Split(':');
                        if (q.Length >= 2)
                        {
                            if (q.Length > 2)
                            {
                                for (int i = 2; i < q.Length; i++)
                                {
                                    q[1] += ":" + q[i];
                                }
                            }
                            val.Add(q[0].Trim('"'), q[1].Trim('"'));
                        }
                    }
                }
                else
                {
                    var qs = str.Split('&');
                    foreach (var t in qs)
                    {
                        if (t.IsNotNullOrEmpty())
                        {
                            var q = t.Split('=');
                            val.Add(q[0], Utils.UrlDecode(q[1]));
                        }
                    }
                }
            }
            catch { }
            return val;
        }

        internal static string QueryString(NameValueCollection query, Encoding encoding)
        {
            var list = new List<string>();
            foreach (var key in query.AllKeys)
            {
                list.Add("{0}={1}".FormatWith(key, HttpUtility.UrlEncode(query[key], encoding)));
            }
            return string.Join("&", list);
        }
    }
}
