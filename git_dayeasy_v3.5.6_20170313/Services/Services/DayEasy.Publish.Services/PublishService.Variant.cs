using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Publish;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Dtos.Variant;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Publish.Services.Helper;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;

namespace DayEasy.Publish.Services
{
    /// <summary> 试卷变式相关 </summary>
    public partial class PublishService
    {
        public IDayEasyRepository<TQ_VariantRelation, string> VariantRelationRepository { private get; set; }
        public IDayEasyRepository<TP_Variant, string> VariantRepository { private get; set; }
        public IDayEasyRepository<TP_ErrorReason, string> ErrorReasonRepository { private get; set; }

        #region 变式过关的问题

        /// <summary>
        /// 变式过关的问题
        /// </summary>
        /// <returns></returns>
        public DResult<VariantQuDto> VariantQuestions(string batchNo, long userId)
        {
            if (string.IsNullOrEmpty(batchNo))
                return DResult.Error<VariantQuDto>("参数错误！");

            var publishModel = UsageRepository.SingleOrDefault(u => u.Id == batchNo);
            if (publishModel == null)
                return DResult.Error<VariantQuDto>("没有找到该作业记录！");

            var status = GroupContract.IsGroupMember(userId, publishModel.ClassId);
            if (status != CheckStatus.Normal)
                return DResult.Error<VariantQuDto>("你没有查看该数据的权限！");

            var paper = PaperRepository.SingleOrDefault(u => u.Id == publishModel.SourceID);
            if (paper == null)
                return DResult.Error<VariantQuDto>("没有找到该试卷！");

            var result = new VariantQuDto
            {
                Batch = publishModel.Id,
                PaperId = paper.Id,
                PaperName = paper.PaperTitle,
                SourceType = publishModel.SourceType
            };

            var qList = new List<string>();

            //教师推送
            var variantList =
                VariantRepository.Where(u => u.Batch == batchNo && u.PaperId == paper.Id)
                    .Select(t => new { t.QID, t.VIDs }).ToList();
            if (variantList.Count > 0)
            {
                //查询老师名称
                var teacher = UserContract.Load(publishModel.UserId);
                if (teacher != null)
                {
                    result.TeacherName = teacher.Name;
                }

                result.VariantQuDic =
                    variantList.Select(u => new { QId = u.QID, Vids = JsonHelper.Json<List<string>>(u.VIDs) })
                        .Where(u => u.Vids != null && u.Vids.Count > 0)
                        .GroupBy(u => u.QId)
                        .ToDictionary(u => u.Key, u => u.SelectMany(t => t.Vids).Distinct().ToList());

                qList = result.VariantQuDic.SelectMany(u => u.Value)
                    .Union(result.VariantQuDic.Keys)
                    .Distinct()
                    .ToList();

                //教师推送的题目的Ids集合
                var variantQuestions = PaperContract.LoadQuestions(qList.ToArray());
                if (variantQuestions != null)
                {
                    result.Questions.AddRange(variantQuestions);
                }
            }
            else
            {
                //查询错误问题Ids
                var errorQuIds =
                    ErrorQuestionRepository.Where(
                        u => u.Batch == batchNo && u.PaperID == paper.Id && u.StudentID == userId)
                        .Select(u => u.QuestionID)
                        .Distinct()
                        .ToList();
                if (errorQuIds.Count < 1)
                    return DResult.Succ(result);
                qList.AddRange(errorQuIds);

                foreach (var q in errorQuIds)
                {
                    var sysResult = Variant(q, 1, qList);
                    if (!sysResult.Status || sysResult.Data == null)
                        continue;

                    var sysQus = sysResult.Data.Select(u => u.Id).ToList();
                    if (sysQus.Count <= 0)
                        continue;
                    qList.AddRange(sysQus);
                    result.DeyiVariantQuDic.Add(q, sysQus);
                    result.Questions.AddRange(sysResult.Data);
                }
                var variantQuestions = PaperContract.LoadQuestions(result.DeyiVariantQuDic.Keys.ToArray());
                if (variantQuestions != null)
                {
                    result.Questions.AddRange(variantQuestions);
                }
            }
            result.Questions = result.Questions.DistinctBy(u => u.Id).ToList();

            return DResult.Succ(result);
        }

