using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DayEasy.AutoMapper;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.SchoolBook;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;

namespace DayEasy.Services.Services
{
    /// <summary> 教材及章节 </summary>
    public partial class SystemService
    {
        public IVersion3Repository<TS_SchoolBook> SchoolBookRepository { private get; set; }

        public IVersion3Repository<TS_SchoolBookChapter> SchoolBookChapterRepository { private get; set; }

        internal string Number2Str(int num)
        {
            //教材及章节的编号规则为：每一层级2位数
            if (num < 0 || num > 99) throw new Exception("悲剧了，教材及章节的编号容量不足");
            return num < 10 ? string.Format("0{0}", num) : num.ToString(CultureInfo.InvariantCulture);
        }

        #region 教材管理

        public DResult<SchoolBookDto> AddSchoolBook(SchoolBookDto dto)
        {
            if (dto == null) return DResult.Error<SchoolBookDto>("参数不能为空");
            if (dto.Title.IsNullOrEmpty()) return DResult.Error<SchoolBookDto>("请填写教材名称");
            var stages = new List<byte>
            {
                (byte) StageEnum.PrimarySchool,
                (byte) StageEnum.JuniorMiddleSchool,
                (byte) StageEnum.HighSchool
            };
            if (!stages.Contains(dto.Stage)) return DResult.Error<SchoolBookDto>("请选择学段");
            if (dto.SubjectId < 0) return DResult.Error<SchoolBookDto>("请选择学科");
            var existSubject = SubjectRepository
                .Exists(s => s.Id == dto.SubjectId && s.Status == (byte)TempStatus.Normal);
            if (!existSubject) return DResult.Error<SchoolBookDto>("所选学科不存在");

            var sbook = new TS_SchoolBook
            {
                Id = IdHelper.Instance.Guid32,
                Title = dto.Title,
                AddedAt = Clock.Now,
                Stage = dto.Stage,
                SubjectId = dto.SubjectId,
                Status = (byte)SchoolBookStatus.Edit
            };
            //注：这里别加状态判断-避免重复存在Code
            var total = SchoolBookRepository.Count(b => b.Stage == dto.Stage && b.SubjectId == dto.SubjectId) + 1;
            sbook.Code = string.Format("{0}{1}{2}", dto.Stage, Number2Str(dto.SubjectId), Number2Str(total));
            SchoolBookRepository.Insert(sbook);
            return DResult.Succ(sbook.MapTo<SchoolBookDto>());
        }

        public DResult EditSchoolBook(SchoolBookDto dto)
        {
            if (dto == null || dto.Id.IsNullOrEmpty()) return DResult.Error("参数不能为空");
            var sbook = SchoolBookRepository.FirstOrDefault(b => b.Id == dto.Id);
            if (sbook == null) return DResult.Error("没有查询到教材资料");
            if (sbook.Status != dto.Status)
            {
                var check = Enum.GetValues(typeof (SchoolBookStatus))
                    .Cast<SchoolBookStatus>().ToList()
                    .Exists(s => (byte) s == dto.Status);
                if (!check) return DResult.Error("状态无效");

                sbook.Status = dto.Status;
                return SchoolBookRepository.Update(b => new { b.Status }, sbook) > 0
                    ? DResult.Success
                    : DResult.Error("操作失败");
            }
            if (dto.Title.IsNullOrEmpty()) return DResult.Error("请填写教材名称");
            sbook.Title = dto.Title;
            return SchoolBookRepository.Update(b => new { b.Title }, sbook) > 0
                ? DResult.Success
                : DResult.Error("操作失败");
        }

        public DResults<SchoolBookDto> SchoolBooks(byte stage, int subjectId, bool ignoreStatus = true)
        {
            Expression<Func<TS_SchoolBook, bool>> condition = b =>
                b.Stage == stage && b.SubjectId == subjectId;
            if (!ignoreStatus) condition = condition.And(b => b.Status == (byte)SchoolBookStatus.Normal);
            var list = SchoolBookRepository.Where(condition)
                .OrderBy(b => b.AddedAt)
                .ToList().MapTo<List<SchoolBookDto>>();
            return DResult.Succ(list, list.Count);
        }

