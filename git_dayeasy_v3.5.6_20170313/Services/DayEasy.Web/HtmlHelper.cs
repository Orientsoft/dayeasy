using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Routing;

namespace System.Web.Mvc
{
    public static class HTMLHelper
    {
        #region css/javascript引用

        private const string JsStr = "<script src=\"{0}{1}\" type=\"text/javascript\"></script>";
        private const string CssStr = "<link href=\"{0}{1}\" rel=\"stylesheet\" media=\"all\" />";

        private static string FormatPath(string path)
        {
            string ext;
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(ext = Path.GetExtension(path)))
                return path;
            if (path.IndexOf("min" + ext, StringComparison.Ordinal) > 0)
                return path;
            if (Consts.Config.IsOnline)
                return path.Replace(ext, ".min" + ext);
            if (path.StartsWith("v3/"))
                return path.Replace("v3/", "v3/source/");
            return string.Format("source/{0}", path.TrimStart('/'));
        }

        private static string BaseLink(string ext)
        {
            switch (ext)
            {
                case ".js":
                    return JsStr;
                case ".css":
                    return CssStr;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 引用css
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="path">css 路径(/XXX/XXX)</param>
        /// <returns></returns>
        public static HtmlString CssLink(this HtmlHelper htmlHelper, string path)
        {
            if (string.IsNullOrEmpty(path))
                return new HtmlString(string.Empty);
            if (path.StartsWith("css/v1/"))
                path = "mention/style/" + path;
            return
                new HtmlString(string.Format(CssStr,
                    new Uri(new Uri(Consts.Config.StaticSite), FormatPath(path)), string.Empty));
        }

        /// <summary>
        /// 引用javascript
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="path">js 路径(/XXX/XXX)</param>
        /// <returns></returns>
        public static HtmlString ScriptLink(this HtmlHelper htmlHelper, string path)
        {
            return string.IsNullOrEmpty(path)
                ? new HtmlString(string.Empty)
                : new HtmlString(string.Format(JsStr,
                    new Uri(new Uri(Consts.Config.StaticSite), FormatPath(path)), string.Empty));
        }

        public static HtmlString CombineLink(this HtmlHelper htmlHelper, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return new HtmlString(string.Empty);
            return htmlHelper.CombineLink(path.Split(','));
        }

        public static HtmlString CombineLink(this HtmlHelper htmlHelper, params string[] pathList)
        {
            if (pathList.IsNullOrEmpty())
                return new HtmlString(string.Empty);
            var ext = Path.GetExtension(pathList.First());
            string link = BaseLink(ext), paths = Consts.Config.StaticSite + "/";
            if (Consts.Config.IsOnline)
            {
                paths =
                    (pathList.Aggregate(paths + "??",
                        (current, item) => current + (FormatPath(item).TrimStart('/') + ",")));
                return new HtmlString(string.Format(link, paths.TrimEnd(','), "&t=" + Consts.Config.StaticTick));
            }
            var sb = new StringBuilder();
            foreach (var p in pathList)
            {
                var item = p;
                if (p.StartsWith("css/v1/"))
                {
                    item = "mention/style/" + item;
                }
                sb.AppendLine(string.Format(link, paths + FormatPath(item).TrimStart('/'),
                    "?t=" + Consts.Config.StaticTick));
            }
            return new HtmlString(sb.ToString());
        }

        #endregion

        #region 构造缩略图
        /// <summary>
        /// 构造缩略图
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="url">图片URL</param>
        /// <param name="size">图片大小（100x100）</param>
        /// <returns></returns>
        public static HtmlString MakeThumb(this HtmlHelper htmlHelper, string url, string size)
        {
            if (string.IsNullOrEmpty(url))
                return new HtmlString(string.Empty);
            var ext = Path.GetExtension(url);
            return new HtmlString(string.IsNullOrEmpty(ext) ? url : url.Replace(ext, "_s" + size + ext));
        }

        public static HtmlString ShowTime(this HtmlHelper htmlHelper, double? minites)
        {
            if (!minites.HasValue) return new HtmlString(string.Empty);
            var min = Math.Floor(minites.Value);
            var second = Math.Round((minites.Value - min) * 100 * 3 / 5);
            return second > 0
                ? new HtmlString(string.Format("{0}分{1}秒", min, second))
                : new HtmlString(string.Format("{0}分钟", min));
        }

        #endregion

        #region 获取枚举描述
        /// <summary>
        /// 获取枚举描述
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="value"></param>
        /// <param name="em"></param>
        /// <returns></returns>
        public static HtmlString GetEnumText(this HtmlHelper htmlHelper, int value, Type em)
        {
            try
            {
                var name = Enum.GetName(em, value);
                var fileInfo = em.GetField(name);
                var objs = fileInfo.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                return objs.Length > 0
                    ? new HtmlString(((System.ComponentModel.DescriptionAttribute)objs[0]).Description)
                    : new HtmlString(string.Empty);
            }
            catch (Exception)
            {
                return new HtmlString(value.ToString(CultureInfo.InvariantCulture));
            }
        }
        #endregion

        #region 根据路由配置显示友好分页URL链接
        /// <summary>
        /// 根据路由配置显示友好分页URL链接
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isShowPagesize"></param>
        /// <returns></returns>
        private static string PrepearRouteUrl(HtmlHelper helper, string pageIndex, string pageSize, bool isShowPagesize)
        {
            const string pageIndexName = "pageIndex";
            const string pageSizeName = "pageSize";

            var routeValues = new RouteValueDictionary();
            routeValues["action"] = helper.ViewContext.RequestContext.RouteData.Values["action"];
            routeValues["controller"] = helper.ViewContext.RequestContext.RouteData.Values["controller"];
            routeValues[pageIndexName] = pageIndex;
            if (isShowPagesize)
            {
                routeValues[pageSizeName] = pageSize;
            }

            var queryStrings = helper.ViewContext.RequestContext.HttpContext.Request.QueryString;
            if (queryStrings.AllKeys.Any())
            {
                foreach (
                    var key in
                        queryStrings.AllKeys.Where(
                            key =>
                                !key.Equals(pageIndexName, StringComparison.CurrentCultureIgnoreCase) &&
                                !key.Equals(pageSizeName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    routeValues[key] = queryStrings[key];
                }
            }

            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext);
            return urlHelper.RouteUrl(routeValues);
        }
        #endregion

        #region MVC分页条

        /// <summary>
        /// 分页条
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="currentPage">当前页</param>
        /// <param name="pageSize">页容量</param>
        /// <param name="totalCount">总条数</param>
        /// <param name="showPageNum">显示页码个数</param>
        /// <param name="isShowPagesize">显示页码个数</param>
        /// <returns></returns>
        public static HtmlString ShowPager(this HtmlHelper htmlHelper, int currentPage, int pageSize,
            int totalCount, int showPageNum = 10, bool isShowPagesize = true)
        {
            if (currentPage == 0)
                currentPage = 1;
            var redirectTo = HttpUtility.UrlDecode(PrepearRouteUrl(htmlHelper, "{0}", "{1}", isShowPagesize));

            if (redirectTo == null)
                return new HtmlString(string.Empty);

            var totalPages = Math.Max((totalCount + pageSize - 1) / pageSize, 1); //总页数
            currentPage = currentPage <= 0 ? 1 : currentPage;
            currentPage = currentPage >= totalPages ? totalPages : currentPage;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            if (isShowPagesize)
                redirectTo = string.Format(redirectTo, "{0}", pageSize);

            var pageLi = "<li><a href='" + redirectTo + "'>{1}</a></li>";
            var currentPageLi = "<li class='active'><a href='" + redirectTo + "'>{1}</a></li>";
            var ignorePageLi = "<li><a href='javascript:void(0);'>···</a></li>";

            //处理页码逻辑
            var output = new StringBuilder();
            if (totalPages > 1 && showPageNum > 0)
            {
                //计算显示的页面范围
                var showFirstIndex = (currentPage <= showPageNum / 2 + 1 ? 1 : (currentPage - showPageNum / 2));
                var showLastIndex = (showFirstIndex + showPageNum - 1 >= totalPages
                    ? totalPages
                    : showFirstIndex + showPageNum - 1);
                if (showLastIndex >= totalPages)
                    showFirstIndex = showLastIndex - showPageNum + 1;
                if (showFirstIndex <= 0)
                    showFirstIndex = 1;
                //显示上一页
                if (currentPage != 1)
                    output.AppendFormat(pageLi, currentPage - 1, "«");

                //判断是否显示第一页的页码
                if (showFirstIndex > 1)
                {
                    output.AppendFormat(pageLi, 1, 1);
                    if (showFirstIndex > 2)
                        output.Append(ignorePageLi);
                }
                //显示页码
                for (var i = showFirstIndex; i <= showLastIndex; i++)
                {
                    output.AppendFormat(currentPage == i ? currentPageLi : pageLi, i, i);
                }
                //判断是否显示最后一页的页码
                if (showLastIndex < totalPages)
                {
                    if (totalPages - showLastIndex > 1)
                        output.Append(ignorePageLi);
                    output.AppendFormat(pageLi, totalPages, totalPages);
                }
                //显示下一页
                if (currentPage != totalPages)
                    output.AppendFormat(pageLi, currentPage + 1, "»");
            }
            else if (totalPages > 1)
            {
                //显示上一页
                if (currentPage != 1)
                    output.AppendFormat(pageLi, currentPage - 1, "«");
                ignorePageLi = "<li><a href='javascript:void(0);'>" + currentPage + " / " + totalPages + "</a></li>";
                output.Append(ignorePageLi);
                //显示下一页
                if (currentPage != totalPages)
                    output.AppendFormat(pageLi, currentPage + 1, "»");
            }

            return new HtmlString(output.Length > 0
                ? "<ul class=\"pagination\">" + output.ToString().ToLower() + "</ul>"
                : string.Empty);
        }

        #endregion

        #region MVC Ajax分页

        /// <summary>
        /// MVC Ajax分页
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="currentPage">当前页</param>
        /// <param name="pageSize">页容量</param>
        /// <param name="totalCount">总记录数</param>
        /// <param name="updateDomId">要更新的元素Id</param>
        /// <param name="showPageNum">显示的页码数量</param>
        /// <returns></returns>
        public static HtmlString ShowAjaxPager(this HtmlHelper htmlHelper, int currentPage, int pageSize,
            int totalCount, string updateDomId, int showPageNum = 10)
        {
            if (currentPage == 0)
                currentPage = 1;
            var fromDataStr = string.Empty;
            var formDataKeys = HttpContext.Current.Request.Form.AllKeys;
            if (formDataKeys.Any())
            {
                fromDataStr =
                    formDataKeys.Where(
                        item =>
                            !item.Trim().Equals("pageIndex", StringComparison.CurrentCultureIgnoreCase) &&
                            !item.Trim().Equals("pageSize", StringComparison.CurrentCultureIgnoreCase))
                        .Aggregate(fromDataStr,
                            (current, item) => current + (item + ":'" + HttpContext.Current.Request.Form[item] + "',"));
            }
            fromDataStr += "pageIndex:{0},pageSize:{1}";
            var url = "javascript:$('#" + updateDomId + "').load('" + Utils.RawUrl() + "',{{" +
                      fromDataStr + "}});";
            var pageLi = "<li><a href='javascript:void(0);' onclick=\"" + url + "\">{2}</a></li>";
            var currentPageLi = "<li class='active'><a href='javascript:void(0);' onclick=\"" + url + "\">{2}</a></li>";
            const string ignorePageLi = "<li><a href='javascript:void(0);'>···</a></li>";

            var totalPages = Math.Max((totalCount + pageSize - 1) / pageSize, 1); //总页数
            currentPage = currentPage <= 0 ? 1 : currentPage;
            currentPage = currentPage >= totalPages ? totalPages : currentPage;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            //处理页码逻辑
            if (totalPages <= 1)
                return new HtmlString(string.Empty);
            var output = new StringBuilder();
            //计算显示的页面范围
            var showFirstIndex = (currentPage <= showPageNum / 2 + 1 ? 1 : (currentPage - showPageNum / 2));
            var showLastIndex = (showFirstIndex + showPageNum - 1 >= totalPages
                ? totalPages
                : showFirstIndex + showPageNum - 1);
            if (showLastIndex >= totalPages)
                showFirstIndex = showLastIndex - showPageNum + 1;
            if (showFirstIndex <= 0)
                showFirstIndex = 1;

            //显示上一页
            if (currentPage != 1)
                output.AppendFormat(pageLi, currentPage - 1, pageSize, "«");
            //判断是否显示第一页的页码
            if (showFirstIndex > 1)
            {
                output.AppendFormat(pageLi, 1, pageSize, 1);
                if (showFirstIndex > 2)
                    output.Append(ignorePageLi);
            }
            //显示页码
            for (var i = showFirstIndex; i <= showLastIndex; i++)
            {
                output.AppendFormat(currentPage == i ? currentPageLi : pageLi, i, pageSize, i);
            }
            //判断是否显示最后一页的页码
            if (showLastIndex < totalPages)
            {
                if (totalPages - showLastIndex > 1)
                    output.Append(ignorePageLi);
                output.AppendFormat(pageLi, totalPages, pageSize, totalPages);
            }
            //显示下一页
            if (currentPage != totalPages)
                output.AppendFormat(pageLi, currentPage + 1, pageSize, "»");

            return new HtmlString(output.Length > 0
                ? "<ul class=\"pagination\">" + output + "</ul>"
                : string.Empty);
        }

        #endregion

        #region 将枚举转换成下拉框选项

        /// <summary>
        /// 将枚举转换成下拉框选项
        /// </summary>
        /// <param name="emType">枚举类型</param>
        /// <param name="selectedValue">是否选中指定项</param>
        /// <param name="hasTip">是否增加“请选择,-1”项</param>
        /// <param name="tipWord">请选择项文本</param>
        /// <returns></returns>
        public static List<SelectListItem> EnumToDropDownList(Type emType, int selectedValue = -1, bool hasTip = false, string tipWord = "请选择")
        {
            var resultList = new List<SelectListItem>();
            if (hasTip)
                resultList.Add(new SelectListItem() { Text = tipWord, Value = "-1" });

            var ems = Enum.GetValues(emType);
            resultList.AddRange(from object i in ems
                                select Convert.ToInt32(i) into item
                                let name = Enum.GetName(emType, item)
                                let objs = emType.GetField(name).GetCustomAttributes(typeof(DescriptionAttribute), false)
                                select new SelectListItem()
                                {
                                    Value = item.ToString(CultureInfo.InvariantCulture),
                                    Text = objs.Length > 0 ? ((DescriptionAttribute)objs[0]).Description : name,
                                    Selected = (selectedValue != -1 && item.ToString(CultureInfo.InvariantCulture) == selectedValue.ToString(CultureInfo.InvariantCulture))
                                });
            return resultList;
        }

        #endregion

        #region 将枚举转成字典

        /// <summary>
        /// 将枚举转成字典
        /// </summary>
        /// <param name="emType">枚举类型</param>
        /// <param name="hasTip">是否增加“请选择,-1”项</param>
        /// <param name="tipWord">请选择项文本</param>
        /// <returns></returns>
        public static Dictionary<int, string> EnumToDictionary(Type emType, bool hasTip = false, string tipWord = "请选择")
        {
            if (!emType.IsEnum)
                return new Dictionary<int, string>();
            var dict = new Dictionary<int, string>();
            var ems = Enum.GetNames(emType);
            if (hasTip)
                dict.Add(-1, tipWord);
            foreach (var item in ems)
            {
                var fileInfo = emType.GetField(item);
                var desc = fileInfo.GetAttribute<DescriptionAttribute>();
                dict.Add((int)Enum.Parse(emType, item), desc != null ? desc.Description : item);
            }
            return dict;
        }

        #endregion

        #region 将List转换成List<SelectListItem>

        /// <summary>
        /// 将List转换成SelectListItemList
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <typeparam name="TS">text类型</typeparam>
        /// <typeparam name="TK">value类型</typeparam>
        /// <param name="list"></param>
        /// <param name="text">text元素</param>
        /// <param name="value">value元素</param>
        /// <param name="hasTip"></param>
        /// <param name="selectedValue">选中的元素集合</param>
        /// <param name="tipWord"></param>
        /// <returns></returns>
        public static List<SelectListItem> ToSlectListItemList<T, TS, TK>(
            this IEnumerable<T> list,
            Expression<Func<T, TS>> text,
            Expression<Func<T, TK>> value,
            TK selectedValue = default(TK),
            bool hasTip = false,
            string tipWord = "请选择")
        {
            var resultList = new List<SelectListItem>();
            if (hasTip)
                resultList.Add(new SelectListItem { Text = tipWord, Value = "-1" });

            if (list == null) return resultList;
            var enumerable = list as T[] ?? list.ToArray();
            if (!enumerable.Any()) return resultList;

            var textList = enumerable.Select(text.Compile()).ToList();
            var valueList = enumerable.Select(value.Compile()).ToList();
            for (var i = 0; i < textList.Count; i++)
            {
                if (textList[i] == null) continue;
                resultList.Add(new SelectListItem()
                {
                    Text = textList[i].ToString(),
                    Value = valueList[i].ToString(),
                    Selected = (selectedValue != null && selectedValue.Equals(valueList[i]))
                });
            }
            return resultList;
        }

        #endregion

        #region URL 地址构造

        /// <summary>
        /// URL 地址构造
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="url">源地址</param>
        /// <param name="paramText">参数名</param>
        /// <param name="paramValue">参数值</param>
        /// <returns></returns>
        public static HtmlString BuildUrl(this HtmlHelper htmlHelper, string url, string paramText, string paramValue)
        {
            url = paramText.SetQuery(paramValue, url);
            return new HtmlString(url.ToLower());
        }

        #endregion

        #region 根据学段和年级编码获取显示的年级文本
        /// <summary>
        /// 根据学段和年级编码获取显示的年级文本
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="stage">学段</param>
        /// <param name="grade">年级</param>
        /// <returns></returns>
        public static HtmlString GetGradeText(this HtmlHelper htmlHelper, byte stage, byte grade)
        {
            string gradeStr = string.Empty;
            if (stage == (byte)StageEnum.PrimarySchool)
            {
                gradeStr = GetEnumText(htmlHelper, grade, typeof(PrimarySchoolGrade)).ToString();
            }
            else if (stage == (byte)StageEnum.JuniorMiddleSchool)
            {
                gradeStr = GetEnumText(htmlHelper, grade, typeof(JuniorMiddleSchoolGrade)).ToString();
            }
            else if (stage == (byte)StageEnum.HighSchool)
            {
                gradeStr = GetEnumText(htmlHelper, grade, typeof(HighSchoolGrade)).ToString();
            }

            return new HtmlString(gradeStr);
        }
        #endregion

        #region 根据URL匹配选中样式

        /// <summary>
        /// 根据URL匹配选中样式
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="url">当前url(/XXX/XXX)</param>
        /// <param name="css"></param>
        /// <returns></returns>
        public static HtmlString GetActiveClass(this HtmlHelper htmlHelper, string url, string css = "active")
        {
            if (string.IsNullOrWhiteSpace(url))
                return new HtmlString(string.Empty);
            var request = HttpContext.Current.Request;
            var currentUrl = string.Format("http://{0}{1}", request.ServerVariables["HTTP_HOST"], request.RawUrl);

            return currentUrl.Contains(url.ToLower())
                ? new HtmlString(css)
                : new HtmlString(string.Empty);
        }
        #endregion

        #region UrlHelper
        public static HtmlString StaticLink(this UrlHelper helper, string path)
        {
            //if (!Regex.IsMatch(path, "^/?v3/", RegexOptions.IgnoreCase))
            //{
            //    path = "v2/" + path;
            //}
            return
                new HtmlString(new Uri(new Uri(Consts.Config.StaticSite), path).AbsoluteUri.Replace("file:",
                    string.Empty));
        }
        #endregion

    }
}
