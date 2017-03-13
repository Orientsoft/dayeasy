using System.Collections.Generic;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.SchoolBook;
using DayEasy.Core;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    /// <summary> 系统业务模块 - 教材章节 </summary>
    public partial interface ISystemContract 
    {
        DResult<SchoolBookDto> AddSchoolBook(SchoolBookDto dto);
        DResult EditSchoolBook(SchoolBookDto dto);
        DResults<SchoolBookDto> SchoolBooks(byte stage, int subjectId, bool ignoreStatus = true);
        DResult<SchoolBookDto> GetSchoolBookByCode(string code);

        DResult<SchoolBookChapterDto> AddSbChapter(SchoolBookChapterDto dto);
        DResult EditSbChapter(SchoolBookChapterDto dto);
        DResults<SchoolBookChapterDto> SbChapters(string code, bool ignoreStatus = true);
        DResults<KeyValuePair<string, string>> SbChapterNav(string code);
        DResult UpdateSbChapterKps(string id, string kps);
        DResults<SchoolBookChapterDto> SbChapterKnowledges(string code);
    }
}
