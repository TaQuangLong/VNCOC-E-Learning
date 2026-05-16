---
applyTo: "backend/**/*.cs"
---

# Backend Coding Instructions — ChurchLearn

## Vertical Slice Folder Pattern
Each feature action gets its own folder:

```
Features/Courses/CreateCourse/
  CreateCourseRequest.cs
  CreateCourseResponse.cs
  CreateCourseValidator.cs
  CreateCourseHandler.cs
  CreateCourseEndpoint.cs
```

## Handler Pattern
```csharp
public class CreateCourseHandler(AppDbContext db, ICurrentUser currentUser)
{
    public async Task<CreateCourseResponse> Handle(
        CreateCourseRequest request,
        CancellationToken cancellationToken)
    {
        // 1. Check authorization / business rules
        // 2. Execute operation against db
        // 3. Return DTO — never return EF entity
    }
}
```

## Request / Response Pattern
- Use C# record types for Request and Response objects
- Request: input fields only, no navigation properties
- Response: output fields only, shaped for the client

## Validation Pattern
```csharp
public class CreateCourseValidator : AbstractValidator<CreateCourseRequest>
{
    public CreateCourseValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200).Matches("^[a-z0-9-]+$");
        RuleFor(x => x.AuthorId).NotEmpty();
    }
}
```

## Endpoint Pattern
```csharp
public static class CreateCourseEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/courses", Handle)
           .RequireAuthorization("AdminOnly")
           .WithTags("Courses");
    }
}
```

## Rules
- Return DTOs, never EF Core entities
- Use record types for Request and Response objects
- FluentValidation validator always runs before handler
- Always check roles in endpoint via RequireAuthorization
- Use cancellationToken in all DbContext async calls
- Add database indexes for UserId, CourseId, LessonId on join tables
- Use ICurrentUser abstraction — never read HttpContext directly in handlers
