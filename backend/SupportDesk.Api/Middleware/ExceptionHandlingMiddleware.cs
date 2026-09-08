using System.Net;
using System.Text.Json;
using SupportDesk.Core.Exceptions;

namespace SupportDesk.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, errorCode, message) = exception switch
        {
            BusinessRuleException ex => (HttpStatusCode.BadRequest, ex.Code, ex.Message),
            KeyNotFoundException ex => (HttpStatusCode.NotFound, "NOT_FOUND", ex.Message),
            ArgumentException ex => (HttpStatusCode.BadRequest, "INVALID_ARGUMENT", ex.Message),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_SERVER_ERROR", "An error occurred while processing your request.")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            error = errorCode,
            message = message,
            status = (int)statusCode,
            timestamp = DateTime.UtcNow
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}