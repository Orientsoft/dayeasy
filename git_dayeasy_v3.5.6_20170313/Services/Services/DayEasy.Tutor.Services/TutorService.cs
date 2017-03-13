using System;
using System.Collections.Generic;
using System.Linq;
using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Dtos.Tutor;
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

namespace DayEasy.Tutor.Services
{
    public partial class TutorService : DayEasyService, ITutorContract
    {
        #region 注入

        private readonly ILogger _logger = LogManager.Logger<TutorService>();
        public TutorService(IDbContextProvider<DayEasyDbContext> context)
            : base(context)
        { }

        public IPaperContract PaperContract { private get; set; }
        public IUserContract UserContract { private get; set; }
        public IDayEasyRepository<TT_Tutorship> TutorshipRepository { private get; set; }
        public IDayEasyRepository<TT_TutorRecord> TutorRecordRepository { private get; set; }
        public IDayEasyRepository<TT_TutorComment> TutorCommentRepository { private get; set; }
        public IDayEasyRepository<TT_TutorContent> TutorContentRepository { private get; set; }
        public IDayEasyRepository<TC_Video> VideoRepository { private get; set; }

        #endregion

        #region Private Method

        #region 查询修改辅导的实体
        /// <summary>
        /// 查询修改辅导的实体
        /// </summary>
        /// <param name="tutorId"></param>
        /// <returns></returns>
        private AddTutorDto GetEditTutorData(string tutorId)
        {
            if (string.IsNullOrEmpty(tutorId))
            {
                return null;
            }

            var model = TutorshipRepository.SingleOrDefault(u => u.Id == tutorId && u.Status == (byte)TutorStatus.Draft);
            if (model == null) return null;

            var result = new AddTutorDto
            {
                Profile = model.Profile,
                Title = model.Title,
                Diff = model.Difficulty,
                Author = model.Author,
                Grade = (byte)model.Grade,
                Kps = JsonHelper.Json<Dictionary<string, string>>(model.Knowledges).Select(u => new NameDto(u.Key, u.Value)).ToList(),
                Tags = model.Tags.JsonToObject<List<string>>(),
                Subject = model.SubjectId,
                Description = model.Summary,
                SolveContent = string.Empty,
                Contents = null
            };

            var contents = TutorContentRepository.Where(u => u.TutorId == tutorId).ToList();
            if (contents.Count <= 0)
                return result;

            var solveModel = contents.FirstOrDefault(u => u.SourceType == (byte)TutorContentType.Feature);
            result.SolveContent = solveModel == null ? string.Empty : solveModel.Content;
            result.Contents =
                contents.Where(u => u.SourceType != (byte)TutorContentType.Feature)
                .Select(u => new ContentsDto()
                {
                    Detail = u.Content,
                    Remarks = u.Remarks,
                    Sort = u.Sort,
                    Type = u.SourceType
                }).ToList();

            return result;
        }
        #endregion

        #endregion

        #region 获取辅导列表数据
        /// <summary>
        /// 获取辅导列表数据
        /// </summary>
        /// <returns></returns>
        public DResults<TutorDto> GetTutorsByUserId(long userId, DPage page)
        {
            var tutors = TutorshipRepository.Where(u => u.Status < (byte)TutorStatus.Delete && u.AddedBy == userId).OrderByDescending(u => u.AddedAt).Skip(page.Page * page.Size).Take(page.Size);

            if (!tutors.Any())
                return DResult.Succ<TutorDto>(null, 0);

            var result = new List<TutorDto>();
            tutors.ToList().Foreach(t =>
            {
                var model = t.MapTo<TutorDto>();
                if (model != null)
                {
                    model.Knowledges = JsonHelper.Json<Dictionary<string, string>>(t.Knowledges);
                    model.Tags = JsonHelper.JsonList<string>(t.Tags).ToList();

                    result.Add(model);
                }
            });

            return DResult.Succ(result, tutors.Count());
        }
        #endregion

