using FluentValidation;

namespace ChurchLearn.Api.Features.Auth.ResetPassword;

public record ResetPasswordRequest(string Email, string Token, string NewPassword);

public class ResetPasswordValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}
