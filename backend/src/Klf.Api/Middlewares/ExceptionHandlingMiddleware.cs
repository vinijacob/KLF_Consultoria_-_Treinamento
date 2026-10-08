using Klf.Domain.Exceptions;

using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Middlewares;

/// <summary>
/// Catches exceptions thrown anywhere in the pipeline and turns them into <see cref="ProblemDetails"/> responses,
/// so controllers never need <c>try/catch</c>. <see cref="DomainException"/> subclasses become 4xx responses with their
/// message; anything else becomes a generic 500 that is logged and never exposes internal details.
/// </summary>
internal sealed partial class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            var problem = ToProblemDetails(exception);
            problem.Instance = context.Request.Path;

            if (problem.Status >= StatusCodes.Status500InternalServerError)
            {
                if (context.Request.Path.StartsWithSegments(AnonymousFeedbackPath, StringComparison.OrdinalIgnoreCase))
                {
                    LogAnonymousFeedbackFailure(logger, exception.GetType().Name);
                }
                else
                {
                    LogUnhandledException(logger, exception);
                }
            }

            context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(problem, problem.GetType(), options: null, "application/problem+json");
        }
    }

    /// <summary>Anonymous feedback routes: errors there are logged without path, message or stack trace (no trace of a respondent).</summary>
    internal const string AnonymousFeedbackPath = "/api/v1/public/feedback";

    /// <summary>Maps an exception to the <see cref="ProblemDetails"/> returned to the client.</summary>
    internal static ProblemDetails ToProblemDetails(Exception exception) => exception switch
    {
        ValidationException e => new ValidationProblemDetails(e.Errors.ToDictionary())
        {
            Title = e.Message,
            Status = StatusCodes.Status400BadRequest,
        },
        NotFoundException e => Problem(StatusCodes.Status404NotFound, "Recurso não encontrado.", e.Message),
        ConflictException e => Problem(StatusCodes.Status409Conflict, "Conflito.", e.Message),
        UnauthorizedException e => Problem(StatusCodes.Status401Unauthorized, "Não autenticado.", e.Message),
        ForbiddenException e => Problem(StatusCodes.Status403Forbidden, "Acesso negado.", e.Message),
        _ => Problem(StatusCodes.Status500InternalServerError, "Erro interno.", "Ocorreu um erro inesperado. Tente novamente mais tarde."),
    };

    private static ProblemDetails Problem(int status, string title, string detail) =>
        new() { Status = status, Title = title, Detail = detail };

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception on an anonymous feedback route ({ExceptionType}); details omitted on purpose")]
    private static partial void LogAnonymousFeedbackFailure(ILogger logger, string exceptionType);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception);
}
