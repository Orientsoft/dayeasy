using DayEasy.Assistant.Solr;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Paper.Services.Model;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using SolrNet;
using SolrNet.Commands.Parameters;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DayEasy.Services.Helper;

namespace DayEasy.Paper.Services.Helper.Question
{
    internal class QuestionManager : SolrManager<SolrQuestion>
    {
        public static QuestionManager Instance
        {
            get { return BaseInstance<QuestionManager>(); }
        }

        /// <summary> 更新整个题目 </summary>
        /// <param name="id"></param>
        public override ResponseHeader Update(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            var repository = CurrentIocManager.Resolve<IDayEasyRepository<TQ_Question>>();
            var qItem = repository.SingleOrDefault(t => t.Id == id && t.Status == (byte)NormalStatus.Normal);
            return Update(qItem);
        }

        /// <summary> 更新整个题目 </summary>
        /// <param name="question"></param>
        public ResponseHeader Update(TQ_Question question)
        {
            if (question == null)
                return null;
            if (question.Status == (byte)NormalStatus.Delete)
            {
                return Delete(question.Id);
            }
            if (!string.IsNullOrWhiteSpace(question.ChangeSourceID))
            {
                var repository = CurrentIocManager.Resolve<IDayEasyRepository<TQ_Question>>();
                var sourceItem =
                    repository.SingleOrDefault(
                        t => t.Id == question.ChangeSourceID && t.Status == (byte)NormalStatus.Delete);
                if (sourceItem != null)
                    Delete(question.ChangeSourceID);
            }
            var item = new SolrQuestion
            {
                QuestionId = question.Id,
                Type = question.QType,
                SubjectId = question.SubjectID,
                UserId = question.AddedBy,
                AddedAt = question.AddedAt,
                UsedCount = question.UsedCount,
                AnswerCount = question.AnswerCount,
                ErrorCount = question.ErrorCount,
                Stage = question.Stage,
                Difficulty = (double)question.DifficultyStar,
                ShareRange = question.ShareRange
            };

            //分享范围

            //标签
            if (!string.IsNullOrWhiteSpace(question.TagIDs))
            {
                var tags = question.TagIDs.JsonToObject2<List<string>>();
                item.Tags = tags.ToArray();
            }
            //知识点
            if (!string.IsNullOrWhiteSpace(question.KnowledgeIDs))
            {
                item.Points = new List<string> { question.KnowledgeIDs };
            }

            //body
            var bodys = new List<string> { question.QContent };
            item.Bodys = bodys.ToArray();
            return Solr.Add(item);
        }

        private List<string> pointslist = new List<string>();
        #region 问题查询

