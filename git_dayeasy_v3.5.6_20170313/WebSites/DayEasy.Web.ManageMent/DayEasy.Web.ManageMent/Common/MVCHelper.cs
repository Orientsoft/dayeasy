using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using DayEasy.Utility.Extend;

namespace DayEasy.Web.ManageMent.Common
{
    public static class MvcHelper
    {
        #region 将枚举转换成下拉框选项

        /// <summary>
        /// 将枚举转换成下拉框选项
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> EnumToDropDownList<T>(bool hasTip = false)
            where T : struct
        {
            return EnumToDropDownList<T>(-1, hasTip);
        }

        /// <summary>
        /// 将枚举转换成下拉框选项(设置选中项)
        /// </summary>
        /// <param name="selectedItems">选中的枚举值</param>
        /// <param name="hasTip"></param>
        /// <param name="tipWord"></param>
        /// <returns></returns>
        public static List<SelectListItem> EnumToDropDownList<T>(List<int> selectedItems, bool hasTip = false,
            string tipWord = "请选择")
            where T : struct
        {
            var resultList = new List<SelectListItem>();
            if (hasTip)
            {
                resultList.Add(new SelectListItem {Text = tipWord, Value = "-1"});
            }
            foreach (T item in Enum.GetValues(typeof (T)))
            {
                var value = item.CastTo<int>();
                var showText = item.GetText();
                var selectItem = new SelectListItem
                {
                    Text = showText,
                    Value = value.ToString(),
                    Selected = (selectedItems != null && selectedItems.Contains(value))
                };
                resultList.Add(selectItem);
            }
            return resultList;
        }

        public static List<SelectListItem> EnumToDropDownList<T>(int selectedItem, bool hasTip = false,
            string tipWord = "请选择")
            where T : struct
        {
            return EnumToDropDownList<T>(new List<int> { selectedItem }, hasTip, tipWord);
        }

        #endregion

        #region 将枚举转成字典

        /// <summary>
        /// 将枚举转成字典
        /// </summary>
        /// <param name="enumModel">枚举</param>
        /// <param name="hasTip">是否存在 '请选择' 默认选项</param>
        /// <param name="tipWord"></param>
        /// <returns></returns>
        public static Dictionary<int, string> EnumToDictionary(Type enumModel, bool hasTip = false, string tipWord = "请选择")
        {
            if (!enumModel.IsEnum)
            {
                return new Dictionary<int, string>();
            }
            var resultDictionary = new Dictionary<int, string>();
            var enumValues = Enum.GetNames(enumModel);
            if (hasTip)
            {
                resultDictionary.Add(-1, tipWord);
            }
            foreach (var item in enumValues)
            {
                int value = (byte)Enum.Parse(enumModel, item);

                var fileInfo = enumModel.GetField(item);
                var desc = fileInfo.GetAttribute<DescriptionAttribute>();
                var showText = (desc != null ? desc.Description : item);

                resultDictionary.Add(value, showText);
            }
            return resultDictionary;
        }

        #endregion

        #region 将List转换成List<SelectListItem>

