﻿using System;

namespace DayEasy.Utility.Config
{
    /// <summary>
    /// 配置文件名属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FileNameAttribute : Attribute
    {
        public string Name { get; set; }

        public FileNameAttribute(string name)
        {
            Name = name;
        }
    }
}
