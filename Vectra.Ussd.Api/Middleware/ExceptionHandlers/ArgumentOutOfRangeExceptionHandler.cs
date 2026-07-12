using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

internal sealed class ArgumentOutOfRangeExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    public ArgumentOutOfRangeExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ArgumentOutOfRangeException)
        {
            return false;
        }
        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Title = "Invalid parameter value",
                Detail = exception.InnerException!.Message ?? "Unknown reson",
                Status = StatusCodes.Status400BadRequest,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
            }
        });
    }
}