using FluentValidation;

namespace ChurchLearn.Api.Features.Progress.SaveVideoProgress;

public class SaveVideoProgressValidator : AbstractValidator<SaveVideoProgressRequest>
{
    public SaveVideoProgressValidator()
    {
        RuleFor(x => x.VideoProgressPercent)
            .InclusiveBetween(0, 100)
            .WithMessage("VideoProgressPercent must be between 0 and 100.");

        RuleFor(x => x.VideoWatchedSeconds)
            .GreaterThanOrEqualTo(0)
            .WithMessage("VideoWatchedSeconds must be 0 or greater.");
    }
}
