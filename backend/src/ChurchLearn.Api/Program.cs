using System.Text;
using System.Threading.RateLimiting;
using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Common.Middleware;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Features.Auth;
using ChurchLearn.Api.Features.Courses;
using ChurchLearn.Api.Features.Enrollments;
using ChurchLearn.Api.Features.Lessons;
using ChurchLearn.Api.Features.Progress;
using ChurchLearn.Api.Features.Discussions;
using ChurchLearn.Api.Features.Quizzes;
using ChurchLearn.Api.Features.Users;
using ChurchLearn.Api.Infrastructure.Email;
using ChurchLearn.Api.Infrastructure.Identity;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services)
              .WriteTo.Console());

    // Database
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Identity
    builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

    // JWT Authentication
    var jwtSecret = builder.Configuration["Jwt:Secret"]
        ?? throw new InvalidOperationException("Jwt:Secret is required.");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero,
        };
    });

    // Rate limiting — 10 requests/min on auth endpoints
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("auth", limiter =>
        {
            limiter.Window = TimeSpan.FromMinutes(1);
            limiter.PermitLimit = 10;
            limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiter.QueueLimit = 0;
        });
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    // OpenAPI
    builder.Services.AddOpenApi();

    // Health checks
    builder.Services.AddHealthChecks();

    // CORS
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? ["http://localhost:5173"];

    builder.Services.AddCors(options =>
        options.AddPolicy("Frontend", policy =>
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()));

    // Authorization
    builder.Services.AddAuthorization();

    // Application services
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUser, CurrentUserService>();
    builder.Services.AddScoped<JwtTokenService>();
    builder.Services.AddScoped<IEmailSender, NoOpEmailSender>();
    builder.Services.AddTransient<GlobalExceptionHandler>();

    // Feature services
    builder.Services.AddAuthFeature();
    builder.Services.AddUsersFeature();
    builder.Services.AddCoursesFeature();
    builder.Services.AddLessonsFeature();
    builder.Services.AddEnrollmentsFeature();
    builder.Services.AddProgressFeature();
    builder.Services.AddQuizzesFeature();
    builder.Services.AddDiscussionsFeature();

    var app = builder.Build();

    // Migrate database and seed on startup
    await using (var scope = app.Services.CreateAsyncScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
    await DatabaseSeeder.SeedAsync(app.Services);

    // Middleware pipeline
    app.UseMiddleware<GlobalExceptionHandler>();
    app.UseSerilogRequestLogging();
    app.UseCors("Frontend");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options => { options.Title = "ChurchLearn API"; });
    }

    app.MapHealthChecks("/api/health");

    // Feature endpoints
    app.MapAuthEndpoints();
    app.MapUsersEndpoints();
    app.MapCoursesEndpoints();
    app.MapLessonsEndpoints();
    app.MapEnrollmentsEndpoints();
    app.MapProgressEndpoints();
    app.MapQuizzesEndpoints();
    app.MapDiscussionsEndpoints();

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}

