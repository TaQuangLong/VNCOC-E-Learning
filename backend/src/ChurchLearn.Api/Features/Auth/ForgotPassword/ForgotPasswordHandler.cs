using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Infrastructure.Email;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace ChurchLearn.Api.Features.Auth.ForgotPassword;

public class ForgotPasswordHandler(
    UserManager<AppUser> userManager,
    IEmailSender emailSender,
    IValidator<ForgotPasswordRequest> validator)
{
    public async Task HandleAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            throw new ArgumentException(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        // Always return success to prevent email enumeration
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive) return;

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);

        var body = $"""
            <p>Click the link below to reset your ChurchLearn password:</p>
            <p><a href="http://localhost:5173/reset-password?email={request.Email}&token={encodedToken}">Reset Password</a></p>
            <p>This link expires in 24 hours.</p>
            """;

        await emailSender.SendAsync(request.Email, "Reset your ChurchLearn password", body, cancellationToken);
    }
}
