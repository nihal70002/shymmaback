using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ClientEcommerce.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message) = exception switch
            {
                ValidationException vex => (HttpStatusCode.BadRequest, vex.Message),
                BadRequestException brex => (HttpStatusCode.BadRequest, brex.Message),
                NotFoundException nex => (HttpStatusCode.NotFound, nex.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized"),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.")
            };

            context.Response.StatusCode = (int)statusCode;

            var response = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = message,
                Type = statusCode switch
                {
                    HttpStatusCode.BadRequest => "https://httpstatuses.com/400",
                    HttpStatusCode.NotFound => "https://httpstatuses.com/404",
                    HttpStatusCode.Unauthorized => "https://httpstatuses.com/401",
                    _ => "https://httpstatuses.com/500"
                }
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
