using EcommerceAPI.Common.Exceptions;
using EcommerceAPI.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IErrorLogRepository errorLogRepository)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("HTTP {Method} {Path} was canceled by the client", context.Request.Method, context.Request.Path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);

            int statusCode = GetStatusCode(ex);

            if (statusCode >= 500)
            {
                await errorLogRepository.LogErrorAsync(
                    logLevel: "Error",
                    message: ex.Message,
                    exceptionType: ex.GetType().Name,
                    stackTrace: ex.StackTrace,
                    innerException: ex.InnerException?.ToString(),
                    requestPath: context.Request.Path,
                    requestMethod: context.Request.Method,
                    statusCode: statusCode
                );
            }

            await HandleExceptionAsync(context, ex, statusCode);
        }
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            AuthenticationException => StatusCodes.Status401Unauthorized,
            System.Security.Authentication.AuthenticationException => StatusCodes.Status401Unauthorized,
            BusinessException => StatusCodes.Status400BadRequest,
            ConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, int statusCode)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        string title = statusCode switch
        {
            StatusCodes.Status401Unauthorized => "Authentication Failed",
            StatusCodes.Status404NotFound => "Resource Not Found",
            StatusCodes.Status409Conflict => "Resource Conflict",
            StatusCodes.Status400BadRequest => "Business Logic Validation Failed",
            _ => "An error occurred while processing your request."
        };

        string type = statusCode switch
        {
            StatusCodes.Status401Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
            StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path,
            Type = type
        };

        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}
