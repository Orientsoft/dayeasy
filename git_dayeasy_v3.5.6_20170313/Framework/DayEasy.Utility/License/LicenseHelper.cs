using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;

namespace DayEasy.Utility.License
{
    /// <summary> 编码/激活码管理 </summary>
    public class LicenseHelper
    {
        private readonly int _codeLength;
        private List<string> _cacheCodes;
        private readonly LicenseType _licenseType;
        /// <summary> 生成规则 </summary>
        private Func<int, List<string>, int, string, string> _generateRole;

        private readonly ILogger _logger = LogManager.Logger<LicenseHelper>();

        internal LicenseHelper(LicenseType type)
        {
            _licenseType = type;
            _codeLength = 8;
            var codeLength = type.GetType().GetField(type.ToString()).GetCustomAttribute<CodeLengthAttribute>();
            if (codeLength != null)
                _codeLength = codeLength.Length;
            _cacheCodes = new List<string>();
        }

        internal void SetGenerateRole(Func<int, List<string>, int, string, string> role)
        {
            _generateRole = role;
        }

        internal void SetCache(IEnumerable<string> caches)
        {
            _cacheCodes.AddRange(caches);
            _cacheCodes = _cacheCodes.Distinct().ToList();
        }

        private string GenerateCode(string prefix = null, int tryCount = 0)
        {
            if (_generateRole != null)
                return _generateRole(_codeLength, _cacheCodes, tryCount, prefix);
            var len = _codeLength;
            if (!string.IsNullOrWhiteSpace(prefix))
                len -= prefix.Length;
            if (len <= 0 || len > 64)
                return string.Empty;
            var code = IdHelper.Instance.Guid32;
            if (len > 32)
                code = string.Concat(code, IdHelper.Instance.Guid32);
            return (prefix ?? string.Empty) + code.Substring(0, len);
        }

        /// <summary> 使用编码/激活码 </summary>
        /// <param name="code"></param>
        public void Used(string code)
        {
            _cacheCodes.Remove(code);
        }

        /// <summary> 使用编码/激活码 </summary>
        /// <param name="code"></param>
        public void SetCache(string code)
        {
            _cacheCodes.Add(code);
        }

        /// <summary> 生成编码/激活码 </summary>
        /// <param name="prefix">前缀，会在总长度上减去前缀的长度</param>
        /// <param name="autoAdd">自动添加到缓存</param>
        /// <returns></returns>
        public string Code(string prefix = null, bool autoAdd = true)
        {
            var watch = new Stopwatch();
            watch.Start();
            var code = GenerateCode(prefix);
            var count = 0;
            while (_cacheCodes.Contains(code))
            {
                count++;
                code = GenerateCode(prefix, count);
            }
            if (autoAdd)
                _cacheCodes.Add(code);
            watch.Stop();
            _logger.Info("生成{2}[{0}],耗时{1}ms", code, watch.ElapsedMilliseconds, _licenseType.GetText());
            return code;
        }
    }
}