        public DResult<SchoolBookDto> GetSchoolBookByCode(string code)
        {
            if (code.IsNullOrEmpty() || code.Length < 5) return DResult.Error<SchoolBookDto>("教材编号错误");
            if (code.Length > 5) code = code.Substring(0, 5);
            var item = SchoolBookRepository.FirstOrDefault(b => b.Code == code);
            if (item == null) return DResult.Error<SchoolBookDto>("没有查询到教材资料");
            if (item.Status == (byte) SchoolBookStatus.Delete)
                return DResult.Error<SchoolBookDto>("该教材已删除");
            return DResult.Succ(item.MapTo<SchoolBookDto>());
        }

        #endregion

        #region 教材章节管理

        public DResult<SchoolBookChapterDto> AddSbChapter(SchoolBookChapterDto dto)
        {
            if (dto == null) return DResult.Error<SchoolBookChapterDto>("参数不能为空");
            if (dto.Title.IsNullOrEmpty()) return DResult.Error<SchoolBookChapterDto>("请填写章节名称");
            //此时的 Code 为 父章节 Code
            if (dto.Code.IsNullOrEmpty() || dto.Code.Length < 5)
                return DResult.Error<SchoolBookChapterDto>("父章节编号错误");
            if (dto.Code.Length == 5)
            {
                if (!SchoolBookRepository.Exists(b => b.Code == dto.Code))
                    return DResult.Error<SchoolBookChapterDto>("没有查询到教材资料");
            }
            else
            {
                var parent = SchoolBookChapterRepository.FirstOrDefault(p => p.Code == dto.Code);
                if (parent == null) return DResult.Error<SchoolBookChapterDto>("没有查询到父章节");
                if (parent.IsLast || parent.Knowledge.IsNotNullOrEmpty())
                    return DResult.Error<SchoolBookChapterDto>("最小章节不能继续添加章节");
                if (!parent.HasChild)
                {
                    parent.HasChild = true;
                    SchoolBookChapterRepository.Update(c => new {c.HasChild}, parent);
                }
            }
            var len = dto.Code.Length + 2;
            var total = SchoolBookChapterRepository.Count(c =>
                c.Code.StartsWith(dto.Code) && c.Code.Length == len) + 1;
            if (dto.Sort < 1) dto.Sort = total;
            var chapter = new TS_SchoolBookChapter
            {
                Id = IdHelper.Instance.GetGuid32(),
                Title = dto.Title,
                Sort = dto.Sort,
                Code = dto.Code + Number2Str(total),
                Status = (byte)NormalStatus.Normal,
                IsLast = false
            };
            SchoolBookChapterRepository.Insert(chapter);
            return DResult.Succ(chapter.MapTo<SchoolBookChapterDto>());
        }

        public DResult EditSbChapter(SchoolBookChapterDto dto)
        {
            if (dto == null || dto.Id.IsNullOrEmpty()) return DResult.Error("参数不能为空");
            var chapter = SchoolBookChapterRepository.FirstOrDefault(c => c.Id == dto.Id);
            if (chapter == null) return DResult.Error("没有查询到章节资料");
            if (chapter.Status != dto.Status)
            {
                chapter.Status = dto.Status;
                return SchoolBookChapterRepository.Update(c => new { c.Status }, chapter) > 0
                    ? DResult.Success
                    : DResult.Error("操作失败");
            }
            if (dto.Title.IsNullOrEmpty()) return DResult.Error("请填写章节名称");

            var param = new List<string> { "Title" };
            chapter.Title = dto.Title;
            if (dto.Sort > 0)
            {
                param.Add("Sort");
                chapter.Sort = dto.Sort;
            }
            return SchoolBookChapterRepository.Update(chapter, param.ToArray()) > 0
                ? DResult.Success
                : DResult.Error("操作失败");
        }

