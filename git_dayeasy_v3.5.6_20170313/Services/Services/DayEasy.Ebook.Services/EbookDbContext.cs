using System.Data.Entity;
using DayEasy.Ebook.Contracts.Models;
using DayEasy.EntityFramework;

namespace DayEasy.Ebook.Services
{
    public class EbookDbContext : CodeFirstDbContext
    {
        public EbookDbContext()
            : base(GetConnection("ebook"), true)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            Database.SetInitializer<EbookDbContext>(null);
        }

        public virtual DbSet<TE_BookDetail> TE_BookDetail { get; set; }
        public virtual DbSet<TE_BookResult> TE_BookResult { get; set; }
        public virtual DbSet<TE_Chapter> TE_Chapter { get; set; }
        public virtual DbSet<TE_ErrorCorrection> TE_ErrorCorrection { get; set; }
        public virtual DbSet<TE_ErrorStatistics> TE_ErrorStatistics { get; set; }
        public virtual DbSet<TE_LearningMemo> TE_LearningMemo { get; set; }
        public virtual DbSet<TE_LearningMemoUsage> TE_LearningMemoUsage { get; set; }
        public virtual DbSet<TE_MemoReview> TE_MemoReview { get; set; }
        public virtual DbSet<TE_Question> TE_Question { get; set; }
        public virtual DbSet<TE_SmallQuestion> TE_SmallQuestion { get; set; }
        public virtual DbSet<TE_StudentGroup> TE_StudentGroup { get; set; }
        public virtual DbSet<TE_StudentGroupMember> TE_StudentGroupMember { get; set; }
        public virtual DbSet<TE_TextBook> TE_TextBook { get; set; }
        public virtual DbSet<TE_TextBookContent> TE_TextBookContent { get; set; }
        public virtual DbSet<TE_TextBookProcess> TE_TextBookProcess { get; set; }
        public virtual DbSet<TE_TextBookUsage> TE_TextBookUsage { get; set; }
        public virtual DbSet<TE_UserMessage> TE_UserMessage { get; set; }
    }
}
