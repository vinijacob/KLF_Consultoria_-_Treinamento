using Klf.Domain.Exceptions;

using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Middlewares;

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
                LogUnhandledException(logger, exception);
            }

            context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(problem, problem.GetType(), options: null, "application/problem+json");
        }
    }

    internal static ProblemDetails ToProblemDetails(Exception exception) => exception switch
    {
        ValidationException e => new ValidationProblemDetails(e.Errors.ToDictionary())
        {
            Title = e.Message,
            Status = StatusCodes.Status400BadRequest,
        },
        NotFoundException e => Problem(StatusCodes.Status404NotFound, "Recurso não encontrado.", e.Message),
        ConflictException e => Problem(StatusCodes.Status409Conflict, "Conflito.", e.Message),
        ForbiddenException e => Problem(StatusCodes.Status403Forbidden, "Acesso negado.", e.Message),
        _ => Problem(StatusCodes.Status500InternalServerError, "Erro interno.", "Ocorreu um erro inesperado. Tente novamente mais tarde."),
    };

    private static ProblemDetails Problem(int status, string title, string detail) =>
        new() { Status = status, Title = title, Detail = detail };

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception);
}
