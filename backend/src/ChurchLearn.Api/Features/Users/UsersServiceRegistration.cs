using ChurchLearn.Api.Features.Users.GetUser;
using ChurchLearn.Api.Features.Users.GetUsers;
using ChurchLearn.Api.Features.Users.UpdateUserRoles;
using FluentValidation;

namespace ChurchLearn.Api.Features.Users;

public static class UsersServiceRegistration
{
    public static IServiceCollection AddUsersFeature(this IServiceCollection services)
    {
        services.AddScoped<GetUsersHandler>();
        services.AddScoped<GetUserHandler>();
        services.AddScoped<UpdateUserRolesHandler>();
        services.AddScoped<IValidator<UpdateUserRolesRequest>, UpdateUserRolesValidator>();
        return services;
    }
}