        #region 获取辅导数据
        /// <summary>
        /// 获取辅导数据
        /// </summary>
        /// <returns></returns>
        public DResult<TutorDto> GetTutorById(string tutorId)
        {
            var tutor = TutorshipRepository.SingleOrDefault(u => u.Id == tutorId);

            if (tutor == null)
                return DResult.Succ<TutorDto>(null);

            var model = tutor.MapTo<TutorDto>();
            if (model == null) return DResult.Succ<TutorDto>(null);

            model.Knowledges = JsonHelper.Json<Dictionary<string, string>>(tutor.Knowledges);
            model.Tags = JsonHelper.Json<List<string>>(tutor.Tags);

            return DResult.Succ(model);
        }
        #endregion

        #region 修改辅导的状态
        /// <summary>
        /// 修改辅导的状态
        /// </summary>
        /// <returns></returns>
        public DResult UpdateStatus(string tutorId, byte status)
        {
            if (string.IsNullOrEmpty(tutorId)) return DResult.Error("操作失败，请稍后重试！");
            if (!Enum.IsDefined(typeof(TutorStatus), status)) return DResult.Error("操作失败，请稍后重试！");

            var model = TutorshipRepository.SingleOrDefault(u => u.Id == tutorId && u.Status != (byte)TutorStatus.Delete);
            if (model == null) return DResult.Error("操作失败，请稍后重试！");

            model.Status = status;
            var result = TutorshipRepository.Update(t => new {t.Status}, model);
            return result > 0 ? DResult.Success : DResult.Error("操作失败，请稍后重试！");
        }
        #endregion

        #region 添加辅导数据处理
        /// <summary>
        /// 添加辅导数据处理
        /// </summary>
        /// <returns></returns>
        public DResult<TutorDataDto> AddTutorData(string tutorId, string paperBaseStr, string videoId, string newQuestionId, string tutorData)
        {
            List<string> chooseQuIds = null;
            var tutorDataStr = string.Empty;

            //处理选题界面的数据传递
            if (!string.IsNullOrEmpty(paperBaseStr))
            {
                var baseData = JsonHelper.Json<PaperBaseDto>(paperBaseStr);
                if (baseData != null)
                {
                    chooseQuIds = baseData.ChooseQus == null ? null : baseData.ChooseQus.Where(u => u != null).SelectMany(u => u.Select(q => q.QId)).ToList();//选择的问题Ids
                    tutorDataStr = baseData.AutoData;//传递的辅导基础数据
                }
            }

            if (string.IsNullOrEmpty(tutorDataStr))//如果没有从添加界面传递数据，则获取
            {
                tutorDataStr = tutorData;
            }

            string editId = string.Empty;//修改的Id
            var currentTutorData = new AddTutorDto();
            if (!string.IsNullOrEmpty(tutorDataStr))//处理辅导基础数据
            {
                var tempJsonData = JsonHelper.Json<AddTutorDto>(tutorDataStr);
                if (tempJsonData != null)
                {
                    currentTutorData = tempJsonData;

                    editId = currentTutorData.EditId;
                }
            }
            else if (!string.IsNullOrEmpty(tutorId))//可能是修改的第一次请求
            {
                var tempData = GetEditTutorData(tutorId);
                if (tempData != null)
                {
                    currentTutorData = tempData;
                }
                else
                {
                    return DResult.Error<TutorDataDto>("没有找到该辅导或者您没有修改该辅导的权限！");
                }
                editId = tutorId;
            }

            //处理选择的问题,加入辅导的内容集合
            if (chooseQuIds != null && chooseQuIds.Count > 0)
            {
                if (currentTutorData.Contents == null)
                {
                    currentTutorData.Contents = new List<ContentsDto>();
                }

                var sort = currentTutorData.Contents.Count;

                chooseQuIds.ForEach(c => currentTutorData.Contents.Add(new ContentsDto()
                {
                    Type = (byte)TutorContentType.Question,
                    Sort = ++sort,
                    Remarks = string.Empty,
                    Detail = c
                }));
            }

            //处理上传的视频
            if (!string.IsNullOrEmpty(videoId))
            {
                if (currentTutorData.Contents == null)
                {
                    currentTutorData.Contents = new List<ContentsDto>();
                }
                var sort = currentTutorData.Contents.Count;

                currentTutorData.Contents.Add(new ContentsDto()
                {
                    Detail = videoId,
                    Remarks = string.Empty,
                    Sort = ++sort,
                    Type = (byte)TutorContentType.Video
                });
            }

            //处理新加的问题
            if (!string.IsNullOrEmpty(newQuestionId))
            {
                if (currentTutorData.Contents == null)
                {
                    currentTutorData.Contents = new List<ContentsDto>();
                }
                var sort = currentTutorData.Contents.Count;

                currentTutorData.Contents.Add(new ContentsDto()
                {
                    Detail = newQuestionId,
                    Remarks = string.Empty,
                    Sort = ++sort,
                    Type = (byte)TutorContentType.Question
                });
            }

            if (currentTutorData.Contents == null || currentTutorData.Contents.Count <= 0)
                return DResult.Succ<TutorDataDto>(new TutorDataDto());

            var result = new TutorDataDto()
            {
                EditId = editId,
                Profile = currentTutorData.Profile,
                Title = currentTutorData.Title,
                Diff = currentTutorData.Diff,
                Author = currentTutorData.Author,
                Grade = currentTutorData.Grade,
                Kps = currentTutorData.Kps,
                Tags = currentTutorData.Tags,
                Subject = currentTutorData.Subject,
                Description = currentTutorData.Description,
                SolveContent = currentTutorData.SolveContent,
                Contents = currentTutorData.Contents
            };

            //查询问题
            var questionIds = result.Contents.Where(u => u.Type == (byte)TutorContentType.Question).Select(u => u.Detail).ToList();
            if (questionIds.Count > 0)
            {
                result.Questions = PaperContract.LoadQuestions(questionIds.ToArray());
            }

            //查询视频
            var videoIds = result.Contents.Where(u => u.Type == (byte)TutorContentType.Video).Select(u => u.Detail).ToList();
            if (videoIds.Count > 0)
            {
                result.Videos = VideoRepository.Where(u => videoIds.Contains(u.Id)).Select(u => new SimpleVideoDto()
                {
                    FrontCover = u.FrontCover,
                    VideoName = u.VideoName,
                    VideoUrl = u.VideoUrl,
                    VideoId = u.Id
                }).ToList();
            }
            return DResult.Succ(result);
        }
        #endregion

