using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using DayEasy.AutoMapper;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Contracts.Dtos.ErrorQuestion;
using DayEasy.Utility.Timing;
using DayEasy.Utility.Helper;
using DayEasy.Contracts.Dtos.Question;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data;
using DayEasy.Core;

namespace DayEasy.Services.Services
{
    public partial class SystemService
    {
        public IDayEasyRepository<TS_Knowledge, int> KnowledgeRepository { private get; set; }
        public IDayEasyRepository<TP_ErrorQuestion> ErrorQuestionRepository { private get; set; }
        public IDayEasyRepository<TQ_Question> QuestionRepository { private get; set; }
        public IDayEasyRepository<TU_User, long> UserRepository { private get; set; }
        public IVersion3Repository<TG_Member> MemberRepository { private get; set; }
        public IDayEasyRepository<TG_Group> GroupRepository { private get; set; }
        #region 知识点

        public List<KnowledgeDto> Knowledges(SearchKnowledgeDto knowlodgeDto)
        {
            if (knowlodgeDto == null || knowlodgeDto.Stage > 3 || knowlodgeDto.SubjectId < 0 ||
                knowlodgeDto.SubjectId > 40)
            {
                return new List<KnowledgeDto>();
            }
            Expression<Func<TS_Knowledge, bool>> condition =
                u => u.Status == (byte)TempStatus.Normal
                     && u.Stage == knowlodgeDto.Stage
                     && u.SubjectID == knowlodgeDto.SubjectId;
            Expression<Func<TS_Knowledge, int>> orderBy = u => (u.Sort == 0 ? 99 : u.Sort);
            if (knowlodgeDto.ParentId >= 0)
                condition = condition.And(u => u.PID == knowlodgeDto.ParentId);
            if (!string.IsNullOrWhiteSpace(knowlodgeDto.ParentCode))
                condition =
                    condition.And(
                        u =>
                            u.Code.StartsWith(knowlodgeDto.ParentCode) &&
                            u.Code.Length == knowlodgeDto.ParentCode.Length + 2);
            if (knowlodgeDto.Version.HasValue)
                condition = condition.And(u => u.KnowledgeVersion == knowlodgeDto.Version);
            if (knowlodgeDto.IsLast)
                condition = condition.And(u => !u.HasChildren);
            if (!string.IsNullOrWhiteSpace(knowlodgeDto.Keyword))
            {
                condition = condition.And(u => u.Name.Contains(knowlodgeDto.Keyword));
                orderBy = u => u.PID;
            }
            var knowledges = KnowledgeRepository.Where(condition)
                .OrderBy(u => u.KnowledgeVersion)
                .ThenBy(u => u.Name == knowlodgeDto.Keyword ? 0 : 1)
                .ThenBy(orderBy)
                .Skip(knowlodgeDto.Size * knowlodgeDto.Page)
                .Take(knowlodgeDto.Size)
                .MapTo<List<KnowledgeDto>>();
            if (knowlodgeDto.LoadPath)
            {
                var parents = KnowledgeParents(knowledges.Select(k => k.Code));
                foreach (var knowledge in knowledges)
                {
                    //                    knowledge.Parents = KnowledgePath(knowledge.Code);
                    knowledge.Parents = parents.Where(t => knowledge.Code.Contains(t.Key))
                        .OrderBy(t => t.Key.Length)
                        .ToDictionary(k => k.Key, v => v.Value);
                }
            }
            return knowledges;
        }

        public List<KnowledgeDto> Knowledges(List<int> kpIds)
        {
            if (kpIds == null || kpIds.Count < 1)
            {
                return new List<KnowledgeDto>();
            }

            Expression<Func<TS_Knowledge, bool>> condition =
                u => u.Status == (byte)TempStatus.Normal
                     && kpIds.Contains(u.Id);

            Expression<Func<TS_Knowledge, int>> orderBy = u => (u.Sort == 0 ? 99 : u.Sort);
            return KnowledgeRepository.Where(condition)
                    .OrderBy(u => u.KnowledgeVersion)
                    .ThenBy(orderBy)
                    .MapTo<List<KnowledgeDto>>();
        }

        public DResults<TreeDto> KnowledgeTrees(SearchKnowledgeDto knowledgeDto)
        {
            var listResult = Knowledges(knowledgeDto);
            if (!listResult.Any())
                return DResult.Succ(new List<TreeDto>(), 0);
            var list = listResult.Select(
                t => new TreeDto
                {
                    Id = t.Id.ToString(CultureInfo.InvariantCulture),
                    ParentId = (t.ParentId > 0 ? t.ParentId.ToString(CultureInfo.InvariantCulture) : "#"),
                    Text = t.Name,
                    HasChildren = t.IsParent,
                    LiAttr = new Attr
                    {
                        Sort = (t.Sort == 0 ? 99 : t.Sort),
                        Title = t.Code
                    }
                }).ToList();
            return DResult.Succ(list, list.Count());
        }


        public Dictionary<string, string> KnowledgePath(string code)
        {
            var dict = new Dictionary<string, string>();

            var codeList = new List<string>();
            while (code.Length > 3)
            {
                code = code.Substring(0, code.Length - 2);
                codeList.Add(code);
            }
            if (codeList.Any())
            {
                dict = KnowledgeRepository.Where(k => codeList.Contains(k.Code))
                    .OrderBy(k => k.Id)
                    .ToDictionary(k => k.Code, v => v.Name);
            }
            return dict;
        }

