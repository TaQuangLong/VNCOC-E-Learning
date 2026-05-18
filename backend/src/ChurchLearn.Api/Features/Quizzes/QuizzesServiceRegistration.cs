using ChurchLearn.Api.Features.Quizzes.AddQuestion;
using ChurchLearn.Api.Features.Quizzes.CreateQuiz;
using ChurchLearn.Api.Features.Quizzes.DeleteQuestion;
using ChurchLearn.Api.Features.Quizzes.GetLessonQuiz;
using ChurchLearn.Api.Features.Quizzes.GetMyQuizAttempts;
using ChurchLearn.Api.Features.Quizzes.SubmitQuiz;
using ChurchLearn.Api.Features.Quizzes.UpdateQuestion;
using ChurchLearn.Api.Features.Quizzes.UpdateQuiz;
using FluentValidation;

namespace ChurchLearn.Api.Features.Quizzes;

public static class QuizzesServiceRegistration
{
    public static IServiceCollection AddQuizzesFeature(this IServiceCollection services)
    {
        services.AddScoped<GetLessonQuizHandler>();
        services.AddScoped<CreateQuizHandler>();
        services.AddScoped<UpdateQuizHandler>();
        services.AddScoped<AddQuestionHandler>();
        services.AddScoped<UpdateQuestionHandler>();
        services.AddScoped<DeleteQuestionHandler>();
        services.AddScoped<SubmitQuizHandler>();
        services.AddScoped<GetMyQuizAttemptsHandler>();

        services.AddScoped<IValidator<CreateQuizRequest>, CreateQuizValidator>();
        services.AddScoped<IValidator<UpdateQuizRequest>, UpdateQuizValidator>();
        services.AddScoped<IValidator<AddQuestionRequest>, AddQuestionValidator>();
        services.AddScoped<IValidator<UpdateQuestionRequest>, UpdateQuestionValidator>();
        services.AddScoped<IValidator<SubmitQuizRequest>, SubmitQuizValidator>();

        return services;
    }
}