        #endregion

        /// <summary> 系统变式题 </summary>
        /// <param name="questionId"></param>
        /// <param name="count"></param>
        /// <param name="excepArray"></param>
        /// <returns></returns>
        public DResults<QuestionDto> Variant(string questionId, int count = 1, List<string> excepArray = null,bool isNull=false)
        {
            excepArray = excepArray ?? new List<string>();
            //根据变式关系推荐变式
            var vids = VariantRelationRepository.Where(t => t.QID == questionId || t.VID == questionId)
                .Select(t => new
                {
                    id = (t.QID == questionId ? t.VID : t.QID),
                    t.UseCount
                }).ToList();
            var variants = new List<QuestionDto>();
            if (vids.Any() && excepArray.Any())
            {
                vids = vids.Where(t => !excepArray.Contains(t.id)).ToList();
            }

            if (vids.Any())
            {
                var list = vids.RandomSort().Select(t => t.id).Take(count).ToArray();
                variants.AddRange(PaperContract.LoadQuestions(list));
                count -= list.Count();
                excepArray.AddRange(list);
            }
            if (count <= 0)
                return DResult.Succ(variants, -1);
            //系统变式
                var systemVariants = PaperContract.Variant(questionId, count, excepArray,isNull);
            if (systemVariants.Any())
            {
                variants.AddRange(systemVariants);
            }
            if (!variants.Any()&& excepArray.Count<3)
                variants.AddRange(PaperContract.Variant(questionId, count, excepArray,true));
            return DResult.Succ(variants, -1);
        }

        public DResult<Dictionary<string, List<QuestionDto>>> Variants(List<string> questionIds, int count = 1,
            List<string> excepArray = null)
        {
            if (questionIds == null || !questionIds.Any())
            {
                return DResult.Error<Dictionary<string, List<QuestionDto>>>("待变式题目不能为空！");
            }
            excepArray = excepArray ?? new List<string>();
            excepArray = excepArray.Union(questionIds).Distinct().ToList();
            //根据变式关系推荐变式
            var vids = VariantRelationRepository.Where(t => questionIds.Contains(t.QID) || questionIds.Contains(t.VID))
                .Select(t => new
                {
                    id = t.QID,
                    vid = t.VID,
                    count = t.UseCount
                }).ToList();
            var dict = new Dictionary<string, List<QuestionDto>>();
            foreach (var id in questionIds)
            {
                var qid = id;
                var list = new List<QuestionDto>();
                var pre = count;
                var variants = vids.Where(t => t.id == qid || t.vid == qid)
                    .Select(t => new { id = t.id == qid ? t.vid : t.id, t.count }).ToList();
                if (variants.Any())
                {
                    variants = variants.Where(t => !excepArray.Contains(t.id)).ToList();
                }

                if (variants.Any())
                {
                    var variantIds = variants.OrderBy(t => t.count).Select(t => t.id).Take(count).ToArray();
                    //todo 每次都查题库~
                    list.AddRange(PaperContract.LoadQuestions(variantIds));
                    pre -= list.Count();
                    excepArray.AddRange(variantIds);
                }
                if (pre > 0)
                {
                    //系统变式
                    var systemVariants = PaperContract.Variant(qid, pre, excepArray);
                    if (systemVariants.Any())
                    {
                        list.AddRange(systemVariants);
                        excepArray.AddRange(systemVariants.Select(t => t.Id));
                    }
                }
                dict.Add(qid, list);
            }
            return DResult.Succ(dict);
        }