        /// <summary> 问题查询 </summary>
        /// <param name="queryInfo"></param>
        /// <returns></returns>
        public SolrQueryResults<SolrQuestion> Query(QuestionQuery queryInfo,bool isNullData=false)
        {
            var options = new QueryOptions
            {
                Start = queryInfo.Page * queryInfo.Size, //开始项
                Rows = queryInfo.Size //数据条数
            };
            List<KeyValuePair<string, string>> extraParams = new List<KeyValuePair<string, string>>();
            if (queryInfo.IsHighLight)
            {
                options.Highlight = new HighlightingParameters
                {
                    Fields = new List<string> { "bodys", "tags" },
                    BeforeTerm = "<font class=\"d-keyword\"><b>",
                    AfterTerm = "</b></font>"
                };
            }

            var querys = new List<ISolrQuery>();
            //学段
            if (queryInfo.Stages != null && queryInfo.Stages.Any())
                querys.Add(new SolrQueryInList("stage",
                    queryInfo.Stages.Select(t => t.ToString(CultureInfo.InvariantCulture))));
            //学科
            if (queryInfo.SubjectId > 0)
                querys.Add(new SolrQueryByField("subject_id", queryInfo.SubjectId.ToString(CultureInfo.InvariantCulture)));
            //题型
            if (queryInfo.QuestionType > 0)
                querys.Add(new SolrQueryByField("type", queryInfo.QuestionType.ToString(CultureInfo.InvariantCulture)));
            //难度系数
            if (queryInfo.Difficulties != null)
                if (queryInfo.Difficulties.Any()) {
                    querys.Add(new SolrQueryInList("difficulty",
                                        queryInfo.Difficulties.Select(t => t.ToString(CultureInfo.InvariantCulture))));
                }
                
            //分享范围
            if (queryInfo.ShareRange >= 0)
            {
                switch (queryInfo.ShareRange)
                {
                    case 2:
                        querys.Add(new SolrQueryByField("share_range", "2"));
                        break;
                    default:
                        querys.Add(new SolrQueryByField("added_by",
                            queryInfo.AddedBy.ToString(CultureInfo.InvariantCulture)));
                        break;
                }
            }
            //关键字
            if (!string.IsNullOrWhiteSpace(queryInfo.Keyword))
            {
                var list = queryInfo.Keyword.Split(' ');
                var queryList = new List<ISolrQuery>();
                foreach (var key in list)
                {
                    var keyQuerys = new List<ISolrQuery>
                    {
                        new SolrQuery("points:\"" + key + "\"").Boost(5),
                        new SolrQuery("tags:\"" + key + "\"").Boost(5),
                        new SolrQueryByField("tags", key)
                    };
                    if (key.StartsWith("[") && key.EndsWith("]"))
                    {
                        keyQuerys.Add(new SolrQuery("bodys:\"" + key.Trim('[', ']') + "\""));
                    }
                    else
                    {
                        keyQuerys.Add(new SolrQueryByField("bodys", key));
                    }
                    queryList.Add(new SolrMultipleCriteriaQuery(keyQuerys, SolrMultipleCriteriaQuery.Operator.OR));
                }
                querys.Add(new SolrMultipleCriteriaQuery(queryList, SolrMultipleCriteriaQuery.Operator.AND));
            }
            //知识点搜索
            if (queryInfo.Points != null && queryInfo.Points.Any())
            {
                var st = queryInfo.Points.ToList();
                if (isNullData)
                {
                    querys.Add(
                    new SolrMultipleCriteriaQuery(
                        queryInfo.Points.Select(t => new SolrQuery("points:" + t)),
                        SolrMultipleCriteriaQuery.Operator.OR));
                }
                else
                {
                    querys.Add(
                    new SolrMultipleCriteriaQuery(
                        queryInfo.Points.Select(t => new SolrQuery("points:" + t)),
                        SolrMultipleCriteriaQuery.Operator.AND));
                }

                //options.ExtraParams = new Dictionary<string, string>()
                //{
                //    {"defType", "edismax"},
                //    {"bf", "if(exists(query({!v='tags:*" + queryInfo.TagSortFirstStr + "*'})),20,0)"}
                //};
                //var pointsQuerys = new SolrQueryByField("points", "\"" + queryInfo.Points.First() + "\""); //new SolrQueryInList("points", queryInfo.Points.Select(t => "\"" + t + "\""));
                //querys.Add(pointsQuerys);
            }
            if (queryInfo.Tags != null && queryInfo.Tags.Any())
            {
                querys.Add(
                    new SolrMultipleCriteriaQuery(
                        queryInfo.Tags.Select(t => new SolrQuery("tags:\"" + t + "\"")),
                        SolrMultipleCriteriaQuery.Operator.AND));
            }
            if (queryInfo.NotInIds != null && queryInfo.NotInIds.Any())
            {
                querys.Add(new SolrNotQuery(new SolrQueryInList("id", queryInfo.NotInIds)));
            }
            //            querys.Add(new SolrQuery("tags:模拟实验"));
            //条件集合之间的关系
            var query = new SolrMultipleCriteriaQuery(querys, SolrMultipleCriteriaQuery.Operator.AND);

            //排序
            if ((byte)queryInfo.Order > 0)
                options.AddOrder(SolrHelper.Instance.SortOrder(queryInfo.Order));
            if (!string.IsNullOrWhiteSpace(queryInfo.Keyword))
            {
                options.AddOrder(new SortOrder("score", Order.DESC));
            }

            if (!string.IsNullOrEmpty(queryInfo.TagSortFirstStr))
            {
                /*
                 *  两种方式都能实现 tags 中包含 queryInfo.TagSortFirstStr 的优先排序 
                 * but
                 *  AddOrder 方式会影响原有的 score 排序权重
                 *  edismax+bf 方式不会影响原有 score 排序权重 相当于把优先的提前了
                 */

                //options.AddOrder(new SortOrder("if(exists(query({!v='tags:*" + queryInfo.TagSortFirstStr + "*'})),20,1)", Order.DESC));

                extraParams.Add(new KeyValuePair<string, string>("{'defType', 'edismax'}", "{'bf', 'if (exists(query({ !v = 'tags:*" + queryInfo.TagSortFirstStr + "*'})),20,0)'}"));
                options.ExtraParams = extraParams;
                //options.ExtraParams = new Dictionary<string, string>()
                //{
                //    {"defType", "edismax"},
                //    {"bf", "if(exists(query({!v='tags:*" + queryInfo.TagSortFirstStr + "*'})),20,0)"}
                //};
            }
            return Solr.Query(query, options);
        }
        private void Combine(List<string> strList, int v_Start, int v_ResultLength, List<string> list)
        {
            if (v_ResultLength == 0)
            {
                string lv_TempCombine = string.Empty;
                for (int j = 0; j < list.Count; j++)
                    lv_TempCombine += list[j].ToString() + ",";
                pointslist.Add(lv_TempCombine);
                return;
            }
            if (v_Start == strList.Count)
                return;
            list.Add(strList[v_Start]);
            Combine(strList, v_Start + 1, v_ResultLength - 1, list);
            list.Remove(strList[v_Start]);
            Combine(strList, v_Start + 1, v_ResultLength, list);
        }
        private void Permutation(List<string> strList)
        {
            //char[] lv_Char = inputStr.ToCharArray();
            List<string> list = new List<string>();
            for (int i = 1, len = strList.Count; i <= len; i++)
                Combine(strList, 0, i, list);
        }

