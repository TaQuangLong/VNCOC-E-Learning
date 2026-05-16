using ChurchLearn.Api.Domain.Enums;
using FluentValidation;

namespace ChurchLearn.Api.Features.Lessons;

internal static class LessonValidationRules
{
    internal static readonly System.Text.RegularExpressions.Regex YouTubeRegex =
        new(@"^https?://(www\.)?(youtube\.com/watch\?.*v=|youtu\.be/)[a-zA-Z0-9_\-]+",
            System.Text.RegularExpressions.RegexOptions.Compiled);

    internal static void ApplyCommonRules<T>(AbstractValidator<T> validator)
        where T : class, ILessonRequest
    {
        validator.RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        validator.RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description != null);
        validator.RuleFor(x => x.DurationSeconds).GreaterThanOrEqualTo(0);
        validator.RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);

        validator.When(x => x.ContentType == ContentType.Video, () =>
        {
            validator.RuleFor(x => x.YouTubeUrl)
                .NotEmpty().WithMessage("YouTubeUrl is required for Video lessons.")
                .Must(url => url != null && YouTubeRegex.IsMatch(url))
                .WithMessage("YouTubeUrl must be a valid YouTube URL.");
        });

        validator.When(x => x.ContentType == ContentType.Text, () =>
        {
            validator.RuleFor(x => x.TextContent)
                .NotEmpty().WithMessage("TextContent is required for Text lessons.");
        });

        validator.When(x => x.ContentType == ContentType.Pdf, () =>
        {
            validator.RuleFor(x => x.PdfUrl)
                .NotEmpty().WithMessage("PdfUrl is required for PDF lessons.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                             (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                .WithMessage("PdfUrl must be a valid HTTP/HTTPS URL.");
        });
    }
}

internal interface ILessonRequest
{
    string Title { get; }
    string? Description { get; }
    ContentType ContentType { get; }
    string? YouTubeUrl { get; }
    string? TextContent { get; }
    string? PdfUrl { get; }
    int DurationSeconds { get; }
    int OrderIndex { get; }
}