        /// <summary> 推荐变式题 </summary>
        /// <param name="questionId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public DResults<QuestionDto> VariantRelationQuestion(string questionId, int count = 1)
        {
            if (count < 1) count = 1;
            var realtions = VariantRelationRepository
                .Where(v => v.QID == questionId || v.VID == questionId)
                .RandomSort().Take(count).ToList();
            var questionIds = new List<string>();
            realtions.Foreach(v =>
            {
                questionIds.Add(v.QID);
                questionIds.Add(v.VID);
            });
            var array = questionIds.Distinct().Where(i => i != questionId).ToArray();
            var questions = PaperContract.LoadQuestions(array);
            return DResult.Succ(questions, questions.Count);
        }

        /// <summary> 历史推荐的变式题 </summary>
        /// <param name="paperId"></param>
        /// <param name="questionId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DResults<QuestionDto> VariantHistory(string paperId, string questionId, long userId)
        {
            var historyItem = VariantRepository
                .Where(v => v.PaperId == paperId && v.QID == questionId && v.AddedBy == userId)
                .OrderByDescending(v => v.AddedAt).FirstOrDefault();
            if (historyItem == null)
                return DResult.Errors<QuestionDto>("没有查询到历史推荐");
            var questionIds = JsonHelper.JsonList<string>(historyItem.VIDs).ToList();
            if (!questionIds.Any())
                return DResult.Errors<QuestionDto>("没有查询到历史推荐");
            var questions = PaperContract.LoadQuestions(questionIds.ToArray());
            return DResult.Succ(questions, questions.Count);
        }

        /// <summary> 添加试卷中单题的变式推送 </summary>
        /// <param name="userId"></param>
        /// <param name="paperId"></param>
        /// <param name="questionId"></param>
        /// <param name="vids"></param>
        /// <returns></returns>
        public DResult AddVariant(long userId, string paperId, string questionId, List<string> vids)
        {
            if (userId < 1 || paperId.IsNullOrEmpty() || questionId.IsNullOrEmpty() || vids == null || !vids.Any())
                return DResult.Error("参数错误");

            if (!PaperRepository.Exists(p => p.Id == paperId))
                return DResult.Error("没有查询到试卷资料");

            var batchs = UsageRepository
                .Where(u => u.SourceID == paperId && u.UserId == userId)
                .Select(u => u.Id).ToList();
            if (!batchs.Any())
                return DResult.Error("没有查询到使用此试卷的班级");

            var exist = VariantRepository.Exists(v =>
                v.PaperId == paperId && v.AddedBy == userId && v.QID == questionId);
            if (exist)
                DResult.Error("已推送该题的变式题");

            var dt = Clock.Now;
            var jsonVids = vids.Distinct().ToJson();
            var entitys = batchs.Select(b => new TP_Variant
            {
                Id = IdHelper.Instance.GetGuid32(),
                AddedAt = dt,
                AddedBy = userId,
                PaperId = paperId,
                QID = questionId,
                VIDs = jsonVids,
                Batch = b
            }).ToList();
            if (VariantRepository.Insert(entitys.ToArray()) < 1)
                return DResult.Error("添加失败");

            //添加变式关系
            Task.Run(() => VariantHelper.AddVariantRelation(questionId, vids));
            return DResult.Success;
        }

        public bool IsSendVariant(string batch, string paperId)
        {
            return VariantRepository.Exists(t => t.Batch == batch && t.PaperId == paperId);
        }

        public DResult SendVariant(long userId, string paperId, Dictionary<string, List<string>> variantDict,
            List<string> classIds)
        {
            if (userId <= 0 || paperId.IsNullOrEmpty() || variantDict.IsNullOrEmpty() || classIds.IsNullOrEmpty())
                return DResult.Error("推送失败！");

            if (!PaperRepository.Exists(p => p.Id == paperId))
                return DResult.Error("没有查询到试卷资料");
            var batchs = UsageRepository
                .Where(u => u.SourceID == paperId && u.UserId == userId && classIds.Contains(u.ClassId))
                .Select(u => u.Id).ToList();
            if (!batchs.Any())
                return DResult.Error("没有查询到使用此试卷的班级");
            var list = new List<TP_Variant>();
            var time = Clock.Now;
            foreach (var batch in batchs)
            {
                list.AddRange(variantDict.Select(v => new TP_Variant
                {
                    Id = IdHelper.Instance.GetGuid32(),
                    AddedAt = time,
                    AddedBy = userId,
                    PaperId = paperId,
                    QID = v.Key,
                    VIDs = v.Value.ToJson(),
                    Batch = batch
                }));
            }
            if (!list.Any())
                return DResult.Error("没有任何变式可推送！");
            if (VariantRepository.Insert(list) <= 0)
                return DResult.Error("推送失败，请稍后重试！");
            Task.Run(() =>
            {
                //添加变式关系
                variantDict.Foreach(v =>
                {
                    VariantHelper.AddVariantRelation(v.Key, v.Value);
                });
            });
            return DResult.Success;
        }

