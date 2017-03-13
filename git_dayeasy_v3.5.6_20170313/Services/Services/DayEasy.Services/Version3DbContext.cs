using System.Data.Entity;
using DayEasy.Contracts.Models;
using DayEasy.EntityFramework;

namespace DayEasy.Services
{
    public class Version3DbContext : CodeFirstDbContext
    {
        public Version3DbContext()
            : base(GetConnection("dayeasy_v3"), true)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            Database.SetInitializer<Version3DbContext>(null);
        }


        #region 圈子

        public virtual DbSet<TG_ApplyRecord> TG_ApplyRecord { get; set; }
        public virtual DbSet<TG_Class> TG_Class { get; set; }
        public virtual DbSet<TG_Group> TG_Group { get; set; }
        public virtual DbSet<TG_Member> TG_Member { get; set; }
        public virtual DbSet<TG_Colleague> TG_Colleague { get; set; }
        public virtual DbSet<TG_Resources> TG_Resources { get; set; }
        public virtual DbSet<TG_Share> TG_Share { get; set; }

        #endregion

        public virtual DbSet<TU_StudentParents> TU_StudentParents { get; set; }

        public virtual DbSet<TS_Agency> TS_Agency { get; set; }
        public virtual DbSet<TS_CodePool> TS_CodePool { get; set; }

        public virtual DbSet<TM_GroupDynamic> TM_GroupDynamic { get; set; }


        #region 帖子

        public virtual DbSet<TB_Reply> TB_Reply { get; set; }
        public virtual DbSet<TB_Topic> TB_Topic { get; set; }
        public virtual DbSet<TB_Vote> TB_Vote { get; set; }
        public virtual DbSet<TB_Praise> TB_Praise { get; set; }
        public virtual DbSet<TB_VoteOption> TB_VoteOption { get; set; }
        public virtual DbSet<TB_VoteUser> TB_VoteUser { get; set; }

        #endregion

        public virtual DbSet<TS_SchoolBook> TS_SchoolBook { get; set; }
        public virtual DbSet<TS_SchoolBookChapter> TS_SchoolBookChapter { get; set; }

        #region 大型考试

        public virtual DbSet<TE_Examination> TE_Examination { get; set; }
        public virtual DbSet<TE_StudentScore> TE_StudentScore { get; set; }
        public virtual DbSet<TE_StudentSubjectScore> TE_StudentSubjectScore { get; set; }
        public virtual DbSet<TE_SubjectScore> TE_SubjectScore { get; set; }
        public virtual DbSet<TE_UnionReport> TE_UnionReport { get; set; }

        #endregion

        public virtual DbSet<TA_TeacherGod> TA_TeacherGod { get; set; } 
    }
}
