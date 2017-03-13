using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using DayEasy.Utility.Helper;

namespace DayEasy.Web.Static.Handler
{
    internal class WebHelper
    {
        public static string SetEncoding(HttpContext context)
        {
            bool gzip, deflate;
            if (!string.IsNullOrEmpty(context.Request.ServerVariables["HTTP_ACCEPT_ENCODING"]))
            {
                string acceptedTypes = context.Request.ServerVariables["HTTP_ACCEPT_ENCODING"].ToLower();
                gzip = acceptedTypes.Contains("gzip") || acceptedTypes.Contains("x-gzip") || acceptedTypes.Contains("*");
                deflate = acceptedTypes.Contains("deflate");
            }
            else
                gzip = deflate = false;

            string encoding = gzip ? "gzip" : (deflate ? "deflate" : "none");

            if (context.Request.Browser.Browser == "IE")
            {
                if (context.Request.Browser.MajorVersion < 6)
                    encoding = "none";
                else if (context.Request.Browser.MajorVersion == 6 &&
                         !string.IsNullOrEmpty(context.Request.ServerVariables["HTTP_USER_AGENT"]) &&
                         context.Request.ServerVariables["HTTP_USER_AGENT"].Contains("EV1"))
                    encoding = "none";
            }
            return encoding;
        }

        public static bool IsCachedOnBrowser(HttpContext context, string hash, string contentType)
        {
            if (!string.IsNullOrEmpty(context.Request.ServerVariables["HTTP_IF_NONE_MATCH"]) &&
                context.Request.ServerVariables["HTTP_IF_NONE_MATCH"].Equals(hash))
            {
                context.Response.ClearHeaders();
                context.Response.Status = "304 Not Modified";
                context.Response.AppendHeader("Content-Length", "0");
                return true;
            }
            return false;
        }

        public static string GetLocalFile(HttpContext context, string path, List<string> fileNames)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            path = path.Split('?')[0];
            string filePath = context.Server.MapPath(path);
            if (File.Exists(filePath))
            {
                fileNames.Add(filePath);
                return File.ReadAllText(filePath, Encoding.UTF8);
            }
            return string.Empty;
        }


        public static string GetMd5Sum(string str)
        {
            Encoder enc = Encoding.Unicode.GetEncoder();

            var unicodeText = new byte[str.Length * 2];
            enc.GetBytes(str.ToCharArray(), 0, str.Length, unicodeText, 0, true);

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(unicodeText);

            var sb = new StringBuilder();
            foreach (byte t in result)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString();
        }

        public static string GetContentType(string ext)
        {
            switch (ext)
            {
                case ".js":
                    return "application/x-javascript";
                case ".css":
                    return "text/css";
                default:
                    return "text/html";
            }
        }
    }
}
