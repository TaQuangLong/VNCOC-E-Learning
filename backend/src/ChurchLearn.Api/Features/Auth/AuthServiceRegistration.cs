using ChurchLearn.Api.Features.Auth.ForgotPassword;
using ChurchLearn.Api.Features.Auth.GetCurrentUser;
using ChurchLearn.Api.Features.Auth.Login;
using ChurchLearn.Api.Features.Auth.RefreshToken;
using ChurchLearn.Api.Features.Auth.Register;
using ChurchLearn.Api.Features.Auth.ResetPassword;
using FluentValidation;

namespace ChurchLearn.Api.Features.Auth;

public static class AuthServiceRegistration
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services)
    {
        // Handlers
        services.AddScoped<RegisterHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshTokenHandler>();
        services.AddScoped<GetCurrentUserHandler>();
        services.AddScoped<ForgotPasswordHandler>();
        services.AddScoped<ResetPasswordHandler>();

        // Validators
        services.AddScoped<IValidator<RegisterRequest>, RegisterValidator>();
        services.AddScoped<IValidator<LoginRequest>, LoginValidator>();
        services.AddScoped<IValidator<ForgotPasswordRequest>, ForgotPasswordValidator>();
        services.AddScoped<IValidator<ResetPasswordRequest>, ResetPasswordValidator>();

        return services;
    }
}
