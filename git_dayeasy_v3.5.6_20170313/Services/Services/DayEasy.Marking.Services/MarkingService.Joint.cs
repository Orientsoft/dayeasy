
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Marking;
using DayEasy.Contracts.Dtos.Marking.Joint;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Marking.Services.Helper;
using DayEasy.Services;
using DayEasy.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace DayEasy.Marking.Services
{
    public partial class MarkingService
    {
        public IDayEasyRepository<TP_JointMarking, string> JointMarkingRepository { private get; set; }
        public IDayEasyRepository<TP_JointQuestionGroup, string> QuestionGroupRepository { private get; set; }
        public IDayEasyRepository<TP_JointDistribution, string> JointDistributionRepository { private get; set; }
        public IDayEasyRepository<TP_JointPictureDistribution, string> JointPictureDistributionRepository { private get; set; }
        public IDayEasyRepository<TQ_Question, string> QuestoinRepository { private get; set; }
        public IDayEasyRepository<TP_JointException, string> JointExceptionRepository { private get; set; }

        #region 私有方法
        private DResult<string> PublishJointCheck(long teacherId, string groupId, string paperNum)
        {
            //基础验证
            if (teacherId <= 0 || string.IsNullOrWhiteSpace(groupId) || string.IsNullOrWhiteSpace(paperNum))
                return DResult.Error<string>("参数错误");
            var teacher = UserContract.Load(teacherId);
            if (teacher == null || !teacher.IsTeacher())
                return DResult.Error<string>("用户身份验证失败！");

            var status = GroupContract.IsGroupMember(teacherId, groupId);
            if (status != CheckStatus.Normal)
                return DResult.Error<string>("没有权限在该圈子发起协同！");

            var groupInfo = GroupContract.LoadById(groupId);
            if (groupInfo == null || groupInfo.Data == null || groupInfo.Data.Type == (byte)GroupType.Class)
                return DResult.Error<string>("该圈子不存在或者不是同事圈！");

            var paper =
                PaperRepository.FirstOrDefault(p => p.PaperNo == paperNum && p.Status == (byte)PaperStatus.Normal);
            if (paper == null)
                return DResult.Error<string>("没有查询到相关试卷信息！");

            if (paper.SubjectID != teacher.SubjectId)
                return DResult.Error<string>("不能发布其他科目的试卷！");

            var exist = JointMarkingRepository.Exists(
                p => p.GroupId == groupId && p.PaperId == paper.Id && p.Status == (byte)JointStatus.Normal);
            if (exist)
                return DResult.Error<string>("已存在未结束的协同阅卷");

            return DResult.Succ(paper.Id);
        }

        /// <summary> 分配任务检测 </summary>
        private DResult DistributionCheck(JDistributionDto dto)
        {
            if (dto == null || dto.JointBatch.IsNullOrEmpty())
                return DResult.Error("协同批次号不正确！");
            if (dto.Details.IsNullOrEmpty())
                return DResult.Error("没有任何批阅任务！");
            if (dto.Details.Any(d => d.Questions.IsNullOrEmpty()) || dto.Details.Any(d => d.TeacherIds.IsNullOrEmpty()))
                return DResult.Error("部分教师没有任何批阅任务！");
            var joint = JointMarkingRepository.Load(dto.JointBatch);
            if (joint == null)
                return DResult.Error("协同批次不存在！");
            if (joint.AddedBy != dto.UserId)
                return DResult.Error("你没有分配任务的权限！");
            var teacherResult = GroupContract.GroupMembers(joint.GroupId, UserRole.Teacher);
            if (!teacherResult.Status)
                return DResult.Error(teacherResult.Message);
            var teacherIds = teacherResult.Data.Select(t => t.Id).ToList();
            var userIds = dto.Details.SelectMany(d => d.TeacherIds).ToList().Distinct();

            if (userIds.Any(u => !teacherIds.Contains(u)))
                return DResult.Error("部分教师不是同事圈成员！");
            var paperResult = PaperContract.PaperDetailById(joint.PaperId);

            //所有主观题ID
            var qids = paperResult.Data.PaperSections.SelectMany(s => s.Questions.Where(q => !q.Question.IsObjective))
                .Select(q => q.Question.Id).ToList();
            if (dto.Details.Any(d => d.Questions.Any(q => !qids.Contains(q))))
                return DResult.Error("分配任务中存在试卷之外的题目！");
            if (!qids.ArrayEquals(dto.Details.SelectMany(d => d.Questions)))
                return DResult.Error("分配题目不完整或有交集！");
            if (JointDistributionRepository.Exists(t => t.JointBatch == dto.JointBatch))
                return DResult.Error("协同任务已分配！");
            //判断交叉
            return new DResult(true, joint.PaperId);
        }

        private DResult<string> CombineCheck(string jointBatch, List<string[]> groups, long teacherId)
        {
            if (string.IsNullOrWhiteSpace(jointBatch) || jointBatch.Length != 32)
                return DResult.Error<string>("协同批次号不正确！");
            if (groups.IsNullOrEmpty())
                return DResult.Error<string>("没有任何协同批阅题目！");
            if (teacherId <= 0)
                return DResult.Error<string>("教师ID异常~！");
            var jointModel = JointMarkingRepository.Load(jointBatch);
            if (jointModel == null)
                return DResult.Error<string>("协同批次号不正确！");
            if (jointModel.Status != (byte)JointStatus.Normal)
                return DResult.Error<string>("该协同已不能再批阅！");
            if (jointModel.PaperACount == 0 && jointModel.PaperBCount == 0)
                return DResult.Error<string>("该协同还没有上传试卷！");
            // 是否有权限批阅
            var qList = JointDistributionRepository.Where(t => t.JointBatch == jointBatch && t.TeacherId == teacherId)
                .Join(QuestionGroupRepository.Where(t => t.JointBatch == jointBatch), s => s.QuestionGroupId, g => g.Id,
                    (s, g) => g.QuestionIds)
                .ToList()
                .SelectMany(t => JsonHelper.JsonList<string>(t).ToArray())
                .ToList();
            if (groups.Any(g => !g.All(qList.Contains)))
                return DResult.Error<string>("你没有批阅部分题目的权限！");
            return DResult.Succ(jointModel.PaperId);
        }

        /// <summary> 客观题掌握情况 </summary>
        /// <param name="scoreList"></param>
        /// <param name="paper"></param>
        /// <param name="objDto"></param>
        /// <returns></returns>
        private ObjectQuestionScoreRate ObjectvieQuestionRate(List<QuestionScoreDto> scoreList, PaperDetailDto paper,
            ObjectQuestionScoreRate objDto)
        {
            //var dtos = new List<QuestionGraspingDto>();
            var isAb = paper.PaperBaseInfo.IsAb;
            var questions = paper.PaperSections.SelectMany(q => q.Questions).Where(w => w.Question.IsObjective);
            Func<string, string, string, int, QuestionGraspingDto> graspingFunc = (prefix, qid, did, sort) =>
            {
                var dto = new QuestionGraspingDto
                {
                    Id = qid,
                    ScoreRate = 0,
                    Sort = sort.ToString()

                };
                var scores = (string.IsNullOrWhiteSpace(did)
                    ? scoreList.Where(s => s.Id == qid)
                    : scoreList.Where(s => s.SmallId == did)).ToList();
                if (!scores.Any())
                {
                    dto.ScoreRate = -1;
                    return dto;
                }
                var total = scores.Sum(d => d.Total);
                if (total > 0)
                {
                    var score = scores.Sum(d => d.Score);
                    dto.ScoreRate = (int)Math.Round((score * 100M) / total, MidpointRounding.AwayFromZero);
                }
                return dto;
            };
            foreach (var question in questions)
            {
                QuestionGraspingDto dto = new QuestionGraspingDto();
                var qid = question.Question.Id;
                var prefix = isAb
                    ? (question.PaperSectionType == (byte)PaperSectionType.PaperA ? "A" : "B")
                    : string.Empty;
                if (question.Question.HasSmall)
                {
                    foreach (var detail in question.Question.Details)
                    {
                        dto = graspingFunc(prefix, qid, detail.Id, detail.Sort);
                        dto.RightKey = string.Join(string.Empty,
                            detail.Answers.Where(w => w.IsCorrect).OrderBy(t => t.Sort).Select(t => t.Tag));
                        if (question.PaperSectionType == (byte)PaperSectionType.PaperB)
                            objDto.PaperBQuestions.Add(dto);
                        else
                            objDto.PaperAQuestions.Add(dto);
                    }
                }
                else
                {
                    dto = graspingFunc(prefix, qid, null, question.Sort);
                    var temp = question.Question.Answers.Where(w => w.IsCorrect).OrderBy(t => t.Sort).Select(t => t.Tag).ToList();
                    dto.RightKey = string.Join(string.Empty, temp);

                    if (question.PaperSectionType == (byte)PaperSectionType.PaperB)
                        objDto.PaperBQuestions.Add(dto);
                    else
                        objDto.PaperAQuestions.Add(dto);
                }
            }
            objDto.PaperAQuestions = objDto.PaperAQuestions.ToList();
            objDto.PaperBQuestions = objDto.PaperBQuestions.OrderBy(d => d.Sort).ToList();
            return objDto;
        }

        #endregion

        /// <summary> 发布协同 </summary>
        public DResult PublishJoint(long teacherId, string groupId, string paperNum)
        {
            var checkResult = PublishJointCheck(teacherId, groupId, paperNum);
            if (!checkResult.Status)
                return checkResult;
            var idHelper = IdHelper.Instance;
            var usage = new TP_JointMarking
            {
                Id = idHelper.Guid32,
                GroupId = groupId,
                PaperId = checkResult.Data,
                AddedBy = teacherId,
                AddedAt = Clock.Now,
                PaperACount = 0,
                PaperBCount = 0,
                Status = (byte)JointStatus.Normal
            };
            var result = JointMarkingRepository.Insert(usage);
            if (string.IsNullOrWhiteSpace(result))
                return DResult.Error("发布失败，请稍后重试！");
            //发布动态
            var messageContract = CurrentIocManager.Resolve<IMessageContract>();
            messageContract.SendDynamic(new DynamicSendDto
            {
                DynamicType = GroupDynamicType.Joint,
                ContentType = (byte)ContentType.Publish,
                ContentId = usage.Id, //协同批次号
                GroupId = usage.GroupId,
                ReceivRole = UserRole.Teacher,
                UserId = usage.AddedBy
            });
            return new DResult(true, usage.Id);
        }

        public DResult<JAllotDto> JointAllot(string jointBatch)
        {
            if (string.IsNullOrWhiteSpace(jointBatch) || jointBatch.Length != 32)
                return DResult.Error<JAllotDto>("协同批次号不存在！");
            var joint =
                JointMarkingRepository.FirstOrDefault(t => t.Id == jointBatch && t.Status == (byte)JointStatus.Normal);
            if (joint == null)
                return DResult.Error<JAllotDto>("协同批次号不存在或已不能分配题目！");
            var paperResult = PaperContract.PaperDetailById(joint.PaperId);
            if (!paperResult.Status)
                return DResult.Error<JAllotDto>(paperResult.Message);
            var teacherResult = GroupContract.GroupMembers(joint.GroupId, UserRole.Teacher);
            if (!teacherResult.Status)
                return DResult.Error<JAllotDto>(teacherResult.Message);
            var paper = paperResult.Data;
            var dto = new JAllotDto
            {
                PaperId = paper.PaperBaseInfo.Id,
                PaperTitle = paper.PaperBaseInfo.PaperTitle,
                IsAb = paper.PaperBaseInfo.IsAb,
                Teachers = teacherResult.Data.ToDictionary(k => k.Id, v => new DUserDto
                {
                    Name = v.Name,
                    Avatar = v.Avatar
                })
            };
            var groups =
                QuestionGroupRepository.Where(t => t.JointBatch == jointBatch).ToList();
            var questions = PaperContract.PaperSorts(paper);
            if (groups.Any())
            {
                dto.Missions = new List<AllotMissionDto>();
                var teachers = JointDistributionRepository.Where(t => t.JointBatch == jointBatch)
                    .GroupBy(t => t.QuestionGroupId)
                    .ToDictionary(k => k.Key, v => v.Select(t => t.TeacherId).OrderBy(t => t).ToList());

                foreach (var @group in groups)
                {
                    var mission = new AllotMissionDto
                    {
                        Id = @group.Id,
                        SectionType = @group.SectionType,
                        Teachers = teachers[@group.Id]
                    };
                    var qList = JsonHelper.JsonList<string>(@group.QuestionIds);
                    mission.Questions = questions.Where(k => qList.Contains(k.Key)).OrderBy(t => t.Value.Split('-')[0].To(0))
                        .ToDictionary(k => k.Key, v => v.Value);
                    dto.Missions.Add(mission);
                }
                //排序
                dto.Missions = dto.Missions.OrderBy(t => t.SectionType)
                    .ThenBy(t => t.Questions.First().Value)
                    .ToList();
                return DResult.Succ(dto);
            }
            //            var types = SystemCache.Instance.QuestionTypes<QuestionTypeDto>()
            //                .ToDictionary(k => k.Id, v => v.Name);

            dto.Sections = paper.PaperSections.Where(s => s.Questions.Any(q => !q.Question.IsObjective))
                .GroupBy(s => s.PaperSectionType)
                .Select(t => new DistributeSectionDto
                {
                    Section = t.Key.GetEnumText<PaperSectionType, byte>(),
                    Types =
                        t.GroupBy(s => new { s.SectionQuType, s.Description })
                            .ToDictionary(k => k.Key.SectionQuType,
                                v => new AllotQuestionTypeDto
                                {
                                    Name = v.Key.Description,
                                    Questions =
                                        questions.Where(
                                            k =>
                                                v.SelectMany(s => s.Questions.Select(q => q.Question.Id))
                                                    .Contains(k.Key)).ToDictionary(k => k.Key, vv => vv.Value)
                                })
                }).ToList();
            return DResult.Succ(dto);
        }

        /// <summary> 分配任务 </summary>
        public DResult DistributionJoint(JDistributionDto dto)
        {
            var checkResult = DistributionCheck(dto);
            if (!checkResult.Status)
                return checkResult;
            var idHelper = IdHelper.Instance;
            var groups = new List<TP_JointQuestionGroup>();
            var inserts = new List<TP_JointDistribution>();
            foreach (var detail in dto.Details)
            {
                var group = new TP_JointQuestionGroup
                {
                    Id = idHelper.Guid32,
                    JointBatch = dto.JointBatch,
                    PaperId = checkResult.Message,
                    SectionType = detail.SectionType,
                    QuestionIds = JsonHelper.ToJson(detail.Questions)
                };
                groups.Add(group);
                inserts.AddRange(detail.TeacherIds.Select(teacherId => new TP_JointDistribution
                {
                    Id = idHelper.Guid32,
                    JointBatch = dto.JointBatch,
                    TeacherId = teacherId,
                    QuestionGroupId = @group.Id
                }));
            }
            var result = UnitOfWork.Transaction(() =>
            {
                if (groups.Any())
                {
                    QuestionGroupRepository.Insert(groups.ToArray());
                }
                if (inserts.Any())
                {
                    JointDistributionRepository.Insert(inserts.ToArray());
                }
            });
            return DResult.FromResult(result);
        }

        /// <summary> 添加任务 </summary>
        /// <param name="id"></param>
        /// <param name="teacherIds"></param>
        /// <returns></returns>
        public DResult AddDistribution(string id, List<long> teacherIds)
        {
            if (string.IsNullOrWhiteSpace(id) || teacherIds.IsNullOrEmpty())
                return DResult.Error("请选择任务组，并添加教师！");
            var group = QuestionGroupRepository.Load(id);
            if (group == null)
                return DResult.Error("没有找到任务组！");
            var joint = JointMarkingRepository.Load(group.JointBatch);
            if (joint.Status != (byte)JointStatus.Normal)
                return DResult.Error("协同已不支持分配任务！");
            var teacherResult = GroupContract.GroupMembers(joint.GroupId, UserRole.Teacher);
            if (!teacherResult.Status)
                return DResult.Error(teacherResult.Message);
            var teachers = teacherResult.Data.Select(t => t.Id).ToList();
            if (!teacherIds.All(t => teachers.Contains(t)))
                return DResult.Error("不能分配批阅任务给同事圈外的教师");
            var list = new List<TP_JointDistribution>();
            foreach (var teacherId in teacherIds)
            {
                var userId = teacherId;
                if (JointDistributionRepository.Exists(d => d.QuestionGroupId == id && d.TeacherId == userId))
                    continue;
                list.Add(new TP_JointDistribution
                {
                    Id = IdHelper.Instance.Guid32,
                    JointBatch = joint.Id,
                    TeacherId = userId,
                    QuestionGroupId = id
                });
            }
            if (!list.Any())
                return DResult.Error("没有可添加的教师！");
            var result = JointDistributionRepository.Insert(list.ToArray());
            return DResult.FromResult(result);
        }

        /// <summary> 协同批阅任务 </summary>
        /// <param name="jointBatch"></param>
        /// <param name="teacherId"></param>
        /// <returns></returns>
        public DResult<JMissionDto> JointMission(string jointBatch, long teacherId)
        {
            var errorResult = new Func<string, DResult<JMissionDto>>(DResult.Error<JMissionDto>);

            if (string.IsNullOrWhiteSpace(jointBatch) || jointBatch.Length != 32)
                return errorResult("协同批次号不正确！");
            var jointModel = JointMarkingRepository.Load(jointBatch);
            if (jointModel == null)
                return errorResult("协同批次不存在！");
            if (jointModel.Status != (byte)JointStatus.Normal)
                return errorResult("协同批次不存在！");
            //分组
            var groups = QuestionGroupRepository.Where(t => t.JointBatch == jointBatch)
                .Select(t => new { t.Id, t.QuestionIds })
                .ToList()
                .ToDictionary(k => k.Id, v => JsonHelper.JsonList<string>(v.QuestionIds).ToList());
            //教师任务
            var missions = JointDistributionRepository.Where(t => t.JointBatch == jointBatch)
                .Select(t => new { t.TeacherId, t.QuestionGroupId })
                .ToList()
                .GroupBy(t => t.QuestionGroupId)
                .ToDictionary(k => k.Key, v => v.Select(t => t.TeacherId).ToList());
            if (!missions.Any())
                return errorResult("该协同尚未分配任务！");
            var teacherResult = GroupContract.GroupMembers(jointModel.GroupId, UserRole.Teacher);
            if (!teacherResult.Status)
                return errorResult(teacherResult.Message);
            var paperResult = PaperContract.PaperDetailById(jointModel.PaperId);
            if (!paperResult.Status)
                return errorResult(paperResult.Message);
            var paper = paperResult.Data;
            var subjectiveList = paper.PaperSections.SelectMany(s => s.Questions)
                .Where(q => !q.Question.IsObjective)
                .ToList();
            var countDict = new Dictionary<byte, int> { { 1, jointModel.PaperACount }, { 2, jointModel.PaperBCount } };
            var areas = JointHelper.QuestionAreaCache(jointBatch);
            var dto = new JMissionDto
            {
                PaperId = jointModel.PaperId,
                IsAb = paper.PaperBaseInfo.IsAb,
                PaperTitle = paper.PaperBaseInfo.PaperTitle,
                IsCreator = (jointModel.AddedBy == teacherId),

                Teachers = teacherResult.Data.ToDictionary(k => k.Id, v => new DUserDto
                {
                    Name = v.Name,
                    Avatar = v.Avatar
                }),
                QuestionSorts = PaperContract.PaperSorts(paper)
            };
            //异常数
            var exceptions = JointExceptionRepository.Where(t => t.JointBatch == jointBatch)
                .Select(t => new
                {
                    t.QuestionIds,
                    t.Status
                }).ToList()
                .Select(t => new
                {
                    ids = JsonHelper.JsonList<string>(t.QuestionIds),
                    status = t.Status
                }).ToList();
            dto.ExceptionCount = exceptions.Count;
            dto.SolveCount = exceptions.Count(t => t.status == (byte)NormalStatus.Delete);

            var types = SystemCache.Instance.QuestionTypes()
                .ToDictionary(k => k.Id, v => v.Name);

            #region 年级批阅进度

            dto.QuestionMissions = subjectiveList.GroupBy(t => t.PaperSectionType)
                .ToDictionary(k => k.Key,
                    v =>
                        v.GroupBy(t => t.Question.Type)
                            .ToDictionary(k2 => types[k2.Key], v2 => v2.Select(t => new QuestionMissionDto
                            {
                                Id = t.Question.Id,
                                Count = countDict[v.Key]
                            }).ToList()));

            var schedules = JointHelper.Instance(jointBatch).MarkingSchedule();
            foreach (var item in dto.QuestionMissions.Values.SelectMany(t => t.Values.SelectMany(q => q)))
            {
                if (schedules.ContainsKey(item.Id))
                {
                    item.Teachers = schedules[item.Id];
                }
                var group = groups.First(t => t.Value.Contains(item.Id));
                foreach (var id in missions[group.Key])
                {
                    if (!item.Teachers.ContainsKey(id))
                        item.Teachers.Add(id, 0);
                }
            }

            #endregion

            var myMissions = missions.Where(t => t.Value.Contains(teacherId));
            if (!myMissions.Any())
                return DResult.Succ(dto);
            //我的任务
            foreach (var mission in myMissions)
            {
                if (!groups.ContainsKey(mission.Key))
                    continue;
                var ids = groups[mission.Key];
                var questions = subjectiveList.Where(q => ids.Contains(q.Question.Id)).ToList();
                var fillList = questions.Where(q => q.Question.Type == 7).ToList();
                var other = questions.Where(q => q.Question.Type != 7);
                //填空题
                if (fillList.Any())
                {
                    var first = fillList.First();
                    var qid = first.Question.Id;
                    var fill = new MissionItemDto
                    {
                        SectionType = first.PaperSectionType,
                        QuestionType = types[first.Question.Type],
                        QuestionIds = fillList.Select(q => q.Question.Id).ToList(),
                        Marked = 0,
                        Left = countDict[first.PaperSectionType],
                        Markingabel = countDict[first.PaperSectionType] > 0 && areas.ContainsKey(qid)
                    };
                    fill.HasException =
                        exceptions.Exists(
                            t => t.ids.ArrayEquals(fill.QuestionIds) && t.status == (byte)NormalStatus.Normal);
                    //:todo 是否有异常~
                    if (schedules.ContainsKey(qid))
                    {
                        var schedule = schedules[qid];
                        if (schedule.ContainsKey(teacherId))
                            fill.Marked = schedule[teacherId];
                        fill.Left -= schedule.Values.Sum();
                    }
                    if (fill.HasException)
                    {
                        fill.Left -= exceptions.Count(
                            t => t.ids.ArrayEquals(fill.QuestionIds) && t.status == (byte)NormalStatus.Normal);
                    }
                    dto.Missions.Add(fill);
                }

                //其他题目
                foreach (var qItem in other)
                {
                    var qid = qItem.Question.Id;
                    var item = new MissionItemDto
                    {
                        SectionType = qItem.PaperSectionType,
                        QuestionType = types[qItem.Question.Type],
                        QuestionIds = new List<string> { qid },
                        Marked = 0,
                        Left = countDict[qItem.PaperSectionType],
                        Markingabel = countDict[qItem.PaperSectionType] > 0 && areas.ContainsKey(qid)
                    };
                    item.HasException =
                        exceptions.Exists(
                            t => t.ids.ArrayEquals(item.QuestionIds) && t.status == (byte)NormalStatus.Normal);
                    if (schedules.ContainsKey(qid))
                    {
                        var schedule = schedules[qid];
                        if (schedule.ContainsKey(teacherId))
                            item.Marked = schedule[teacherId];
                        item.Left -= schedule.Values.Sum();
                    }
                    if (item.HasException)
                    {
                        item.Left -= exceptions.Count(
                            t => t.ids.ArrayEquals(item.QuestionIds) && t.status == (byte)NormalStatus.Normal);
                    }
                    dto.Missions.Add(item);
                }
            }
            dto.Missions = dto.Missions.OrderBy(t => t.SectionType)
                .ThenBy(t => dto.QuestionSorts[t.QuestionIds.First()].Split('-')[0].To(0))
                .ToList();
            return DResult.Succ(dto);
        }

        public DResult<JCombineDto> JointCombine(string jointBatch, List<string[]> groups, long teacherId)
        {
            var errorResult = new Func<string, DResult<JCombineDto>>(DResult.Error<JCombineDto>);
            var checkResult = CombineCheck(jointBatch, groups, teacherId);
            if (!checkResult.Status)
                return errorResult(checkResult.Message);
            var areas = JointHelper.QuestionAreaCache(jointBatch);
            if (areas == null || !areas.Any())
                return errorResult("该协同还没有上传试卷！");
            var paperResult = PaperContract.PaperDetailById(checkResult.Data);
            if (!paperResult.Status)
                return errorResult(paperResult.Message);
            var paper = paperResult.Data;
            var dto = new JCombineDto
            {
                PaperId = paper.PaperBaseInfo.Id,
                PaperTitle = paper.PaperBaseInfo.PaperTitle,
                IsAb = paper.PaperBaseInfo.IsAb
            };
            var sorts = PaperContract.PaperSorts(paper);
            var questionDict = paper.PaperSections.SelectMany(s => s.Questions)
                .Where(q => !q.Question.IsObjective)
                .ToDictionary(k => k.Question.Id, v => new
                {
                    section = v.PaperSectionType,
                    type = v.Question.Type,
                    sort = sorts[v.Question.Id],
                    score = v.Score
                });
            foreach (var @group in groups)
            {
                if (@group.IsNullOrEmpty())
                    continue;
                var groupDto = new JGroupDto
                {
                    Section = (dto.IsAb ? questionDict[@group[0]].section : (byte)0),
                    QuestionIds = @group.ToList()
                };
                var rects = new List<RectangleF>();
                foreach (var id in @group)
                {
                    if (!questionDict.ContainsKey(id) || !areas.ContainsKey(id))
                        continue;
                    var dict = questionDict[id];
                    var area = areas[id];
                    dto.Questions.Add(id, new JQuestionDto
                    {
                        Type = dict.type,
                        Sort = dict.sort,
                        Score = dict.score,
                        Area = new DArea
                        {
                            X = area.X,
                            Y = area.Y,
                            W = area.Width,
                            H = area.Height
                        }
                    });
                    rects.Add(new RectangleF(area.X, area.Y, area.Width, area.Height));
                }
                var region = ImageCls.CombineRegion(rects, 20);
                groupDto.Region = new DArea
                {
                    X = 0,
                    Y = region.Y,
                    W = 780,
                    H = region.Height
                };
                dto.Groups.Add(groupDto);
            }
            return DResult.Succ(dto);
        }

        public DResults<JPictureDto> ChangePictures(string jointBatch, List<JGroupStepDto> groups, long teacherId)
        {
            var errorResult = new Func<string, DResults<JPictureDto>>(DResult.Errors<JPictureDto>);
            var checkResult = CombineCheck(jointBatch, groups.Select(g => g.Ids).ToList(), teacherId);
            if (!checkResult.Status)
                return errorResult(checkResult.Message);
            var areas = JointHelper.QuestionAreaCache(jointBatch);
            if (areas == null || !areas.Any())
                return errorResult("该协同还没有上传试卷！");
            var dtos = new List<JPictureDto>();
            var helper = JointHelper.Instance(jointBatch);
            var pictureIds = new List<string>();
            foreach (var @group in groups)
            {
                if (@group.Ids.IsNullOrEmpty())
                    continue;
                var dto = helper.MarkingJointPicture(teacherId, @group.Ids, @group.Step);
                if (!string.IsNullOrWhiteSpace(dto.Id))
                {
                    pictureIds.Add(dto.Id);
                    foreach (var id in group.Ids)
                    {
                        dto.Details.Add(id, new JSubmitDetailDto());
                    }
                }
                dtos.Add(dto);
            }
            if (dtos.All(t => string.IsNullOrWhiteSpace(t.Id)))
            {
                return errorResult(groups.Any(g => g.Step < -1) ? "已没有更多的批阅记录！" : "所有题目已批阅完毕！");
            }
            //拿图片信息
            var pictures = MarkingPictureRepository.Where(t => pictureIds.Contains(t.Id))
                .Select(t => new
                {
                    t.Id,
                    t.AnswerImgUrl,
                    t.BatchNo,
                    t.StudentID,
                    t.RightAndWrong
                }).ToList();

            foreach (var dto in dtos)
            {
                if (string.IsNullOrWhiteSpace(dto.Id))
                    continue;
                var picture = pictures.FirstOrDefault(p => p.Id == dto.Id);
                if (picture == null)
                    continue;
                var rects = new List<RectangleF>();
                var qids = dto.Details.Keys.ToList();
                foreach (var qid in qids)
                {
                    if (!areas.ContainsKey(qid))
                        continue;
                    var area = areas[qid];
                    rects.Add(new RectangleF(area.X, area.Y, area.Width, area.Height));
                }
                var region = ImageCls.CombineRegion(rects, 20);
                //区域
                dto.Picture = picture.AnswerImgUrl.PaperImage(0, (int)Math.Ceiling(region.Y), 780,
                    (int)Math.Ceiling(region.Height));
                var marks =
                    (JsonHelper.JsonList<MkSymbol>(picture.RightAndWrong) ?? new List<MkSymbol>())
                        .ToList();
                dto.Marks = marks.Where(m => qids.Contains(m.QuestionId)).ToList();
                dto.Details = MarkingDetailRepository.Where(
                    d => d.Batch == picture.BatchNo && d.StudentID == picture.StudentID && qids.Contains(d.QuestionID))
                    .Select(d => new { d.Id, d.QuestionID, d.CurrentScore })
                    .ToList()
                    .ToDictionary(k => k.QuestionID, v => new JSubmitDetailDto
                    {
                        Id = v.Id,
                        Score = v.CurrentScore
                    });
            }
            //图片信息
            return DResult.Succ(dtos, -1);
        }

        public DResult JointSubmit(JSubmitDto dto)
        {
            if (dto == null || dto.TeacherId <= 0)
                return DResult.Error("提交参数异常！");
            if (dto.Pictures.IsNullOrEmpty() && dto.Details.IsNullOrEmpty())
                return DResult.Success;
            var details = new List<TP_MarkingDetail>();
            var pictures = new List<TP_MarkingPicture>();
            if (dto.Details.Any())
            {
                foreach (var detail in dto.Details)
                {
                    var item = MarkingDetailRepository.Load(detail.Id);
                    if (item == null)
                        continue;
                    item.MarkingAt = Clock.Now;
                    item.MarkingBy = dto.TeacherId;
                    item.CurrentScore = (detail.Score > item.Score ? item.Score : detail.Score);
                    item.IsCorrect = (item.Score == item.CurrentScore);
                    details.Add(item);
                }
            }
            if (dto.Pictures.Any())
            {
                //合并同一张图片
                var list = dto.Pictures.GroupBy(p => p.Id).Select(t => new
                {
                    id = t.Key,
                    marks = t.SelectMany(m => m.Marks)
                });
                foreach (var picture in list)
                {
                    var item = MarkingPictureRepository.Load(picture.id);
                    if (item == null)
                        continue;
                    var marks =
                        (JsonHelper.JsonList<MkSymbol>(item.RightAndWrong) ?? new List<MkSymbol>())
                            .ToList();
                    foreach (var mark in picture.marks)
                    {
                        if (mark.SymbolType < 0)
                        {
                            //删除
                            var temp = marks.FirstOrDefault(t => t.X == mark.X && t.Y == mark.Y);
                            if (temp != null)
                                marks.Remove(temp);
                        }
                        else
                        {
                            marks.Add(mark);
                        }
                    }
                    item.RightAndWrong = JsonHelper.ToJson(marks);
                    item.LastMarkingTime = Clock.Now;
                    pictures.Add(item);
                }
            }
            var result = MarkingDetailRepository.UnitOfWork.Transaction(() =>
            {
                if (details.Any())
                {
                    MarkingDetailRepository.Update(d => new
                    {
                        d.MarkingBy,
                        d.MarkingAt,
                        d.CurrentScore,
                        d.IsCorrect
                    }, details.ToArray());
                }
                if (pictures.Any())
                {
                    MarkingPictureRepository.Update(p => new
                    {
                        p.RightAndWrong,
                        p.LastMarkingTime
                    }, pictures.ToArray());
                }
            });
            return DResult.FromResult(result);
        }

        public DResult ReportException(JExceptionDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.JointBatch) || string.IsNullOrWhiteSpace(dto.PictureId))
                return DResult.Error("参数异常，请刷新重试！");
            if (dto.TeacherId <= 0)
                return DResult.Error("用户Id异常！");
            var picture = MarkingPictureRepository.Load(dto.PictureId);
            if (picture == null)
                return DResult.Error("试卷图片异常！");
            if (dto.QuestionIds.IsNullOrEmpty())
                return DResult.Error("未获取到题目信息！");
            var ids = JsonHelper.ToJson(dto.QuestionIds);
            if (
                JointExceptionRepository.Exists(
                    t =>
                        t.JointBatch == dto.JointBatch && t.PictureId == dto.PictureId &&
                        t.Status == (byte)NormalStatus.Normal
                        && t.QuestionIds == ids))
            {
                return DResult.Error("不能重复报告异常！");
            }
            var item = new TP_JointException
            {
                Id = IdHelper.Instance.Guid32,
                JointBatch = dto.JointBatch,
                PictureId = dto.PictureId,
                ExceptionType = dto.Type,
                Message = dto.Message,
                UserId = dto.TeacherId,
                StudentName = picture.StudentName,
                AddedAt = Clock.Now,
                Status = (byte)NormalStatus.Normal
            };
            if (!dto.QuestionIds.IsNullOrEmpty())
            {
                item.QuestionIds = JsonHelper.ToJson(dto.QuestionIds);
            }
            var result = JointExceptionRepository.Insert(item);
            if (string.IsNullOrWhiteSpace(result))
                return DResult.Error("操作异常，请稍后重试！");
            //更新状态
            if (!dto.QuestionIds.IsNullOrEmpty())
            {
                var pictureId = dto.PictureId;
                var qids = dto.QuestionIds ?? new List<string>();
                Task.Factory.StartNew(() =>
                {
                    JointHelper.Instance(dto.JointBatch).UpdateException(pictureId, qids);
                });
            }
            return DResult.Success;
        }

        public DResult SolveException(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return DResult.Error("异常ID不正确！");
            var model = JointExceptionRepository.Load(id);
            if (model == null)
                return DResult.Error("异常未找到！");
            if (model.Status != (byte)NormalStatus.Normal)
                return DResult.Error("异常已被解决！");
            model.Status = (byte)NormalStatus.Delete;
            model.SolveTime = Clock.Now;
            var result = JointExceptionRepository.Update(t => new
            {
                t.Status,
                t.SolveTime
            }, model);
            if (result <= 0)
                return DResult.Error("操作异常，请稍后重试！");
            var qids = (JsonHelper.JsonList<string>(model.QuestionIds) ?? new List<string>()).ToList();
            if (!qids.IsNullOrEmpty())
            {
                var pictureId = model.PictureId;
                Task.Factory.StartNew(() =>
                {
                    JointHelper.Instance(model.JointBatch).UpdateException(pictureId, qids, true);
                });
            }
            return DResult.Success;
        }

        /// <summary> 协同列表 </summary>
        public DResults<JointGroupDto> JointList(string paperId, long userId = -1)
        {
            if (string.IsNullOrWhiteSpace(paperId) || paperId.Length != 32)
                return DResult.Errors<JointGroupDto>("试卷ID不正确！");
            var joints =
                JointMarkingRepository.Where(t =>
                    t.PaperId == paperId && t.Status != (byte)JointStatus.Finished
                    && t.Status != (byte)JointStatus.Delete);
            if (userId > 0)
            {
                var groups = GroupContract.Groups(userId, (int)GroupType.Colleague);
                if (groups.Status && groups.TotalCount > 0)
                {
                    var ids = groups.Data.Select(t => t.Id).ToList();
                    joints = joints.Where(j => ids.Contains(j.GroupId));
                }
                else
                {
                    return DResult.Errors<JointGroupDto>("没有协同批次！");
                }
            }
            var list = joints.ToList();
            if (!list.Any())
                return DResult.Errors<JointGroupDto>("没有协同批次！");
            var jointGroups = new List<JointGroupDto>();
            foreach (var jointMarking in list)
            {
                var item = new JointGroupDto
                {
                    JointBatch = jointMarking.Id,
                    GroupId = jointMarking.GroupId,
                    UserId = jointMarking.AddedBy,
                    AddedAt = jointMarking.AddedAt
                };
                var group = GroupContract.LoadById(item.GroupId);
                if (group.Status)
                {
                    item.GroupName = group.Data.Name;
                    item.GroupCode = group.Data.Code;
                }
                var user = UserRepository.Load(item.UserId);
                if (user != null)
                {
                    item.UserName = user.TrueName;
                }
                var classList = GroupContract.ColleagueClasses(item.GroupId);
                if (classList.Status)
                    item.ClassList = classList.Data.ToList();
                jointGroups.Add(item);
            }
            return DResult.Succ(jointGroups, jointGroups.Count);
        }

        /// <summary> 查询各班级考试批次号 </summary>
        public List<string> GetBatchByJoint(string jointBatch)
        {
            if (jointBatch.IsNullOrEmpty()) return null;
            return UsageRepository.Where(u => u.JointBatch == jointBatch).Select(u => u.Id).ToList();
        }

        /// <summary> 客观题得分率 </summary>
        /// <param name="jointBatch"></param>
        /// <returns></returns>
        public DResult<ObjectQuestionScoreRate> ObjectiveQuestionScore(string jointBatch)
        {
            var dto = new ObjectQuestionScoreRate();
            if (string.IsNullOrEmpty(jointBatch) || jointBatch.Length != 32)
            {
                return DResult.Error<ObjectQuestionScoreRate>("协同批次号不正确");
            }
            var jointModel = JointMarkingRepository.Load(jointBatch);
            if (jointModel == null)
            {
                return DResult.Error<ObjectQuestionScoreRate>("协同批次号不存在");
            }
            if (jointModel.Status != (byte)JointStatus.Normal)
                return DResult.Error<ObjectQuestionScoreRate>("协同批次号不存在");
            var paperResult = PaperContract.PaperDetailById(jointModel.PaperId);
            dto.PaperACount = jointModel.PaperACount;
            dto.PaperBCount = jointModel.PaperBCount;
            if (!paperResult.Status)
                return DResult.Error<ObjectQuestionScoreRate>(paperResult.Message);
            //根据协同批次获取各班级考试批次号
            var batchs = GetBatchByJoint(jointBatch);
            //根据试卷ID和批次编号获取题目信息
            var details = MarkingDetailRepository.Where(w => batchs.Contains(w.Batch) && w.PaperID == jointModel.PaperId)
                .Select(t => new QuestionScoreDto
                {
                    Id = t.QuestionID,
                    SmallId = t.SmallQID,
                    Total = t.Score,
                    Score = t.CurrentScore
                }).ToList();
            if (!details.Any())
            {
                return DResult.Succ(dto);
            }
            if (paperResult.Status && paperResult.Data != null)
            {
                ObjectQuestionScoreRate result = new ObjectQuestionScoreRate();
                result = ObjectvieQuestionRate(details, paperResult.Data, dto);
                if (result != null)
                {
                    dto = result;
                }
                // dto.Questions = ObjectvieQuestionRate(details, paperResult.Data);
                dto.IsAb = paperResult.Data.PaperBaseInfo.IsAb;
            }
            return DResult.Succ(dto);
        }


    }
}
