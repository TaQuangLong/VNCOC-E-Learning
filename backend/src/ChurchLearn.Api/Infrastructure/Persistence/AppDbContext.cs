using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser>(options)
{
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<QuizAttemptAnswer> QuizAttemptAnswers => Set<QuizAttemptAnswer>();
    public DbSet<Discussion> Discussions => Set<Discussion>();
    public DbSet<LearningPath> LearningPaths => Set<LearningPath>();
    public DbSet<LearningPathSection> LearningPathSections => Set<LearningPathSection>();
    public DbSet<LearningPathCourse> LearningPathCourses => Set<LearningPathCourse>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("public");

        builder.Entity<Author>(a =>
        {
            a.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Course>(c =>
        {
            c.Property(x => x.Status)
             .HasConversion<string>()
             .HasMaxLength(20);

            c.HasIndex(x => x.Slug).IsUnique();

            c.HasOne(x => x.Author)
             .WithMany(a => a.Courses)
             .HasForeignKey(x => x.AuthorId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Lesson>(l =>
        {
            l.Property(x => x.ContentType)
             .HasConversion<string>()
             .HasMaxLength(10);

            l.HasIndex(x => new { x.CourseId, x.OrderIndex });

            l.HasOne(x => x.Course)
             .WithMany(c => c.Lessons)
             .HasForeignKey(x => x.CourseId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Resource>(r =>
        {
            r.HasIndex(x => x.LessonId);

            r.HasOne(x => x.Lesson)
             .WithMany(l => l.Resources)
             .HasForeignKey(x => x.LessonId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Enrollment>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.CourseId }).IsUnique();
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.CourseId);

            e.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Course)
             .WithMany()
             .HasForeignKey(x => x.CourseId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<LessonProgress>(lp =>
        {
            lp.HasIndex(x => new { x.UserId, x.LessonId }).IsUnique();
            lp.HasIndex(x => new { x.UserId, x.CourseId });

            lp.HasOne(x => x.User)
              .WithMany()
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Cascade);

            lp.HasOne(x => x.Lesson)
              .WithMany()
              .HasForeignKey(x => x.LessonId)
              .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Quiz>(q =>
        {
            q.HasIndex(x => x.LessonId).IsUnique();

            q.HasOne(x => x.Lesson)
             .WithOne(l => l.Quiz)
             .HasForeignKey<Quiz>(x => x.LessonId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Question>(q =>
        {
            q.Property(x => x.Type)
             .HasConversion<string>()
             .HasMaxLength(20);

            q.HasOne(x => x.Quiz)
             .WithMany(qz => qz.Questions)
             .HasForeignKey(x => x.QuizId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AnswerOption>(a =>
        {
            a.HasOne(x => x.Question)
             .WithMany(q => q.Options)
             .HasForeignKey(x => x.QuestionId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<QuizAttempt>(qa =>
        {
            qa.HasIndex(x => new { x.UserId, x.QuizId });

            qa.HasOne(x => x.Quiz)
              .WithMany(q => q.Attempts)
              .HasForeignKey(x => x.QuizId)
              .OnDelete(DeleteBehavior.Cascade);

            qa.HasOne(x => x.User)
              .WithMany()
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<QuizAttemptAnswer>(qaa =>
        {
            qaa.HasIndex(x => x.QuizAttemptId);

            qaa.HasOne(x => x.QuizAttempt)
               .WithMany(a => a.Answers)
               .HasForeignKey(x => x.QuizAttemptId)
               .OnDelete(DeleteBehavior.Cascade);

            qaa.HasOne(x => x.Question)
               .WithMany()
               .HasForeignKey(x => x.QuestionId)
               .OnDelete(DeleteBehavior.Restrict);

            qaa.HasOne(x => x.SelectedAnswerOption)
               .WithMany()
               .HasForeignKey(x => x.SelectedAnswerOptionId)
               .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Discussion>(d =>
        {
            d.HasIndex(x => x.LessonId);
            d.HasIndex(x => x.UserId);
            d.HasIndex(x => x.ParentDiscussionId);

            d.HasOne(x => x.Lesson)
             .WithMany()
             .HasForeignKey(x => x.LessonId)
             .OnDelete(DeleteBehavior.Cascade);

            d.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            d.HasOne(x => x.ParentDiscussion)
             .WithMany(x => x.Replies)
             .HasForeignKey(x => x.ParentDiscussionId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<LearningPath>(lp =>
        {
            lp.Property(x => x.Status)
              .HasConversion<string>()
              .HasMaxLength(20);

            lp.HasIndex(x => x.Slug).IsUnique();
        });

        builder.Entity<LearningPathSection>(lps =>
        {
            lps.HasIndex(x => new { x.LearningPathId, x.OrderIndex }).IsUnique();

            lps.HasOne(x => x.LearningPath)
               .WithMany(lp => lp.Sections)
               .HasForeignKey(x => x.LearningPathId)
               .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<LearningPathCourse>(lpc =>
        {
            lpc.HasIndex(x => new { x.LearningPathId, x.CourseId }).IsUnique();
            lpc.HasIndex(x => new { x.LearningPathSectionId, x.OrderIndex }).IsUnique();
            lpc.HasIndex(x => x.CourseId);

            lpc.HasOne(x => x.LearningPath)
               .WithMany(lp => lp.Courses)
               .HasForeignKey(x => x.LearningPathId)
               .OnDelete(DeleteBehavior.Cascade);

            lpc.HasOne(x => x.LearningPathSection)
               .WithMany(lps => lps.Courses)
               .HasForeignKey(x => x.LearningPathSectionId)
               .OnDelete(DeleteBehavior.Cascade);

            lpc.HasOne(x => x.Course)
               .WithMany(c => c.LearningPathCourses)
               .HasForeignKey(x => x.CourseId)
               .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
