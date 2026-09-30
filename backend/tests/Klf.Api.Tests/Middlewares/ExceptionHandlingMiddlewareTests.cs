using Klf.Api.Middlewares;
using Klf.Domain.Exceptions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Tests.Middlewares;

public sealed class ExceptionHandlingMiddlewareTests
{
    public static TheoryData<Exception, int> Cases => new()
    {
        { new NotFoundException("x"), StatusCodes.Status404NotFound },
        { new ConflictException("x"), StatusCodes.Status409Conflict },
        { new ForbiddenException(), StatusCodes.Status403Forbidden },
        { new ValidationException("Name", "x"), StatusCodes.Status400BadRequest },
        { new InvalidOperationException("secret"), StatusCodes.Status500InternalServerError },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Status_matches_exception_when_converted_to_problem_details(Exception exception, int expectedStatus)
    {
        var problem = ExceptionHandlingMiddleware.ToProblemDetails(exception);

        Assert.Equal(expectedStatus, problem.Status);
    }

    [Fact]
    public void Internal_details_are_hidden_when_exception_is_unexpected()
    {
        var problem = ExceptionHandlingMiddleware.ToProblemDetails(new InvalidOperationException("connection string xyz"));

        Assert.DoesNotContain("xyz", problem.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void Field_errors_are_returned_when_validation_fails()
    {
        var problem = ExceptionHandlingMiddleware.ToProblemDetails(new ValidationException("Name", "Obrigatório"));

        var validation = Assert.IsType<ValidationProblemDetails>(problem);
        Assert.Equal(["Obrigatório"], validation.Errors["Name"]);
    }
}
