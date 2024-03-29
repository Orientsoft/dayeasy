﻿using System.ComponentModel;
using System.Linq;

namespace DayEasy.Utility.Extend
{
    public static class EnumExtension
    {
        public static int GetValue<T>(this T t)
            where T : struct
        {
            var type = typeof (T);
            if (!type.IsEnum)
                return default(int);
            try
            {
                return (int) (object) t;
            }
            catch
            {
                return default(int);
            }
        }

        public static string GetText<T>(this T tp)
            where T : struct
        {
            try
            {
                var ms = tp.GetType();
                if (!ms.IsEnum)
                    return "枚举类型错误";
                var field = ms.GetFields().FirstOrDefault(t => t.Name == tp + "");
                if (field != null)
                {
                    var desc = field.GetCustomAttributes(true).FirstOrDefault(t => (t as DescriptionAttribute) != null);
                    if (desc != null)
                        return ((DescriptionAttribute) desc).Description;
                    return field.Name;
                }
                return "枚举错误";
            }
            catch
            {
                return "枚举异常";
            }
        }

        private static readonly string[] FontColors = new[] {"Gray", "Black", "Green", "Blue", "Fuchsia", "Red"};
        private const string EnumHtml = "<font color='{0}'>{1}</font>";

        public static string GetEnumCssText<T>(this T tp, string[] colors)
            where T:struct 
        {
            var types = tp.GetType().GetFields().Where(t => t.IsLiteral).ToList();
            int index = types.IndexOf(types.FirstOrDefault(t => t.Name == tp + ""));
            if (index >= 0)
                return string.Format(EnumHtml, colors[(index >= colors.Length ? (colors.Length - 1) : index)],
                                     tp.GetText());
            return "";
        }

        public static string GetEnumCssText<T>(this T tp)
            where T : struct
        {
            return tp.GetEnumCssText(FontColors);
        }

        public static string GetEnumText<T, TV>(this TV type)
            where T : struct
            where TV : struct
        {
            try
            {
                return ((T) (object) type).GetText();
            }
            catch
            {
                return "枚举异常";
            }
        }

        public static string GetEnumText<T>(this int type)
            where T : struct
        {
            return type.GetEnumText<T, int>();
        }

        public static string GetEnumCssText<T, TV>(this TV type)
            where T : struct
            where TV : struct
        {
            try
            {
                return ((T) (object) type).GetEnumCssText();
            }
            catch
            {
                return "枚举异常";
            }
        }

        public static string GetEnumCssText<T>(this int type)
            where T : struct
        {
            return type.GetEnumCssText<T, int>();
        }

        public static T ToEnum<T, TV>(this TV type)
            where T : struct
            where TV : struct
        {
            try
            {
                return (T) (object) type;
            }
            catch
            {
                return default(T);
            }
        }

        public static T ToEnum<T>(this int type)
            where T : struct
        {
            return type.ToEnum<T, int>();
        }
    }
}