        #region 保存辅导
        /// <summary>
        /// 保存辅导
        /// </summary>
        /// <returns></returns>
        public DResult SaveTutor(string tutorDataStr, long userId, string draft)
        {
            if (string.IsNullOrEmpty(tutorDataStr)) return null;

            tutorDataStr = tutorDataStr.HtmlDecode();

            var data = JsonHelper.Json<AddTutorDto>(tutorDataStr);
            if (data == null) return DResult.Error("保存失败，请稍后重试！");

            var isDraft = !string.IsNullOrEmpty(draft);

            if (string.IsNullOrEmpty(data.Title))
                return DResult.Error("请输入辅导的标题！");

            if (string.IsNullOrEmpty(data.Author))
                return DResult.Error("请输入辅导的作者！");

            if (data.Kps == null || data.Kps.Count < 1)
                return DResult.Error("请选择知识点！");

            if (string.IsNullOrEmpty(data.SolveContent))
                return DResult.Error("请输入知识点特征与常见解法！");

            if (data.Contents == null || data.Contents.Count < 1)
                return DResult.Error("请添加辅导的内容！");

            var newTutor = new TT_Tutorship()
            {
                Id = IdHelper.Instance.Guid32,
                Title = data.Title,
                Summary = data.Description,
                Knowledges = data.Kps.ToDictionary(u => u.Id, u => u.Name).ToJson(),
                Grade = data.Grade,
                Difficulty = data.Diff,
                SubjectId = data.Subject,
                Tags = data.Tags.ToJson(),
                Profile = data.Profile,
                Author = data.Author,
                Status = isDraft ? (byte)TutorStatus.Draft : (byte)TutorStatus.Nomal,
                UseCount = 0,
                CommentCount = 0,
                Score = 0,
                AddedAt = Clock.Now,
                AddedBy = userId
            };

            var contents = new List<TT_TutorContent>
            {
                new TT_TutorContent
                {
                    Id = IdHelper.Instance.Guid32,
                    TutorId = newTutor.Id,
                    Remarks = string.Empty,
                    Content = data.SolveContent,
                    SourceType = (byte)TutorContentType.Feature,
                    Sort = -1
                }
            };

            data.Contents.ForEach(c =>
            {
                var content = new TT_TutorContent
                {
                    Id = IdHelper.Instance.Guid32,
                    TutorId = newTutor.Id,
                    Remarks = c.Remarks,
                    Content = c.Detail,
                    SourceType = c.Type,
                    Sort = c.Sort
                };

                contents.Add(content);
            });

            var result = UnitOfWork.Transaction(() =>
              {
                  if (!string.IsNullOrEmpty(data.EditId))
                  {
                      TutorContentRepository.Delete(u => u.TutorId == data.EditId);
                      TutorshipRepository.Delete(u => u.Id == data.EditId);
                  }

                  TutorshipRepository.Insert(newTutor);
                  TutorContentRepository.Insert(contents);
              });

            return result > 0 ? DResult.Success : DResult.Error("保存失败，请稍后重试！");
        }
        #endregion