        public DResult UpdateSbChapterKps(string id, string kps)
        {
            if (id.IsNullOrEmpty()) return DResult.Error("章节ID不能为空");
            var chapter = SchoolBookChapterRepository.FirstOrDefault(c => c.Id == id);
            if (chapter == null) return DResult.Error("没有查询到章节资料");
            if (chapter.HasChild) return DResult.Error("存在子章节，不能添加知识点");
            chapter.IsLast = true;
            chapter.Knowledge = kps.IsNullOrEmpty() ? null : kps;
            return SchoolBookChapterRepository.Update(c => new {c.Knowledge, c.IsLast}, chapter) > 0
                ? DResult.Success
                : DResult.Error("知识点更新失败");
        }

        public DResults<SchoolBookChapterDto> SbChapters(string code, bool ignoreStatus = true)
        {
            if (code.IsNullOrEmpty() || code.Length < 5)
                return DResult.Errors<SchoolBookChapterDto>("父章节编号错误");
            if (code.Length == 5)
            {
                var sbook = SchoolBookRepository.FirstOrDefault(b => b.Code == code);
                if (sbook == null || sbook.Status == (byte)NormalStatus.Delete)
                    return DResult.Errors<SchoolBookChapterDto>("教材不存在，或已删除");
            }
            else
            {
                var chapter = SchoolBookChapterRepository.FirstOrDefault(c => c.Code == code);
                if (chapter == null || chapter.Status == (byte)NormalStatus.Delete)
                    return DResult.Errors<SchoolBookChapterDto>("父章节不存在，或已删除");
            }
            var len = code.Length + 2;
            Expression<Func<TS_SchoolBookChapter, bool>> condition =
                c => c.Code.StartsWith(code) && c.Code.Length == len;
            if (!ignoreStatus) condition = condition.And(c => c.Status == (byte) NormalStatus.Normal);
            var list = SchoolBookChapterRepository.Where(condition)
                .OrderBy(c => c.Sort).ToList().MapTo<List<SchoolBookChapterDto>>();
            return DResult.Succ(list, list.Count);
        }

        public DResults<KeyValuePair<string, string>> SbChapterNav(string code)
        {
            if (code.IsNullOrEmpty() || code.Length < 5)
                return DResult.Errors<KeyValuePair<string, string>>("父章节编号错误");
            //教材编号5位
            var bookCode = code.Substring(0, 5);
            var sbook = SchoolBookRepository.FirstOrDefault(b => b.Code == bookCode);
            if (sbook == null) return DResult.Errors<KeyValuePair<string, string>>("没有查询到教材资料");
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(sbook.Code, sbook.Title)
            };
            var codes = new List<string>();
            //顶级编号7位
            while (code.Length >= 7)
            {
                codes.Add(code);
                code = code.Substring(0, code.Length - 2);
            }
            if (!codes.Any()) return DResult.Succ(list, list.Count);
            var chapters = SchoolBookChapterRepository.Where(c => codes.Contains(c.Code)).ToList();
            codes.Reverse();
            codes.ForEach(k =>
            {
                var chapter = chapters.FirstOrDefault(c => c.Code == k);
                var title = chapter != null ? chapter.Title : string.Empty;
                list.Add(new KeyValuePair<string, string>(k, title));
            });
            return DResult.Succ(list, list.Count);
        }

        public DResults<SchoolBookChapterDto> SbChapterKnowledges(string code)
        {
            var result = SbChapters(code, false);
            if (!result.Status) return result;
            var chapters = result.Data.ToList();
            var kpList = SchoolBookChapterRepository.Where(c =>
                c.Status == (byte) NormalStatus.Normal && c.IsLast &&
                c.Knowledge != null && c.Code.StartsWith(code))
                .Select(c => new {c.Code, c.Knowledge}).ToList();
            chapters.ForEach(cpt =>
            {
                var tmp = new List<NameDto>();
                var kps = kpList.Where(k => k.Code.StartsWith(cpt.Code) && k.Knowledge.IsNotNullOrEmpty()).ToList();
                if (kps.Any())
                {
                    kps.ForEach(k =>
                    {
                        var kp = JsonHelper.JsonList<NameDto>(k.Knowledge).ToList();
                        if (kp.Any()) tmp.AddRange(kp);
                    });
                }
                cpt.KnowledgeList = tmp;
            });
            return DResult.Succ(chapters, chapters.Count);
        }

        #endregion

    }
}
