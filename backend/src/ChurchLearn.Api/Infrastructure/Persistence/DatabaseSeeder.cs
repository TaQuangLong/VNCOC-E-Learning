using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace ChurchLearn.Api.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        string[] roles = [AppRoles.Student, AppRoles.Admin, AppRoles.SuperAdmin];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var email = config["SuperAdmin:Email"] ?? "superadmin@churchlearn.local";
        var password = config["SuperAdmin:Password"] ?? "Admin@123456!";

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is null)
        {
            var superAdmin = new AppUser
            {
                UserName = email,
                Email = email,
                DisplayName = "Super Admin",
                IsActive = true,
                EmailConfirmed = true,
            };
            var result = await userManager.CreateAsync(superAdmin, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(superAdmin, AppRoles.SuperAdmin);
        }
        else
        {
            // Ensure account is active and not locked out
            if (!existingUser.IsActive)
            {
                existingUser.IsActive = true;
                await userManager.UpdateAsync(existingUser);
            }

            if (existingUser.LockoutEnd.HasValue && existingUser.LockoutEnd > DateTimeOffset.UtcNow)
                await userManager.SetLockoutEndDateAsync(existingUser, null);

            await userManager.ResetAccessFailedCountAsync(existingUser);

            if (!await userManager.IsInRoleAsync(existingUser, AppRoles.SuperAdmin))
                await userManager.AddToRoleAsync(existingUser, AppRoles.SuperAdmin);
        }

        // Course seeding — Development, Staging, or when demo data flag is set (e.g. fly.io demo)
        var seedDemoData = config.GetValue<bool>("Seed:DemoData");
        if (env.IsDevelopment() || env.IsStaging() || seedDemoData)
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await SeedCoursesAsync(db);
        }
    }

    private static async Task SeedCoursesAsync(AppDbContext db)
    {
        // ── Authors ──────────────────────────────────────────────────────────
        var authorSeeds = new[]
        {
            new { Name = "Pastor David Kim",       Bio = "Lead pastor with 20 years in spiritual formation and discipleship.",         AvatarUrl = "https://images.unsplash.com/photo-1599566150163-29194dcaad36?w=200&q=80" },
            new { Name = "Dr. Sarah Chen",         Bio = "Scholar in systematic theology and New Testament studies.",                  AvatarUrl = "https://images.unsplash.com/photo-1494790108755-2616b612b786?w=200&q=80" },
            new { Name = "Rev. Michael Thompson",  Bio = "Church historian, ordained minister, and leadership coach.",                 AvatarUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=200&q=80" },
            new { Name = "Dr. Grace Williams",     Bio = "Licensed family counselor and relationship ministry director.",              AvatarUrl = "https://images.unsplash.com/photo-1580489944761-15a19d654956?w=200&q=80" },
            new { Name = "Pastor James Carter",    Bio = "Global missions director and evangelism trainer.",                          AvatarUrl = "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=200&q=80" },
        };

        foreach (var a in authorSeeds)
        {
            if (!await db.Authors.AnyAsync(x => x.Name == a.Name))
                db.Authors.Add(new Author { Name = a.Name, Bio = a.Bio, AvatarUrl = a.AvatarUrl });
        }
        await db.SaveChangesAsync();

        var authorMap = await db.Authors.ToDictionaryAsync(a => a.Name);
        int davidId   = authorMap["Pastor David Kim"].Id;
        int sarahId   = authorMap["Dr. Sarah Chen"].Id;
        int michaelId = authorMap["Rev. Michael Thompson"].Id;
        int graceId   = authorMap["Dr. Grace Williams"].Id;
        int jamesId   = authorMap["Pastor James Carter"].Id;

        // ── Courses ───────────────────────────────────────────────────────────
        var courseSeeds = new[]
        {
            // Spiritual Growth
            new {
                Title = "Foundations of Spiritual Growth",
                Slug  = "foundations-of-spiritual-growth",
                ShortDescription = "Discover the core habits and practices that fuel a vibrant spiritual life.",
                Description = "This beginner course walks you through the essential disciplines of the Christian faith — prayer, Scripture reading, worship, and community — to help you build a strong spiritual foundation.",
                ThumbnailUrl = "https://images.unsplash.com/photo-1519985459-d9a63e76d0d0?w=800&q=80",
                Category = "Spiritual Growth", Level = "Beginner", AuthorId = davidId,
            },
            new {
                Title = "Deep Roots: Advanced Spiritual Disciplines",
                Slug  = "deep-roots-advanced-spiritual-disciplines",
                ShortDescription = "Go deeper with fasting, solitude, contemplative prayer, and spiritual direction.",
                Description = "Designed for mature believers who want to move beyond basics and develop a rich interior life through advanced spiritual disciplines drawn from centuries of Christian tradition.",
                ThumbnailUrl = "https://images.unsplash.com/photo-1506126613408-eca07ce68773?w=800&q=80",
                Category = "Spiritual Growth", Level = "Advanced", AuthorId = davidId,
            },

            // Theology & Doctrine
            new {
                Title = "Core Christian Doctrines",
                Slug  = "core-christian-doctrines",
                ShortDescription = "An accessible introduction to what Christians believe and why it matters.",
                Description = "Explore the foundational doctrines of Christianity: the Trinity, the nature of Christ, salvation, Scripture, and the Church — taught in plain language for those new to theology.",
                ThumbnailUrl = "https://images.unsplash.com/photo-1481627834876-b7833e8f5dc1?w=800&q=80",
                Category = "Theology & Doctrine", Level = "Beginner", AuthorId = sarahId,
            },
            new {
                Title = "Systematic Theology Essentials",
                Slug  = "systematic-theology-essentials",
                ShortDescription = "A structured journey through the major themes of Christian theology.",
                Description = "From bibliology to eschatology, this intermediate course gives a comprehensive overview of systematic theology, helping students think carefully and biblically about every area of the faith.",
                ThumbnailUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=800&q=80",
                Category = "Theology & Doctrine", Level = "Intermediate", AuthorId = sarahId,
            },

            // Church History
            new {
                Title = "The Early Church: 100–500 AD",
                Slug  = "the-early-church-100-500-ad",
                ShortDescription = "Trace the dramatic growth of Christianity from the apostles to Augustine.",
                Description = "Examine the councils, creeds, persecutions, and key figures that shaped the early church — including Ignatius, Polycarp, Athanasius, and Augustine. Understand how the church defined orthodoxy.",
                ThumbnailUrl = "https://images.unsplash.com/photo-1461896836934-ffe607ba8211?w=800&q=80",
                Category = "Church History", Level = "Intermediate", AuthorId = michaelId,
            },
            new {
                Title = "Reformation & Modern Christianity",
                Slug  = "reformation-and-modern-christianity",
                ShortDescription = "From Luther's 95 Theses to today — how the church was reformed and transformed.",
                Description = "An advanced study of the Protestant Reformation, Counter-Reformation, the Great Awakenings, and major movements that have shaped Christianity in the 20th and 21st centuries.",
                ThumbnailUrl = "https://images.unsplash.com/photo-1548438294-1ad5d5f4f063?w=800&q=80",
                Category = "Church History", Level = "Advanced", AuthorId = michaelId,
            },

            // Relationship & Marriage
            new {
                Title = "Building a Christ-Centered Marriage",
                Slug  = "building-a-christ-centered-marriage",
                ShortDescription = "Biblical principles for couples seeking a godly, thriving marriage.",
                Description = "Whether newly married or long-time partners, this course offers practical tools grounded in Scripture for communication, conflict resolution, intimacy, and building a shared spiritual life.",
                ThumbnailUrl = "https://images.unsplash.com/photo-1511632765153-994ad063cbb7?w=800&q=80",
                Category = "Relationship & Marriage", Level = "Beginner", AuthorId = graceId,
            },
            new {
                Title = "Healthy Relationships in Community",
                Slug  = "healthy-relationships-in-community",
                ShortDescription = "How to love, forgive, and build genuine connections within the church family.",
                Description = "Explore what the Bible teaches about community, friendship, forgiveness, and accountability. Learn practical skills for navigating conflict and cultivating deep, Christ-honoring relationships.",
                ThumbnailUrl = "https://images.unsplash.com/photo-1529156069898-49953e39b3ac?w=800&q=80",
                Category = "Relationship & Marriage", Level = "Intermediate", AuthorId = graceId,
            },

            // Leadership & Ministry
            new {
                Title = "Servant Leadership in the Church",
                Slug  = "servant-leadership-in-the-church",
                ShortDescription = "Lead like Jesus — an introduction to servant-hearted ministry.",
                Description = "Based on the model of Christ, this beginner course explores the principles of servant leadership, discovering your gifts, and stepping faithfully into a role of influence within your church.",
                ThumbnailUrl = "https://images.unsplash.com/photo-1515187029135-18ee286d815b?w=800&q=80",
                Category = "Leadership & Ministry", Level = "Beginner", AuthorId = michaelId,
            },
            new {
                Title = "Growing Your Ministry Team",
                Slug  = "growing-your-ministry-team",
                ShortDescription = "Equip, empower, and multiply leaders in your ministry context.",
                Description = "An intermediate course on team-building, delegation, discipleship pipelines, and creating a culture of empowerment. Ideal for ministry leaders, small group leaders, and department heads.",
                ThumbnailUrl = "https://images.unsplash.com/photo-1522071820081-009f0129c71c?w=800&q=80",
                Category = "Leadership & Ministry", Level = "Intermediate", AuthorId = michaelId,
            },

            // Bible Study – Old Testament
            new {
                Title = "Walking Through the Old Testament",
                Slug  = "walking-through-the-old-testament",
                ShortDescription = "A panoramic survey of the Hebrew Scriptures from Genesis to Malachi.",
                Description = "Journey through the narrative arc of the Old Testament — creation, covenant, law, prophecy, and wisdom — discovering how every part points forward to Jesus Christ.",
                ThumbnailUrl = "https://images.unsplash.com/photo-1476055439777-977cdf3a5699?w=800&q=80",
                Category = "Bible Study (Old Testament)", Level = "Beginner", AuthorId = sarahId,
            },

            // Bible Study – New Testament
            new {
                Title = "Life of Christ: The Four Gospels",
                Slug  = "life-of-christ-the-four-gospels",
                ShortDescription = "An in-depth look at the life, teachings, death, and resurrection of Jesus.",
                Description = "Study Matthew, Mark, Luke, and John side-by-side to discover the full picture of Jesus' life and ministry, understanding the unique perspective each gospel writer brings.",
                ThumbnailUrl = "https://images.unsplash.com/photo-1473042904451-00171c69419d?w=800&q=80",
                Category = "Bible Study (New Testament)", Level = "Beginner", AuthorId = sarahId,
            },

            // Prayer & Worship
            new {
                Title = "The Art of Prayer",
                Slug  = "the-art-of-prayer",
                ShortDescription = "Learn to pray with depth, consistency, and confidence.",
                Description = "From the Lord's Prayer to intercession, this course teaches different forms of prayer, how to develop a daily prayer habit, and how to lead others in corporate worship and prayer.",
                ThumbnailUrl = "https://images.unsplash.com/photo-1508672019048-805c876b67e2?w=800&q=80",
                Category = "Prayer & Worship", Level = "Beginner", AuthorId = davidId,
            },

            // Evangelism & Missions
            new {
                Title = "Sharing Your Faith",
                Slug  = "sharing-your-faith",
                ShortDescription = "Practical tools for sharing the gospel naturally in everyday life.",
                Description = "Overcome fear and build confidence to share your faith in conversations, relationships, and your community. Covers evangelism methods, apologetics basics, and cross-cultural sensitivity.",
                ThumbnailUrl = "https://images.unsplash.com/photo-1488521787816-2f5e7deb5462?w=800&q=80",
                Category = "Evangelism & Missions", Level = "Intermediate", AuthorId = jamesId,
            },

            // Youth & Family
            new {
                Title = "Raising Faith-Filled Families",
                Slug  = "raising-faith-filled-families",
                ShortDescription = "Equip parents to disciple children and build a Christ-centered home.",
                Description = "Practical, biblically grounded guidance for parents on how to have faith conversations, establish family rhythms of prayer and Scripture, and model the Christian life at every stage of parenting.",
                ThumbnailUrl = "https://images.unsplash.com/photo-1536746803623-cef87080bfc8?w=800&q=80",
                Category = "Youth & Family", Level = "Beginner", AuthorId = graceId,
            },
        };

        foreach (var c in courseSeeds)
        {
            if (!await db.Courses.AnyAsync(x => x.Slug == c.Slug))
            {
                db.Courses.Add(new Course
                {
                    Title            = c.Title,
                    Slug             = c.Slug,
                    ShortDescription = c.ShortDescription,
                    Description      = c.Description,
                    ThumbnailUrl     = c.ThumbnailUrl,
                    Category         = c.Category,
                    Level            = c.Level,
                    Language         = "English",
                    AuthorId         = c.AuthorId,
                    Status           = CourseStatus.Published,
                });
            }
        }
        await db.SaveChangesAsync();
    }
}
