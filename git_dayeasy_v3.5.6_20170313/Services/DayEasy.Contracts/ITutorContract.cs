using DayEasy.Contracts.Dtos.Tutor;
using DayEasy.Core;
using DayEasy.Core.Domain;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    /// <summary> 辅导业务模块 </summary>
    public partial interface ITutorContract : IDependency
    {
        /// <summary>
        /// 获取辅导列表数据
        /// </summary>
        /// <returns></returns>
        DResults<TutorDto> GetTutorsByUserId(long userId, DPage page);

        /// <summary>
        /// 修改辅导的状态
        /// </summary>
        /// <returns></returns>
        DResult UpdateStatus(string tutorId, byte status);

        /// <summary>
        /// 添加辅导数据处理
        /// </summary>
        /// <returns></returns>
        DResult<TutorDataDto> AddTutorData(string tutorId, string paperBaseStr, string videoId, string newQuestionId, string tutorData);

        /// <summary>
        /// 保存辅导
        /// </summary>
        /// <returns></returns>
        DResult SaveTutor(string tutorDataStr, long userId, string draft);

        /// <summary>
        /// 辅导使用记录
        /// </summary>
        /// <returns></returns>
        DResults<TutorRecordsDto> Records(string tutorId, DPage page);

        /// <summary>
        /// 获取辅导数据
        /// </summary>
        /// <returns></returns>
        DResult<TutorDto> GetTutorById(string tutorId);

        /// <summary>
        /// 获取反馈信息列表
        /// </summary>
        /// <returns></returns>
        DResults<FeedBackDto> GetFeedBackData(string tutorId, DPage page);

        /// <summary>
        ///  获取反馈的统计信息
        /// </summary>
        /// <param name="tutorId"></param>
        /// <returns></returns>
        DResults<FeedBackPointDto> GetFeedBackStatistics(string tutorId);

        /// <summary>
        /// 添加视频
        /// </summary>
        /// <returns></returns>
        DResult AddVideo(string videoName, string videoUrl, string videoDesc, string faceImg, decimal time, int grade, long userId, int subjectId);

        /// <summary>
        /// 查询辅导详情
        /// </summary>
        /// <param name="tutorId"></param>
        /// <returns></returns>
        DResult<TutorDetailDto> GetTutorDetail(string tutorId);

        /// <summary>
        /// 添加辅导使用记录
        /// </summary>
        /// <param name="tutorId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult AddTutorRecord(string tutorId, long userId);

        /// <summary>
        /// 添加辅导评价
        /// </summary>
        /// <param name="tutorId"></param>
        /// <param name="userId"></param>
        /// <param name="comment"></param>
        /// <param name="type">暂无枚举，2、4、8、16、32</param>
        /// <param name="score"></param>
        /// <returns></returns>
        DResult AddTutorComment(string tutorId, long userId, string comment, byte? type, decimal? score);

        /// <summary>
        /// 获取学生对辅导的评价
        /// </summary>
        /// <param name="tutorId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult<TutorCommentDto> GetTutorStudentRecord(string tutorId, long userId);
    }
}
