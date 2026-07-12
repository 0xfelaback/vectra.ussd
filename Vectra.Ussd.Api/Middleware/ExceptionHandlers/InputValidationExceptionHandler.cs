using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

internal sealed class InputValidationExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    public InputValidationExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not FluentValidation.ValidationException validationException)
        {
            return false;
        }
        var errors = validationException.Errors.GroupBy(e => e.PropertyName).ToDictionary(
            g => g.Key,
            g => g.Select(e => e.ErrorMessage).ToArray()
        );
        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = validationException,
            ProblemDetails = new ValidationProblemDetails
            {
                Title = "Validation Error",
                Type = exception.GetType().Name,
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation failures occurred.",
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
            }
        });
    }
}