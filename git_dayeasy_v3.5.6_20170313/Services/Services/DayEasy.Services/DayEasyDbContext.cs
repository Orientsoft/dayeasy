using System.Data.Entity;
using DayEasy.Contracts.Models;
using DayEasy.EntityFramework;

namespace DayEasy.Services
{
    public class DayEasyDbContext : CodeFirstDbContext
    {
        public DayEasyDbContext()
            : base(GetConnection("dayeasy"), true)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            Database.SetInitializer<DayEasyDbContext>(null);
        }

        #region 课堂相关

        public virtual DbSet<TC_AgencyVideo> TC_AgencyVideo { get; set; }
        public virtual DbSet<TC_ClassContentRecord> TC_ClassContentRecord { get; set; }
        public virtual DbSet<TC_StudentQuestion> TC_StudentQuestion { get; set; }
        public virtual DbSet<TC_Video> TC_Video { get; set; }
        public virtual DbSet<TC_VideoClass> TC_VideoClass { get; set; }
        public virtual DbSet<TC_VideoClassContent> TC_VideoClassContent { get; set; }

        #endregion

        public virtual DbSet<TC_Usage> TC_Usage { get; set; }

        #region 日志相关

        public virtual DbSet<TL_SystemLog> TL_SystemLog { get; set; }
        public virtual DbSet<TL_UserLog> TL_UserLog { get; set; }

        #endregion

        #region 试卷题目

        public virtual DbSet<TP_AgencyPaper> TP_AgencyPaper { get; set; }
        public virtual DbSet<TP_AnswerShare> TP_AnswerShare { get; set; }
        public virtual DbSet<TP_ErrorQuestion> TP_ErrorQuestion { get; set; }
        public virtual DbSet<TP_ErrorReason> TP_ErrorReason { get; set; }
        public virtual DbSet<TP_ErrorReasonComment> TP_ErrorReasonComment { get; set; }
        public virtual DbSet<TP_ErrorTag> TP_ErrorTag { get; set; }
        public virtual DbSet<TP_ErrorTagStatistic> TP_ErrorTagStatistic { get; set; }
        public virtual DbSet<TP_MarkingDetail> TP_MarkingDetail { get; set; }
        public virtual DbSet<TP_MarkingMark> TP_MarkingMark { get; set; }
        public virtual DbSet<TP_MarkingPicture> TP_MarkingPicture { get; set; }
        public virtual DbSet<TP_MarkingResult> TP_MarkingResult { get; set; }
        public virtual DbSet<TP_Paper> TP_Paper { get; set; }
        public virtual DbSet<TP_PaperAllot> TP_PaperAllot { get; set; }
        public virtual DbSet<TP_PaperAnswer> TP_PaperAnswer { get; set; }
        public virtual DbSet<TP_PaperContent> TP_PaperContent { get; set; }
        public virtual DbSet<TP_PaperSection> TP_PaperSection { get; set; }
        public virtual DbSet<TP_SmallQScore> TP_SmallQScore { get; set; }
        public virtual DbSet<TP_VariantQuestion> TP_VariantQuestion { get; set; }
        public virtual DbSet<TP_WorshipDetail> TP_WorshipDetail { get; set; }
        public virtual DbSet<TQ_AgencyQuestion> TQ_AgencyQuestion { get; set; }
        public virtual DbSet<TQ_Analysis> TQ_Analysis { get; set; }
        public virtual DbSet<TQ_Answer> TQ_Answer { get; set; }
        public virtual DbSet<TQ_Question> TQ_Question { get; set; }
        public virtual DbSet<TQ_SmallQuestion> TQ_SmallQuestion { get; set; }
        public virtual DbSet<TP_Variant> TP_Variant { get; set; }
        public virtual DbSet<TQ_VariantRelation> TQ_VariantRelation { get; set; }

        #endregion

        #region 系统相关