        public DResult<VariantListDto> VariantList(string batch, string paperId)
        {
            var classId = UsageRepository.Where(u => u.Id == batch).Select(u => u.ClassId).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(classId))
                return DResult.Error<VariantListDto>("未找到发布批次！");
            var list = VariantRepository.Where(t => t.Batch == batch && t.PaperId == paperId)
                .Select(t => new { t.QID, t.VIDs }).ToList();
            var dto = new VariantListDto();
            if (!list.Any())
                return DResult.Succ(dto);
            var sorts = PaperContract.QuestionSorts(paperId,false);
            var errorDict = ErrorQuestionRepository.Where(t => t.Batch == batch && t.PaperID == paperId)
                .GroupBy(t => t.QuestionID)
                .Select(t => new { id = t.Key, count = t.Count() })
                .ToDictionary(k => k.id, v => v.count);
            //            dto.StudentCount = MarkingResultRepository.Count(t => t.Batch == batch && t.PaperID == paperId);
            //            if (dto.StudentCount <= 0)
            //            {
            //                dto.StudentCount = GroupContract.GroupMemberCount(classId, UserRole.Student);
            //            }

            foreach (var item in list)
            {
                var variant = new QuestionVariantDto
                {
                    Id = item.QID,
                    VariantIds = item.VIDs.JsonToObject<List<string>>()
                };
                if (sorts.ContainsKey(variant.Id))
                {
                    var sort = sorts[variant.Id];
                    variant.SectionType = sort.Key;
                    variant.Sort = sort.Value;
                }
                if (errorDict.ContainsKey(variant.Id))
                    variant.ErrorCount = errorDict[variant.Id];
                dto.Variants.Add(variant);
            }
            dto.Variants = dto.Variants.OrderBy(v => v.Sort).ToList();
            var qids = dto.Variants.SelectMany(t => t.VariantIds).Union(dto.Variants.Select(t => t.Id));
            dto.Questions = PaperContract.LoadQuestions(qids.ToArray())
                .ToDictionary(k => k.Id, v => v);
            return DResult.Succ(dto);
        }