        private Dictionary<string, string> KnowledgeParents(IEnumerable<string> codes)
        {
            var dict = new Dictionary<string, string>();
            var parentCodes = new List<string>();
            foreach (var code in codes)
            {
                var item = code;
                while (item.Length > 3)
                {
                    item = item.Substring(0, item.Length - 2);
                    if (!parentCodes.Contains(item))
                        parentCodes.Add(item);
                }
            }
            if (parentCodes.Any())
            {
                dict = KnowledgeRepository.Where(k => parentCodes.Contains(k.Code))
                    .OrderBy(k => k.Id)
                    .ToDictionary(k => k.Code, v => v.Name);
            }
            return dict;
        }
        public DResults<ErrorQuestionKnowledgeDto> Knowledges(SearchErrorQuestionDto dto)
        {
            if (string.IsNullOrEmpty(dto.GroupId))
            {
                return DResult.Errors<ErrorQuestionKnowledgeDto>("圈子ID不能为空");
            }
            var members = MemberRepository.Where(w => w.GroupId == dto.GroupId && w.Status == (byte)NormalStatus.Normal && (w.MemberRole & (byte)UserRole.Student) > 0)
               .Select(w => w.MemberId).ToList();
            if (!members.Any())
                return DResult.Errors<ErrorQuestionKnowledgeDto>("圈子没有学生");
            Expression<Func<TP_ErrorQuestion, bool>> condition =
            u => u.SubjectID == dto.SubjectId;
            if (dto.UserId > 0)
                condition = condition.And(w => w.StudentID == dto.UserId);
            else
                condition = condition.And(w => members.Contains(w.StudentID));
            if (dto.DateRange > 0)
            {
                var dateNow = Clock.Now;
                var pastTimes = dateNow.AddDays(-dto.DateRange);
                condition = condition.And(w => w.AddedAt >= pastTimes && w.AddedAt <= dateNow);
            }
            if (dto.QuestionType > 0)
            {
                condition = condition.And(w => w.QType == dto.QuestionType);
            }
            condition = condition.And(w => w.SubjectID == dto.SubjectId);
            //根据条件获取错题数
            var errorQuestions = ErrorQuestionRepository.Where(condition);
            if (!errorQuestions.Any())
                return DResult.Errors<ErrorQuestionKnowledgeDto>("无错题");
            var questions = QuestionRepository.Table;
            var join = errorQuestions.Join(questions, a => a.QuestionID, b => b.Id, (a, b) =>
            new { b.KnowledgeIDs, b.Id }).DistinctBy(w => w.Id).ToList();
            if (!join.Any())
                return DResult.Errors<ErrorQuestionKnowledgeDto>("无知识点");
            List<ErrorQuestionKnowledgeDto> rlist = new List<ErrorQuestionKnowledgeDto>();//包含重复的知识点
            //将json转换成对象
            foreach (var question in join)
            {
                var dic = question.KnowledgeIDs.JsonToObject<Dictionary<string, string>>();
                dic.Foreach(kn =>
                {
                    var count = join.Where(w => w.KnowledgeIDs.Contains(kn.Key)).Count();
                    rlist.Add(new ErrorQuestionKnowledgeDto { Code = kn.Key, Name = kn.Value, ErrCount = count });
                });
            }
            rlist = rlist.DistinctBy(w=>w.Code).OrderByDescending(w => w.ErrCount).ToList();
            return DResult.Succ(rlist, rlist.Count);
        }
        public DResults<ErrorUserDto> ErrorUsers(string groupId, int subjectId)
        {
            if (string.IsNullOrEmpty(groupId))
                return DResult.Errors<ErrorUserDto>("圈子ID不能为空");
            SqlParameter[] prams = new SqlParameter[2];
            byte status = (byte)NormalStatus.Normal;
            var userid = MemberRepository.Where(w => w.GroupId == groupId && w.Status == status).Select(w => w.MemberId).ToList();
            if (!userid.Any())
                return DResult.Errors<ErrorUserDto>("该圈子没有学生");
            string ids = "(";
            userid.Foreach(t =>
            {
                ids += t + ",";
            });
            ids = ids.Substring(0, ids.Length - 1) + ")";
            string sql = @"select u.UserID as Id,COUNT(*) as ErrorCount,max(u.NickName) as Nick  ,max(u.TrueName) as Name, max(u.HeadPhoto) as Avatar from TP_ErrorQuestion q 
                            join TU_User u on q.StudentID=u.UserID where u.UserID in " + ids + "  and u.Status=@status and q.SubjectID=@subjectId group by u.UserID   order by ErrorCount desc";
            prams[0] = new SqlParameter("@status", status);
            prams[1] = new SqlParameter("@subjectId", subjectId);
            //prams[1] = new SqlParameter("@userIds", ids);
            var errorUsers = UnitOfWork.SqlQuery<ErrorUserDto>(sql, prams).ToList();
            errorUsers.ForEach(d =>
            {
                if (string.IsNullOrEmpty(d.Avatar))
                    d.Avatar = Consts.DefaultAvatar();
            });
            return DResult.Succ(errorUsers, errorUsers.Count);
        }
        #endregion
    }
}
