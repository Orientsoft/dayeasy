using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Marking;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.EntityFramework;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;

namespace DayEasy.Marking.Services
{
    public class AnswerShareService : DayEasyService, IAnswerShareContract
    {
        public IGroupContract GroupContract { private get; set; }
        public IDayEasyRepository<TC_Usage, string> UsageRepository { private get; set; }
        public IDayEasyRepository<TP_AnswerShare, string> AnswerShareRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingDetail, string> MarkingDetailRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingPicture, string> MarkingPictureRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingMark, string> MarkingMarkRepository { private get; set; }
        public IDayEasyRepository<TP_WorshipDetail, string> WorshipDetailRepository { private get; set; }
        public IDayEasyRepository<TP_PaperContent, string> PaperContentRepository { private get; set; }

        public AnswerShareService(IDbContextProvider<DayEasyDbContext> context)
            : base(context)
        { }

        /// <summary> 同学分享的答案 </summary>
        /// <param name="questionId"></param>
        /// <param name="groupId"></param>
        /// <param name="all"></param>
        /// <returns></returns>
        public DResults<AnswerShareDto> Shares(string questionId, string groupId, bool all = false)
        {
            if (questionId.IsNullOrEmpty())
                return DResult.Errors<AnswerShareDto>("问题ID不能为空");
            if (groupId.IsNullOrEmpty())
                return DResult.Errors<AnswerShareDto>("班级圈ID不能为空");
            Expression<Func<TP_AnswerShare, bool>> condition = a =>
                a.Status == (byte)AnswerShareStatus.Normal
                && a.QuestionId == questionId;
            var classCondition = condition.And(a => a.ClassId == groupId);

            if (AnswerShareRepository.Exists(classCondition))
            {
                condition = classCondition;
            }
            var shares = AnswerShareRepository.Where(condition).OrderByDescending(a => a.WorshipCount)
                .Select(a => new AnswerShareDto
                {
                    Id = a.Id,
                    Name = a.AddedName,
                    Count = a.WorshipCount
                });
            if (!all)
                shares = shares.Take(10);
            return DResult.Succ(shares.ToList(), shares.Count());
        }

        /// <summary>
        /// 添加分享
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public DResult Add(AnswerShareAddModelDto item)
        {
            if (item == null || item.Batch.IsNullOrEmpty() || item.GroupId.IsNullOrEmpty() ||
                item.PaperId.IsNullOrEmpty() || item.QuestionId.IsNullOrEmpty())
                return DResult.Error("参数错误");

            var exist = GroupContract.IsGroupMember(item.UserId, item.GroupId);
            if (exist != CheckStatus.Normal)
                return DResult.Error("没有分享到该圈子的权限！");

            var share = AnswerShareRepository.FirstOrDefault(a =>
                a.Batch == item.Batch && a.QuestionId == item.QuestionId && a.AddedBy == item.UserId);
            if (share != null)
                return DResult.Success;

            if (item.Status == AnswerShareStatus.Normal)
            {
                var details = MarkingDetailRepository.Where(d =>
                    d.Batch == item.Batch && d.PaperID == item.PaperId &&
                    d.QuestionID == item.QuestionId && d.StudentID == item.UserId).ToList();
                if (!details.Any() || !details.All(d => d.IsCorrect.HasValue && d.IsCorrect.Value))
                    return DResult.Error("你的答案没有得到满分，不能分享");
            }

            var id = IdHelper.Instance.GetGuid32();
            var model = new TP_AnswerShare
            {
                Id = id,
                Batch = item.Batch,
                PaperId = item.PaperId,
                ClassId = item.GroupId,
                QuestionId = item.QuestionId,
                AddedBy = item.UserId,
                AddedName = item.UserName,
                AddedAt = Clock.Now,
                WorshipCount = 0,
                Status = (byte)item.Status
            };
            return AnswerShareRepository.Insert(model).IsNotNullOrEmpty()
                ? DResult.Success
                : DResult.Error("分享失败");
        }

        /// <summary>
        /// 膜拜同学分享的答案
        /// </summary>
        /// <param name="id"></param>
        /// <param name="studentId"></param>
        /// <param name="studentName"></param>
        /// <returns></returns>
        public DResult Worship(string id, long studentId, string studentName)
        {
            if (id.IsNullOrEmpty())
                return DResult.Error("分享答案ID为空！");
            var share = AnswerShareRepository.FirstOrDefault(
                s => s.Id == id && s.Status == (byte)AnswerShareStatus.Normal);
            if (share == null)
                return DResult.Error("没有找到答案分享！");
            var exists = WorshipDetailRepository.Exists(w => w.ShareId == id && w.AddedBy == studentId);
            if (exists) return DResult.Success;
            var entity = new TP_WorshipDetail
            {
                Id = IdHelper.Instance.GetGuid32(),
                ShareId = id,
                AddedBy = studentId,
                AddedName = studentName,
                AddedAt = Clock.Now
            };
            share.WorshipCount += 1;
            WorshipDetailRepository.Insert(entity);
            AnswerShareRepository.Update(a => new { a.WorshipCount }, share);
            return DResult.Success;
        }