        /// <summary>
        /// 将List转换成SelectListItemList
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <typeparam name="S">text类型</typeparam>
        /// <typeparam name="K">value类型</typeparam>
        /// <param name="list"></param>
        /// <param name="text">text元素</param>
        /// <param name="value">value元素</param>
        /// <param name="hasTip"></param>
        /// <param name="selectedValue">选中的元素集合</param>
        /// <param name="tipWord"></param>
        /// <returns></returns>
        public static List<SelectListItem> ToSelectListItemList<T, S, K>(this IEnumerable<T> list,
            Expression<Func<T, S>> text, Expression<Func<T, K>> value, bool hasTip = false,
            K selectedValue = default(K), string tipWord = "请选择")
        {
            var resultList = new List<SelectListItem>();
            if (list != null && list.Count() > 0)
            {
                resultList = new List<SelectListItem>();
                if (hasTip)
                {
                    resultList.Add(new SelectListItem() { Text = tipWord, Value = "-1" });
                }

                var textList = list.Select<T, S>(text.Compile()).ToArray();
                var valueList = list.Select<T, K>(value.Compile()).ToArray();
                for (int i = 0; i < textList.Length; i++)
                {
                    if (textList[i] != null)
                    {
                        if (selectedValue != null && selectedValue.Equals(valueList[i]))
                        {
                            resultList.Add(new SelectListItem()
                            {
                                Text = textList[i].ToString(),
                                Value = valueList[i].ToString(),
                                Selected = true
                            });
                        }
                        else
                        {
                            resultList.Add(new SelectListItem()
                            {
                                Text = textList[i].ToString(),
                                Value = valueList[i].ToString()
                            });
                        }
                    }
                }
            }
            else
            {
                resultList.Add(new SelectListItem() { Text = tipWord, Value = "-1" });
            }
            return resultList;
        }

        #endregion

        #region 将List转换成List<SelectListItem>

        /// <summary>
        /// 将List转换成SelectListItemList
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <typeparam name="S">text类型</typeparam>
        /// <typeparam name="K">value类型</typeparam>
        /// <param name="list"></param>
        /// <param name="Text">text元素</param>
        /// <param name="Value">value元素</param>
        /// <param name="selectedValue">选中的元素集合</param>
        /// <returns></returns>
        public static List<SelectListItem> ToSelectListItemList<T, S, K>(this IEnumerable<T> list,
            Expression<Func<T, S>> Text, Expression<Func<T, K>> Value, List<K> selectedValue = null)
        {
            List<SelectListItem> resultList = new List<SelectListItem>();
            if (list != null && list.Count() > 0)
            {
                resultList = new List<SelectListItem>();
                var textList = list.Select<T, S>(Text.Compile()).ToArray();
                var valueList = list.Select<T, K>(Value.Compile()).ToArray();
                for (int i = 0; i < textList.Length; i++)
                {
                    if (textList[i] != null)
                    {
                        if (selectedValue != null && selectedValue.Contains(valueList[i]))
                        {
                            resultList.Add(new SelectListItem()
                            {
                                Text = textList[i].ToString(),
                                Value = valueList[i].ToString(),
                                Selected = true
                            });
                        }
                        else
                        {
                            resultList.Add(new SelectListItem()
                            {
                                Text = textList[i].ToString(),
                                Value = valueList[i].ToString()
                            });
                        }
                    }

                }
            }
            else
            {
                resultList.Add(new SelectListItem() { Text = "请选择", Value = "-1" });
            }
            return resultList;
        }

        #endregion

        #region 将List转换成List<SelectListItem>

        /// <summary>
        /// 将List转换成SelectListItemList
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <typeparam name="S">text类型</typeparam>
        /// <typeparam name="K">value类型</typeparam>
        /// <param name="list"></param>
        /// <param name="Text">text元素</param>
        /// <param name="Value">value元素</param>
        /// <param name="selectedValue">选中的元素集合</param>
        /// <returns></returns>
        public static List<SelectListItem> ToSelectListItemList<T, S, K>(this IEnumerable<T> list,
            Expression<Func<T, S>> Text, Expression<Func<T, K>> Value)
        {
            List<SelectListItem> resultList = new List<SelectListItem>();
            if (list != null && list.Count() > 0)
            {
                resultList = new List<SelectListItem>();
                var textList = list.Select<T, S>(Text.Compile()).ToArray();
                var valueList = list.Select<T, K>(Value.Compile()).ToArray();
                for (int i = 0; i < textList.Length; i++)
                {
                    if (textList[i] != null)
                    {
                        resultList.Add(new SelectListItem()
                        {
                            Text = textList[i].ToString(),
                            Value = valueList[i].ToString()
                        });
                    }
                }
            }
            else
            {
                resultList.Add(new SelectListItem() { Text = "请选择", Value = "-1" });
            }
            return resultList;
        }

        #endregion
    }
}