        public DResults<QuestionDto> Search(QuestionQuery questionInfo,bool isNullData=false)
        {
            var list = Query(questionInfo,isNullData);
            if (!list.Any())
                return DResult.Succ(new List<QuestionDto>(), 0);
            var questions =
                CurrentIocManager.Resolve<IPaperContract>().LoadQuestions(list.Select(t => t.QuestionId).ToArray());
            var hasFormula = SystemCache.Instance.SubjectLoadFormula(questionInfo.SubjectId);
            var users = new Dictionary<long, string>();
            if (questionInfo.ShareRange != (byte)ShareRange.Self && questionInfo.LoadCreator)
            {
                var ids = questions.Select(t => t.UserId).Distinct();
                users = CurrentIocManager.Resolve<IUserContract>().UserNames(ids);
            }
            foreach (var item in list)
            {
                var question = questions.First(t => t.Id == item.QuestionId);
                if (users.ContainsKey(item.UserId))
                    question.UserName = users[item.UserId];
                //高亮 Todo 有公式时，解析异常！~
                if (questionInfo.IsHighLight && list.Highlights.ContainsKey(question.Id))
                {
                    var high = list.Highlights[question.Id];
                    if (high.ContainsKey("bodys"))
                    {
                        var bodys = high["bodys"];
                        if (bodys.Count > 0)
                        {
                            var body = bodys.Aggregate(string.Empty, (c, t) => c + t);
                            question.Body = RepleaceBody(question.Body, body, questionInfo.Keyword, hasFormula);
                        }
                    }
                }
                question.Range = (byte)item.ShareRange;
            }
            return DResult.Succ(questions, list.NumFound);
        }

        public DResults<string> SearchForIds(QuestionQuery questionInfo)
        {
            var list = Query(questionInfo);
            return DResult.Succ(list.Select(t => t.QuestionId), list.NumFound);
        }