        /// <summary>
        /// 分享的答案详细
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DResult<AnswerShareDetailDto> Detail(string id, long userId)
        {
            if (id.IsNullOrEmpty())
                return DResult.Error<AnswerShareDetailDto>("参数错误");
            //分享记录
            var share = AnswerShareRepository.FirstOrDefault(s =>
                s.Id == id && s.Status == (byte)AnswerShareStatus.Normal);
            if (share == null)
                return DResult.Error<AnswerShareDetailDto>("没有查询到分享记录");
            var usage = UsageRepository.FirstOrDefault(u => u.Id == share.Batch);
            if (usage == null)
                return DResult.Error<AnswerShareDetailDto>("没有查询到发布记录");

            //问题对应的试卷版块类型：A卷、B卷
            var paperContent = PaperContentRepository.FirstOrDefault(c =>
                c.PaperID == share.PaperId && c.QuestionID == share.QuestionId);
            if (paperContent == null)
                return DResult.Error<AnswerShareDetailDto>("没有查询到试卷详细资料");
            var isPaperB = paperContent.PaperSectionType == (byte)PaperSectionType.PaperB;
            //学生答卷图片
            Expression<Func<TP_MarkingPicture, bool>> conditionPicture = p =>
                p.BatchNo == share.Batch
                && p.ClassID == share.ClassId
                && p.StudentID == share.AddedBy
                && (isPaperB
                    ? p.AnswerImgType == (byte)MarkingPaperType.PaperB
                    : (p.AnswerImgType == (byte)MarkingPaperType.PaperA ||
                       p.AnswerImgType == (byte)MarkingPaperType.Normal));

            var picture = MarkingPictureRepository.FirstOrDefault(conditionPicture);
            if (picture == null || picture.AnswerImgUrl.IsNullOrEmpty())
                return DResult.Error<AnswerShareDetailDto>("没有查询到同学的答卷资料");

            //问题所在试卷区域
            var areaBatch = usage.JointBatch.IsNotNullOrEmpty() ? usage.JointBatch : usage.Id;
            Expression<Func<TP_MarkingMark, bool>> conditionArea =
                a => a.BatchNo == areaBatch
                     && (isPaperB
                         ? a.PaperType == (byte)MarkingPaperType.PaperB
                         : (a.PaperType == (byte)MarkingPaperType.PaperA ||
                            a.PaperType == (byte)MarkingPaperType.Normal));
            var areas = MarkingMarkRepository.Where(conditionArea).ToList();
            if (!areas.Any())
                return DResult.Error<AnswerShareDetailDto>("没有查询到同学的答卷资料");

            var marks = new List<MkQuestionAreaDto>();
            areas.ForEach(a => marks.AddRange(JsonHelper.JsonList<MkQuestionAreaDto>(a.Mark).ToList()));
            var mark = marks.FirstOrDefault(m => m.Id == share.QuestionId);
            if (mark == null)
                return DResult.Error<AnswerShareDetailDto>("没有查询到同学的答卷资料");

            //膜拜记录
            var worshipStr = string.Empty;
            bool hadWorship = false;
            if (share.WorshipCount > 0)
            {
                var worships = WorshipDetailRepository
                    .Where(s => s.ShareId == share.Id)
                    .OrderByDescending(s => s.AddedAt)
                    .Take(5).Select(a => new { a.AddedBy, a.AddedName })
                    .ToList();
                if (worships.Any())
                {
                    hadWorship = worships.Exists(u => u.AddedBy == userId);
                    worshipStr = string.Join(",", worships.Select(u => u.AddedName).ToList());
                }
            }

            var img = string.Empty;

            try
            {
                img = ImageHelper.MakeImage((new HttpHelper(picture.AnswerImgUrl)).GetStream(),
                    (int)mark.X, (int)mark.Y, (int)mark.Width, (int)mark.Height);
            }
            catch
            {
            }

            return DResult.Succ(new AnswerShareDetailDto
            {
                Img = img,
                WorshipCount = share.WorshipCount,
                WorshipName = worshipStr,
                HadWorship = hadWorship,
                StudentName = share.AddedName
            });
        }

        /// <summary>
        /// 试卷中已经分享答案的题目
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        public DResults<string> SharedQuestions(string batch, string paperId, long studentId)
        {
            if (string.IsNullOrEmpty(batch) || string.IsNullOrEmpty(paperId) || studentId < 1)
                return DResult.Errors<string>("参数错误");

            var ids = AnswerShareRepository.Where(u =>
                    u.Batch == batch && u.PaperId == paperId && u.AddedBy == studentId &&
                    u.Status == (byte)AnswerShareStatus.Normal)
                    .Select(u => u.QuestionId)
                    .ToList();
            return DResult.Succ(ids, ids.Count);
        }

        /// <summary>
        /// 已分享的答案 - 在线阅卷使用
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<TP_AnswerShare> BatchShares(string batch, AnswerShareStatus status = AnswerShareStatus.PreShare)
        {
            return AnswerShareRepository.Where(s => s.Batch == batch && s.Status == (byte)status).ToList();
        }

    }
}
