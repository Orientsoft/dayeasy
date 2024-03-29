﻿using System;
using System.Linq;
using System.Reflection;
using AutoMapper;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Logging;

namespace DayEasy.AutoMapper
{
    public class AutoMapperHelper
    {
        private static readonly ILogger Logger = LogManager.Logger<AutoMapperHelper>();
        public static void CreateMap(Type type)
        {
            CreateMap<AutoMapFromAttribute>(type);
            CreateMap<AutoMapToAttribute>(type);
            CreateMap<AutoMapAttribute>(type);
        }

        public static void CreateMap<TAttribute>(Type type)
            where TAttribute : AutoMapAttribute
        {
            if (!type.IsDefined(typeof (TAttribute)))
            {
                return;
            }

            try
            {
                foreach (var autoMapToAttribute in type.GetCustomAttributes<TAttribute>())
                {
                    if (autoMapToAttribute.TargetTypes.IsNullOrEmpty())
                    {
                        continue;
                    }

                    foreach (var targetType in autoMapToAttribute.TargetTypes)
                    {
                        if (autoMapToAttribute.Direction.HasFlag(AutoMapDirection.To))
                        {
                            var map = Mapper.CreateMap(type, targetType);
                            var props =
                                type.GetProperties().Where(t => t.IsDefined(typeof (MapFromAttribute)) && t != null);
                            foreach (var prop in props)
                            {
                                var mapfrom = prop.GetCustomAttribute<MapFromAttribute>();
                                map.ForMember(mapfrom.SourceName, opt => opt.MapFrom(prop.Name));
                            }
                        }

                        if (autoMapToAttribute.Direction.HasFlag(AutoMapDirection.From))
                        {
                            var map = Mapper.CreateMap(targetType, type);
                            var props = type.GetProperties().Where(t => t.IsDefined(typeof (MapFromAttribute)));
                            foreach (var prop in props)
                            {
                                var mapfrom = prop.GetCustomAttribute<MapFromAttribute>();
                                map.ForMember(prop.Name, s => s.MapFrom(mapfrom.SourceName));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public static void CreateMapper<TTarget, TSource>()
        {
            Mapper.CreateMap<TTarget, TSource>();
        }
    }
}
