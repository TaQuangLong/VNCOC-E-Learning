using Microsoft.AspNetCore.Http;

namespace ChurchLearn.Api.Common.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
        => result.IsSuccess ? onSuccess(result.Value!) : MapError(result.ErrorCode, result.Error);

    public static IResult ToHttpResult(this Result result, Func<IResult> onSuccess)
        => result.IsSuccess ? onSuccess() : MapError(result.ErrorCode, result.Error);

    private static IResult MapError(string? errorCode, string? error) =>
        errorCode switch
        {
            ErrorCodes.NotFound     => Results.NotFound(new { error }),
            ErrorCodes.Conflict     => Results.Conflict(new { error }),
            ErrorCodes.Unauthorized => Results.Unauthorized(),
            ErrorCodes.Forbidden    => Results.Forbid(),
            _                       => Results.BadRequest(new { error }),
        };
}
