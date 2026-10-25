using System.Net;
using System.Text.Json;
using FiapNext.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FiapNext.Api.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items[CorrelationIdMiddleware.CorrelationIdHeaderName]?.ToString() ?? "N/A";

        var (statusCode, title, detail) = exception switch
        {
            DomainValidationException dve => (HttpStatusCode.BadRequest, "Erro de Validação de Domínio", dve.Message),
            KeyNotFoundException knf => (HttpStatusCode.NotFound, "Recurso Não Encontrado", knf.Message),
            UnauthorizedAccessException uae => (HttpStatusCode.Unauthorized, "Não Autorizado", uae.Message),
            _ => (HttpStatusCode.InternalServerError, "Erro Interno do Servidor", "Ocorreu um erro inesperado. Tente novamente mais tarde.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "[{CorrelationId}] Erro não tratado: {Message}", correlationId, exception.Message);
        }
        else
        {
            _logger.LogWarning("[{CorrelationId}] Falha na requisição: {Title} - {Detail}", correlationId, title, detail);
        }

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["correlationId"] = correlationId;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(json);
    }
}
