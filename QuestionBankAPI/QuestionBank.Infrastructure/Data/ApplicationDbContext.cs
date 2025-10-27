using Microsoft.EntityFrameworkCore;
using QuestionBank.Domain.Entities;

namespace QuestionBank.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users => Set<User>();
    public DbSet<KnowledgePoint> KnowledgePoints => Set<KnowledgePoint>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionKnowledgePoint> QuestionKnowledgePoints => Set<QuestionKnowledgePoint>();
    public DbSet<Paper> Papers => Set<Paper>();
    public DbSet<PaperQuestion> PaperQuestions => Set<PaperQuestion>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamAttempt> ExamAttempts => Set<ExamAttempt>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<WrongQuestion> WrongQuestions => Set<WrongQuestion>();
    public DbSet<FavoriteQuestion> FavoriteQuestions => Set<FavoriteQuestion>();
    public DbSet<QuestionNote> QuestionNotes => Set<QuestionNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User配置
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20);

            // 全局查询过滤器 - 软删除
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // KnowledgePoint配置
        modelBuilder.Entity<KnowledgePoint>(entity =>
        {
            entity.ToTable("knowledge_points");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Question配置
        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("questions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CorrectAnswer).IsRequired();
            entity.Property(e => e.Chapter).HasMaxLength(200);

            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.MaterialQuestion)
                .WithMany(e => e.SubQuestions)
                .HasForeignKey(e => e.MaterialQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // QuestionKnowledgePoint配置 (多对多)
        modelBuilder.Entity<QuestionKnowledgePoint>(entity =>
        {
            entity.ToTable("question_knowledge_points");
            entity.HasKey(e => new { e.QuestionId, e.KnowledgePointId });

            entity.HasOne(e => e.Question)
                .WithMany(e => e.QuestionKnowledgePoints)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.KnowledgePoint)
                .WithMany(e => e.QuestionKnowledgePoints)
                .HasForeignKey(e => e.KnowledgePointId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Paper配置
        modelBuilder.Entity<Paper>(entity =>
        {
            entity.ToTable("papers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // PaperQuestion配置 (多对多)
        modelBuilder.Entity<PaperQuestion>(entity =>
        {
            entity.ToTable("paper_questions");
            entity.HasKey(e => new { e.PaperId, e.QuestionId });

            entity.HasOne(e => e.Paper)
                .WithMany(e => e.PaperQuestions)
                .HasForeignKey(e => e.PaperId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Question)
                .WithMany(e => e.PaperQuestions)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Exam配置
        modelBuilder.Entity<Exam>(entity =>
        {
            entity.ToTable("exams");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.HasOne(e => e.Paper)
                .WithMany(e => e.Exams)
                .HasForeignKey(e => e.PaperId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ExamAttempt配置
        modelBuilder.Entity<ExamAttempt>(entity =>
        {
            entity.ToTable("exam_attempts");
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Exam)
                .WithMany(e => e.ExamAttempts)
                .HasForeignKey(e => e.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(e => e.ExamAttempts)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Answer配置
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.ToTable("answers");
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.ExamAttempt)
                .WithMany(e => e.Answers)
                .HasForeignKey(e => e.ExamAttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Question)
                .WithMany(e => e.Answers)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Grader)
                .WithMany()
                .HasForeignKey(e => e.GradedBy)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // WrongQuestion配置
        modelBuilder.Entity<WrongQuestion>(entity =>
        {
            entity.ToTable("wrong_questions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.QuestionId }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany(e => e.WrongQuestions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Question)
                .WithMany()
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // FavoriteQuestion配置
        modelBuilder.Entity<FavoriteQuestion>(entity =>
        {
            entity.ToTable("favorite_questions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.QuestionId }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany(e => e.FavoriteQuestions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Question)
                .WithMany()
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // QuestionNote配置
        modelBuilder.Entity<QuestionNote>(entity =>
        {
            entity.ToTable("question_notes");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.QuestionId });

            entity.HasOne(e => e.User)
                .WithMany(e => e.QuestionNotes)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Question)
                .WithMany()
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}
