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

## Result Pattern
All handlers return `Result<T>` — never throw exceptions for expected domain errors.

```csharp
// Common/Result.cs
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }

    private Result(T value) { IsSuccess = true; Value = value; }
    private Result(string error, string? errorCode = null)
    {
        IsSuccess = false;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error, string? errorCode = null) => new(error, errorCode);
}
```

## Handler Pattern
```csharp
public class CreateCourseHandler(AppDbContext db, ICurrentUser currentUser)
{
    public async Task<Result<CreateCourseResponse>> Handle(
        CreateCourseRequest request,
        CancellationToken cancellationToken)
    {
        // 1. Check authorization / business rules — return Result.Failure on violation
        if (await db.Courses.AnyAsync(c => c.Slug == request.Slug, cancellationToken))
            return Result<CreateCourseResponse>.Failure("Slug already exists.", "SLUG_CONFLICT");

        // 2. Execute operation against db
        // 3. Return Result.Success(dto) — never return EF entity
        return Result<CreateCourseResponse>.Success(new CreateCourseResponse(/* ... */));
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

    private static async Task<IResult> Handle(
        CreateCourseRequest request,
        CreateCourseHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(request, ct);
        return result.IsSuccess
            ? Results.Created($"/api/admin/courses/{result.Value!.Id}", result.Value)
            : result.ErrorCode switch
            {
                "SLUG_CONFLICT" => Results.Conflict(new { error = result.Error }),
                "FORBIDDEN"     => Results.Forbid(),
                _               => Results.BadRequest(new { error = result.Error })
            };
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
- Handlers always return `Result<T>` — never throw exceptions for expected domain errors
- Endpoints map Result errors to HTTP status codes via switch on ErrorCode
- No bare `throw` or `try/catch` for expected error paths (not found, conflict, forbidden)
