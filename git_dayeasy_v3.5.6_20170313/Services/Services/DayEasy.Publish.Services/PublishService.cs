using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Dtos.Publish;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Core.Domain;
using DayEasy.EntityFramework;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.Publish.Services
{
    public partial class PublishService : DayEasyService, IPublishContract
    {
        private readonly ILogger _logger = LogManager.Logger<PublishService>();

        public IStatisticContract StatisticContract { private get; set; }
        public IGroupContract GroupContract { private get; set; }
        public IPaperContract PaperContract { private get; set; }
        public IUserContract UserContract { private get; set; }
        public IDayEasyRepository<TC_Usage> UsageRepository { private get; set; }
        public IDayEasyRepository<TP_Paper> PaperRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingResult> MarkingResultRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingDetail> MarkingDetailRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingPicture> MarkingPictureRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingMark, string> MarkingMarkRepository { private get; set; }
        public IDayEasyRepository<TP_ErrorQuestion> ErrorQuestionRepository { private get; set; }
        public PublishService(IDbContextProvider<DayEasyDbContext> context)
            : base(context)
        { }

        #region Private Method

        #region 知识点分析
        /// <summary>
        /// 知识点分析
        /// </summary>
        /// <param name="errorQIds"></param>
        /// <returns></returns>
        private List<KpAnalysisDto> KpAnalysis(List<string> errorQIds)
        {
            if (errorQIds == null || errorQIds.Count < 1)
                return null;

            //查询问题知识点
            var questionKps =
                CurrentIocManager.Resolve<IDayEasyRepository<TQ_Question>>()
                    .Where(u => errorQIds.Contains(u.Id))
                    .Select(q => new
                    {
                        QId = q.Id,
                        Kps = q.KnowledgeIDs
                    }).ToList();

            if (questionKps.Count < 1)
                return null;

            //所有的知识点
            var kpDicList = questionKps.Select(u => JsonHelper.Json<Dictionary<string, string>>(u.Kps)).ToList();
            if (!kpDicList.Any())
                return null;

            var kpList = kpDicList.SelectMany(u => u.Select(d => new KpAnalysisDto { KpCode = d.Key, KpName = d.Value }))
                .GroupBy(u => u.KpCode)
                .Select(u => new KpAnalysisDto()
                {
                    KpCode = u.Key,
                    KpName = u.First().KpName,
                    ErrorCount = u.Count()
                }).ToList();

            return kpList;
        }
        #endregion

        #region 推送到班级圈发布记录
        /// <summary>
        /// 推送到班级圈发布记录
        /// </summary>
        /// <returns></returns>
        private List<TC_Usage> PubPaper2Class(long userId, int subjectId, string pId, List<string> groupIds, PublishType type, string sourceGId)
        {
            if (groupIds == null || groupIds.Count < 1)
                return null;

            var dt = Clock.Now;
            var list = new List<TC_Usage>();
            groupIds.ForEach(groupId =>
            {
                var item = new TC_Usage
                {
                    Id = IdHelper.Instance.Guid32,
                    AddedAt = dt,
                    AddedIP = Utils.GetRealIp(),
                    ClassId = groupId,
                    ExpireTime = dt,
                    IsControlOrder = false,
                    SourceID = pId,
                    SourceType = (byte)type,
                    StartTime = dt,
                    SubjectId = subjectId,
                    UserId = userId,
                    MarkingStatus = (byte)MarkingStatus.NotMarking,
                    ColleagueGroupId = sourceGId,
                    Status = (byte)NormalStatus.Normal
                };
                if (type == PublishType.Print)
                    item.ApplyType = (byte)ApplyType.Print;
                else
                {
                    item.ApplyType = (byte)ApplyType.Web;
                }
                list.Add(item);
            });
            return list;
        }
        #endregion

        #region 推送到班级圈的动态消息
        /// <summary>
        /// 推送到班级圈的动态消息
        /// </summary>
        /// <returns></returns>
        private List<DynamicSendDto> PubPaper2Class(long userId, Dictionary<string, string> batchGroupDic, string remark)
        {
            if (batchGroupDic == null || batchGroupDic.Count < 1)
                return null;

            var msgList = new List<DynamicSendDto>();
            batchGroupDic.Foreach(batchGroup => msgList.Add(new DynamicSendDto()
            {
                ContentType = (byte)ContentType.Publish,
                ContentId = batchGroup.Key,//批次号
                DynamicType = GroupDynamicType.Homework,
                GroupId = batchGroup.Value,
                Message = remark,
                ReceivRole = UserRole.Student | UserRole.Teacher,
                UserId = userId
            }));

            return msgList;
        }
        #endregion

        #region 推送到同事圈的动态消息
        /// <summary>
        /// 推送到同事圈的动态消息
        /// </summary>
        /// <returns></returns>
        private List<DynamicSendDto> PubPaper2Colleague(long userId, string pId, List<string> groupIds, string remark)
        {
            if (groupIds == null || groupIds.Count < 1)
                return null;

            var msgList = new List<DynamicSendDto>();
            groupIds.Foreach(groupId => msgList.Add(new DynamicSendDto()
            {
                ContentType = (byte)ContentType.Paper,
                ContentId = pId, //试卷Id
                DynamicType = GroupDynamicType.Homework,
                GroupId = groupId,
                Message = remark,
                ReceivRole = UserRole.Teacher,
                UserId = userId
            }));

            return msgList;
        }
        #endregion

        #endregion

        #region 推送试卷

        /// <summary> 推送试卷 </summary>
        /// <returns></returns>
        public DResult PulishPaper(string pId, string groupIds, string sourceGId, long userId, string sendMsg)
        {
            if (string.IsNullOrEmpty(pId))
                return DResult.Error("请选择要推送的试卷！");

            if (string.IsNullOrEmpty(groupIds))
                return DResult.Error("请选择要推送到的圈子！");

            var groupList = JsonHelper.Json<List<string>>(groupIds);
            if (groupList == null || groupList.Count < 1)
                return DResult.Error("请选择要推送到的圈子！");

            var paperModel = PaperRepository.SingleOrDefault(p => p.Id == pId && p.Status == (byte)PaperStatus.Normal);
            if (paperModel == null)
                return DResult.Error("该试卷不存在！");

            //TODO:验证是否是圈子内的卷子
            //if (paperModel.AddedBy != userId && paperModel.ShareRange != (byte)ShareRange.Public)
            //    return DResult.Error("你没有推送该试卷的权限，请刷新重试！");

            if (groupList.Select(g => GroupContract.IsGroupMember(userId, g))
                .Any(status => status != CheckStatus.Normal))
            {
                return DResult.Error("只能推送到自己的圈子！");
            }

            var group = GroupContract.SearchGroups(groupList);
            if (!group.Status || group.Data == null) return DResult.Error("没有找到相关圈子，推送失败，请稍后重试！");

            if (paperModel.AddedBy != userId)
            {
                //验证是不是我的同事圈子
                var groups = GroupContract.IsGroupMember(userId, sourceGId);
                if (groups != CheckStatus.Normal)
                    return DResult.Error("您没有发布该试卷的权限！");

                //验证试卷是不是在该同事圈内
                var existsPaper =
                    CurrentIocManager.Resolve<IVersion3Repository<TM_GroupDynamic>>()
                        .Exists(
                            u =>
                                u.ContentId == paperModel.Id
                                && u.GroupId == sourceGId
                                && u.Status == (byte)NormalStatus.Normal
                                && u.ContentType == (byte)ContentType.Paper);
                if (!existsPaper)
                    return DResult.Error("您没有发布该试卷的权限！");
            }

            if (!string.IsNullOrEmpty(sendMsg) && sendMsg.Length > 140)
            {
                sendMsg = sendMsg.Substring(0, 140);
            }

            //处理推送到班级圈+同事圈
            //班级圈Ids
            var classIds = group.Data.Where(u => u.Type == (byte)GroupType.Class).Select(u => u.Id).ToList();

            //同事圈Ids
            var colleagueIds = group.Data.Where(u => u.Type == (byte)GroupType.Colleague).Select(u => u.Id).ToList();

            try
            {
                //动态消息
                var msgList = new List<DynamicSendDto>();

                //发布表数据
                var usageList = PubPaper2Class(userId, paperModel.SubjectID, paperModel.Id, classIds, PublishType.Test,
                    sourceGId);
                if (usageList != null && usageList.Count > 0)
                {
                    var batchGroupDic = usageList.ToDictionary(u => u.Id, u => u.ClassId);

                    var pubMsgList = PubPaper2Class(userId, batchGroupDic, sendMsg); //推送到班级的动态消息
                    if (pubMsgList != null && pubMsgList.Count > 0)
                    {
                        msgList.AddRange(pubMsgList);
                    }
                }
                //推送到同事圈的动态消息
                var pubCollMsgList = PubPaper2Colleague(userId, paperModel.Id, colleagueIds, sendMsg);
                if (pubCollMsgList != null && pubCollMsgList.Count > 0)
                {
                    msgList.AddRange(pubCollMsgList);
                }

                UnitOfWork.Transaction(() =>
                {
                    if (usageList != null && usageList.Count > 0)
                    {
                        UsageRepository.Insert(usageList); //新增到发布表
                        StatisticContract.UpdateStatistic(new TeacherStatisticDto() //更新教师统计数据
                        {
                            PublishPaperCount = usageList.Count,
                            UserID = userId
                        });

                        //更新试卷使用次数
                        paperModel.UseCount = paperModel.UseCount + usageList.Count;
                        paperModel.IsUsed = true;
                        PaperRepository.Update(p => new { p.UseCount, p.IsUsed }, paperModel);
                    }

                    if (msgList.Count > 0)
                    {
                        //todo:ybg 事务需要调一调
                        CurrentIocManager.Resolve<IMessageContract>().SendDynamics(msgList); //推送动态消息（班级圈+同事圈）
                    }
                });
                return DResult.Succ("推送成功！老师辛苦了！");
            }
            catch (Exception ex)
            {
                _logger.Info(ex);
                return DResult.Error("推送失败，请稍后重试！");
            }
        }

        #endregion

        #region 获取发布详细信息

        PublishModelDto ConvertToPublishModelDto(TC_Usage usageModel)
        {
            if (usageModel == null) return null;
            return new PublishModelDto
            {
                ApplyType = usageModel.ApplyType,
                Batch = usageModel.Id,
                ClassId = usageModel.ClassId,
                CreatorId = usageModel.UserId,
                ExpireTime = usageModel.ExpireTime,
                SourceId = usageModel.SourceID,
                SourceType = usageModel.SourceType,
                StartTime = usageModel.StartTime,
                PrintType = usageModel.PrintType,
                MarkingStatus = usageModel.MarkingStatus,
                ColleagueGroupId = usageModel.ColleagueGroupId
            };
        }

        /// <summary>
        /// 获取发布详细信息
        /// </summary>
        /// <returns></returns>
        public DResult<PublishModelDto> GetUsageDetail(string batch)
        {
            if (string.IsNullOrEmpty(batch)) return DResult.Error<PublishModelDto>("参数错误！");
            var usageModel = UsageRepository.SingleOrDefault(u => u.Id == batch);
            if (usageModel == null) return DResult.Error<PublishModelDto>("没有相关数据");
            var result = ConvertToPublishModelDto(usageModel);

            var group = GroupContract.LoadById(result.ClassId);
            if (group.Status && group.Data != null) result.ClassName = group.Data.Name;

            return DResult.Succ(result);
        }

        /// <summary>
        /// 获取发布详细信息
        /// </summary>
        /// <returns></returns>
        public DResults<PublishModelDto> GetUsageDetailByJointBatch(string jointBatch)
        {
            if (jointBatch.IsNullOrEmpty()) return DResult.Errors<PublishModelDto>("参数错误");
            var usages = UsageRepository.Where(u => u.JointBatch == jointBatch).ToList();
            if (!usages.Any()) return DResult.Errors<PublishModelDto>("没有相关数据");
            var list = new List<PublishModelDto>();
            usages.ForEach(usage => list.Add(ConvertToPublishModelDto(usage)));
            return DResult.Succ(list, list.Count);
        }

        #endregion

        #region 获取学生的作业
        /// <summary> 获取学生的作业 </summary>
        /// <returns></returns>
        public DResults<StudentWorksDto> GetStudentHomeworks(SearchStuWorkDto searchDto)
        {
            //查询我的圈子
            var groups = GroupContract.Groups(searchDto.UserId, (int)GroupType.Class, containsAll: true);
            if (!groups.Status || groups.Data == null || !groups.Data.Any())
                return DResult.Errors<StudentWorksDto>("没有找到加入的任何圈子！");

            var groupIds = groups.Data.Select(u => u.Id).ToList();//圈子Id

            //查询作业列表
            var studentWorks = UsageRepository.Where(u => (u.SourceType != (byte)PublishType.Video) && u.Status == (byte)NormalStatus.Normal && u.StartTime < Clock.Now && groupIds.Contains(u.ClassId));


            if (searchDto.SubjectId > 0)
            {
                studentWorks = studentWorks.Where(u => u.SubjectId == searchDto.SubjectId);
            }

            Expression<Func<TP_Paper, bool>> paperCondition = u => true;
            if (!string.IsNullOrEmpty(searchDto.Key))
            {
                paperCondition = paperCondition.And(p => p.PaperTitle.Contains(searchDto.Key) || p.KnowledgeIDs.Contains(searchDto.Key) || p.TagIDs.Contains(searchDto.Key));
            }

            var tempResult = studentWorks.Join(PaperRepository.Table.Where(paperCondition), s => s.SourceID, p => p.Id, (s, p) => new StudentWorksDto
            {
                Batch = s.Id,
                GroupId = s.ClassId,
                StartTime = s.StartTime,
                PaperId = p.Id,
                PaperName = p.PaperTitle,
                SubjectId = s.SubjectId,
                IsFinished = (s.MarkingStatus & (byte)MarkingStatus.AllFinished) > 0,
                SourceType = s.SourceType
            });

            var count = tempResult.Count();
            var result = tempResult.OrderByDescending(u => u.StartTime).Skip(searchDto.Size * searchDto.Page).Take(searchDto.Size).ToList();

            //查询圈子名称
            if (result.Count > 0)
            {
                result.ForEach(s =>
                {
                    var group = groups.Data.SingleOrDefault(u => u.Id == s.GroupId);
                    s.GroupName = group == null ? string.Empty : group.Name;
                });
            }

            return DResult.Succ(result, count);
        }
        #endregion

        #region 获取教师布置的作业

        public DResults<PublishPaperDto> GetTeacherPubWorks(long teacherId, string key, DPage page)
        {
            if (teacherId <= 0)
                return DResult.Succ(new List<PublishPaperDto>(), -1);
            Expression<Func<TC_Usage, bool>> condition =
                uc => uc.UserId == teacherId && uc.Status == (byte)NormalStatus.Normal
                      && uc.SourceType != (byte)PublishType.Video
                      && (uc.JointBatch == null || uc.JointBatch == "" ||
                          uc.MarkingStatus == (byte)MarkingStatus.AllFinished);
            var usages = UsageRepository.Where(condition)
                .Join(PaperRepository.Table, s => s.SourceID, p => p.Id, (uc, p) => new { uc, p });
            if (!string.IsNullOrWhiteSpace(key))
            {
                usages =
                    usages.Where(
                        t =>
                            t.p.PaperTitle.Contains(key) || t.p.PaperNo == key || t.p.KnowledgeIDs.Contains(key) ||
                            t.p.TagIDs.Contains(key));
            }
            var count = usages.Count();
            var pageList = usages.Select(t => new PublishPaperDto
            {
                Batch = t.uc.Id,
                PaperId = t.p.Id,
                PaperName = t.p.PaperTitle,
                PaperType = t.p.PaperType,
                ExpireTime = t.uc.ExpireTime,
                GroupId = t.uc.ClassId,
                SubjectId = t.p.SubjectID,
                MarkingStatus = t.uc.MarkingStatus,
                SourceType = t.uc.SourceType,
                ACount = 0,
                BCount = 0,
                IsJoint = t.uc.JointBatch != null,
                GroupName = string.Empty
            })
                .OrderByDescending(t => t.ExpireTime)
                .Skip(page.Page * page.Size)
                .Take(page.Size)
                .ToList();
            var groupIds = pageList.Select(t => t.GroupId).Distinct().ToList();
            var classDict = GroupContract.GroupDict(groupIds);

            var batchDict = pageList.GroupBy(p => p.SourceType)
                .ToDictionary(k => k.Key, v => v.Select(t => t.Batch).ToList());
            var countDict = new Dictionary<string, int>();
            if (batchDict.ContainsKey((byte)PublishType.Print))
            {
                var batchList = batchDict[(byte)PublishType.Print];
                var pictures = MarkingPictureRepository.Where(mp => batchList.Contains(mp.BatchNo))
                    .Select(mp => new { mp.BatchNo, mp.AnswerImgType })
                    .ToList()
                    .GroupBy(mp => mp).ToList();
                foreach (var picture in pictures)
                {
                    if (picture.Key.AnswerImgType == (byte)MarkingPaperType.Normal)
                        countDict.Add(picture.Key.BatchNo, picture.Count());
                    else
                        countDict.Add(picture.Key.BatchNo + picture.Key.AnswerImgType, picture.Count());
                }
            }
            if (batchDict.ContainsKey((byte)PublishType.Test))
            {
                var batchList = batchDict[(byte)PublishType.Test];
                var errorList = ErrorQuestionRepository.Where(e => batchList.Contains(e.Batch))
                    .Select(e => new { e.Batch, e.StudentID })
                    .ToList()
                    .GroupBy(e => e.Batch).ToList();
                foreach (var error in errorList)
                {
                    countDict.Add(error.Key, error.GroupBy(t => t.StudentID).Count());
                }
            }

            pageList.ForEach(p =>
            {
                if (classDict.ContainsKey(p.GroupId))
                    p.GroupName = classDict[p.GroupId];
                if (p.SourceType == (byte)PublishType.Test || p.PaperType == (byte)PaperType.Normal)
                {
                    if (countDict.ContainsKey(p.Batch))
                        p.ACount = countDict[p.Batch];
                }
                else
                {
                    if (countDict.ContainsKey(p.Batch + (int)PaperSectionType.PaperA))
                        p.ACount = countDict[p.Batch + (int)PaperSectionType.PaperA];
                    if (countDict.ContainsKey(p.Batch + (int)PaperSectionType.PaperB))
                        p.BCount = countDict[p.Batch + (int)PaperSectionType.PaperB];
                }
            });
            return DResult.Succ(pageList, count);
        }

        /// <summary>
        /// 作业提交批阅数量
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="sCount"></param>
        /// <param name="mCount"></param>
        void SubmitInfo(string batch, string paperId, out int sCount, out int mCount)
        {
            sCount = mCount = 0;
            if (batch.IsNullOrEmpty()) return;
            var list = MarkingResultRepository.Where(m =>
                m.Batch == batch && (paperId == null || m.PaperID == paperId))
                .Select(m => new
                {
                    m.IsFinished
                }).ToList();
            sCount = list.Count;
            mCount = list.Count(m => m.IsFinished);
        }

        #endregion

        #region 根据批次号查询圈子

        public DResult<GroupDto> LoadByBatch(string batch)
        {
            if (batch.IsNullOrEmpty())
                return DResult.Error<GroupDto>("参数错误");
            var usage = UsageRepository.FirstOrDefault(u => u.Id == batch);
            if (usage == null)
                return DResult.Error<GroupDto>("没有查询到发布记录");
            return GroupContract.LoadById(usage.ClassId);
        }

        #endregion

        #region 查询试卷中题目的错误人数
        public DResult<Dictionary<string, List<long>>> GetPaperErrorQuCount(string batchNo)
        {
            var details = CurrentIocManager.Resolve<IDayEasyRepository<TP_ErrorQuestion>>()
                .Where(t => t.Batch == batchNo).ToList();
            if (!details.Any())
                return DResult.Succ<Dictionary<string, List<long>>>(null);

            var errorCounts = details.GroupBy(t => t.QuestionID).Select(q => new
            {
                QId = q.Key,
                ids = q.Select(i => i.StudentID).ToList()
            }).ToDictionary(u => u.QId, u => u.ids);

            return DResult.Succ(errorCounts);
        }
        #endregion

        #region 学生成绩统计

        /// <summary>
        /// 学生成绩统计
        /// </summary>
        /// <returns></returns>
        public DResult<StudentScoreStatisticDto> StudentScoreStatistic(string batchNo, long userId)
        {
            if (string.IsNullOrEmpty(batchNo))
                return DResult.Error<StudentScoreStatisticDto>("参数错误！");

            var publishModel = UsageRepository.SingleOrDefault(u => u.Id == batchNo);
            if (publishModel == null)
                return DResult.Error<StudentScoreStatisticDto>("没有找到该作业记录！");

            var status = GroupContract.IsGroupMember(userId, publishModel.ClassId);
            if (status != CheckStatus.Normal)
                return DResult.Error<StudentScoreStatisticDto>("你没有查看该数据的权限！");

            var paper = PaperRepository.SingleOrDefault(u => u.Id == publishModel.SourceID);
            if (paper == null)
                return DResult.Error<StudentScoreStatisticDto>("没有找到该试卷！");

            var result = new StudentScoreStatisticDto()
            {
                Batch = publishModel.Id,
                PaperId = paper.Id,
                PaperName = paper.PaperTitle,
                SourceType = publishModel.SourceType
            };

            var paperScores = JsonHelper.Json<PaperScoresDto>(paper.PaperScores);
            if (paperScores != null)
            {
                result.PaperScore = paperScores.TScore;
            }

            //查询成绩和排名 以及班级平均分
            var studentScore =
                CurrentIocManager.Resolve<IDayEasyRepository<TS_StuScoreStatistics>>()
                    .SingleOrDefault(u => u.Batch == publishModel.Id && u.StudentId == userId);
            if (studentScore != null)
            {
                result.CurrentScore = studentScore.CurrentScore.ToString("0.#");
                result.Rank = studentScore.CurrentSort.ToString(CultureInfo.InvariantCulture);
            }
            var classScore = CurrentIocManager.Resolve<IDayEasyRepository<TS_ClassScoreStatistics>>()
                .SingleOrDefault(u => u.Batch == publishModel.Id && u.PaperId == paper.Id);
            if (classScore != null)
            {
                result.AvgScore = classScore.AverageScore.ToString("0.#");

                var scoreGroupsDtos = JsonHelper.Json<List<ScoreGroupsDto>>(classScore.ScoreGroups);
                if (scoreGroupsDtos != null)
                {
                    if (scoreGroupsDtos.Count > 10)
                    {
                        var count = (int)Math.Floor((scoreGroupsDtos.Count - 1) * 0.6);
                        var lowCount = scoreGroupsDtos.GetRange(0, count).Sum(c => c.Count);
                        var strs = scoreGroupsDtos[count].ScoreInfo.Split('-');
                        scoreGroupsDtos.RemoveRange(0, count);
                        scoreGroupsDtos.Insert(0,
                            new ScoreGroupsDto { Count = lowCount, ScoreInfo = (strs.Length > 0 ? strs[0] + "分以下" : "") });
                    }

                    scoreGroupsDtos.Reverse();
                    result.ScoreGroups = scoreGroupsDtos;
                }
            }

            //查询错误问题Ids
            var errorQuIds =
                CurrentIocManager.Resolve<IDayEasyRepository<TP_ErrorQuestion>>()
                    .Where(u => u.Batch == publishModel.Id && u.PaperID == paper.Id && u.StudentID == userId)
                    .Select(u => u.QuestionID)
                    .ToList();
            if (errorQuIds.Count < 1)
                return DResult.Succ(result);

            result.ErrorQuCount = errorQuIds.Count;
            result.KpAnalysis = KpAnalysis(errorQuIds); //知识点分析

            return DResult.Succ(result);
        }

        #endregion

        #region 获取相关辅导
        /// <summary>
        /// 获取相关辅导
        /// </summary>
        /// <returns></returns>
        public DResult<RecommendTutorDto> GetRecommendTutors(string batchNo, long userId)
        {
            if (string.IsNullOrEmpty(batchNo))
                return DResult.Error<RecommendTutorDto>("参数错误！");

            var publishModel = UsageRepository.SingleOrDefault(u => u.Id == batchNo);
            if (publishModel == null)
                return DResult.Error<RecommendTutorDto>("没有找到该作业记录！");

            //            var status = GroupContract.IsGroupMember(userId, publishModel.ClassId);
            //            if (status != CheckStatus.Normal)
            //                return DResult.Error<RecommendTutorDto>("你没有查看该数据的权限！");

            var paper = PaperRepository.SingleOrDefault(u => u.Id == publishModel.SourceID);
            if (paper == null)
                return DResult.Error<RecommendTutorDto>("没有找到该试卷！");

            var result = new RecommendTutorDto()
            {
                Batch = publishModel.Id,
                PaperId = paper.Id,
                PaperName = paper.PaperTitle,
                SourceType = publishModel.SourceType
            };

            //查询错误问题Ids
            var errorQuIds =
                CurrentIocManager.Resolve<IDayEasyRepository<TP_ErrorQuestion>>()
                    .Where(u => u.Batch == publishModel.Id && u.PaperID == paper.Id && u.StudentID == userId)
                    .Select(u => u.QuestionID)
                    .ToList();

            var kps = KpAnalysis(errorQuIds);//查询错误知识点
            if (kps != null && kps.Count > 0)
            {
                //先查询错误的知识点的相关辅导
                Expression<Func<TT_Tutorship, bool>> condition =
                    u => u.SubjectId == paper.SubjectID && u.Status == (byte)TutorStatus.Grounding;

                Expression<Func<TT_Tutorship, bool>> tempCondition1 = u => true;
                kps.ForEach(k =>
                {
                    condition = condition.And(w => w.Knowledges.Contains(k.KpCode));
                    //tempCondition1 = tempCondition1.Or(u => u.Knowledges.Contains("\"" + k.KpCode));
                });

                //condition = condition.And(tempCondition1);

                var errorTutors =
                    CurrentIocManager.Resolve<IDayEasyRepository<TT_Tutorship>>()
                        .Where(condition)
                        .OrderByDescending(u => u.AddedAt)
                        .Take(20)
                        .Select(u => new TutorItemDto()
                        {
                            TutorId = u.Id,
                            TutorName = u.Title,
                            FrontCover = u.Profile,
                            UsedCount = u.UseCount
                        }).DistinctBy(u => u.TutorId).ToList();

                result.Tutors.AddRange(errorTutors);
            }

            if (result.Tutors.Count >= 20)
                return DResult.Succ(result);

            //正确的知识点
            var paperKps = JsonHelper.Json<Dictionary<string, string>>(paper.KnowledgeIDs);
            if (paperKps == null)
                return DResult.Succ(result);

            var rightKps = paperKps.Keys.ToList();
            if (kps != null && kps.Count > 0)
            {
                var errorkpCodes = kps.Select(u => u.KpCode).ToList();//errorKpCodes

                rightKps = paperKps.Where(u => !errorkpCodes.Contains(u.Key)).Select(u => u.Key).ToList();
            }

            //先查询错误的知识点的相关辅导
            Expression<Func<TT_Tutorship, bool>> condition1 =
                u => u.SubjectId == paper.SubjectID && u.Status == (byte)TutorStatus.Grounding;

            if (result.Tutors != null && result.Tutors.Count > 0)
            {
                var tutorIds = result.Tutors.Select(u => u.TutorId).ToList();
                condition1 = condition1.And(u => !tutorIds.Contains(u.Id));
            }

            Expression<Func<TT_Tutorship, bool>> tempCondition = u => true;
            rightKps.ForEach(k =>
            {
                tempCondition = tempCondition.Or(u => u.Knowledges.Contains("\"" + k));
            });

            condition1 = condition1.And(tempCondition);

            var count = 20 - result.Tutors.Count;
            var rightTutors = CurrentIocManager.Resolve<IDayEasyRepository<TT_Tutorship>>()
                .Where(condition1)
                .OrderByDescending(u => u.AddedAt)
                .Take(count)
                .Select(u => new TutorItemDto
                {
                    TutorId = u.Id,
                    TutorName = u.Title,
                    FrontCover = u.Profile,
                    UsedCount = u.UseCount
                }).DistinctBy(u => u.TutorId).ToList();

            result.Tutors.AddRange(rightTutors);

            return DResult.Succ(result);
        }
        #endregion

        #region 获取未提交信息（A卷或B卷）
        /// <summary>
        /// 获取未提交信息（A卷或B卷）
        /// </summary>
        /// <returns></returns>
        public DResult<string> GetNotSubmitted(string batchNo, long userId)
        {
            if (string.IsNullOrEmpty(batchNo))
                return DResult.Succ("");

            var usage = UsageRepository.SingleOrDefault(u => u.Id == batchNo && u.SourceType == (byte)PublishType.Print);
            if (usage == null)
                return DResult.Succ("");

            var markPics = MarkingPictureRepository.Where(u => u.BatchNo == batchNo && u.StudentID == userId).ToList();
            switch (markPics.Count)
            {
                case 0:
                    return DResult.Succ("未提交");
                case 1:
                    return markPics.Exists((u => u.AnswerImgType == (byte)MarkingPaperType.Normal)) ? DResult.Succ("") : DResult.Succ(markPics.Exists(u => u.AnswerImgType == (byte)MarkingPaperType.PaperA) ? "B卷未提交" : "A卷未提交");
            }

            return DResult.Succ("");
        }
        #endregion

        #region 撤回试卷
        /// <summary>
        /// 撤回试卷
        /// </summary>
        /// <returns></returns>
        public DResult DeletePubPaper(string batch, long userId)
        {
            if (string.IsNullOrEmpty(batch))
                return DResult.Error("参数错误！");

            var usage = UsageRepository.SingleOrDefault(u => u.Id == batch && u.UserId == userId);
            if (usage == null)
                return DResult.Error("没有找到你要撤回的试卷！");

            usage.Status = (byte)NormalStatus.Delete;

            var count = UsageRepository.Update(u => new { u.Status }, usage);

            return count > 0 ? DResult.Success : DResult.Error("撤回失败，请稍后重试！");
        }
        #endregion
    }
}
