﻿using DayEasy.Utility.Helper;
using System;

namespace DayEasy.Utility.Extend
{
    public interface IConvert : IExtension<string> { }

    public static class ConvertExtension
    {
        public static int ToInt(this IConvert c, int def)
        {
            return ConvertHelper.StrToInt(c.GetValue(), def);
        }

        public static int ToInt(this IConvert c)
        {
            return c.ToInt(-1);
        }

        public static float ToFloat(this IConvert c, float def)
        {
            return ConvertHelper.StrToFloat(c.GetValue(), def);
        }

        public static float ToFloat(this IConvert c)
        {
            return c.ToFloat(-1F);
        }

        public static decimal ToDecimal(this IConvert c, decimal def)
        {
            return (decimal)c.ToFloat((float)def);
        }

        public static decimal ToDecimal(this IConvert c)
        {
            return (decimal)c.ToFloat(-1F);
        }

        public static DateTime ToDateTime(this IConvert c, DateTime def)
        {
            return ConvertHelper.StrToDateTime(c.GetValue(), def);
        }

        public static DateTime ToDateTime(this IConvert c)
        {
            return ConvertHelper.StrToDateTime(c.GetValue());
        }
    }
}
