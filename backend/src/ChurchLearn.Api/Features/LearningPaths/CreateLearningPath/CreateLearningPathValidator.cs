using FluentValidation;

namespace ChurchLearn.Api.Features.LearningPaths.CreateLearningPath;

public class CreateLearningPathValidator : AbstractValidator<CreateLearningPathRequest>
{
    public CreateLearningPathValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(200)
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase kebab-case (e.g. foundations-of-faith).");
        RuleFor(x => x.ShortDescription)
            .MaximumLength(500)
            .When(x => x.ShortDescription != null);
        RuleFor(x => x.ThumbnailUrl)
            .MaximumLength(2048)
            .Must(BeValidHttpUrl)
            .WithMessage("ThumbnailUrl must be a valid absolute URL.")
            .When(x => x.ThumbnailUrl != null);

        RuleFor(x => x.Sections)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .WithMessage("A learning path must have at least one section.")
            .Must(HaveUniqueSectionOrderIndices)
            .WithMessage("Section order indices must be unique.")
            .Must(HaveUniqueCourses)
            .WithMessage("A course cannot appear more than once in a learning path.");

        RuleForEach(x => x.Sections).ChildRules(section =>
        {
            section.RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            section.RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
            section.RuleFor(x => x.Courses)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .NotEmpty()
                .WithMessage("Each learning path section must have at least one course.")
                .Must(HaveUniqueCourseOrderIndices)
                .WithMessage("Course order indices must be unique within a section.");
            section.RuleForEach(x => x.Courses).ChildRules(course =>
            {
                course.RuleFor(x => x.CourseId).GreaterThan(0);
                course.RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
            });
        });
    }

    private static bool BeValidHttpUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);

    private static bool HaveUniqueSectionOrderIndices(
        IReadOnlyList<CreateLearningPathSectionRequest> sections)
    {
        if (sections.Any(section => section is null))
            return false;

        return sections.Select(section => section.OrderIndex).Distinct().Count() == sections.Count;
    }

    private static bool HaveUniqueCourses(
        IReadOnlyList<CreateLearningPathSectionRequest> sections)
    {
        if (sections.Any(section =>
                section is null
                || section.Courses is null
                || section.Courses.Any(course => course is null)))
            return false;

        var courseIds = sections.SelectMany(section => section.Courses).Select(course => course.CourseId).ToList();
        return courseIds.Distinct().Count() == courseIds.Count;
    }

    private static bool HaveUniqueCourseOrderIndices(
        IReadOnlyList<CreateLearningPathCourseRequest> courses)
    {
        if (courses.Any(course => course is null))
            return false;

        return courses.Select(course => course.OrderIndex).Distinct().Count() == courses.Count;
    }
}
