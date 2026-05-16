using FluentValidation;

namespace ChurchLearn.Api.Features.Auth.ForgotPassword;

public record ForgotPasswordRequest(string Email);

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
