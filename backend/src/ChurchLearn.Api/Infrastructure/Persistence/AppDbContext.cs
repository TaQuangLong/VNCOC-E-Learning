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
    }
}
