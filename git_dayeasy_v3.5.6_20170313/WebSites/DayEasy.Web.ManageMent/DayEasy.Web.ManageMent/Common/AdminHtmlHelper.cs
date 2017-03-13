using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Utility.Extend;

namespace DayEasy.Web.ManageMent.Common
{
    public static class AdminHtmlHelper
    {
        const string LabelTemplate = "<label class=\"label {0}\">{1}</label>";

        private static readonly string[] LabelCss =
        {
            "label-default", "label-info", "label-primary", "label-success", "label-warning",
            "label-danger"
        };

        private static IDictionary<string, object> DeepCopy(this IDictionary<string, object> sourceDict)
        {
            return sourceDict.ToDictionary(p => p.Key, p => p.Value);
        }

        /// <summary>
        /// 多选框列表
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="name">name</param>
        /// <param name="selectList">选项列表</param>
        /// <param name="attributes">html 属性</param>
        /// <returns></returns>
        public static HtmlString CheckBoxButtonList(this HtmlHelper htmlHelper, string name,
            IEnumerable<SelectListItem> selectList, object attributes = null)
        {
            IDictionary<string, object> htmlAttributes = new Dictionary<string, object>();
            htmlAttributes.Add("type", "checkbox");
            htmlAttributes.Add("name", name);

            StringBuilder stringBuilder = new StringBuilder();

            foreach (SelectListItem selectItem in selectList)
            {
                IDictionary<string, object> newHtmlAttributes = htmlAttributes.DeepCopy();
                newHtmlAttributes.Add("value", selectItem.Value);
                if (selectItem.Selected)
                {
                    newHtmlAttributes.Add("checked", "checked");
                }
                TagBuilder tagBuilder = new TagBuilder("input");
                tagBuilder.MergeAttributes(newHtmlAttributes);
                string inputAllHtml = tagBuilder.ToString(TagRenderMode.SelfClosing);

                TagBuilder tagBuilder1 = new TagBuilder("label");
                var attrs = HtmlHelper.AnonymousObjectToHtmlAttributes(attributes);
                tagBuilder1.MergeAttributes(attrs);
                tagBuilder1.InnerHtml = inputAllHtml + selectItem.Text;

                stringBuilder.Append(tagBuilder1);
            }
            return new HtmlString(stringBuilder.ToString());
        }

        /// <summary>
        /// 是否选中-checked
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="isChecked"></param>
        /// <returns></returns>
        public static HtmlString IsChecked(this HtmlHelper helper, bool isChecked)
        {
            return new HtmlString(isChecked ? "checked = 'checked'" : string.Empty);
        }

        /// <summary>
        /// 获取枚举描述
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="enumValue"></param>
        /// <param name="indexs"></param>
        /// <returns></returns>
        public static HtmlString EnumText<T, TV>(this HtmlHelper htmlHelper, TV enumValue, int[] indexs = null)
            where T : struct
            where TV : struct
        {
            var value = enumValue.CastTo<int>();
            if (typeof(T) == typeof(NormalStatus) || typeof(T) == typeof(UserStatus))
            {
                indexs = new[] { 3, 0, 0, 0, 5 };
            }
            else if (typeof(T) == typeof(TempStatus))
            {
                indexs = new[] { 0, 3, 0, 0, 5 };
            }
            int index;
            if (indexs != null && indexs.Any())
            {
                index = indexs[value % indexs.Length];
            }
            else
            {
                index = value % LabelCss.Length;
            }
            return new HtmlString(string.Format(LabelTemplate, LabelCss[index], enumValue.GetEnumText<T, TV>()));
        }

        public static HtmlString EnumText<T>(this HtmlHelper htmlHelper, T enumValue)
            where T : struct
        {
            var value = enumValue.CastTo<int>();
            var index = value % LabelCss.Length;
            if (typeof(T) == typeof(NormalStatus))
            {
                index = (value == (int)NormalStatus.Normal ? 3 : 5);
            }
            else if (typeof(T) == typeof(TempStatus))
            {
                index = (value == (int)TempStatus.Normal ? 3 : 5);
            }
            return new HtmlString(string.Format(LabelTemplate, LabelCss[index], enumValue.GetText()));
        }

        public static HtmlString BooleanText(this HtmlHelper htmlHelper, bool value)
        {
            return new HtmlString(string.Format(LabelTemplate, LabelCss[value ? 3 : 5], value));
        }

        public static HtmlString LabelText(this HtmlHelper htmlHelper, string text, int index)
        {
            return new HtmlString(string.Format(LabelTemplate, LabelCss[index % LabelCss.Length], text));
        }

        public static HtmlString ShowUserName(this HtmlHelper helper, TU_User user, bool trueName = true)
        {
            string name;
            if (trueName && user.TrueName.IsNotNullOrEmpty())
                name = user.TrueName;
            else if (user.Email.IsNotNullOrEmpty())
                name = user.Email;
            else if (user.Mobile.IsNotNullOrEmpty())
                name = user.Mobile;
            else if (user.NickName.IsNotNullOrEmpty())
                name = user.NickName;
            else
                name = "匿名用户";
            return new HtmlString(name);
        }
    }
}