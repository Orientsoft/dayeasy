using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using DayEasy.Utility.Helper;

namespace DayEasy.Utility
{
    /// <summary> 基础数据结果类 </summary>
    [Serializable]
    [DataContract(Name = "result")]
    public class DResult
    {
        /// <summary> 处理结果 </summary>
        [DataMember(Name = "status", Order = 1)]
        public bool Status { get; set; }

        /// <summary> 返回消息 </summary>
        [DataMember(Name = "message", Order = 2)]
        public string Message { get; set; }

        public DResult(bool status, string message)
        {
            Status = status;
            Message = message;
        }

        public DResult(string message)
            : this(false, message)
        {
        }

        public static DResult Success
        {
            get { return new DResult(true, string.Empty); }
        }

        public static DResult Error(string message)
        {
            return new DResult(false, message);
        }

        /// <summary> 根据数据库操作结构返回DResult </summary>
        /// <param name="result">试卷库操作结果</param>
        public static DResult FromResult(int result)
        {
            return result > 0 ? Success : Error("系统繁忙，请稍候重试！");
        }

        public static DResult<T> Succ<T>(T data)
        {
            return new DResult<T>(true, data);
        }

        public static DResult<T> Error<T>(string message)
        {
            return new DResult<T>(message);
        }

        public static DResults<T> Succ<T>(IEnumerable<T> data, int count = -1)
        {
            return count < 0 ? new DResults<T>(data) : new DResults<T>(data, count);
        }

        public static DResults<T> Errors<T>(string message)
        {
            return new DResults<T>(message);
        }

        public override string ToString()
        {
#if DEBUG
            return JsonHelper.ToJson(this, NamingType.CamelCase, true);
#else
            return JsonHelper.ToJson(this, NamingType.CamelCase);
#endif
        }
    }

    /// <summary> 基础数据结果类 </summary>
    [Serializable]
    [DataContract(Name = "result")]
    public class DResult<T> : DResult
    {
        /// <summary> 返回数据 </summary>
        [DataMember(Name = "data", Order = 3)]
        public T Data { get; set; }

        public DResult(bool status, T data)
            : base(status, string.Empty)
        {
            Data = data;
        }

        public DResult(string message)
            : base(false, message)
        {
        }
    }

    /// <summary> 基础数据结果类 </summary>
    [Serializable]
    [DataContract(Name = "result")]
    public class DResults<T> : DResult
    {
        [DataMember(Name = "count", Order = 3)]
        public int TotalCount { get; set; }

        [DataMember(Name = "data", Order = 4)]
        public IEnumerable<T> Data { get; set; }

        public DResults(string message)
            : base(false, message)
        {
            Data = new List<T>();
        }

        public DResults(IEnumerable<T> list)
            : base(true, string.Empty)
        {
            list = list ?? new List<T>();
            var data = list as T[] ?? list.ToArray();
            Data = data;
            TotalCount = data.Count();
        }

        public DResults(IEnumerable<T> list, int totalCount)
            : base(true, string.Empty)
        {
            list = list ?? new List<T>();
            Data = list;
            TotalCount = totalCount;
        }
    }
}