        /// <summary> 变式 </summary>
        /// <param name="question"></param>
        /// <param name="count"></param>
        /// <param name="notInIds"></param>
        /// <returns></returns>
        public DResults<QuestionDto> Variant(QuestionDto question, int count = 1, List<string> notInIds = null, bool isNullData = false)
        {
            var difficulty = (double)question.Difficulty;
            //            var reg = new Regex("(<[a-z0-9]+[^>]*>)|(\\[[\\w\\W]+?\\])", RegexOptions.IgnoreCase);

            notInIds = notInIds ?? new List<string>();
            notInIds.Add(question.Id);

            //            var key = reg.Replace(question.Body, string.Empty);
            var searchInfo = new QuestionQuery
            {
                SubjectId = question.SubjectId,
                Points = question.Knowledges != null ? question.Knowledges.Select(k => k.Id) : null,
                Difficulties = new[] { difficulty - 1, difficulty, difficulty + 1 },
                //Keyword = key,
                Stages = new[] { (int)question.Stage },
                QuestionType = question.Type,
                ShareRange = -1,
                Page = 0,
                Size = count,
                NotInIds = notInIds,
                TagSortFirstStr = "精选"
            };
            DResults<QuestionDto> temp = DResult.Errors<QuestionDto>("");
            pointslist = new List<string>();
            Permutation(searchInfo.Points.ToList());
            pointslist = pointslist.OrderByDescending(w => w.Length).ToList();
            var isSb = pointslist == null || pointslist.Count <= 0 ? false : true;
            if (isNullData)
            {
                searchInfo.Points = pointslist;
                temp = Search(searchInfo,true);
            }
            else
            {
                foreach (var item in pointslist)
                {
                    string[] arr = item.Replace(',', '.').Trim('.').Split('.');
                    searchInfo.Points = arr;
                    temp = Search(searchInfo,isNullData);
                    if (temp.Status && temp.TotalCount > 0)
                    {
                        foreach (var t in temp.Data)
                        {
                            foreach (var k in t.Knowledges)
                            {
                                var point = pointslist.FirstOrDefault().Replace(',', '.').Split('.').Where(w => w.Contains(k.Id)).FirstOrDefault();
                                if (point != null)
                                    isSb = true;
                                else { isSb = false; break; }
                            }
                        }
                        if (isSb)
                            return temp;
                        else { temp = DResult.Succ(new List<QuestionDto>(), 0); continue; }

                    }
                }

            }
            return temp;
        }
        /// <summary> 变式（返回变式的Ids） </summary>
        /// <param name="question"></param>
        /// <param name="count"></param>
        /// <param name="notInIds"></param>
        /// <returns></returns>
        public DResults<string> VariantIds(QuestionDto question, int count = 1, List<string> notInIds = null)
        {
            var difficulty = (double)question.Difficulty;
            //            var reg = new Regex("(<[a-z0-9]+[^>]*>)|(\\[[\\w\\W]+?\\])", RegexOptions.IgnoreCase);

            notInIds = notInIds ?? new List<string>();
            notInIds.Add(question.Id);

            //            var key = reg.Replace(question.Body, string.Empty);
            var searchInfo = new QuestionQuery
            {
                SubjectId = question.SubjectId,
                Points = question.Knowledges.Select(k => k.Id),
                Difficulties = new[] { difficulty - 1, difficulty, difficulty + 1 },
                //Keyword = key,
                Stages = new[] { (int)question.Stage },
                QuestionType = question.Type,
                ShareRange = -1,
                Page = 0,
                Size = count,
                NotInIds = notInIds,
                TagSortFirstStr = "精选"
            };

            var list = Query(searchInfo);
            return list.Any()
                ? DResult.Succ(list.Select(t => t.QuestionId).ToList(), list.NumFound)
                : DResult.Succ(new List<string>(), list.NumFound);
        }


        private static readonly Regex FormulaReg = new Regex(@"(\\\[[\w\W]*?\\\])|(\\\([\w\W]*?\\\))|(\$[^\$]*\$)",
            RegexOptions.Multiline);

        private static readonly Regex HighReg = new Regex("<font class=\"d-keyword\"><b>([^<]*)</b></font>",
            RegexOptions.IgnoreCase);

        private static readonly Regex NumberKeyReg = new Regex("[0-9a-z\\+\\-\\*\\/]", RegexOptions.IgnoreCase);

        /// <summary>
        /// 高亮处理，解决有公式时渲染问题
        /// </summary>
        /// <param name="body"></param>
        /// <param name="highLights"></param>
        /// <param name="keyword"></param>
        /// <param name="hasFormula"></param>
        /// <returns></returns>
        private string RepleaceBody(string body, string highLights, string keyword, bool hasFormula = false)
        {
            if (string.IsNullOrWhiteSpace(highLights) || string.IsNullOrWhiteSpace(body))
                return body;
            if (NumberKeyReg.IsMatch(keyword))
                return body;
            //公式搜索
            if (hasFormula && FormulaReg.IsMatch(keyword))
            {
                foreach (Match match in FormulaReg.Matches(body))
                {
                    if (match.Value == keyword)
                        body = body.Replace(match.Value,
                            string.Format("<span class=\"d-keyword\">{0}</span>", match.Value));
                }
                return body;
            }

            foreach (Match match in HighReg.Matches(highLights))
            {
                body = body.Replace(match.Groups[1].Value, match.Value);
            }
            return body;
        }

        #endregion
    }
}
