using ChurchLearn.Api.Features.Discussions.AdminDeleteDiscussion;
using ChurchLearn.Api.Features.Discussions.CreateDiscussion;
using ChurchLearn.Api.Features.Discussions.CreateReply;
using ChurchLearn.Api.Features.Discussions.DeleteDiscussion;
using ChurchLearn.Api.Features.Discussions.GetDiscussionReplies;
using ChurchLearn.Api.Features.Discussions.GetLessonDiscussions;
using ChurchLearn.Api.Features.Discussions.UpdateDiscussion;
using FluentValidation;

namespace ChurchLearn.Api.Features.Discussions;

public static class DiscussionsServiceRegistration
{
    public static IServiceCollection AddDiscussionsFeature(this IServiceCollection services)
    {
        services.AddScoped<GetLessonDiscussionsHandler>();
        services.AddScoped<GetDiscussionRepliesHandler>();
        services.AddScoped<CreateDiscussionHandler>();
        services.AddScoped<CreateReplyHandler>();
        services.AddScoped<UpdateDiscussionHandler>();
        services.AddScoped<DeleteDiscussionHandler>();
        services.AddScoped<AdminDeleteDiscussionHandler>();

        services.AddScoped<IValidator<CreateDiscussionRequest>, CreateDiscussionValidator>();
        services.AddScoped<IValidator<CreateReplyRequest>, CreateReplyValidator>();
        services.AddScoped<IValidator<UpdateDiscussionRequest>, UpdateDiscussionValidator>();

        return services;
    }
}
