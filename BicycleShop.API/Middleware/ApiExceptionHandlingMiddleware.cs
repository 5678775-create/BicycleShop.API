using System.Text.Json;
using BicycleShop.Application.DTOs.Errors;
using BicycleShop.Application.Exceptions;
using FluentValidation;

namespace BicycleShop.API.Middleware;

public class ApiExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ApiExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ApiExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            InsufficientInventoryException => StatusCodes.Status409Conflict,
            InvalidOrderStatusTransitionException => StatusCodes.Status409Conflict,
            BusinessRuleValidationException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled API exception.");
        }

        var response = new ApiErrorResponse
        {
            Message = statusCode == StatusCodes.Status500InternalServerError
                ? "Unexpected server error."
                : exception.Message,
            Details = exception is ValidationException validationException
                ? string.Join("; ", validationException.Errors.Select(e => e.ErrorMessage))
                : _environment.IsDevelopment() ? exception.StackTrace : null,
            StatusCode = statusCode
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
