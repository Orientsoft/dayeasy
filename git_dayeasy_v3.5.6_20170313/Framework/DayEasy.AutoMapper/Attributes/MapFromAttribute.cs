using System;

namespace DayEasy.AutoMapper.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MapFromAttribute : Attribute
    {
        public string SourceName { get; private set; }

        public MapFromAttribute(string sourceName)
        {
            SourceName = sourceName;
        }
    }
}