        #region 辅导使用记录
        /// <summary>
        /// 辅导使用记录
        /// </summary>
        /// <returns></returns>
        public DResults<TutorRecordsDto> Records(string tutorId, DPage page)
        {
            if (string.IsNullOrEmpty(tutorId))
                return DResult.Errors<TutorRecordsDto>("参数错误！");

            var list = TutorRecordRepository.Where(u => u.Status == (byte)NormalStatus.Normal && u.TutorId == tutorId);
            var count = list.Count();
            var records = list.OrderByDescending(u => u.AddedAt)
                .Skip(page.Size * page.Page)
                .Take(page.Size)
                .Select(u => new TutorRecordsDto
                {
                    Time = u.AddedAt,
                    UserId = u.UserId
                });

            if (!records.Any()) return DResult.Succ<TutorRecordsDto>(null, 0);

            var data = records.ToList();

            var userIds = data.Select(u => u.UserId).Distinct().ToList();
            var userList = UserContract.UserNames(userIds);
            data.ForEach(d =>
            {
                if (userList.ContainsKey(d.UserId))
                    d.UserName = userList[d.UserId];
            });

            return DResult.Succ(data, count);
        }
        #endregion

        #region 获取反馈信息列表
        /// <summary>
        /// 获取反馈信息列表
        /// </summary>
        /// <returns></returns>
        public DResults<FeedBackDto> GetFeedBackData(string tutorId, DPage page)
        {
            if (string.IsNullOrEmpty(tutorId))
                return DResult.Errors<FeedBackDto>("参数错误！");

            var comments = TutorCommentRepository.Where(u => u.Status == (byte)NormalStatus.Normal && u.TutorId == tutorId).OrderByDescending(u => u.AddedAt).Skip(page.Size * page.Page).Take(page.Size).Select(u => new FeedBackDto()
            {
                Comment = u.Comment,
                Time = u.AddedAt,
                UserId = u.AddedBy,
            });

            if (!comments.Any()) return DResult.Succ<FeedBackDto>(null, 0);

            var data = comments.ToList();

            var userIds = data.Select(u => u.UserId).Distinct().ToList();
            var userList = CurrentIocManager.Resolve<IDayEasyRepository<TU_User, long>>().Where(u => userIds.Contains(u.Id)).ToList();
            data.Foreach(d =>
            {
                var user = userList.SingleOrDefault(u => u.Id == d.UserId);
                d.UserName = user == null ? string.Empty : user.TrueName;
                d.HeadPhoto = user == null ? string.Empty : user.HeadPhoto;
            });

            return DResult.Succ(data, comments.Count());
        }
        #endregion

