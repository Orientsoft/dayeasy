using DayEasy.Contracts.Dtos.Marking;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.Utility;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DayEasy.Contracts
{
    /// <summary> 批阅相关业务契约 </summary>
    public partial interface IMarkingContract : IDependency
    {
        #region 阅卷标记区域

        /// <summary> 进入阅卷区域标记界面验证 </summary>
        /// <param name="batch"></param>
        /// <param name="type"></param>
        /// <param name="userId"></param>
        /// <param name="isJoint"></param>
        /// <returns></returns>
        DResult<MarkingAreaDto> MkAreaCheck(string batch, int type, long userId, bool isJoint = false);

        /// <summary>
        /// 答卷答题区域标记 - 切换图片
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="type"></param>
        /// <param name="isJoint"></param>
        /// <returns></returns>
        string MkAreaChangePicture(string batch, int type, bool isJoint = false);

        /// <summary>
        /// 答卷答题区域标记 - 问题序号列表
        /// </summary>
        /// <param name="paperId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        DResults<MarkingAreaQuestion> MkAreaQuestions(string paperId, int type);

        /// <summary> 答卷答题区域标记 - 保存 </summary>
        /// <param name="batch"></param>
        /// <param name="type"></param>
        /// <param name="areas"></param>
        /// <returns></returns>
        DResult MkAreaSave(string batch, int type, string areas);

        #endregion

        #region 提交批阅、结束阅卷

        /// <summary>
        /// 首次提交，生成必要数据
        /// </summary>
        /// <param name="pictureId"></param>
        /// <returns></returns>
        DResult Commit(string pictureId);

        /// <summary>
        /// 提交批阅
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        DResult Submit(MkSubmitDto item);

        ///// <summary> 完成阅卷 </summary>
        ///// <param name="batch">批次号</param>
        ///// <param name="paperId">试卷ID</param>
        ///// <param name="type">阅卷状态</param>
        ///// <param name="autoSetIcon">是否默认标记 “ √ ”</param>
        ///// <param name="autoSetScore">是否默认标记 分数</param>
        ///// <param name="userId">当前登录用户ID</param>
        ///// <returns></returns>
        //DResult Finished(string batch, string paperId, MarkingStatus type, bool autoSetIcon, bool autoSetScore, long userId);

        /// <summary> 更新批阅状态(普通阅卷) </summary>
        /// <param name="batch"></param>
        /// <param name="status"></param>
        DResult UpdateMarkingStatus(string batch, byte status);

        /// <summary> 完成阅卷 </summary>
        /// <param name="inputDto"></param>
        DResult CompleteMarking(CompleteMarkingInputDto inputDto);

        #endregion

        #region 待批阅数据 - 非协同

        /// <summary>
        /// 进入批阅验证
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="type"></param>
        /// <returns>bool：正常、需要标记区域</returns>
        DResult<bool> MkCheck(string batch, int type);

        /// <summary>
        /// 答卷图片列表
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        DResult<MarkingDataDto> MkPictureList(string batch, int type);

        /// <summary>
        /// 答卷图片详细
        /// </summary>
        /// <param name="pictureId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        DResult<MkPictureDto> MkPictureDetail(string pictureId, int type);

        #endregion

        #region 阅卷结束后 - 相关查询

        /// <summary>
        /// 查询试卷批改详情
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResults<MarkedDetailDto> GetMarkedDetail(string batch, string paperId, long userId);

        /// <summary>
        /// 查询答卷图片
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="studentId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        DResult<MkPictureDto> LoadPictureDto(string batch, string paperId, long studentId,
            MarkingPaperType type = MarkingPaperType.Normal);

        /// <summary>
        /// 查询阅卷结果
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        DResult<MarkingResultDto> LoadResultDto(string batch, string paperId, long studentId);

        #endregion

        #region 结束阅卷后 - 更新成绩统计

        /// <summary>
        /// 编辑统计分数
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="teacherId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        DResult UpdateScoreStatistics(string batch, string paperId, long teacherId, List<StudentRankInfoDto> data);

        //#endregion

        //#region 更新批阅结果 - 未完成阅卷时，更改选择题答案

        ///// <summary>
        ///// 更新批阅结果 - 未完成阅卷时，更改选择题答案
        ///// </summary>
        ///// <param name="questionId">问题Id</param>
        ///// <param name="paperId">试卷ID，为空则更新所有包含该问题的试卷</param>
        ///// <param name="smallId">小问ID</param>
        //void MkUpdateDetailByQuestionChange(string questionId, string paperId, string smallId = null);

        #endregion

        /// <summary> 异步提交图片 </summary>
        /// <param name="paperId"></param>
        /// <param name="pictureIds"></param>
        /// <param name="userId"></param>
        /// <param name="jointBatch"></param>
        /// <returns></returns>
        Task CommitPictureAsync(long userId, string paperId, List<string> pictureIds, string jointBatch = null);

        /// <summary> 是否已完成阅卷 </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        bool IsFinished(string batch);

    }
}
