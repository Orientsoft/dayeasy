using System;
using System.Collections.Generic;
using System.Linq;
using DayEasy.Utility.Helper;

namespace DayEasy.Utility.License
{
    public static class DCode
    {
        private static readonly int[] SkipCode =
        {
            10000, 11111, 22222, 33333, 44444, 55555, 66666, 77777, 88888, 99999,
            12345, 23456, 34567, 45678, 56789,
            98765, 87654, 76543, 65432, 54321,
            68686, 68888, 66888, 66688, 66668,
            86868, 88666, 88866, 88886
        };

        /// <summary> 随机数字 </summary>
        private static int Random(int len)
        {
            var random = RandomHelper.Random();
            Func<int> numberFunc = () => random.Next((int)Math.Pow(10, len - 1), (int)Math.Pow(10, len));
            if (len < 0 || len > 15)
                return random.Next();
            var number = numberFunc();
            while (SkipCode.Contains(number))
                number = numberFunc();
            return number;
        }

        /// <summary> 生成规则 </summary>
        /// <param name="len">长度</param>
        /// <param name="caches">缓存</param>
        /// <param name="retry">重试次数</param>
        /// <param name="prefix">前缀</param>
        /// <returns></returns>
        public static string Code(int len, List<string> caches, int retry, string prefix = null)
        {
            if (len < 0 || len > 15)
                return string.Empty;
            var code = string.Concat(prefix, Random(len));
            int count = 0;
            while (caches.Contains(code) && count <= retry)
            {
                code = string.Concat(prefix, Random(len));
                count++;
            }
            return code;
        }
    }
}
