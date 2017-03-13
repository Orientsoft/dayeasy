using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web;

namespace DayEasy.Utility
{
    /// <summary>
    /// 常用类 create by shoy
    /// </summary>
    public class Utils
    {
        private const string DefaultDateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        private static readonly ILogger Logger = LogManager.Logger<Utils>();
        /// <summary>
        /// 获得当前绝对路径
        /// </summary>
        /// <param name="strPath">指定的路径</param>
        /// <returns>绝对路径</returns>
        public static string GetMapPath(string strPath)
        {
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Server.MapPath(strPath);
            }
            strPath = strPath.Replace("/", "\\");
            if (strPath.StartsWith("\\"))
            {
                strPath = strPath.Substring(strPath.IndexOf('\\', 1)).TrimStart('\\');
            }
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, strPath);
        }

        public static string GetTimeNow(string format = DefaultDateTimeFormat)
        {
            try
            {
                return Clock.Now.ToString(format);
            }
            catch
            {
                return Clock.Now.ToString(DefaultDateTimeFormat);
            }
        }

        private static Mutex _mut;



        //2012-08-16
        /// <summary> 获取绝对地址 </summary>
        /// <param name="host">网站根地址</param>
        /// <param name="url">当前地址</param>
        /// <returns></returns>
        public static string GetAbsoluteUrl(string host, string url)
        {
            if (!host.StartsWith("http://") && !host.StartsWith("https://"))
                host = "http://" + host;
            var t = new Uri(new Uri(host), url);
            return t.AbsoluteUri;
        }

        /// <summary> 是否外链 </summary>
        /// <param name="url"></param>
        /// <param name="domains"></param>
        /// <returns></returns>
        public static bool IsOutUrl(string url, string[] domains)
        {
            if (domains.Length == 0) return true;
            var host = "www" + domains[0];
            if (!host.StartsWith("http://") && !host.StartsWith("https://"))
                host = "http://" + host;
            var t = new Uri(new Uri(host), url);
            var domain = RegexHelper.GetDomain(t.Host);
            return !domains.Contains(domain);
        }

        /// <summary>
        /// 返回 URL 字符串的编码结果
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="encoding"></param>
        /// <returns>编码结果</returns>
        public static string UrlEncode(string str, Encoding encoding)
        {
            return HttpUtility.UrlEncode(str, encoding);
        }

        public static string UrlEncode(string str)
        {
            return UrlEncode(str, Encoding.Default);
        }

        public static string UrlDecode(string str, Encoding encoding)
        {
            return HttpUtility.UrlDecode(str, encoding);
        }

        public static string UrlDecode(string str)
        {
            return UrlDecode(str, Encoding.Default);
        }

        /// <summary> PageSize型 </summary>
        /// <param name="url"></param>
        /// <param name="pageQ"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static string GetNextPageUrl(string url, string pageQ, int pageSize)
        {
            if (string.IsNullOrEmpty(url))
                return url;
            var pageStr = RegexHelper.Match(url, "[?&]" + pageQ + "=(\\d+)");
            int page = 1;
            if (!string.IsNullOrEmpty(pageStr))
            {
                page = ConvertHelper.StrToInt(pageStr, 0);
                url = url.Replace(RegexHelper.Match(url, "([?&]" + pageQ + "=\\d+)"), string.Empty);
            }
            page += pageSize;
            return url.IndexOf('?') >= 0 ? (url + "&" + pageQ + "=" + page) : (url + "?" + pageQ + "=" + page);
        }

        /// <summary>
        /// Page型,从1开始
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pageQ"></param>
        /// <param name="startOne"></param>
        /// <returns></returns>
        public static string GetNextPageUrl(string url, string pageQ = "page", bool startOne = true)
        {
            if (string.IsNullOrEmpty(url))
                return url;
            var pageStr = RegexHelper.Match(url, "[?&]" + pageQ + "=(\\d+)");
            int page = ConvertHelper.StrToInt(pageStr, (startOne ? 1 : 0));
            if (!string.IsNullOrEmpty(pageStr))
            {
                url = url.Replace(RegexHelper.Match(url, "([?&]" + pageQ + "=\\d+)"), string.Empty);
            }
            page++;
            return url.IndexOf('?') >= 0 ? (url + "&" + pageQ + "=" + page) : (url + "?" + pageQ + "=" + page);
        }

        public static string GetJsonTime()
        {
            return RegexHelper.Match(DateTime.Now.ToJson(), "(\\d+)");
        }
        public static string GetTime(DateTime date, string format = null)
        {
            var now = DateTime.Now;
            var sp = now - date;
            if (sp.TotalMinutes < 1) return "刚刚";
            if (sp.TotalHours < 1) return sp.Minutes + "分钟前";
            if (sp.TotalDays < 1) return sp.Hours + "小时前";
            return date.ToString(format ?? "yyyy-MM-dd");
        }

        /// <summary> 获取流的字符信息 </summary>
        /// <returns></returns>
        public static string GetTxtFromStream(Stream stream, Encoding encoding)
        {
            if (stream == null)
                return string.Empty;
            _mut = (_mut ?? new Mutex());
            _mut.WaitOne();
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(stream, encoding);
                return sr.ReadToEnd();
            }
            catch
            {
                return string.Empty;
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
        }

        /// <summary> 获取流的字符信息 </summary>
        public static List<string> GetListFromStream(Stream stream, Encoding encoding)
        {
            var list = new List<string>();
            if (stream == null)
                return list;
            _mut = (_mut ?? new Mutex());
            _mut.WaitOne();
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(stream, encoding);
                var str = sr.ReadToEnd();
                if (!string.IsNullOrEmpty(str))
                {
                    list = str.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                return list;
            }
            catch
            {
                return list;
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
        }

        /// <summary> 读文件 </summary>
        /// <param name="path">文件路径</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static string GetHtmlFromFile(string path, Encoding encoding)
        {
            string html;
            if (!File.Exists(path))
                return string.Empty;
            StreamReader sr = null;
            _mut = (_mut ?? new Mutex());
            _mut.WaitOne();
            try
            {
                sr = new StreamReader(path, encoding);
                html = sr.ReadToEnd();
            }
            catch
            {
                html = string.Empty;
            }
            finally
            {
                if (sr != null)
                    sr.Close();
                _mut.ReleaseMutex();
            }
            return html;
        }

        /// <summary> 获取txt文本文件数据 </summary>
        /// <param name="path">txt文件路径</param>
        /// <param name="code">编码</param>
        /// <returns></returns>
        public static IEnumerable<string> GetListFromFile(string path, Encoding code)
        {
            var list = new List<string>();
            var txt = GetHtmlFromFile(path, code);
            if (!string.IsNullOrEmpty(txt))
            {
                list = txt.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            return list;
        }

        /// <summary> 获取文件内容(简化) </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static IEnumerable<string> GetListFromFile(string path)
        {
            return GetListFromFile(path, Encoding.Default);
        }

        ///<summary> 判断List值相等</summary>
        ///<param name="l1"></param>
        ///<param name="l2"></param>
        ///<typeparam name="T"></typeparam>
        ///<returns></returns>
        public static bool ListEquals<T>(IEnumerable<T> l1, List<T> l2)
        {
            return l1.ArrayEquals(l2);
        }

        /// <summary> 当前文件夹 </summary>
        public static string GetCurrentDir()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            // 或者 AppDomain.CurrentDomain.SetupInformation.ApplicationBase
            return dir;
        }



        [DllImport("wininet.dll")]
        private static extern bool InternetGetConnectedState(out int connectionDescription, int reservedValue);

        /// <summary> 是否有网络链接 </summary>
        public static bool IsNetConnected
        {
            get
            {
                int i;
                return InternetGetConnectedState(out i, 0);
            }
        }

        /// <summary> 获取真实IP </summary>
        public static string GetRealIp()
        {
            return GetRealIp(HttpContext.Current);
        }

        /// <summary> 获取真实IP </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetRealIp(HttpContext context)
        {
            if (context == null) return "127.0.0.1";
            string userHostAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(userHostAddress))
            {
                userHostAddress = context.Request.ServerVariables["REMOTE_ADDR"];
            }
            if (string.IsNullOrEmpty(userHostAddress))
            {
                userHostAddress = context.Request.UserHostAddress;
            }
            if (!(!string.IsNullOrEmpty(userHostAddress) && RegexHelper.IsIp(userHostAddress)))
            {
                return "127.0.0.1";
            }
            return userHostAddress;
        }

        public static string RawUrl()
        {
            if (HttpContext.Current == null)
                return string.Empty;
            try
            {
                var request = HttpContext.Current.Request;
                return string.Format("{0}://{1}{2}", request.Url.Scheme, request.ServerVariables["HTTP_HOST"],
                    request.RawUrl);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary> 设置参数 </summary>
        /// <param name="url">url</param>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns></returns>
        public static string SetQuery(string url, string key, object value)
        {
            if (key.IsNullOrEmpty())
                return url;
            if (value == null)
                value = string.Empty;
            if (url.IsNullOrEmpty())
            {
                url = RawUrl();
            }
            var qs = url.Split('?');
            string search;
            var list = new System.Collections.Specialized.NameValueCollection();
            if (qs.Length < 2)
            {
                list.Add(key, UrlEncode(value.ToString()));
            }
            else
            {
                search = qs[1];
                foreach (var query in search.Split('&'))
                {
                    var item = query.Split('=');
                    list.Add(item[0], item[1]);
                }
                list[key] = UrlEncode(value.ToString());
            }
            search = string.Empty;
            for (var i = 0; i < list.Count; i++)
            {
                search += list.AllKeys[i] + "=" + list[i];
                if (i < list.Count - 1)
                    search += "&";
            }
            return qs[0] + "?" + search;
        }

        /// <summary> 获得拼音缩写 </summary>
        /// <param name="cnStr"></param>
        /// <returns></returns>
        public static string GetSpellCode(string cnStr)
        {
            var strTemp = new StringBuilder();
            int iLen = cnStr.Length;
            int i;

            for (i = 0; i <= iLen - 1; i++)
            {
                strTemp.Append(GetShortSpell(cnStr.Substring(i, 1)));
            }

            return strTemp.ToString();
        }

        #region 将一串中文转化为拼音

        /// <summary>
        /// 将一串中文转化为拼音
        /// 如果给定的字符为非中文汉字将不执行转化，直接返回原字符；
        /// </summary>
        /// <param name="chsstr">指定汉字</param>
        /// <returns>拼音码</returns>
        public static string ChsString2Spell(string chsstr)
        {
            string strRet = string.Empty;

            char[] arrChar = chsstr.ToCharArray();

            foreach (char c in arrChar)
            {
                strRet += SingleChs2Spell(c.ToString(CultureInfo.InvariantCulture));
            }

            return strRet.ToLower();
        }

        #endregion

        #region 单个汉字转化为拼音
        /// <summary>
        /// 单个汉字转化为拼音
        /// </summary>
        /// <param name="singleChs">单个汉字</param>
        /// <returns>拼音</returns>
        public static string SingleChs2Spell(string singleChs)
        {

            #region 编码定义

            int[] pyvalue = new int[]
            {
                -20319, -20317, -20304, -20295, -20292, -20283, -20265, -20257, -20242, -20230, -20051, -20036, -20032,
                -20026,
                -20002, -19990, -19986, -19982, -19976, -19805, -19784, -19775, -19774, -19763, -19756, -19751, -19746,
                -19741, -19739, -19728,
                -19725, -19715, -19540, -19531, -19525, -19515, -19500, -19484, -19479, -19467, -19289, -19288, -19281,
                -19275, -19270, -19263,
                -19261, -19249, -19243, -19242, -19238, -19235, -19227, -19224, -19218, -19212, -19038, -19023, -19018,
                -19006, -19003, -18996,
                -18977, -18961, -18952, -18783, -18774, -18773, -18763, -18756, -18741, -18735, -18731, -18722, -18710,
                -18697, -18696, -18526,
                -18518, -18501, -18490, -18478, -18463, -18448, -18447, -18446, -18239, -18237, -18231, -18220, -18211,
                -18201, -18184, -18183,
                -18181, -18012, -17997, -17988, -17970, -17964, -17961, -17950, -17947, -17931, -17928, -17922, -17759,
                -17752, -17733, -17730,
                -17721, -17703, -17701, -17697, -17692, -17683, -17676, -17496, -17487, -17482, -17468, -17454, -17433,
                -17427, -17417, -17202,
                -17185, -16983, -16970, -16942, -16915, -16733, -16708, -16706, -16689, -16664, -16657, -16647, -16474,
                -16470, -16465, -16459,
                -16452, -16448, -16433, -16429, -16427, -16423, -16419, -16412, -16407, -16403, -16401, -16393, -16220,
                -16216, -16212, -16205,
                -16202, -16187, -16180, -16171, -16169, -16158, -16155, -15959, -15958, -15944, -15933, -15920, -15915,
                -15903, -15889, -15878,
                -15707, -15701, -15681, -15667, -15661, -15659, -15652, -15640, -15631, -15625, -15454, -15448, -15436,
                -15435, -15419, -15416,
                -15408, -15394, -15385, -15377, -15375, -15369, -15363, -15362, -15183, -15180, -15165, -15158, -15153,
                -15150, -15149, -15144,
                -15143, -15141, -15140, -15139, -15128, -15121, -15119, -15117, -15110, -15109, -14941, -14937, -14933,
                -14930, -14929, -14928,
                -14926, -14922, -14921, -14914, -14908, -14902, -14894, -14889, -14882, -14873, -14871, -14857, -14678,
                -14674, -14670, -14668,
                -14663, -14654, -14645, -14630, -14594, -14429, -14407, -14399, -14384, -14379, -14368, -14355, -14353,
                -14345, -14170, -14159,
                -14151, -14149, -14145, -14140, -14137, -14135, -14125, -14123, -14122, -14112, -14109, -14099, -14097,
                -14094, -14092, -14090,
                -14087, -14083, -13917, -13914, -13910, -13907, -13906, -13905, -13896, -13894, -13878, -13870, -13859,
                -13847, -13831, -13658,
                -13611, -13601, -13406, -13404, -13400, -13398, -13395, -13391, -13387, -13383, -13367, -13359, -13356,
                -13343, -13340, -13329,
                -13326, -13318, -13147, -13138, -13120, -13107, -13096, -13095, -13091, -13076, -13068, -13063, -13060,
                -12888, -12875, -12871,
                -12860, -12858, -12852, -12849, -12838, -12831, -12829, -12812, -12802, -12607, -12597, -12594, -12585,
                -12556, -12359, -12346,
                -12320, -12300, -12120, -12099, -12089, -12074, -12067, -12058, -12039, -11867, -11861, -11847, -11831,
                -11798, -11781, -11604,
                -11589, -11536, -11358, -11340, -11339, -11324, -11303, -11097, -11077, -11067, -11055, -11052, -11045,
                -11041, -11038, -11024,
                -11020, -11019, -11018, -11014, -10838, -10832, -10815, -10800, -10790, -10780, -10764, -10587, -10544,
                -10533, -10519, -10331,
                -10329, -10328, -10322, -10315, -10309, -10307, -10296, -10281, -10274, -10270, -10262, -10260, -10256,
                -10254
            };

            string[] pystr = new string[]
            {
                "a", "ai", "an", "ang", "ao", "ba", "bai", "ban", "bang", "bao", "bei", "ben", "beng", "bi", "bian",
                "biao",
                "bie", "bin", "bing", "bo", "bu", "ca", "cai", "can", "cang", "cao", "ce", "ceng", "cha", "chai", "chan"
                , "chang", "chao", "che", "chen",
                "cheng", "chi", "chong", "chou", "chu", "chuai", "chuan", "chuang", "chui", "chun", "chuo", "ci", "cong"
                , "cou", "cu", "cuan", "cui",
                "cun", "cuo", "da", "dai", "dan", "dang", "dao", "de", "deng", "di", "dian", "diao", "die", "ding",
                "diu", "dong", "dou", "du", "duan",
                "dui", "dun", "duo", "e", "en", "er", "fa", "fan", "fang", "fei", "fen", "feng", "fo", "fou", "fu", "ga"
                , "gai", "gan", "gang", "gao",
                "ge", "gei", "gen", "geng", "gong", "gou", "gu", "gua", "guai", "guan", "guang", "gui", "gun", "guo",
                "ha", "hai", "han", "hang",
                "hao", "he", "hei", "hen", "heng", "hong", "hou", "hu", "hua", "huai", "huan", "huang", "hui", "hun",
                "huo", "ji", "jia", "jian",
                "jiang", "jiao", "jie", "jin", "jing", "jiong", "jiu", "ju", "juan", "jue", "jun", "ka", "kai", "kan",
                "kang", "kao", "ke", "ken",
                "keng", "kong", "kou", "ku", "kua", "kuai", "kuan", "kuang", "kui", "kun", "kuo", "la", "lai", "lan",
                "lang", "lao", "le", "lei",
                "leng", "li", "lia", "lian", "liang", "liao", "lie", "lin", "ling", "liu", "long", "lou", "lu", "lv",
                "luan", "lue", "lun", "luo",
                "ma", "mai", "man", "mang", "mao", "me", "mei", "men", "meng", "mi", "mian", "miao", "mie", "min",
                "ming", "miu", "mo", "mou", "mu",
                "na", "nai", "nan", "nang", "nao", "ne", "nei", "nen", "neng", "ni", "nian", "niang", "niao", "nie",
                "nin", "ning", "niu", "nong",
                "nu", "nv", "nuan", "nue", "nuo", "o", "ou", "pa", "pai", "pan", "pang", "pao", "pei", "pen", "peng",
                "pi", "pian", "piao", "pie",
                "pin", "ping", "po", "pu", "qi", "qia", "qian", "qiang", "qiao", "qie", "qin", "qing", "qiong", "qiu",
                "qu", "quan", "que", "qun",
                "ran", "rang", "rao", "re", "ren", "reng", "ri", "rong", "rou", "ru", "ruan", "rui", "run", "ruo", "sa",
                "sai", "san", "sang",
                "sao", "se", "sen", "seng", "sha", "shai", "shan", "shang", "shao", "she", "shen", "sheng", "shi",
                "shou", "shu", "shua",
                "shuai", "shuan", "shuang", "shui", "shun", "shuo", "si", "song", "sou", "su", "suan", "sui", "sun",
                "suo", "ta", "tai",
                "tan", "tang", "tao", "te", "teng", "ti", "tian", "tiao", "tie", "ting", "tong", "tou", "tu", "tuan",
                "tui", "tun", "tuo",
                "wa", "wai", "wan", "wang", "wei", "wen", "weng", "wo", "wu", "xi", "xia", "xian", "xiang", "xiao",
                "xie", "xin", "xing",
                "xiong", "xiu", "xu", "xuan", "xue", "xun", "ya", "yan", "yang", "yao", "ye", "yi", "yin", "ying", "yo",
                "yong", "you",
                "yu", "yuan", "yue", "yun", "za", "zai", "zan", "zang", "zao", "ze", "zei", "zen", "zeng", "zha", "zhai"
                , "zhan", "zhang",
                "zhao", "zhe", "zhen", "zheng", "zhi", "zhong", "zhou", "zhu", "zhua", "zhuai", "zhuan", "zhuang",
                "zhui", "zhun", "zhuo",
                "zi", "zong", "zou", "zu", "zuan", "zui", "zun", "zuo"
            };

            #endregion

            int iAsc;
            string strRtn = string.Empty;

            byte[] array = Encoding.Default.GetBytes(singleChs);

            try
            {
                iAsc = (short)(array[0]) * 256 + (short)(array[1]) - 65536;
            }
            catch
            {
                iAsc = 1;
            }

            if (iAsc > 0 && iAsc < 160)
                return singleChs;

            for (var i = (pyvalue.Length - 1); i >= 0; i--)
            {
                if (pyvalue[i] <= iAsc)
                {
                    strRtn = pystr[i];
                    break;
                }
            }

            //将首字母转为大写
            if (strRtn.Length > 1)
            {
                strRtn = strRtn.Substring(0, 1).ToUpper() + strRtn.Substring(1);
            }

            return strRtn.ToLower();
        }
        #endregion

        /// <summary> 
        /// 得到一个汉字的拼音第一个字母，如果是一个英文字母则直接返回大写字母 
        /// </summary> 
        /// <param name="cnChar">单个汉字</param> 
        /// <returns>单个大写字母</returns> 
        private static string GetShortSpell(string cnChar)
        {
            byte[] arrCn = Encoding.Default.GetBytes(cnChar);
            if (arrCn.Length > 1)
            {
                int area = arrCn[0];
                int pos = arrCn[1];
                int code = (area << 8) + pos;
                int[] areacode =
                    {
                        45217, 45253, 45761, 46318, 46826, 47010, 47297, 47614,
                        48119, 48119, 49062, 49324, 49896, 50371, 50614, 50622,
                        50906, 51387, 51446, 52218, 52698, 52698, 52698, 52980, 53689, 54481
                    };
                for (int i = 0; i < 26; i++)
                {
                    int max = 55290;
                    if (i != 25)
                        max = areacode[i + 1];
                    if (areacode[i] <= code && code < max)
                        return Encoding.Default.GetString(new[] { (byte)(65 + i) });
                }
                return "*";
            }
            return cnChar;
        }

        public static readonly Func<int, IEnumerable<int>> EachMax = delegate (int max)
        {
            max = Math.Abs(max);
            return Enumerable.Range(0, max);
        };

        public static readonly Func<int, int, IEnumerable<int>> Each = delegate (int min, int max)
        {
            min = Math.Min(min, max);
            return Enumerable.Range(min, Math.Abs(max - min));
        };

        /// <summary> 配置文件读取 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parseFunc">类型转换方法</param>
        /// <param name="defaultValue">默认值</param>
        /// <param name="key">配置名</param>
        /// <param name="supressKey">配置别名</param>
        /// <returns></returns>
        public static T GetAppSetting<T>(Func<string, T> parseFunc = null, T defaultValue = default(T),
            [CallerMemberName] string key = null, string supressKey = null)
        {
            return ConfigHelper.GetAppSetting(parseFunc, defaultValue, key, supressKey);
        }

        public static string SystemEnvironment()
        {
            if (HttpContext.Current == null)
                return string.Empty;
            return HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"];
        }

        /// <summary> 方法监控 </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public static void WatchAction(string name, Action action)
        {
            WatchAction(name, t => action(), Logger.Info);
        }

        /// <summary> 方法监控 </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <param name="logAction"></param>
        public static void WatchAction(string name, Action<Action<string>> action, Action<string> logAction = null)
        {
            var watch = new Stopwatch();
            watch.Start();
            var sb = new StringBuilder();
            try
            {
                action(s => sb.AppendLine(s));
            }
            catch (Exception ex)
            {
                sb.AppendLine($"异常信息：{ex.Message}");
                Logger.Error(ex.Message, ex);
            }
            finally
            {
                watch.Stop();
                sb.AppendLine($"{name}耗时：{watch.ElapsedMilliseconds}ms");
                logAction?.Invoke(sb.ToString());
            }
        }
    }
}
