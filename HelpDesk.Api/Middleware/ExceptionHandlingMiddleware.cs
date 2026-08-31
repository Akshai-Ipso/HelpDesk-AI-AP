using System.Text.Json;
using HelpDesk.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (Exception exception)
            {
                await FehlerBehandelnAsync(context, exception);
            }
        }

        private async Task FehlerBehandelnAsync(
            HttpContext context,
            Exception exception)
        {
            var (status, titel, detail) = exception switch
            {
                TicketGeschlossenException =>
                    (
                        StatusCodes.Status409Conflict,
                        "Konflikt",
                        exception.Message
                    ),

                KeyNotFoundException =>
                    (
                        StatusCodes.Status404NotFound,
                        "Ressource nicht gefunden",
                        exception.Message
                    ),

                ArgumentException =>
                    (
                        StatusCodes.Status400BadRequest,
                        "Ungültige Anfrage",
                        exception.Message
                    ),

                _ =>
                    (
                        StatusCodes.Status500InternalServerError,
                        "Interner Serverfehler",
                        "Bei der Verarbeitung der Anfrage ist " +
                        "ein unerwarteter Fehler aufgetreten."
                    )
            };

            if (status >= 500)
            {
                _logger.LogError(
                    exception,
                    "Unerwarteter Fehler bei {Methode} {Pfad}",
                    context.Request.Method,
                    context.Request.Path);
            }
            else
            {
                _logger.LogWarning(
                    exception,
                    "Anfragefehler bei {Methode} {Pfad}",
                    context.Request.Method,
                    context.Request.Path);
            }

            var problemDetails = new ProblemDetails
            {
                Status = status,
                Title = titel,
                Detail = detail,
                Instance = context.Request.Path
            };

            problemDetails.Extensions["traceId"] =
                context.TraceIdentifier;

            context.Response.StatusCode = status;
            context.Response.ContentType =
                "application/problem+json";

            await JsonSerializer.SerializeAsync(
                context.Response.Body,
                problemDetails,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
    }
}