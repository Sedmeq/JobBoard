using FluentValidation;
using JobBoard.Core.DTOs.Common;
using JobBoard.Core.Exceptions;
using System.Net;
using System.Text.Json;

namespace JobBoard.API.Middleware
{

    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
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

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, code, message, details) = exception switch
            {
                ValidationException vex => (
                    HttpStatusCode.BadRequest,
                    "VALIDATION_ERROR",
                    "Daxil edilən məlumatlar yanlışdır.",
                    vex.Errors.Select(e => new FieldError { Field = e.PropertyName, Message = e.ErrorMessage }).ToList()
                ),
                NotFoundException => (HttpStatusCode.NotFound, "NOT_FOUND", exception.Message, (List<FieldError>?)null),
                UnauthorizedException => (HttpStatusCode.Unauthorized, "UNAUTHORIZED", exception.Message, null),
                ForbiddenException => (HttpStatusCode.Forbidden, "FORBIDDEN", exception.Message, null),
                ConflictException => (HttpStatusCode.Conflict, "CONFLICT", exception.Message, null),
                BadRequestException => (HttpStatusCode.BadRequest, "BAD_REQUEST", exception.Message, null),
                _ => (
                    HttpStatusCode.InternalServerError,
                    "SERVER_ERROR",
                    _env.IsDevelopment() ? exception.Message : "Serverdə xəta baş verdi.",
                    null
                )
            };

            context.Response.StatusCode = (int)statusCode;

            var response = ApiResponse<object>.Fail(code, message, details);
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