        public DResult<VariantListDto> VariantListFromSystem(string batch, string paperId, long studentId = 0, int max = 15,
            int pre = 1)
        {
            var classId = UsageRepository.Where(u => u.Id == batch).Select(u => u.ClassId).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(classId))
                return DResult.Error<VariantListDto>("未找到发布批次！");
            var dto = new VariantListDto();
            Expression<Func<TP_ErrorQuestion, bool>> condition = t => t.Batch == batch && t.PaperID == paperId;
            //班级错题情况
            var errorDict = ErrorQuestionRepository.Where(condition)
                .GroupBy(t => t.QuestionID)
                .Select(t => new { id = t.Key, count = t.Count() })
                .ToDictionary(k => k.id, v => v.count);
            List<string> errorIds;
            if (studentId > 0)
            {
                //学生查询所有自己的错题
                condition = condition.And(t => t.StudentID == studentId);
                errorIds = ErrorQuestionRepository.Where(condition)
                    .Select(t => t.QuestionID).Distinct().ToList();
            }
            else
            {
                errorIds = errorDict.OrderByDescending(t => t.Value).Select(t => t.Key).Take(max).ToList();
            }
            //题目序号
            var sorts = PaperContract.QuestionSorts(paperId,false);
            //参考人数 or 班级总人数
            //            dto.StudentCount = MarkingResultRepository.Count(t => t.Batch == batch && t.PaperID == paperId);
            //            if (dto.StudentCount <= 0)
            //            {
            //                dto.StudentCount = GroupContract.GroupMemberCount(classId, UserRole.Student);
            //            }
            var excepts = sorts.Keys.ToList();
            var qlist = new List<QuestionDto>();
            foreach (var errorId in errorIds)
            {
                var variant = new QuestionVariantDto
                {
                    Id = errorId
                };
                if (errorDict.ContainsKey(errorId))
                    variant.ErrorCount = errorDict[errorId];
                if (sorts.ContainsKey(variant.Id))
                {
                    var sort = sorts[variant.Id];
                    variant.SectionType = sort.Key;
                    variant.Sort = sort.Value;
                }
                //加载变式数量
                if (pre > 0)
                {
                    var list = Variant(errorId, pre, excepts);
                    if (list.Status && list.Data != null && list.Data.Any())
                    {
                        qlist.AddRange(list.Data);
                        variant.VariantIds = list.Data.Select(t => t.Id).ToList();
                        excepts.AddRange(variant.VariantIds);
                    }
                }
                dto.Variants.Add(variant);
            }
            //变式排序
            dto.Variants = (studentId > 0
                ? dto.Variants.OrderBy(v => v.Sort).ToList()
                : dto.Variants.OrderByDescending(v => v.ErrorCount).ThenBy(v => v.Sort).ToList());
            var qids = dto.Variants.Select(t => t.Id).ToArray();
            //加载原题
            qlist.AddRange(PaperContract.LoadQuestions(qids));
            dto.Questions = qlist.ToDictionary(k => k.Id, v => v);
            return DResult.Succ(dto);
        }

        public DResult<PaperWeakDto> PaperWeak(string batch, string paperId)
        {
            var dto = new PaperWeakDto();
            //错因标签
            var tagData =
                ErrorReasonRepository.Where(
                    t => t.Batch == batch && t.PaperId == paperId && t.Tags != null && t.Tags != "")
                    .Select(t => t.Tags).ToList();
            if (tagData.Any())
            {
                var tags = tagData.SelectMany(t => JsonHelper.JsonList<NameDto>(t)).GroupBy(t => new { t.Id, t.Name })
                    .OrderByDescending(d => d.Count()).Take(5)
                    .ToDictionary(k => k.Key.Name, v => v.Count());
                dto.ErrorTags = tags;
            }
            //知识点
            var errorData = ErrorQuestionRepository.Where(t => t.Batch == batch && t.PaperID == paperId)
                .Select(t => t.QuestionID).GroupBy(t => t)
                .ToDictionary(k => k.Key, v => v.Count());
            if (errorData.Any())
            {
                var dict = new Dictionary<string, int>();
                var list = PaperContract.LoadQuestions(errorData.Keys.ToArray());
                foreach (var item in list)
                {
                    int count = errorData[item.Id];
                    foreach (var knowledge in item.Knowledges)
                    {
                        if (dict.ContainsKey(knowledge.Name))
                            dict[knowledge.Name] += count;
                        else
                        {
                            dict.Add(knowledge.Name, count);
                        }
                    }
                }
                dto.Knowledges = dict.OrderByDescending(t => t.Value).Take(5).ToDictionary(k => k.Key, v => v.Value);
            }
            return DResult.Succ(dto);
        }

        public Dictionary<string, string> UsageList(string paperId, long userId)
        {
            var usages =
                UsageRepository.Where(
                    t =>
                        t.SourceID == paperId && t.UserId == userId &&
                        (t.SourceType == (byte)PublishType.Test || t.MarkingStatus == (byte)MarkingStatus.AllFinished))
                    .Select(t => t.ClassId).Distinct().ToList();
            return usages.Any() ? GroupContract.GroupDict(usages) : new Dictionary<string, string>();
        }
    }
}
