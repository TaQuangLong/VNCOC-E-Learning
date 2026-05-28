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
            await SeedLessonsAsync(db, CancellationToken.None);
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

    private static async Task SeedLessonsAsync(AppDbContext db, CancellationToken ct)
    {
        // (slug, lessons[]) — 6 video lessons per course using VNCOC YouTube content
        // DurationMinutes = round(durationSeconds / 60)
        var courseLessons = new (string Slug, (int OrderIndex, string Title, string Description, string VideoId, int DurationMinutes, bool IsPreview)[] Lessons)[]
        {
            (
                "foundations-of-spiritual-growth",
                [
                    (1, "Introduction to Spiritual Disciplines",   "A short overview of the journey of faith — 31 years of VNCOC and the spiritual disciplines that sustain it.",  "EQ84qkq2Q6s",  7, true),
                    (2, "The Role of Prayer in Growth",            "Explores how prayer transforms the believer and serves as the foundation of a growing spiritual life.",         "O0MyKgMzWBk", 86, false),
                    (3, "Reading Scripture with Understanding",    "Discover how learning from followers of Jesus deepens our engagement with the Word of God.",                  "4l85ZMZXaG8", 61, false),
                    (4, "Worship as a Way of Life",                "Understand what it means to live in holiness and worship God through everyday choices.",                      "NJHh5SRtF7g", 49, false),
                    (5, "Finding Community in the Church",         "Jesus as the source of our fellowship — how the church is designed for mutual growth and belonging.",          "LpcXBQw1f_s", 51, false),
                    (6, "Living Out Your Faith Daily",             "A Sunday worship reflection on practical faith in action and the rhythms of the Christian life.",              "pQ9CKq4xl9E", 38, false),
                ]
            ),
            (
                "deep-roots-advanced-spiritual-disciplines",
                [
                    (1, "Fasting and Solitude",                         "An exploration of God's love as the grounding of all spiritual disciplines, including fasting and solitude.",  "Saw84fmOBBs", 53, true),
                    (2, "Contemplative Prayer",                         "How transformative questions deepen our inner life and contemplative prayer practice.",                         "O0MyKgMzWBk", 86, false),
                    (3, "Spiritual Direction and Accountability",       "Learning to wait on God when success feels distant — the discipline of patient trust.",                         "CtjdTNuzTzc", 57, false),
                    (4, "Sabbath and Rest as Discipline",               "Loving yourself rightly as God's image-bearer — the theology of Sabbath rest and self-care.",                   "yaf4uGpe3gI", 50, false),
                    (5, "Service and Humility",                         "Stones of remembrance — how gratitude and humility fuel a life of service.",                                     "DTDdX3F7rao", 39, false),
                    (6, "Spiritual Milestones and Ebenezer Stones",     "Drawing lessons from the followers of Jesus on marking spiritual milestones in your walk with God.",             "4l85ZMZXaG8", 61, false),
                ]
            ),
            (
                "core-christian-doctrines",
                [
                    (1, "The Trinity Explained",              "God is holy — a foundational look at the Trinity through the lens of worship and holy living.",                 "NJHh5SRtF7g", 49, true),
                    (2, "The Nature of Christ",               "Jesus as the source of our fellowship — exploring the full humanity and divinity of Christ.",                   "LpcXBQw1f_s", 51, false),
                    (3, "The Doctrine of Salvation",          "God is love — understanding salvation through the character and redemptive work of a loving God.",              "Saw84fmOBBs", 53, false),
                    (4, "The Authority of Scripture",         "How learning from followers of Jesus reveals the authority and sufficiency of Scripture.",                       "4l85ZMZXaG8", 61, false),
                    (5, "The Church and Its Purpose",         "A worship service reflection on the gathered church as God's redemptive community in the world.",               "pQ9CKq4xl9E", 38, false),
                    (6, "The Final Hope — Eschatology Basics","Stones of remembrance — grounding our eschatological hope in God's faithfulness throughout history.",           "DTDdX3F7rao", 39, false),
                ]
            ),
            (
                "systematic-theology-essentials",
                [
                    (1, "Bibliology — The Word of God",                "An in-depth look at Scripture's authority, inspiration, and sufficiency through the example of Jesus' followers.", "4l85ZMZXaG8", 61, true),
                    (2, "Theology Proper — Who Is God?",               "God is holy — a systematic study of the divine attributes and what it means to worship a holy God.",              "NJHh5SRtF7g", 49, false),
                    (3, "Christology — The Person of Christ",          "Jesus as source of our fellowship — a systematic treatment of the incarnation and two natures of Christ.",         "LpcXBQw1f_s", 51, false),
                    (4, "Soteriology — The Doctrine of Salvation",     "Learning to wait on God — a systematic study of grace, faith, justification, and perseverance.",                   "CtjdTNuzTzc", 57, false),
                    (5, "Ecclesiology — The Doctrine of the Church",   "A worship service exploration of the church as the body of Christ and its purpose in God's redemptive plan.",      "pQ9CKq4xl9E", 38, false),
                    (6, "Eschatology — Last Things",                   "Stones of remembrance — a systematic overview of last things, resurrection, and the coming Kingdom.",              "DTDdX3F7rao", 39, false),
                ]
            ),
            (
                "the-early-church-100-500-ad",
                [
                    (1, "The Apostolic Fathers",                    "One man's journey of following Christ — tracing the faithfulness of the apostolic fathers who carried on the mission.",  "FOgkX2wqGuQ",  3, true),
                    (2, "The First Persecutions",                   "Called back from Japan to serve God — parallels between early church perseverance and modern missionary sacrifice.",      "rbxx65C2f_U",  4, false),
                    (3, "Defining Orthodoxy — The Councils",        "31 years of faithful witness — how the early councils defined orthodoxy and protected the faith once delivered.",         "EQ84qkq2Q6s",  7, false),
                    (4, "Ignatius, Polycarp, and the Martyrs",      "Transformed by questions — how the martyr testimonies of Ignatius and Polycarp shaped the early church's theology.",    "O0MyKgMzWBk", 86, false),
                    (5, "Athanasius and the Arian Controversy",     "God is love — exploring Athanasius' defense of Trinitarian doctrine against Arianism.",                                  "Saw84fmOBBs", 53, false),
                    (6, "Augustine and Christian Thought",          "Learning from followers of Jesus — Augustine's life and how his theology shaped Western Christianity.",                  "4l85ZMZXaG8", 61, false),
                ]
            ),
            (
                "reformation-and-modern-christianity",
                [
                    (1, "Luther and the 95 Theses",              "31 years of VNCOC — a reflection on how one act of conviction, like Luther's, can change the course of history.",    "EQ84qkq2Q6s",  7, true),
                    (2, "Calvin and Reformed Theology",          "Stones of remembrance — tracing how God's faithfulness runs through Reformed theology and covenant history.",          "DTDdX3F7rao", 39, false),
                    (3, "The Counter-Reformation",               "A worship service reflection on renewal, reform, and how the Spirit moves within institutional Christianity.",         "pQ9CKq4xl9E", 38, false),
                    (4, "The Great Awakenings",                  "When waiting for success — how seasons of spiritual drought preceded the great revivals of the 18th century.",         "CtjdTNuzTzc", 57, false),
                    (5, "Missions and the Modern Church",        "Jesus — source of our fellowship — how mission movements connected churches across the world.",                        "LpcXBQw1f_s", 51, false),
                    (6, "Christianity in the 21st Century",      "Loving yourself rightly — how the modern church must reckon with identity, culture, and the call to discipleship.",    "yaf4uGpe3gI", 50, false),
                ]
            ),
            (
                "building-a-christ-centered-marriage",
                [
                    (1, "God's Design for Marriage",              "Jesus — source of our fellowship — how Christ's covenant love is the pattern for Christian marriage.",          "LpcXBQw1f_s", 51, true),
                    (2, "Communication and Listening Well",       "Loving yourself rightly — how healthy self-understanding enables deep listening and honest communication.",      "yaf4uGpe3gI", 50, false),
                    (3, "Conflict Resolution in Marriage",        "Transformed by questions — how asking better questions de-escalates conflict and restores connection.",         "O0MyKgMzWBk", 86, false),
                    (4, "Building Spiritual Intimacy Together",   "God is holy — how a shared pursuit of holiness and worship deepens a couple's spiritual intimacy.",            "NJHh5SRtF7g", 49, false),
                    (5, "Forgiveness and Grace in Marriage",      "God is love — applying the grace and forgiveness of the gospel to the everyday challenges of marriage.",       "Saw84fmOBBs", 53, false),
                    (6, "Praying Together as a Couple",           "A Sunday worship reflection on the spiritual power of a husband and wife praying side-by-side.",              "pQ9CKq4xl9E", 38, false),
                ]
            ),
            (
                "healthy-relationships-in-community",
                [
                    (1, "What the Bible Says About Community",      "Jesus — source of our fellowship — a biblical foundation for authentic community and belonging in the church.",  "LpcXBQw1f_s", 51, true),
                    (2, "The Art of Listening and Empathy",         "Loving yourself rightly — how inner wholeness frees us to listen with empathy and without judgment.",             "yaf4uGpe3gI", 50, false),
                    (3, "Accountability and Genuine Friendship",    "When waiting for success — how seasons of vulnerability forge the deep accountability of true friendship.",       "CtjdTNuzTzc", 57, false),
                    (4, "Forgiveness — Releasing the Debt",         "God is love — releasing bitterness and extending the same radical forgiveness we have received from Christ.",      "Saw84fmOBBs", 53, false),
                    (5, "Navigating Conflict Biblically",           "Stones of remembrance — how remembering God's faithfulness gives us courage to address conflict biblically.",     "DTDdX3F7rao", 39, false),
                    (6, "Building Lasting Bonds of Trust",          "Learning from followers of Jesus — how lives of integrity and consistency build unshakeable trust.",              "4l85ZMZXaG8", 61, false),
                ]
            ),
            (
                "servant-leadership-in-the-church",
                [
                    (1, "Jesus the Servant Leader",                          "Jesus — source of our fellowship — the towel and basin as the defining image of servant leadership.",      "LpcXBQw1f_s", 51, true),
                    (2, "Discovering Your Spiritual Gifts",                  "God is holy — how a posture of worship and surrender reveals the spiritual gifts God has placed in you.",  "NJHh5SRtF7g", 49, false),
                    (3, "Leading with Humility",                             "Learning from followers of Jesus — what the disciples' examples teach us about leading without ego.",      "4l85ZMZXaG8", 61, false),
                    (4, "Serving Without Burning Out",                       "When waiting for success — setting a sustainable pace for ministry and learning the rhythm of rest.",      "CtjdTNuzTzc", 57, false),
                    (5, "Integrity and Character in Leadership",             "Loving yourself rightly — how self-knowledge and character are the bedrock of trustworthy leadership.",   "yaf4uGpe3gI", 50, false),
                    (6, "Developing the Next Generation of Leaders",         "A worship service reflection on multiplication, mentorship, and leaving a legacy of servant leaders.",   "pQ9CKq4xl9E", 38, false),
                ]
            ),
            (
                "growing-your-ministry-team",
                [
                    (1, "Vision Casting for Your Ministry",           "31 years of VNCOC — how a clear, God-given vision sustains a ministry team through challenges and growth.",    "EQ84qkq2Q6s",  7, true),
                    (2, "Identifying and Recruiting Leaders",         "One man's journey of following Christ — recognising God-given potential and calling people into leadership.",   "rbxx65C2f_U",  4, false),
                    (3, "Delegation and Empowerment",                 "Transformed by questions — using powerful questions to help team members discover their own solutions.",        "O0MyKgMzWBk", 86, false),
                    (4, "Building a Team Culture of Grace",           "God is love — creating a ministry environment where grace is the norm and failure is a learning opportunity.",  "Saw84fmOBBs", 53, false),
                    (5, "Discipleship Pipelines That Work",           "Stones of remembrance — building reproducible pathways that move people from attendee to disciple-maker.",      "DTDdX3F7rao", 39, false),
                    (6, "Evaluating and Growing Your Ministry",       "Learning from followers of Jesus — how honest evaluation and adjustment keep a ministry healthy and fruitful.", "4l85ZMZXaG8", 61, false),
                ]
            ),
            (
                "walking-through-the-old-testament",
                [
                    (1, "Creation, Fall, and the Promise",                      "31 years of covenant faithfulness — tracing God's promise from Eden through the entire Old Testament.", "EQ84qkq2Q6s",  7, true),
                    (2, "Abraham and the Covenant",                             "One man's journey — how Abraham's call mirrors every believer's step of faith into the unknown.",        "rbxx65C2f_U",  4, false),
                    (3, "Moses, the Law, and the Exodus",                       "Transformed by questions — how the Exodus narrative answers the deepest questions of identity and freedom.", "O0MyKgMzWBk", 86, false),
                    (4, "The Psalms — Songs of the Heart",                      "God is holy — entering the Psalms as Israel's hymnal of honest worship before a holy God.",              "NJHh5SRtF7g", 49, false),
                    (5, "The Prophets — God's Voice to His People",             "Stones of remembrance — how the prophets called Israel back to covenant faithfulness.",                  "DTDdX3F7rao", 39, false),
                    (6, "How the Old Testament Points to Christ",               "Learning from followers of Jesus — reading the Hebrew Scriptures through a Christocentric lens.",        "4l85ZMZXaG8", 61, false),
                ]
            ),
            (
                "life-of-christ-the-four-gospels",
                [
                    (1, "The Birth and Early Life of Jesus",                    "Called back to serve — the incarnation as God's ultimate entry into human history.",                       "FOgkX2wqGuQ",  3, true),
                    (2, "The Baptism and Temptation of Jesus",                  "One man's journey of following Christ — Jesus' identity confirmed at baptism, tested in the wilderness.",  "rbxx65C2f_U",  4, false),
                    (3, "The Teachings and Parables of Jesus",                  "Jesus — source of our fellowship — how the parables create community around the Kingdom of God.",          "LpcXBQw1f_s", 51, false),
                    (4, "Miracles and the Kingdom of God",                      "Learning from followers of Jesus — how the miracles reveal the in-breaking of God's Kingdom.",             "4l85ZMZXaG8", 61, false),
                    (5, "The Passion, Death, and Resurrection",                 "God is love — the cross and empty tomb as the fullest expression of God's love for humanity.",             "Saw84fmOBBs", 53, false),
                    (6, "The Great Commission — Go and Make Disciples",         "A worship service reflection on the church's mission flowing from the resurrection of Christ.",            "pQ9CKq4xl9E", 38, false),
                ]
            ),
            (
                "the-art-of-prayer",
                [
                    (1, "What Is Prayer and Why It Matters",          "31 years of answered prayer — an introduction to prayer as conversation with the living God.",                  "EQ84qkq2Q6s",  7, true),
                    (2, "The Lord's Prayer — A Model to Follow",      "Transformed by questions — using Jesus' own prayer to structure and deepen our own prayer life.",              "O0MyKgMzWBk", 86, false),
                    (3, "Intercession — Praying for Others",          "When waiting for success — the discipline of persistent intercession for those who are still waiting.",         "CtjdTNuzTzc", 57, false),
                    (4, "Listening Prayer and Silence",               "Loving yourself rightly — creating space to hear God speak through silence, solitude, and stillness.",          "yaf4uGpe3gI", 50, false),
                    (5, "Prayer and Fasting Together",                "God is love — combining fasting and prayer as an act of radical dependence on a loving God.",                   "Saw84fmOBBs", 53, false),
                    (6, "Leading Corporate Prayer and Worship",       "God is holy — how leading others in prayer is an act of priestly service and holy intercession.",              "NJHh5SRtF7g", 49, false),
                ]
            ),
            (
                "sharing-your-faith",
                [
                    (1, "Why Every Believer Is Called to Share",              "Called back from Japan to serve God — a personal testimony that every believer has a mission field.", "FOgkX2wqGuQ",  3, true),
                    (2, "Overcoming Fear in Evangelism",                      "One man's journey of following Christ — how personal testimony defeats fear in evangelism.",           "rbxx65C2f_U",  4, false),
                    (3, "Your Personal Testimony — Telling Your Story",       "31 years of VNCOC — how a community's story equips every member to tell their own.",                   "EQ84qkq2Q6s",  7, false),
                    (4, "Everyday Evangelism — Natural Conversations",        "Jesus — source of our fellowship — how Jesus' relational style models natural, everyday evangelism.",  "LpcXBQw1f_s", 51, false),
                    (5, "Apologetics Basics — Answering Hard Questions",      "When waiting for success — how perseverance through hard questions builds unshakeable conviction.",    "CtjdTNuzTzc", 57, false),
                    (6, "Cross-Cultural Sensitivity in Ministry",             "A worship service reflection on sharing the gospel across cultural and generational differences.",     "pQ9CKq4xl9E", 38, false),
                ]
            ),
            (
                "raising-faith-filled-families",
                [
                    (1, "The Parent as Discipler",                       "Called back to serve — the parent's role as the primary discipler in a child's faith formation.",             "FOgkX2wqGuQ",  3, true),
                    (2, "Family Devotions That Actually Work",            "Loving yourself rightly — how a healthy, honest faith life in parents creates space for family devotion.",   "yaf4uGpe3gI", 50, false),
                    (3, "Talking About Faith at Every Stage",             "Transformed by questions — using questions to open faith conversations with children at every age.",         "O0MyKgMzWBk", 86, false),
                    (4, "Modeling the Christian Life for Kids",           "Jesus — source of our fellowship — how children learn faith by watching parents live in community.",         "LpcXBQw1f_s", 51, false),
                    (5, "Navigating Doubts with Your Children",           "God is love — how a loving, honest response to doubt builds rather than destroys a child's faith.",          "Saw84fmOBBs", 53, false),
                    (6, "Passing Down a Heritage of Faith",               "God is holy — the call to raise a generation that worships in spirit and in truth.",                         "NJHh5SRtF7g", 49, false),
                ]
            ),
        };

        foreach (var (slug, lessons) in courseLessons)
        {
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Slug == slug, ct);
            if (course is null) continue;

            if (await db.Lessons.AnyAsync(l => l.CourseId == course.Id, ct)) continue;

            foreach (var l in lessons)
            {
                db.Lessons.Add(new Lesson
                {
                    CourseId        = course.Id,
                    Title           = l.Title,
                    Description     = l.Description,
                    ContentType     = ContentType.Video,
                    YouTubeUrl      = $"https://www.youtube.com/watch?v={l.VideoId}",
                    DurationMinutes = l.DurationMinutes,
                    OrderIndex      = l.OrderIndex,
                    IsPreview       = l.IsPreview,
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
