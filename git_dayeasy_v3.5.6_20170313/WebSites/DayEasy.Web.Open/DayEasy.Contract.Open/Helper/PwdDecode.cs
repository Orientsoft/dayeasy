using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DayEasy.Utility.Extend;

namespace DayEasy.Contract.Open.Helper
{
    public static class PwdDecode
    {
        private const string Key = "e9dv3n1t";
        private const string Iv = "ac3f6n2d";

        private static string Decode(string str)
        {
            using (var des = new DESCryptoServiceProvider())
            {
                str = str.UrlDecode();
                str = str.Replace("~", "=");
                str = str.Replace("_", "/");
                str = str.Replace("-", "+");
                var dataByte = Convert.FromBase64String(str);
                var byKey = Encoding.UTF8.GetBytes(Key);
                var byIv = Encoding.UTF8.GetBytes(Iv);
                using (var ms = new MemoryStream())
                {
                    using (var cst = new CryptoStream(ms, des.CreateDecryptor(byKey, byIv), CryptoStreamMode.Write))
                    {
                        cst.Write(dataByte, 0, dataByte.Length);
                        cst.FlushFinalBlock();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }
        public static string Encode(string data, string key, string iv)
        {
            try
            {
                byte[] byKey = Encoding.UTF8.GetBytes(key);
                byte[] byIv = Encoding.UTF8.GetBytes(iv);
                var dataByte = Encoding.UTF8.GetBytes(data);

                using (var des = new DESCryptoServiceProvider())
                {
                    using (var ms = new MemoryStream())
                    {
                        using (
                            var cst = new CryptoStream(ms, des.CreateEncryptor(byKey, byIv),
                                                       CryptoStreamMode.Write))
                        {
                            cst.Write(dataByte, 0, dataByte.Length);
                            cst.FlushFinalBlock();
                            var msg = Convert.ToBase64String(ms.ToArray());
                            msg = msg.Replace("+", "-");
                            msg = msg.Replace("/", "_");
                            msg = msg.Replace("=", "~");
                            return msg.UrlEncode(Encoding.UTF8);
                        }
                    }
                }
            }
            catch
            {
                return data;
            }
        }

        /// <summary> 解密密码字段 </summary>
        /// <param name="encodePwd"></param>
        /// <returns></returns>
        public static string DecodePwd(this string encodePwd)
        {
            return encodePwd.IsNullOrEmpty() ? encodePwd : Decode(encodePwd);
        }

        public static string EncodePwd(this string pwd)
        {
            return pwd.IsNullOrEmpty() ? pwd : Encode(pwd, Key, Iv);
        }
    }
}
