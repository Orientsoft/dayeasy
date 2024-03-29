﻿using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using DayEasy.Utility.Helper;

namespace DayEasy.Utility.Extend
{
    ///<summary>
    /// 字符串通用扩展类
    ///</summary>
    public static class CommonExtension
    {
        /// <summary>
        /// 判断是否为空
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// 判断是否不为空
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static bool IsNotNullOrEmpty(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// 字符串格式化
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="arg0">参数0</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatWith(this string str, object arg0)
        {
            return string.Format(str, arg0);
        }

        /// <summary>
        /// 字符串格式化
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="arg0">参数0</param>
        /// <param name="arg1">参数1</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatWith(this string str, object arg0, object arg1)
        {
            return string.Format(str, arg0, arg1);
        }

        /// <summary>
        /// 字符串格式化
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="arg0">参数0</param>
        /// <param name="arg1">参数1</param>
        /// <param name="arg2">参数2</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatWith(this string str, object arg0, object arg1, object arg2)
        {
            return string.Format(str, arg0, arg1, arg2);
        }

        /// <summary>
        /// 字符串格式化
        /// </summary>
        /// <param name="str"></param>
        /// <param name="args">参数集</param>
        /// <returns></returns>
        public static string FormatWith(this string str, params object[] args)
        {
            return string.Format(str, args);
        }

        /// <summary>
        /// 倒置字符串，输入"abcd123"，返回"321dcba"
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Reverse(this string str)
        {
            char[] input = str.ToCharArray();
            var output = new char[str.Length];
            for (int i = 0; i < input.Length; i++)
                output[input.Length - 1 - i] = input[i];
            return new string(output);
        }

        /// <summary>
        /// 截断字符扩展
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="start">起始位置</param>
        /// <param name="len">长度</param>
        /// <param name="v">省略符</param>
        /// <returns></returns>
        public static string Sub(this string str, int start, int len, string v)
        {
            //(注:中文的范围:\u4e00 - \u9fa5, 日文在\u0800 - \u4e00, 韩文为\xAC00-\xD7A3)
            var reg = "[\u4e00-\u9fa5]".As<IRegex>().ToRegex(RegexOptions.Compiled);
            var chars = str.ToCharArray();
            var result = string.Empty;
            int index = 0;
            foreach (char t in chars)
            {
                if (index >= start && index < (start + len))
                    result += t;
                else if (index >= (start + len))
                {
                    result += v;
                    break;
                }
                index += (reg.IsMatch(t.ToString(CultureInfo.InvariantCulture)) ? 2 : 1);
            }
            return result;
        }

        /// <summary>
        /// 截断字符扩展
        /// </summary>
        /// <param name="str"></param>
        /// <param name="len"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static string Sub(this string str, int len, string v)
        {
            return str.Sub(0, len, v);
        }

        /// <summary>
        /// 截断字符扩展
        /// </summary>
        /// <param name="str"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string Sub(this string str, int len)
        {
            return str.Sub(0, len, "...");
        }

        /// <summary>
        /// 对传递的参数字符串进行处理，防止注入式攻击
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ConvertSql(this string str)
        {
            str = str.Trim();
            str = str.Replace("'", "''");
            str = str.Replace(";--", "");
            str = str.Replace("=", "");
            str = str.Replace(" or ", "");
            str = str.Replace(" and ", "");
            return str;
        }

        /// <summary>
        /// json字符串转换为obj
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T JsonToObject<T>(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default(T);
            var serializer = new JavaScriptSerializer();
            var section =
                ConfigurationManager.GetSection("system.web.extensions/scripting/webServices/jsonSerialization")
                as ScriptingJsonSerializationSection;
            if (section == null)
                return serializer.Deserialize<T>(json);
            serializer.MaxJsonLength = section.MaxJsonLength;
            serializer.RecursionLimit = section.RecursionLimit;
            return serializer.Deserialize<T>(json);
        }

        /// <summary>
        /// 将obj转换为json字符
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson(this object obj)
        {
            if (obj == null)
                return string.Empty;
            var serializer = new JavaScriptSerializer();
            var section =
                ConfigurationManager.GetSection("system.web.extensions/scripting/webServices/jsonSerialization")
                as ScriptingJsonSerializationSection;
            if (section == null)
                return serializer.Serialize(obj);
            serializer.MaxJsonLength = section.MaxJsonLength;
            serializer.RecursionLimit = section.RecursionLimit;
            return serializer.Serialize(obj);
        }

        public static T JsonToObject2<T>(this string json, Encoding encoding)
        {
            var ser = new DataContractJsonSerializer(typeof(T));
            using (var mStream = new MemoryStream(encoding.GetBytes(json)))
            {
                return (T)ser.ReadObject(mStream);
            }
        }

        public static T JsonToObject2<T>(this string json)
        {
            return json.JsonToObject2<T>(Encoding.UTF8);
        }

        public static string ToJson2(this object obj, Encoding encoding)
        {
            var ser = new DataContractJsonSerializer(obj.GetType());
            using (var stream = new MemoryStream())
            {
                ser.WriteObject(stream, obj);
                var dataBytes = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(dataBytes, 0, (int)stream.Length);
                return encoding.GetString(dataBytes);
            }
        }

        public static string ToJson2(this object obj)
        {
            return obj.ToJson2(Encoding.UTF8);
        }

        /// <summary>
        /// Html编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string HtmlEncode(this string str)
        {
            return string.IsNullOrWhiteSpace(str) ? str : HttpUtility.HtmlEncode(str);
        }

        /// <summary>
        /// Html解码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string HtmlDecode(this string str)
        {
            return string.IsNullOrWhiteSpace(str) ? str : HttpUtility.HtmlDecode(str);
        }

        /// <summary>
        /// Url编码
        /// </summary>
        /// <param name="str"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string UrlEncode(this string str, Encoding encoding)
        {
            return string.IsNullOrWhiteSpace(str) ? str : HttpUtility.UrlEncode(str, encoding);
        }

        /// <summary>
        /// Url编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UrlEncode(this string str)
        {
            return string.IsNullOrWhiteSpace(str) ? str : HttpUtility.UrlEncode(str);
        }

        /// <summary>
        /// Url解码
        /// </summary>
        /// <param name="str"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string UrlDecode(this string str, Encoding encoding)
        {
            return string.IsNullOrWhiteSpace(str) ? str : HttpUtility.UrlDecode(str, encoding);
        }

        /// <summary>
        /// Url解码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UrlDecode(this string str)
        {
            return string.IsNullOrWhiteSpace(str) ? str : HttpUtility.UrlDecode(str);
        }

        /// <summary>
        /// 获取该字符串的QueryString值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="str">字符串</param>
        /// <param name="def">默认值</param>
        /// <returns></returns>
        public static T Query<T>(this string str, T def)
        {
            if (string.IsNullOrWhiteSpace(str))
                return def;
            try
            {
                var c = HttpContext.Current;
                var qs = c.Request.QueryString[str].Trim();
                return qs.CastTo(def);
            }
            catch
            {
                return def;
            }
        }

        /// <summary>
        /// 获取该字符串的Form值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="str">字符串</param>
        /// <param name="def">默认值</param>
        /// <returns></returns>
        public static T Form<T>(this string str, T def)
        {
            if (string.IsNullOrWhiteSpace(str))
                return def;
            try
            {
                var c = HttpContext.Current;
                var qs = c.Request.Form[str].Trim();
                return qs.CastTo(def);
            }
            catch
            {
                return def;
            }
        }

        /// <summary>
        /// 获取该字符串QueryString或Form值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="str"></param>
        /// <param name="def">默认值</param>
        /// <returns></returns>
        public static T QueryOrForm<T>(this string str, T def)
        {
            if (string.IsNullOrWhiteSpace(str))
                return def;
            try
            {
                var c = HttpContext.Current;
                var qs = c.Request[str].Trim();
                return qs.CastTo(def);
            }
            catch
            {
                return def;
            }
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="url">url</param>
        /// <returns></returns>
        public static string SetQuery(this string key, object value, string url = null)
        {
            return Utils.SetQuery(url, key, value);
        }

        /// <summary>
        /// 将字符串写入到文件
        /// </summary>
        /// <param name="msg">字符串</param>
        /// <param name="path">文件路径</param>
        /// <param name="encoding">编码</param>
        public static void WriteTo(this string msg, string path, Encoding encoding)
        {
            FileHelper.WriteFile(path, msg, encoding);
        }

        /// <summary>
        /// 获取该值的MD5
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Md5(this string str)
        {
            if (str.IsNullOrEmpty())
                return str;
            return SecurityHelper.Md5(str);
        }

        /// <summary> 读取配置文件 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configName"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static T Config<T>(this string configName, T def = default(T))
        {
            return Utils.GetAppSetting(null, def, supressKey: configName);
        }

        /// <summary>
        /// 字符串转换为指定类型
        /// </summary>
        /// <param name="str"></param>
        /// <param name="def"></param>
        /// <param name="splitor"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T To<T>(this string str, T def = default(T), string splitor = ",")
        {
            var type = typeof(T);

            if (!type.IsArray && type.Name != "List`1")
                return str.CastTo(def);
            try
            {
                Type st = typeof(string);
                bool isList = false;
                if (type.IsArray)
                    st = Type.GetType(type.FullName.TrimEnd('[', ']'));
                else if (type.Name == "List`1")
                {
                    isList = true;
                    var typeName = RegexHelper.Match(type.FullName, "System.Collections.Generic.List`1\\[\\[([^,]+),", 1);
                    st = Type.GetType(typeName);
                }
                var arr = str.Split(new[] { splitor }, StringSplitOptions.RemoveEmptyEntries);
                if (st == typeof(string) || st == null)
                    return (isList ? (T)(object)arr.ToList() : (T)(object)arr);
                if (st == typeof(int))
                {
                    var rt = Array.ConvertAll(arr, s => s.CastTo(0));
                    return (isList ? (T)(object)rt.ToList() : (T)(object)rt);
                }
                if (st == typeof(double))
                {
                    var rt = Array.ConvertAll(arr, s => s.CastTo(0.0));
                    return (isList ? (T)(object)rt.ToList() : (T)(object)rt);
                }
                if (st == typeof(decimal))
                {
                    var rt = Array.ConvertAll(arr, s => s.CastTo(0M));
                    return (isList ? (T)(object)rt.ToList() : (T)(object)rt);
                }
                if (st == typeof(float))
                {
                    var rt = Array.ConvertAll(arr, s => s.CastTo(0F));
                    return (isList ? (T)(object)rt.ToList() : (T)(object)rt);
                }
                if (st == typeof(DateTime))
                {
                    var rt = Array.ConvertAll(arr, s => s.CastTo(DateTime.MinValue));
                    return (isList ? (T)(object)rt.ToList() : (T)(object)rt);
                }
                return (isList ? (T)(object)arr.ToList() : (T)(object)arr);
            }
            catch
            {
                return def;
            }
        }

        /// <summary>
        /// Xml反序列化
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="path">xml路径</param>
        /// <returns></returns>
        public static T XmlToObject<T>(this string path)
        {
            return XmlHelper.XmlDeserialize<T>(path);
        }

        /// <summary> 小驼峰命名法 </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToCamelCase(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            if (!char.IsUpper(s[0]))
                return s;

            var chars = s.ToCharArray();

            for (var i = 0; i < chars.Length; i++)
            {
                var hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                    break;
                chars[i] = char.ToLower(chars[i], CultureInfo.InvariantCulture);
            }

            return new string(chars);
        }

        /// <summary> url命名法 </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToUrlCase(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            var chars = s.ToCharArray();
            var str = new StringBuilder();
            for (var i = 0; i < chars.Length; i++)
            {
                if (char.IsUpper(chars[i]))
                {
                    if (i > 0 && !char.IsUpper(chars[i - 1]))
                        str.Append("_");
                    str.Append(char.ToLower(chars[i], CultureInfo.InvariantCulture));
                }
                else
                {
                    str.Append(chars[i]);
                }
            }
            return str.ToString();
        }

