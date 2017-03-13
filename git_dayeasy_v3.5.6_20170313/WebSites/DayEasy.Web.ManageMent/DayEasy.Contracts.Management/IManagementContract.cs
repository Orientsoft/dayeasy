
using System.Collections.Generic;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Models;
using DayEasy.Contracts.Models.Mongo;
using DayEasy.Core;
using DayEasy.Core.Domain;
using DayEasy.Utility;

namespace DayEasy.Contracts.Management
{
    public partial interface IManagementContract : IDependency
    {
        #region Application
        /// <summary> 应用列表 </summary>
        /// <param name="hasDelete"></param>
        /// <returns></returns>
        List<TS_Application> Applications(bool hasDelete);

        TS_Application Application(int id);

        /// <summary> 添加或更新应用 </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        DResult InsertOrUpdateApp(AppDto dto);

        DResult DeleteApplication(int id);

        /// <summary> 附加应用 - 拥有的用户列表 </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        DResults<UserAppDto> ApplicationUsers(int id, DPage page = null);

        /// <summary> 移除用户附加应用 </summary>
        /// <param name="appId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult RemoveUserApp(int appId, long userId);

        #endregion

        #region Subject

        TS_Subject Subject(int id);
        List<TS_Subject> Subjects();
        DResult DeleteSubject(int id);

        DResult InsertOrUpdateSubject(SubjectDto subjectDto);

        #endregion

        #region QuestionType

        List<TS_QuestionType> QuestionTypes();

        TS_QuestionType QuestionType(int id);

        DResult InsertOrUpdateQuestionType(QuestionTypeDto dto);

        DResult DeleteQuestionType(int id);

        #endregion

        #region Log

        /// <summary> 日志列表 </summary>
        /// <param name="status"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        DResults<TS_SystemLog> Logs(int status = -1, int page = 0, int size = 15);

        /// <summary> 解决异常日志 </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        DResult ResolveLog(string id);

        /// <summary> 下载日志 </summary>
        /// <param name="type"></param>
        /// <param name="keyword"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        DResults<TS_DownloadLog> DownloadLogs(int type, string keyword, DPage page = null);

        #endregion
    }
}
