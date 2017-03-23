using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Core.Domain;
using DayEasy.Paper.Services.Helper;
using DayEasy.Paper.Services.Model;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DayEasy.Paper.Services
{
    /// <summary> 试卷业务模块 - 试卷相关业务 </summary>
    public partial class PaperService
    {
        #region 注入

        public IDayEasyRepository<TP_Paper, string> PaperRepository { private get; set; }
        public IDayEasyRepository<TP_PaperSection, string> PaperSectionRepository { private get; set; }
        public IDayEasyRepository<TP_PaperContent, string> PaperContentRepository { private get; set; }
        public IDayEasyRepository<TP_SmallQScore, string> SmallQScoreRepository { private get; set; }
        public IDayEasyRepository<TP_PaperAnswer, string> PaperAnswerRepository { private get; set; }
        public IDayEasyRepository<TS_TeacherStatistic, long> TeacherStatisticRepository { get; set; }
        public ISystemContract SystemContract { private get; set; }

        private static readonly object Obj = new object();
        private static DateTime _time = Clock.Now.AddDays(-1).Date;
        private static int _startNum;
        #endregion

        #region Private Method

        #region 验证 VpMakePaper 数据
        /// <summary>
        /// 验证 VpMakePaper 数据
        /// </summary>
        /// <param name="vpPaper"></param>
        /// <returns></returns>
        private DResult ValidateVpMakePaper(MakePaperDto vpPaper)
        {
            if (string.IsNullOrEmpty(vpPaper.PaperTitle))
            {
                return DResult.Error("请输入试卷标题！");
            }
            if (vpPaper.PaperTitle.Length > 64)
            {
                return DResult.Error("试卷标题太长了！");
            }
            if (vpPaper.Grade < 1)
            {
                return DResult.Error("请选择年级！");
            }
            //if (vpPaper.Kps == null || !vpPaper.Kps.Any())
            //{
            //    return DResult.Error("请选择知识点！");
            //}
            if (vpPaper.PSection == null || vpPaper.PSection.Count < 1)
            {
                return DResult.Error("请选择试卷所含的题型！");
            }
            if (vpPaper.PSection.Any(u => u.Questions == null || u.Questions.Count < 1))
            {
                return DResult.Error("请确保所有的题型都包含有题目！");
            }
            var questions = vpPaper.PSection.Where(p => p.Questions != null && p.Questions.Count > 0).SelectMany(u => u.Questions).ToList();
            if (questions.Count < 1)
            {
                return DResult.Error("请给试卷添加题目！");
            }
            if (questions.Count > 100)
            {
                return DResult.Error("试卷题目数量不能超过100！");
            }
            return DResult.Success;
        }
        #endregion

        #region 验证客观题答案
        /// <summary>
        /// 验证客观题答案
        /// </summary>
        /// <returns></returns>
        private DResult ValidateObjectiveAnswer(List<MakePaperAnswerDto> answerList)
        {
            if (answerList == null || answerList.Count < 1)
            {
                return DResult.Error("参数错误，请稍后重试！");
            }

            var qIds = answerList.Select(u => u.QId).Distinct().ToList();

            var objectiveQuIds = QuestionRepository.Where(u => qIds.Contains(u.Id) && u.IsObjective).Select(u => u.Id).ToList();
            if (objectiveQuIds.Any())
            {
                var questionList = answerList.Where(u => objectiveQuIds.Contains(u.QId)).ToList();
                if (questionList.Any())
                {
                    if (questionList.Exists(u => string.IsNullOrEmpty(u.Answer)))
                    {
                        return DResult.Error("请先录入客观题答案，方便自动阅卷！");
                    }
                }
            }

            return DResult.Success;
        }
        #endregion

        #region 构造保存试卷的数据结构

        /// <summary>
        /// 构造保存试卷的数据结构
        /// </summary>
        /// <param name="vpPaper"></param>
        /// <param name="paperId"></param>
        /// <returns></returns>
        private SavePaper MakeSavePaper(MakePaperDto vpPaper, string paperId = null)
        {
            var paper = new TP_Paper();
            var pSectionList = new List<TP_PaperSection>();
            var pContentList = new List<TP_PaperContent>();
            var pSmallQScoreList = new List<TP_SmallQScore>();

            #region 查询知识点

            var qIds = vpPaper.PSection.SelectMany(u => u.Questions.Select(q => q.QuestionID)).ToList();
            var qKps = QuestionRepository.Where(u => qIds.Contains(u.Id)).Select(u => u.KnowledgeIDs).ToList();
            var kps = new Dictionary<string, string>();
            foreach (var kp in qKps)
            {
                var dict = kp.JsonToObject<Dictionary<string, string>>();
                if (dict == null)
                {
                    continue;
                }
                foreach (var key in dict.Keys)
                {
                    if (!kps.ContainsKey(key))
                        kps.Add(key, dict[key]);
                    if (kps.Count >= 80)
                        break;
                }
            }
            vpPaper.Kps = kps;

            #endregion

            #region 组装试卷信息

            paper.Id = IdHelper.Instance.Guid32;
            paper.PaperNo = MakePaperNo();
            if (!string.IsNullOrEmpty(paperId))
            {
                paper.Id = paperId;
            }
            paper.PaperTitle = vpPaper.PaperTitle;
            paper.PaperType = (byte)PaperType.Normal;
            if (vpPaper.PaperType.Trim() == "AB")
            {
                paper.PaperType = (byte)PaperType.AB;
            }
            paper.SubjectID = vpPaper.SubjectId;
            paper.AddedBy = vpPaper.UserId;
            paper.TagIDs = vpPaper.Tags.ToJson();
            paper.Grade = vpPaper.Grade;

            if (Enum.IsDefined(typeof(PrimarySchoolGrade), vpPaper.Grade))
            {
                paper.Stage = (byte)StageEnum.PrimarySchool;
            }
            else if (Enum.IsDefined(typeof(JuniorMiddleSchoolGrade), vpPaper.Grade))
            {
                paper.Stage = (byte)StageEnum.JuniorMiddleSchool;
            }
            else if (Enum.IsDefined(typeof(HighSchoolGrade), vpPaper.Grade))
            {
                paper.Stage = (byte)StageEnum.HighSchool;
            }
            if (vpPaper.Kps != null && vpPaper.Kps.Any())
                paper.KnowledgeIDs = vpPaper.Kps.ToJson();
            paper.PaperScores = vpPaper.PScores.ToJson();
            paper.ShareRange = (byte)ShareRange.Self;
            paper.Source = (byte)Comefrom.Web;
            paper.Status = (byte)PaperStatus.Normal;
            paper.IsUsed = false;
            paper.UseCount = 0;
            paper.ChangeSourceID = vpPaper.ChangeSourceId;
            paper.AddedAt = Clock.Now;
            paper.AddedIP = Utils.GetRealIp();

            #endregion

            #region 组装试卷题型板块信息

            foreach (var pSection in vpPaper.PSection)
            {
                var paperSection = new TP_PaperSection
                {
                    Id = IdHelper.Instance.Guid32,
                    Description = pSection.Description,
                    Sort = pSection.Sort,
                    PaperID = paper.Id,
                    AddedBy = vpPaper.UserId,
                    SectionQuType = pSection.SectionQuType
                };
                if (pSection.PaperSectionType.Trim() == "A")
                {
                    paperSection.PaperSectionType = (byte)PaperSectionType.PaperA;
                }
                else
                {
                    paperSection.PaperSectionType = (byte)PaperSectionType.PaperB;
                }
                paperSection.SectionScore = pSection.SectionScore;

                pSectionList.Add(paperSection);

                #region 组装试卷问题内容

                var questionList = pSection.Questions;
                if (questionList != null && questionList.Count > 0)
                {
                    foreach (var question in questionList)
                    {
                        var paperContent = new TP_PaperContent
                        {
                            Id = IdHelper.Instance.Guid32,
                            PaperID = paper.Id,
                            PaperSectionType = paperSection.PaperSectionType,
                            SectionID = paperSection.Id,
                            QuestionID = question.QuestionID,
                            Sort = question.Sort,
                            Score = question.Score
                        };

                        pContentList.Add(paperContent);

                        #region 组装小问题的分数

                        var smallQuScoreList = question.SmallQuestionScore;
                        if (smallQuScoreList != null && smallQuScoreList.Count > 0)
                        {
                            foreach (var smallQuItem in smallQuScoreList)
                            {
                                var smallQuestion = new TP_SmallQScore
                                {
                                    Id = paper.Id,
                                    SmallQID = smallQuItem.QuestionID,
                                    Score = smallQuItem.Score
                                };

                                pSmallQScoreList.Add(smallQuestion);
                            }
                        }

                        #endregion
                    }
                }

                #endregion

            }

            #endregion

            return new SavePaper
            {
                Paper = paper,
                PaperSections = pSectionList,
                PaperContents = pContentList,
                SmallQScores = pSmallQScoreList
            };
        }

        #endregion

        #region 构造保存试卷的试卷答案

        private static string FormatAnswer(string answer)
        {
            if (string.IsNullOrWhiteSpace(answer))
                return answer;
            var reg = answer.As<IRegex>();
            return reg.IsMatch("^[a-z\\s]+$", RegexOptions.IgnoreCase)
                ? reg.Replace("\\s", string.Empty).ToUpper()
                : answer;
        }

        /// <summary> 构造保存试卷的试卷答案 </summary>
        /// <returns></returns>
        public List<TP_PaperAnswer> MakePaperAnswer(List<MakePaperAnswerDto> answers, string paperId)
        {
            if (answers.IsNullOrEmpty())
                return null;

            var answerList = answers.Select(a => new TP_PaperAnswer
            {
                Id = IdHelper.Instance.Guid32,
                PaperId = paperId,
                QuestionId = a.QId,
                SmallQuId = a.DetailId,
                AnswerContent = FormatAnswer(a.Answer)
            }).ToList();
            return answerList;
        }

        #endregion

        #region 构造试卷编号
        /// <summary>
        /// 构造试卷编号
        /// </summary>
        /// <returns></returns>
        private string MakePaperNo()
        {
            lock (Obj)
            {
                if (Clock.Now.Date > _time)
                {
                    _time = Clock.Now.Date;

                    _startNum = 0;
                }

                if (_startNum < 1)
                {
                    var dateTime = Clock.Now.Date;
                    var todayMaxPaper = PaperRepository.Where(u => u.AddedAt >= dateTime).OrderByDescending(u => u.AddedAt).FirstOrDefault();

                    if (todayMaxPaper != null)
                    {
                        var pNo = todayMaxPaper.PaperNo.Substring(6);

                        _startNum = System.Convert.ToInt32(pNo);
                    }
                    else
                    {
                        _startNum = new Random(Guid.NewGuid().GetHashCode()).Next(10000, 39999);
                    }
                }

                _startNum += 1;
                string paperNo = Clock.Now.ToString("yyMMdd") + _startNum;

                if (string.IsNullOrEmpty(paperNo))
                {
                    paperNo = MakePaperNo();
                }

                return paperNo;
            }
        }
        #endregion

        #endregion

        #region 保存试卷

        /// <summary> 保存试卷 </summary>
        /// <param name="paper"></param>
        /// <param name="answerList"></param>
        /// <returns></returns>
        public DResult SavePaper(MakePaperDto paper, List<MakePaperAnswerDto> answerList)
        {
            var validateResult = ValidateVpMakePaper(paper);
            if (!validateResult.Status)
            {
                return DResult.Error(validateResult.Message);
            }

            //判断是不是客观题都有答案
            if (!paper.IsTemp)
            {
                var validateAnswer = ValidateObjectiveAnswer(answerList);
                if (!validateAnswer.Status)
                {
                    return DResult.Error(validateAnswer.Message);
                }
            }

            //构造数据结构
            var savePaper = MakeSavePaper(paper);
            if (paper.IsTemp)
            {
                savePaper.Paper.Status = (byte)PaperStatus.Draft;
            }

            //构造答案
            List<TP_PaperAnswer> paperAnswers = null;
            if (!paper.IsTemp)
            {
                paperAnswers = MakePaperAnswer(answerList, savePaper.Paper.Id);
            }

            //查询试卷里面的问题
            var qIDs = savePaper.PaperContents.Select(u => u.QuestionID).ToList();
            var questions = QuestionRepository.Where(u => qIDs.Contains(u.Id)).ToList();

            var result = UnitOfWork.Transaction(() =>
            {
                PaperRepository.Insert(savePaper.Paper);
                PaperSectionRepository.Insert(savePaper.PaperSections);
                PaperContentRepository.Insert(savePaper.PaperContents);
                if (paperAnswers != null && paperAnswers.Any())
                {
                    PaperAnswerRepository.Insert(paperAnswers);
                }

                //得到问题的IDs
                foreach (var qItem in questions)
                {
                    qItem.UsedCount = qItem.UsedCount + 1;
                    qItem.IsUsed = true;
                    qItem.LastModifyTime = Clock.Now;
                }
                QuestionRepository.Update(q => new
                {
                    q.UsedCount,
                    q.IsUsed,
                    q.LastModifyTime
                }, questions.ToArray());

                if (savePaper.SmallQScores != null && savePaper.SmallQScores.Count > 0)
                {
                    SmallQScoreRepository.Insert(savePaper.SmallQScores);
                }
            });

            if (result <= 0)
                return DResult.Error("操作失败了，请稍后重试！");
            if (paper.IsTemp)
                return DResult.Success;
            var ids = savePaper.PaperContents.Select(t => t.QuestionID);
            var qids =
                questions.Where(q => q.Status == (byte)NormalStatus.Delete && ids.Contains(q.Id))
                    .Select(q => q.Id)
                    .ToList();
            PaperTask.Instance.GeneratePaperTaskAsync(paperAnswers, qids, savePaper.Paper.AddedBy,
                savePaper.Paper.TagIDs);

            ClearQuestionCacheAsync(qids.ToArray());
            //foreach (var item in qids)
            //{
            //    ClearQuestionCacheAsync(item);
            //   // ClearQuestionByRedis(item);
            //}

            PaperHelper.ClearPaperCache(savePaper.Paper.Id);
            return DResult.Success;
        }

        #endregion

        #region 试卷列表
        /// <summary> 试卷列表 </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public DResults<PaperDto> PaperList(SearchPaperDto search)
        {
            var papers = PaperRepository.Where(p => p.Status == (byte)PaperStatus.Draft);
            if (search.Status != (byte)PaperStatus.Draft)
            {
                papers = PaperRepository.Where(p => p.Status == (byte)PaperStatus.Normal);
            }

            if (search.SubjectId != -1)
            {
                papers = papers.Where(p => p.SubjectID == search.SubjectId);
            }
            if (search.Grade > -1)
            {
                papers = papers.Where(p => p.Grade == search.Grade);
            }

            if (search.Key.IsNotNullOrEmpty())
            {
                papers = papers.Where(p =>
                    p.PaperTitle.Contains(search.Key)
                    || p.TagIDs.Contains(search.Key)
                    || p.KnowledgeIDs.Contains(search.Key)
                    || p.PaperNo == search.Key);
            }
            //判断学段
            if (search.Stage > -1)
            {
                papers = papers.Where(p => search.Stage == p.Stage);
            }

            //私有、全网
            papers = search.Share == (byte)ShareRange.Self
                ? papers.Where(p => p.AddedBy == search.UserId)
                : papers.Where(p => p.ShareRange == search.Share);

            var data = papers.OrderByDescending(u => u.AddedAt).Skip(search.Size * search.Page).Take(search.Size).ToList();
            var count = papers.Count();

            if (data.Count < 1)
                return DResult.Succ<PaperDto>(null, 0);

            var result = data.Select(p =>
            {
                var temp = MakePaperDetails(p, false);
                return temp.PaperBaseInfo;
            }).ToList();

            return DResult.Succ(result, count);
        }

        public DResults<PaperDto> PaperList(List<string> paperIds)
        {
            if (paperIds == null || paperIds.Count < 1)
                return DResult.Succ<PaperDto>(null, 0);

            var paperList = PaperRepository.Where(p => p.Status == (byte)PaperStatus.Normal && paperIds.Contains(p.Id)).ToList();

            if (paperList.Count > 0)
            {
                var result = paperList.Select(p =>
                {
                    var temp = MakePaperDetails(p, false);
                    return temp.PaperBaseInfo;
                }).ToList();

                return DResult.Succ(result, result.Count);
            }
            return DResult.Succ<PaperDto>(null, 0);
        }
        #endregion

        #region 试卷发布列表
        /// <summary> 试卷发布列表 </summary>
        /// <param name="paperId"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public DResults<PaperPublishDto> PaperPublishList(string paperId, SearchPaperDto search)
        {
            if (!string.IsNullOrEmpty(paperId))//发布独立的试卷
            {
                var paper = PaperRepository.SingleOrDefault(u => u.Id == paperId);
                if (paper == null) return DResult.Errors<PaperPublishDto>("没有找到该试卷！");

                var data = new List<PaperPublishDto>
                {
                    new PaperPublishDto()
                    {
                        PaperName = paper.PaperTitle,
                        PaperId = paper.Id,
                        Time = paper.AddedAt.ToString("yyyy-MM-dd")
                    }
                };

                return DResult.Succ(data, data.Count);
            }

            //自己的试卷列表
            var tempResult = PaperList(new SearchPaperDto()
            {
                Page = search.Page,
                Size = search.Size,
                Share = (byte)ShareRange.Self,
                Status = (byte)PaperStatus.Normal,
                SubjectId = search.SubjectId,
                UserId = search.UserId
            });

            if (!tempResult.Status || tempResult.Data == null)
                return DResult.Succ<PaperPublishDto>(null, 0);

            var result = tempResult.Data.Select(u => new PaperPublishDto()
            {
                PaperName = u.PaperTitle,
                PaperId = u.Id,
                Time = u.AddedAt.ToString("yyyy-MM-dd")
            }).ToList();

            return DResult.Succ(result, tempResult.TotalCount);
        }

        #endregion

        #region  试卷详情 by paperId
        //public NPaperDto LoadPaperById(string paperId)
        //{
        //    return LoadPaper(paperId);
        //}

        /// <summary> 试卷详情 by paperId </summary>
        /// <param name="paperId"></param>
        /// <param name="loadQuestion"></param>
        /// <returns></returns>
        public DResult<PaperDetailDto> PaperDetailById(string paperId, bool loadQuestion = true)
        {
            if (string.IsNullOrEmpty(paperId))
                return DResult.Succ<PaperDetailDto>(null);
            //查询试卷基本信息
            var paperModel = PaperRepository.Load(paperId);
            if (paperModel == null)
                return DResult.Succ<PaperDetailDto>(null);

            var result = MakePaperDetails(paperModel, loadQuestion);

            return DResult.Succ(result);
        }
        #endregion

        #region 试卷详情 by paperNo

        /// <summary> 试卷详情 by paperNo </summary>
        /// <param name="paperNo"></param>
        /// <param name="loadQuestion"></param>
        /// <returns></returns>
        public DResult<PaperDetailDto> PaperDetailByPaperNo(string paperNo, bool loadQuestion = true)
        {
            if (string.IsNullOrEmpty(paperNo))
                return DResult.Succ<PaperDetailDto>(null);
            //查询试卷基本信息
            var paperModel = PaperRepository.SingleOrDefault(u => u.PaperNo == paperNo);
            if (paperModel == null)
                return DResult.Succ<PaperDetailDto>(null);

            var result = MakePaperDetails(paperModel, loadQuestion);

            return DResult.Succ(result);
        }
        #endregion

        #region 编辑试卷

        /// <summary>
        /// 编辑试卷
        /// </summary>
        /// <param name="paperId"></param>
        /// <param name="paper"></param>
        /// <param name="answerList"></param>
        /// <returns></returns>
        public DResult EditPaper(string paperId, MakePaperDto paper, List<MakePaperAnswerDto> answerList)
        {
            var validateResult = ValidateVpMakePaper(paper); //校验
            if (!validateResult.Status)
            {
                return DResult.Error(validateResult.Message);
            }

            if (!paper.IsTemp)
            {
                //判断是不是客观题都有答案
                var validateAnswer = ValidateObjectiveAnswer(answerList);
                if (!validateAnswer.Status)
                {
                    return DResult.Error(validateAnswer.Message);
                }
            }

            var paperModel = PaperRepository.SingleOrDefault(u => u.Id == paperId);
            if (paperModel == null)
                return DResult.Error("操作失败了，请稍后重试！");
            //构造保存试卷的数据结构
            var savePaper = MakeSavePaper(paper, paperId);

            var newPaper = savePaper.Paper;
            paperModel.PaperTitle = newPaper.PaperTitle;
            paperModel.Grade = newPaper.Grade;
            paperModel.KnowledgeIDs = newPaper.KnowledgeIDs;
            paperModel.TagIDs = newPaper.TagIDs;
            paperModel.PaperScores = newPaper.PaperScores;
            paperModel.Status = paper.IsTemp ? (byte)PaperStatus.Draft : (byte)PaperStatus.Normal;

            List<TP_PaperAnswer> paperAnswers = null;
            if (!paper.IsTemp)
            {
                //构造答案
                paperAnswers = MakePaperAnswer(answerList, savePaper.Paper.Id);
            }
            //修改试卷，将试卷中的所有题目使用次数减一
            var oldQuestionIds = PaperContentRepository.Where(u => u.Id == paperId)
                .Select(u => u.QuestionID)
                .ToList();
            //将旧问题使用次数减一
            var oldQuestions = QuestionRepository.Where(q => oldQuestionIds.Contains(q.Id)).ToList();

            var result = UnitOfWork.Transaction(() =>
            {
                foreach (var qItem in oldQuestions)
                {
                    qItem.LastModifyTime = Clock.Now;
                    qItem.UsedCount -= 1;
                    if (qItem.UsedCount <= 0)
                    {
                        qItem.UsedCount = 0;
                        qItem.IsUsed = false;
                    }
                    QuestionRepository.Update(q => new
                    {
                        q.LastModifyTime,
                        q.UsedCount,
                        q.IsUsed,
                        q.Status
                    }, qItem);
                }

                //删除旧的问题
                SmallQScoreRepository.Delete(u => u.Id == paperId);
                PaperContentRepository.Delete(u => u.PaperID == paperId);
                PaperSectionRepository.Delete(u => u.PaperID == paperId);
                //删除旧的答案
                PaperAnswerRepository.Delete(u => u.PaperId == paperId);

                //修改试卷部分
                PaperRepository.Update(p => new
                {
                    p.PaperTitle,
                    p.Grade,
                    p.KnowledgeIDs,
                    p.TagIDs,
                    p.PaperScores,
                    p.Status
                }, paperModel);
                PaperSectionRepository.Insert(savePaper.PaperSections.ToArray());
                PaperContentRepository.Insert(savePaper.PaperContents.ToArray());
                if (paperAnswers != null && paperAnswers.Count > 0)
                {
                    //设置试卷答案
                    PaperAnswerRepository.Insert(paperAnswers.ToArray());
                }
                //得到问题的IDs
                var qIDs = savePaper.PaperContents.Select(u => u.QuestionID).ToList();
                var questions = QuestionRepository.Where(u => qIDs.Contains(u.Id)).ToList();
                foreach (var qItem in questions)
                {
                    qItem.LastModifyTime = Clock.Now;
                    qItem.UsedCount = qItem.UsedCount + 1;
                    qItem.IsUsed = true;
                    if (qItem.Status == (byte)NormalStatus.Delete && qItem.AddedBy == paper.UserId && !paper.IsTemp)
                        qItem.Status = (byte)NormalStatus.Normal;
                    QuestionRepository.Update(q => new
                    {
                        q.LastModifyTime,
                        q.UsedCount,
                        q.IsUsed,
                        q.Status
                    }, qItem);

                }
                qIDs.AddRange(oldQuestionIds);
                foreach (var item in qIDs)
                {
                    ClearQuestionCacheAsync(item);
                    //ClearQuestionByRedis(item);
                }
                if (savePaper.SmallQScores != null && savePaper.SmallQScores.Count > 0)
                {
                    SmallQScoreRepository.Insert(savePaper.SmallQScores.ToArray());
                }
            });


            if (result <= 0)
                return DResult.Error("操作失败了，请稍后重试！");
            if (paper.IsTemp)
                return DResult.Success;
            //            var ids = savePaper.PaperContents.Select(t => t.QuestionID);
            //            var qids = oldQuestions.Where(q => q.Status == (byte)NormalStatus.Delete && ids.Contains(q.Id))
            //                .Select(q => q.Id)
            //                .ToList();
            PaperTask.Instance.GeneratePaperTaskAsync(paperAnswers, new List<string>(), savePaper.Paper.AddedBy,
                savePaper.Paper.TagIDs);

            PaperHelper.ClearPaperCache(paperId);

            return DResult.Success;
        }

        #endregion

        #region 删除试卷

        /// <summary> 删除试卷 </summary>
        /// <param name="paperId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DResult DeletePaper(string paperId, long userId)
        {
            if (string.IsNullOrEmpty(paperId))
            {
                return DResult.Error("参数错误，请稍后重试！");
            }

            var paper =
                PaperRepository.SingleOrDefault(
                    u => u.Id == paperId && u.Status != (byte)PaperStatus.Delete && u.AddedBy == userId);
            if (paper == null)
                return DResult.Error("该试卷不存在！");
            paper.Status = (byte)PaperStatus.Delete;

            var result = PaperRepository.Update(p => new { p.Status }, paper);
            if (result <= 0)
                return DResult.Error("删除失败，请稍后重试！");
            var statistic = TeacherStatisticRepository.Load(paper.AddedBy);
            if (statistic == null)
                return DResult.Success;
            statistic.AddPaperCount -= 1;
            if (statistic.AddPaperCount < 0)
                statistic.AddPaperCount = 0;
            TeacherStatisticRepository.Update(s => new { s.AddPaperCount }, statistic);
            PaperHelper.ClearPaperCache(paperId);
            return DResult.Success;
        }

        #endregion

        #region 试卷选题--试卷列表
        /// <summary> 试卷选题--试卷列表 </summary>
        /// <param name="topicDto"></param>
        /// <returns></returns>
        public DResults<TopicPaperDto> TopicsPaper(SearchTopicDto topicDto)
        {
            var papers = PaperRepository.Where(p => p.Status == (byte)PaperStatus.Normal);

            if (topicDto.Grade > -1)
            {
                papers = papers.Where(u => u.Grade == topicDto.Grade);
            }
            if (topicDto.Stage > -1)
            {
                var stage = (byte)topicDto.Stage;
                papers = papers.Where(u => u.Stage == stage);
            }
            if (topicDto.Key.IsNotNullOrEmpty())
            {
                papers = papers.Where(p =>
                    p.PaperTitle.Contains(topicDto.Key)
                    || p.TagIDs.Contains(topicDto.Key)
                    || p.PaperNo == topicDto.Key);
            }
            if (topicDto.Kp.IsNotNullOrEmpty())
            {
                papers = papers.Where(u => u.KnowledgeIDs.Contains("\"" + topicDto.Kp));
            }
            if (topicDto.SubjectId > -1)
            {
                papers = papers.Where(u => u.SubjectID == topicDto.SubjectId);
            }

            if (topicDto.Source > -1)
            {
                if (topicDto.Source == 0) //我的
                {
                    papers = papers.Where(u => u.AddedBy == topicDto.UserId);
                }
                else if (topicDto.Source == 1)
                {
                    //我的同事圈
                    var groups = CurrentIocManager.Resolve<IGroupContract>()
                        .Groups(topicDto.UserId, (byte)GroupType.Colleague);
                    if (!groups.Status || groups.Data == null)
                        return DResult.Succ<TopicPaperDto>(null, 0);

                    var groupIds = groups.Data.Select(u => u.Id).ToList();
                    var paperTempData = CurrentIocManager.Resolve<IVersion3Repository<TM_GroupDynamic>>()
                        .Where(u => groupIds.Contains(u.GroupId) && u.Status == (byte)NormalStatus.Normal)
                        .Join(papers, d => d.ContentId, p => p.Id, (d, p) => new
                        {
                            p.Id,
                            p.PaperTitle,
                            d.AddedAt
                        });

                    var paperData =
                        paperTempData.OrderByDescending(u => u.AddedAt)
                            .Skip(topicDto.Size * topicDto.Page)
                            .Take(topicDto.Size)
                            .ToList();
                    if (paperData.Count < 1) return DResult.Succ<TopicPaperDto>(null, 0);

                    var paperIds = paperData.Select(u => u.Id).ToList();
                    var paperQuestionCount =
                        PaperContentRepository.Where(u => paperIds.Contains(u.PaperID))
                            .GroupBy(u => u.PaperID)
                            .Select(u => new { u.Key, Count = u.Count() })
                            .ToList();

                    var paperList = new List<TopicPaperDto>();
                    paperData.ForEach(p =>
                    {
                        var paperCount = paperQuestionCount.Any()
                            ? paperQuestionCount.SingleOrDefault(u => u.Key == p.Id)
                            : null;

                        if (paperCount == null) return;

                        var paperSearch = new TopicPaperDto
                        {
                            PaperId = p.Id,
                            PaperName = p.PaperTitle,
                            QuestionCount = paperCount.Count
                        };

                        paperList.Add(paperSearch);
                    });

                    return DResult.Succ(paperList, paperTempData.Count());
                }
                else if (topicDto.Source == 2) //真题
                {
                    papers = papers.Where(u => u.TagIDs.Contains("中考"));
                }
            }
            else
            {
                papers = papers.Where(u => u.AddedBy == topicDto.UserId || u.TagIDs.Contains("中考"));
            }

            var data = papers.OrderByDescending(u => u.AddedAt).Skip(topicDto.Size * topicDto.Page).Take(topicDto.Size).ToList();
            var count = papers.Count();

            if (data.Count > 0)
            {
                var list = data.Select(u => u.Id).ToList();

                var paperQuestionCount = PaperContentRepository.Where(u => list.Contains(u.PaperID)).GroupBy(u => u.PaperID).Select(u => new { u.Key, Count = u.Count() }).ToList();

                var paperList = new List<TopicPaperDto>();
                data.ForEach(p =>
                {
                    var paperCount = paperQuestionCount.Any() ? paperQuestionCount.SingleOrDefault(u => u.Key == p.Id) : null;

                    if (paperCount != null)
                    {
                        var paperSearch = new TopicPaperDto
                        {
                            PaperId = p.Id,
                            PaperName = p.PaperTitle,
                            QuestionCount = paperCount.Count
                        };

                        paperList.Add(paperSearch);
                    }
                });

                return DResult.Succ(paperList, count);
            }

            return DResult.Succ<TopicPaperDto>(null, 0);
        }
        #endregion

        #region 获取试卷问题包含知识点的数量
        /// <summary>
        /// 获取试卷问题包含知识点的数量
        /// </summary>
        /// <returns></returns>
        public dynamic GetPaperKpCount(List<string> paperIds, string kp)
        {
            var result = PaperContentRepository.Where(p => paperIds.Contains(p.PaperID))
                 .Join(QuestionRepository.Table.Where(u => u.KnowledgeIDs.Contains("\"" + kp)), l => l.QuestionID, c => c.Id, (l, c) => new { l.QuestionID, l.PaperID }).GroupBy(u => u.PaperID).Select(u => new { QId = u.Key, Count = u.Count() }).ToList();

            return result;
        }
        #endregion

        #region 修改试卷答案

        /// <summary>
        /// 修改试卷答案
        /// </summary>
        /// <param name="paperId"></param>
        /// <param name="paperAnswerDto"></param>
        /// <param name="userId"></param>
        /// <param name="containsFinished"></param>
        /// <returns></returns>
        public DResult EditPaperAnswer(string paperId, IEnumerable<MakePaperAnswerDto> paperAnswerDto, long userId, bool containsFinished = false)
        {
            if (string.IsNullOrEmpty(paperId))
                return DResult.Error("保存失败，请稍后重试！");
            var paper =
                PaperRepository.SingleOrDefault(
                    u => u.Id == paperId && u.AddedBy == userId && u.Status == (byte)PaperStatus.Normal);
            if (paper == null)
                return DResult.Error("要修改的试卷不存在或者还是草稿试卷！");

            var makePaperAnswerDtos = paperAnswerDto as IList<MakePaperAnswerDto> ?? paperAnswerDto.ToList();
            if (!makePaperAnswerDtos.Any())
                return DResult.Error("没有要修改的答案！");

            var answerList = MakePaperAnswer(makePaperAnswerDtos.ToList(), paperId);
            var updateList = new List<TP_PaperAnswer>();
            if (PaperAnswerRepository.Exists(a => a.PaperId == paperId))
            {
                //当前存在已有答案,查询已修改
                var currents = PaperAnswerRepository.Where(u => u.PaperId == paperId)
                    .Select(a => new
                    {
                        a.Id,
                        a.QuestionId,
                        a.SmallQuId,
                        a.AnswerContent
                    }).ToList();
                foreach (var current in currents)
                {
                    var item =
                        answerList.FirstOrDefault(
                            t =>
                                t.QuestionId == current.QuestionId &&
                                (t.SmallQuId == null || t.SmallQuId == current.SmallQuId));
                    if (item == null)
                        continue;
                    if (item.AnswerContent != current.AnswerContent)
                    {
                        item.Id = current.Id;
                        updateList.Add(item);
                    }
                    answerList.Remove(item);
                }
            }
            if (!answerList.Any() && !updateList.Any())
                return DResult.Error("没有要修改的答案！");
            var result = UnitOfWork.Transaction(() =>
            {
                if (updateList.Any())
                {
                    PaperAnswerRepository.Update(a => new
                    {
                        a.AnswerContent
                    }, updateList.ToArray());
                }
                if (answerList.Any())
                {
                    PaperAnswerRepository.Insert(answerList.ToArray());
                }
            });
            if (result <= 0)
                return DResult.Error("保存失败，请稍后重试！");
            if (updateList.Any())
            {
                var task = PaperTask.Instance;
                task.EditMyselfAnswerAsync(updateList, userId);
                task.ChangeMarkingAsync(updateList, paperId, userId, containsFinished);
            }
            PaperHelper.ClearPaperCache(paperId);
            return DResult.Success;
        }
        #endregion

        #region 获取试卷所有的小问分数
        /// <summary>
        /// 获取试卷所有的小问分数
        /// </summary>
        /// <param name="paperId"></param>
        /// <returns></returns>
        public DResults<SmallQuScoreDto> GetSmallQuScore(string paperId)
        {
            if (string.IsNullOrEmpty(paperId))
                return DResult.Errors<SmallQuScoreDto>("参数错误！");

            var smallScore = SmallQScoreRepository.Where(u => u.Id == paperId).ToList();
            if (smallScore.Count > 0)
            {
                var result = smallScore.Select(u => new SmallQuScoreDto()
                {
                    SmallQId = u.SmallQID,
                    Score = u.Score
                }).ToList();

                return DResult.Succ(result, result.Count);
            }

            return DResult.Succ<SmallQuScoreDto>(null, 0);
        }
        #endregion

        #region 获取草稿的数量
        /// <summary>
        /// 获取草稿的数量
        /// </summary>
        /// <returns></returns>
        public DResult<int> GetCount(long userId, int subjectId, PaperStatus status)
        {
            var count = PaperRepository.Count(u => u.AddedBy == userId && u.SubjectID == subjectId && u.Status == (byte)status);

            return DResult.Succ(count);
        }
        #endregion

        #region 查询试卷的答案
        /// <summary>
        /// 查询试卷的答案
        /// </summary>
        /// <param name="paperId"></param>
        /// <returns></returns>
        public DResults<PaperAnswerDto> GetPaperAnswers(string paperId)
        {
            if (string.IsNullOrEmpty(paperId))
                return DResult.Errors<PaperAnswerDto>("参数错误！");

            var paperAnswers = PaperAnswerRepository.Where(u => u.PaperId == paperId).ToList();

            var result = paperAnswers.MapTo<List<PaperAnswerDto>>();

            return DResult.Succ<PaperAnswerDto>(result);
        }
        #endregion

        #region 是否全是客观题
        /// <summary>
        /// 是否全是客观题
        /// </summary>
        /// <returns></returns>
        public bool AllObjectiveQuestion(string paperId, PaperSectionType paperSectionType)
        {
            if (string.IsNullOrEmpty(paperId))
            {
                return false;
            }

            var qIdCount = PaperContentRepository.Where(u => u.PaperID == paperId && u.PaperSectionType == (byte)paperSectionType).Join(QuestionRepository.Table.Where(u => !u.IsObjective), p => p.QuestionID, q => q.Id, (p, q) => new { q.Id }).Count();

            return qIdCount < 1;
        }
        #endregion

        public Dictionary<string, DKeyValue<byte, int>> QuestionSorts(string paperId, bool smallSort = true)
        {
            var paper = PaperRepository.Load(paperId);
            if (paper == null)
                return new Dictionary<string, DKeyValue<byte, int>>();
            var list = PaperSectionRepository.Where(s => s.PaperID == paperId)
                .Join(PaperContentRepository.Where(c => c.PaperID == paperId), s => s.Id, d => d.SectionID,
                    (s, d) => new
                    {
                        d.QuestionID,
                        type = s.PaperSectionType,
                        s1 = s.Sort,
                        s2 = d.Sort
                    })
                .OrderBy(t => t.type)
                .ThenBy(t => t.s1).ThenBy(t => t.s2).ToList();
            var i = 1;
            if (paper.SubjectID != 3 || !smallSort)
            {
                return list.ToDictionary(k => k.QuestionID, v => new DKeyValue<byte, int>(v.type, i++));
            }
            //英语
            var dict = new Dictionary<string, DKeyValue<byte, int>>();
            var qids = list.Select(t => t.QuestionID).Distinct().ToList();
            var details = SmallQuestionRepository.Where(d => qids.Contains(d.QID))
                .Select(d => new
                {
                    d.QID,
                    d.Id,
                    d.Sort
                }).ToList()
                .GroupBy(d => d.QID)
                .ToDictionary(k => k.Key, v => v.Select(d => new { d.Id, d.Sort }).ToList());
            var type = 0;
            foreach (var question in list)
            {
                //AB卷重新排
                if (type != question.type)
                {
                    i = 1;
                    type = question.type;
                }
                if (question.type == (byte)PaperSectionType.PaperA && details.ContainsKey(question.QuestionID))
                {
                    var dList = details[question.QuestionID].OrderBy(d => d.Sort);
                    foreach (var dItem in dList)
                    {
                        if (!dict.ContainsKey(dItem.Id))
                        {
                            dict.Add(dItem.Id, new DKeyValue<byte, int>(question.type, i++));
                        }
                    }
                }
                else
                {
                    if (!dict.ContainsKey(question.QuestionID))
                    {
                        dict.Add(question.QuestionID, new DKeyValue<byte, int>(question.type, i++));
                    }
                }
            }
            return dict;
        }

        public DResults<KnowledgeQuestionsDto> KnowledgeQuestions(string paperId)
        {
            if (string.IsNullOrWhiteSpace(paperId))
                return DResult.Errors<KnowledgeQuestionsDto>("试卷编号不正确！");
            var paperResult = PaperDetailById(paperId);
            if (!paperResult.Status)
                return DResult.Succ(new List<KnowledgeQuestionsDto>(), 0);
            return KnowledgeQuestions(paperResult.Data);
        }

        public DResults<KnowledgeQuestionsDto> KnowledgeQuestions(PaperDetailDto paper)
        {
            var list = new List<KnowledgeQuestionsDto>();
            var questions = paper.PaperSections.SelectMany(s => s.Questions);
            foreach (var question in questions)
            {
                if (question.Question.Knowledges.IsNullOrEmpty())
                    continue;
                var qid = question.Question.Id;
                var prefix = paper.PaperBaseInfo.IsAb
                    ? (question.PaperSectionType == (byte)PaperSectionType.PaperA ? "A" : "B")
                    : string.Empty;
                foreach (var knowledge in question.Question.Knowledges)
                {
                    var item = list.FirstOrDefault(t => t.Code == knowledge.Id);
                    if (item == null)
                    {
                        item = new KnowledgeQuestionsDto(knowledge.Id, knowledge.Name);
                        list.Add(item);
                    }
                    if (question.Question.IsObjective && question.Question.HasSmall)
                    {
                        foreach (var detail in question.Question.Details)
                        {
                            item.Questions.Add(new DQuestionSortDto
                            {
                                Id = qid,
                                Sort = string.Concat(prefix, detail.Sort)
                            });
                        }
                    }
                    else
                    {
                        item.Questions.Add(new DQuestionSortDto
                        {
                            Id = qid,
                            Sort = string.Concat(prefix, question.Sort)
                        });
                    }
                }
            }
            return DResult.Succ(list, -1);
        }

        /// <summary> 试卷序号 </summary>
        /// <param name="paperId"></param>
        /// <param name="isObjective"></param>
        /// <param name="sectionType"></param>
        /// <returns></returns>
        public Dictionary<string, string> PaperSorts(string paperId, bool isObjective = false, int sectionType = -1)
        {
            var dict = new Dictionary<string, string>();
            var paperResult = PaperDetailById(paperId);
            if (!paperResult.Status)
                return dict;
            var paper = paperResult.Data;
            return PaperSorts(paper, isObjective, sectionType);
        }

        /// <summary> 试卷序号 </summary>
        /// <param name="paper"></param>
        /// <param name="isObjective"></param>
        /// <param name="sectionType"></param>
        /// <returns></returns>
        public Dictionary<string, string> PaperSorts(PaperDetailDto paper, bool isObjective = false, int sectionType = -1)
        {
            var dict = new Dictionary<string, string>();
            if (paper == null || paper.PaperSections.IsNullOrEmpty())
                return dict;
            var sortType = paper.PaperBaseInfo.SortType();
            var sections = paper.PaperSections;
            if (sectionType > 0)
                sections = sections.Where(s => s.PaperSectionType == (byte)sectionType).ToList();
            if (!sections.Any())
                return dict;
            var questions =
                sections.SelectMany(s => s.Questions.Where(q => q.Question.IsObjective == isObjective)).ToList();
            if (!questions.Any())
                return dict;
            foreach (var dto in questions)
            {
                var smallRow = sortType.SmallRow(dto.PaperSectionType);
                if (isObjective)
                {
                    if (dto.Question.HasSmall && dto.Question.Details != null)
                    {
                        foreach (var detail in dto.Question.Details)
                        {
                            dict.Add(detail.Id,
                                smallRow ? detail.Sort.ToString() : string.Concat(dto.Sort, ".", detail.Sort));
                        }
                    }
                    else
                    {
                        dict.Add(dto.Question.Id, dto.Sort.ToString());
                    }
                }
                else
                {
                    if (smallRow && dto.Question.HasSmall && dto.Question.Details != null)
                    {
                        var min = dto.Question.Details.Min(d => d.Sort);
                        var max = dto.Question.Details.Max(d => d.Sort);
                        dict.Add(dto.Question.Id, (max == min ? min.ToString() : string.Concat(min, "-", max)));
                    }
                    else
                    {
                        dict.Add(dto.Question.Id, dto.Sort.ToString());
                    }
                }
            }
            return dict;
        }

        public Task ClearPaperCacheAsync(params string[] paperIds)
        {
            return Task.Factory.StartNew(() =>
            {
                if (paperIds == null || paperIds.Length == 0)
                    return;
                PaperHelper.ClearPaperCache(paperIds);
            });
        }
    }
}