        public virtual DbSet<TS_Application> TS_Application { get; set; }
        public virtual DbSet<TS_Area> TS_Area { get; set; }
        public virtual DbSet<TS_DynamicNews> TS_DynamicNews { get; set; }
        public virtual DbSet<TS_Knowledge> TS_Knowledge { get; set; }
        public virtual DbSet<TS_Log> TS_Log { get; set; }
        public virtual DbSet<TS_Message> TS_Message { get; set; }
        public virtual DbSet<TS_QuestionType> TS_QuestionType { get; set; }
        public virtual DbSet<TS_ShowPaper> TS_ShowPaper { get; set; }
        public virtual DbSet<TS_SpecialTreatment> TS_SpecialTreatment { get; set; }
        public virtual DbSet<TS_Subject> TS_Subject { get; set; }
        public virtual DbSet<TS_SystemLog> TS_SystemLog { get; set; }
        public virtual DbSet<TS_Tag> TS_Tag { get; set; }
        public virtual DbSet<TS_TempGroup> TS_TempGroup { get; set; }
        public virtual DbSet<TS_DownloadLog> TS_DownloadLog { get; set; }

        #endregion

        #region 选修课

        public virtual DbSet<TS_ElectiveBatch> TS_ElectiveBatch { get; set; }
        public virtual DbSet<TS_ElectiveCourse> TS_ElectiveCourse { get; set; }
        public virtual DbSet<TS_ElectiveDetail> TS_ElectiveDetail { get; set; }

        #endregion

        #region 统计相关

        public virtual DbSet<TS_ClassScoreStatistics> TS_ClassScoreStatistics { get; set; }
        public virtual DbSet<TS_StudentKpStatistic> TS_StudentKpStatistic { get; set; }
        public virtual DbSet<TS_StudentStatistic> TS_StudentStatistic { get; set; }
        public virtual DbSet<TS_StuScoreStatistics> TS_StuScoreStatistics { get; set; }
        public virtual DbSet<TS_TeacherKpStatistic> TS_TeacherKpStatistic { get; set; }
        public virtual DbSet<TS_TeacherStatistic> TS_TeacherStatistic { get; set; }

        #endregion

        #region 用户相关

        public virtual DbSet<TU_AdminUserRole> TU_AdminUserRole { get; set; }
        public virtual DbSet<TU_Agency> TU_Agency { get; set; }
        public virtual DbSet<TU_AgencyApplication> TU_AgencyApplication { get; set; }
        public virtual DbSet<TU_AgencySubject> TU_AgencySubject { get; set; }
        public virtual DbSet<TU_Class> TU_Class { get; set; }
        public virtual DbSet<TU_MangagerHistory> TU_MangagerHistory { get; set; }
        public virtual DbSet<TU_ThirdPlatform> TU_ThirdPlatform { get; set; }
        public virtual DbSet<TU_User> TU_User { get; set; }
        public virtual DbSet<TU_UserAgencyRelation> TU_UserAgencyRelation { get; set; }
        public virtual DbSet<TU_UserApplication> TU_UserApplication { get; set; }
        public virtual DbSet<TU_UserToken> TU_UserToken { get; set; }
        public virtual DbSet<TU_UserAgency> TU_UserAgency { get; set; }
        public virtual DbSet<TU_Impression> TU_Impression { get; set; }
        public virtual DbSet<TU_Quotations> TU_Quotations { get; set; }
        public virtual DbSet<TU_ImpressionLike> TU_ImpressionLike { get; set; }
        public virtual DbSet<TU_Visit> TU_Visit { get; set; }
        public virtual DbSet<TU_AgencyVisit> TU_AgencyVisit { get; set; }

        #endregion

        #region 辅导相关

        public virtual DbSet<TT_TutorComment> TT_TutorComment { get; set; }
        public virtual DbSet<TT_TutorContent> TT_TutorContent { get; set; }
        public virtual DbSet<TT_TutorRecord> TT_TutorRecord { get; set; }
        public virtual DbSet<TT_Tutorship> TT_Tutorship { get; set; }

        #endregion

        #region 协同阅卷

        public virtual DbSet<TP_JointMarking> TP_JointMarking { get; set; }
        public virtual DbSet<TP_JointQuestionGroup> TP_JointQuestionGroup { get; set; }
        public virtual DbSet<TP_JointDistribution> TP_JointDistribution { get; set; }
        public virtual DbSet<TP_JointPictureDistribution> TP_JointPictureDistribution { get; set; }
        public virtual DbSet<TP_JointBag> TP_JointBag { get; set; }
        public virtual DbSet<TP_JointException> TP_JointException { get; set; }

        #endregion
    }
}