        #region 获取反馈的统计信息
        /// <summary>
        ///  获取反馈的统计信息
        /// </summary>
        /// <param name="tutorId"></param>
        /// <returns></returns>
        public DResults<FeedBackPointDto> GetFeedBackStatistics(string tutorId)
        {
            if (string.IsNullOrEmpty(tutorId)) return DResult.Succ<FeedBackPointDto>(null, 0);

            var comments = TutorCommentRepository.Where(u => u.TutorId == tutorId && u.Status == (byte)NormalStatus.Normal && u.ChooseComment != null).Select(u => u.ChooseComment).ToList();

            if (comments.Count <= 0) return DResult.Succ<FeedBackPointDto>(null, 0);

            var oneCount = comments.Count(u => (u & (byte)ChooseComment.One) > 0);
            var twoCount = comments.Count(u => (u & (byte)ChooseComment.Two) > 0);
            var threeCount = comments.Count(u => (u & (byte)ChooseComment.Three) > 0);
            var fourCount = comments.Count(u => (u & (byte)ChooseComment.Four) > 0);
            var fiveCount = comments.Count(u => (u & (byte)ChooseComment.Five) > 0);

            var data = new List<FeedBackPointDto>()
            {
                new FeedBackPointDto(){x = ChooseComment.One.GetText(),y = oneCount},
                new FeedBackPointDto(){x = ChooseComment.Two.GetText(),y = twoCount},
                new FeedBackPointDto(){x = ChooseComment.Three.GetText(),y = threeCount},
                new FeedBackPointDto(){x = ChooseComment.Four.GetText(),y = fourCount},
                new FeedBackPointDto(){x = ChooseComment.Five.GetText(),y = fiveCount}
            };

            return DResult.Succ<FeedBackPointDto>(data);
        }
        #endregion

        #region 添加视频
        /// <summary>
        /// 添加视频
        /// </summary>
        /// <returns></returns>
        public DResult AddVideo(string videoName, string videoUrl, string videoDesc, string faceImg, decimal time, int grade, long userId, int subjectId)
        {
            if (string.IsNullOrEmpty(videoName))
            {
                return DResult.Error("视频名称不能为空！");
            }
            if (videoName.Length > 50)
            {
                return DResult.Error("视频名称太长了，最多50个字！");
            }
            if (string.IsNullOrEmpty(videoUrl))
            {
                return DResult.Error("视频上传失败了，请稍后重试！");
            }
            if (grade < 0)
            {
                return DResult.Error("请选择适用年级！");
            }

            var newVideo = new TC_Video
            {
                Id = IdHelper.Instance.Guid32,
                VideoName = videoName,
                VideoDescription = videoDesc,
                VideoUrl = videoUrl,
                FrontCover = faceImg,
                UserId = userId,
                Speaker = string.Empty,
                Duration = time,
                ShareRange = (byte)ShareRange.Self,
                SubjectId = subjectId,
                KnowPointIds = null,
                TagIDs = null,
                VideoStatus = (byte)NormalStatus.Normal,
                UsedCount = 0,
                AddedAt = Clock.Now,
                AddedIp = Utils.GetRealIp(),
                IsUsed = false,
                Grade = (byte)grade
            };
            if (System.Enum.IsDefined(typeof(PrimarySchoolGrade), newVideo.Grade))
            {
                newVideo.Stage = (byte)StageEnum.PrimarySchool;
            }
            else if (System.Enum.IsDefined(typeof(JuniorMiddleSchoolGrade), newVideo.Grade))
            {
                newVideo.Stage = (byte)StageEnum.JuniorMiddleSchool;
            }
            else if (System.Enum.IsDefined(typeof(HighSchoolGrade), newVideo.Grade))
            {
                newVideo.Stage = (byte)StageEnum.HighSchool;
            }

            var result = CurrentIocManager.Resolve<IDayEasyRepository<TC_Video>>().Insert(newVideo);
            return string.IsNullOrEmpty(result) ? DResult.Error("添加视频失败，请稍后重试！") : DResult.Succ(result);
        }
        #endregion

