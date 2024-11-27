
using EmployeeAdministration.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace EmployeeAdministration.API.Common;

internal class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly Dictionary<Type, (int, string)> _exceptionCodes = new()
    {
        { typeof(EntityNotFoundException), (StatusCodes.Status404NotFound, "Object not found") },
        { typeof(ExistingProjectMemberException), (StatusCodes.Status400BadRequest, "Pre-existing project member") },
        { typeof(NonEmployeeUserException), (StatusCodes.Status400BadRequest, "Non-employee user") },
        { typeof(NotAProjectMemberException), (StatusCodes.Status400BadRequest, "Non-member user") },
        { typeof(UnauthorizedException), (StatusCodes.Status401Unauthorized, "Unauthorized") },
        { typeof(UncompletedTasksAssignedToEntityException), (StatusCodes.Status400BadRequest, "Unfinished tasks persisting") },
        { typeof(InvalidPasswordException), (StatusCodes.Status400BadRequest, "Incorrect password") },
    };

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(
                new ValidationProblemDetails { Errors = ex.Errors });
        }
        catch (BaseException ex) when (_exceptionCodes.ContainsKey(ex.GetType()))
        {
            await HandleApiErrors(ex, context);
        }
        catch (Exception ex)
        {
            Log.Error(ex, ex is BaseException ? "MISSING_ERROR_CODE_FOR_APP_EXCEPTION" : "API_ERROR");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Title = "Server error",
                    Detail = "Something went wrong in the server"
                });
        }
    }

    private async Task HandleApiErrors(BaseException ex, HttpContext context)
    {
        ProblemDetails details = new()
        {
            Title = _exceptionCodes[ex.GetType()].Item2,
            Detail = ex.Message
        };

        context.Response.StatusCode = _exceptionCodes[ex.GetType()].Item1;
        await context.Response.WriteAsJsonAsync(details);
    }
}
