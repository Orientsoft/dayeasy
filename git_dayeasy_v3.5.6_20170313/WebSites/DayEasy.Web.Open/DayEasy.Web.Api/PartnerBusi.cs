using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using DayEasy.Utility;
using DayEasy.Utility.Config;
using DayEasy.Utility.Extend;
using DayEasy.Web.Api.Config;

namespace DayEasy.Web.Api
{
    public class PartnerBusi
    {
        public static PartnerBusi Instance
        {
            get { return (Singleton<PartnerBusi>.Instance ?? (Singleton<PartnerBusi>.Instance = new PartnerBusi())); }
        }

        public IEnumerable<PartnerInfo> GetPartners()
        {
            var config = ConfigUtils<PartnerXml>.Config;
            if (config == null) return new List<PartnerInfo>();
            return config.Partners;
        }

        public string GetKey(string partner)
        {
            var item = GetPartners().FirstOrDefault(t => t.AppKey == partner);
            return item == null ? "" : item.AppSecret;
        }

        public int GetComefrom(string partner)
        {
            var item = GetPartners().FirstOrDefault(t => t.AppKey == partner);
            return item == null ? 0 : item.Comefrom;
        }

        public byte Comefrom
        {
            get { return (byte) GetComefrom(Partner); }
        }

        public string Partner
        {
            get { return "partner".QueryOrForm(string.Empty); }
        }

        public string SignPartner(string partner, NameValueCollection qs)
        {
            var key = GetKey(partner);
            if (string.IsNullOrWhiteSpace(key))
                return string.Empty;
            var noIn = new[] { "partner", "sign" };

            var reqArray = BubbleSort(qs.AllKeys.Except(noIn).ToArray());
            var sign = string.Empty;
            foreach (var reqKey in reqArray)
            {
                sign += reqKey + "=" + Utils.UrlDecode(qs[reqKey], Encoding.UTF8) + "&";
            }
            return (sign.TrimEnd('&') + "+" + key).Md5().ToLower();
        }

        public string SignPartner(string qs)
        {
            var array = qs.Split('&');
            string partner = String.Empty;
            var nv = new NameValueCollection();
            foreach (var arr in array)
            {
                var item = arr.Split('=');
                if (item.Length != 2)
                    continue;
                if (item[0] == "partner")
                    partner = item[1];
                else
                {
                    nv[item[0]] = Utils.UrlDecode(item[1], Encoding.UTF8);
                }
            }
            if (string.IsNullOrEmpty(partner))
                return qs + "&sign=";
            return qs + "&sign=" + SignPartner(partner, nv);
        }

        /// <summary>
        /// 冒泡排序法
        /// 按照字母序列从a到z的顺序排列
        /// </summary>
        private static string[] BubbleSort(string[] r)
        {
            //交换标志 
            int i;
            //最多做R.Length-1趟排序
            for (i = 0; i < r.Length; i++)
            {
                bool exchange = false;

                int j; //交换标志 
                for (j = r.Length - 2; j >= i; j--)
                {
                    //交换条件
                    if (String.CompareOrdinal(r[j + 1], r[j]) < 0)
                    {
                        string temp = r[j + 1];
                        r[j + 1] = r[j];
                        r[j] = temp;
                        //发生了交换，故将交换标志置为真
                        exchange = true;
                    }
                }
                //本趟排序未发生交换，提前终止算法
                if (!exchange)
                {
                    break;
                }
            }
            return r;
        }
    }
}
