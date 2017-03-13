using DayEasy.Utility.Helper;
using System;

namespace DayEasy.Core.Domain
{
    /// <summary>Key-Value</summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TV"></typeparam>
    [Serializable]
    public class DKeyValue<T, TV>
    {
        public T Key { get; set; }
        public TV Value { get; set; }

        public DKeyValue(T key, TV value)
        {
            Key = key;
            Value = value;
        }

        public override string ToString()
        {
            return JsonHelper.ToJson(this, NamingType.CamelCase);
        }
    }

    public class DKeyValue<T>
        : DKeyValue<T, string>
    {
        public DKeyValue(T key, string value)
            : base(key, value)
        {
        }
    }

    public class DKeyValue
        : DKeyValue<string, string>
    {
        public DKeyValue(string key, string value)
            : base(key, value)
        {
        }
    }
}