        public static bool HasTags(this string body)
        {
            return body.As<IRegex>().IsMatch("<[a-z0-9]+[^>]*>", RegexOptions.IgnoreCase);
        }

        /// <summary> 格式化题干 </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static string FormatBody(this string body)
        {
            if (string.IsNullOrWhiteSpace(body) || !body.HasTags())
                return body;
            body = body.As<IRegex>().Replace("(^<p>)|(<\\/p>)|(<p>\\s*<\\/p>)|(\\u000a)|(\\u0009)", string.Empty, RegexOptions.IgnoreCase);
            body = body.As<IRegex>().Replace("<p>", "<br/>", RegexOptions.IgnoreCase);
            body = body.As<IRegex>().Replace("(<br/>)+", "<br/>", RegexOptions.IgnoreCase);
            return body;
        }

        public static string FormatMessage(this string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return message;
            message = message.As<IRegex>().Replace("(\n)+", "<br/>");
            message = message.As<IRegex>().Replace("\\s", "&nbsp;");
            return message;
        }

        public static string RawMessage(this string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return message;
            message = message.As<IRegex>().Replace("<br/>", "\\n");
            message = message.As<IRegex>().Replace("&nbsp;", "\\s");
            return message;
        }

        /// <summary> 班级序号，用于排序 </summary>
        /// <param name="classsName"></param>
        /// <returns></returns>
        public static int ClassIndex(this string classsName)
        {
            return classsName.As<IRegex>().Match("(\\d+)?班", 1).To(99);
        }

        /// <summary> 试卷截图 </summary>
        /// <returns></returns>
        public static string PaperImage(this string picture, int x, int y, int width, int height)
        {
            if (picture.IsNullOrEmpty())
                return picture;
            var ext = Path.GetExtension(picture);
            if (string.IsNullOrWhiteSpace(ext))
                return picture;
            picture = picture.Replace(ext, "_c{0}-{1}-{2}-{3}{4}".FormatWith(x, y, width, height, ext));
            picture = picture.Replace("/upload/paper/marking/", "/paper/");
            return picture;
        }
    }
}