        #region 查询辅导详情
        /// <summary>
        /// 查询辅导详情
        /// </summary>
        /// <param name="tutorId"></param>
        /// <returns></returns>
        public DResult<TutorDetailDto> GetTutorDetail(string tutorId)
        {
            if (string.IsNullOrEmpty(tutorId))
                return DResult.Error<TutorDetailDto>("参数错误！");

            var tutorItem = GetTutorById(tutorId);
            if (!tutorItem.Status || tutorItem.Data == null)
                return DResult.Error<TutorDetailDto>("该辅导不存在！");

            var result = new TutorDetailDto()
            {
                Tutor = tutorItem.Data
            };

            var contents = TutorContentRepository.Where(u => u.TutorId == tutorItem.Data.Id).ToList();
            if (contents.Count < 1)
                return DResult.Succ(result);

            //特性与解法
            var featureModel = contents.SingleOrDefault(u => u.SourceType == (byte)TutorContentType.Feature);
            if (featureModel != null)
            {
                result.Feature = featureModel.Content;
            }

            //处理问题
            List<QuestionDto> questions = null;
            var qIds = contents.Where(u => u.SourceType == (byte)TutorContentType.Question).Select(u => u.Content).ToList();
            if (qIds.Count > 0)
            {
                questions = PaperContract.LoadQuestions(qIds.ToArray());
            }

            //处理视频
            List<SimpleVideoDto> videos = null;
            var videoIds = contents.Where(u => u.SourceType == (byte)TutorContentType.Video).Select(u => u.Content).ToList();
            if (videoIds.Count > 0)
            {
                videos = VideoRepository.Where(u => videoIds.Contains(u.Id)).Select(u => new SimpleVideoDto()
                {
                    FrontCover = u.FrontCover,
                    VideoName = u.VideoName,
                    VideoUrl = u.VideoUrl,
                    VideoId = u.Id
                }).ToList();
            }

            contents.Foreach(c =>
            {
                if (c.SourceType == (byte)TutorContentType.Feature)
                    return;

                var contentItem = new TutorContentDto()
                {
                    ContentId = c.Id,
                    ContentType = c.SourceType,
                    Remarks = c.Remarks,
                    Sort = c.Sort
                };

                if (c.SourceType == (byte)TutorContentType.Question && questions != null)
                {
                    contentItem.QItem = questions.SingleOrDefault(u => u.Id == c.Content);
                }
                else if (c.SourceType == (byte)TutorContentType.Video && videos != null)
                {
                    contentItem.VideoItem = videos.SingleOrDefault(u => u.VideoId == c.Content);
                }
                else//Text
                {
                    contentItem.Content = c.Content;
                }

                result.Contents.Add(contentItem);
            });

            return DResult.Succ(result);
        }
        #endregion

        #region 添加辅导使用记录
        /// <summary>
        /// 添加辅导使用记录
        /// </summary>
        /// <param name="tutorId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DResult AddTutorRecord(string tutorId, long userId)
        {
            var exist = TutorRecordRepository.Exists(r => r.TutorId == tutorId && r.UserId == userId);
            if (exist) return DResult.Success;
            var result = TutorRecordRepository.Insert(new TT_TutorRecord
            {
                Id = IdHelper.Instance.GetGuid32(),
                TutorId = tutorId,
                UserId = userId,
                AgencyId = "",
                ClassId = "",
                AddedAt = Clock.Now,
                Status = (byte)NormalStatus.Normal
            });

            if (string.IsNullOrEmpty(result)) return DResult.Error("记录失败");

            var tutor = TutorshipRepository.SingleOrDefault(t => t.Id == tutorId);
            if (tutor == null) return DResult.Success;

            tutor.UseCount += 1;
            TutorshipRepository.Update(tutor, "UseCount");

            return DResult.Success;
        }
        #endregion

        #region 添加辅导评价
        /// <summary>
        /// 添加辅导评价
        /// </summary>
        /// <param name="tutorId"></param>
        /// <param name="userId"></param>
        /// <param name="comment"></param>
        /// <param name="type">暂无枚举，2、4、8、16、32</param>
        /// <param name="score"></param>
        /// <returns></returns>
        public DResult AddTutorComment(string tutorId, long userId, string comment, byte? type, decimal? score)
        {
            var exist = TutorCommentRepository.Exists(c => c.TutorId == tutorId && c.AddedBy == userId);
            if (exist) return DResult.Success;
            var result = TutorCommentRepository.Insert(new TT_TutorComment
            {
                Id = IdHelper.Instance.GetGuid32(),
                TutorId = tutorId,
                AddedBy = userId,
                AddedAt = Clock.Now,
                ChooseComment = type,
                Comment = comment,
                Score = score,
                Status = (byte)NormalStatus.Normal
            });

            return result != null ? DResult.Success : DResult.Error("操作失败");
        }
        #endregion

        #region 获取学生对辅导的评价
        /// <summary>
        /// 获取学生对辅导的评价
        /// </summary>
        /// <param name="tutorId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DResult<TutorCommentDto> GetTutorStudentRecord(string tutorId, long userId)
        {
            var result = TutorCommentRepository.SingleOrDefault(u => u.TutorId == tutorId && u.AddedBy == userId);

            return DResult.Succ(result.MapTo<TutorCommentDto>());
        }
        #endregion
    }
}
