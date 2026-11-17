using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EncuestAPP__API.Middleware;

public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            var statusCode = exception switch
            {
                ApiException apiException => apiException.StatusCode,
                DbUpdateException updateException when SqlServerErrorDetector.EsRestriccionUnica(updateException) => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Error inesperado procesando {Path}", context.Request.Path);
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = statusCode switch
                {
                    StatusCodes.Status400BadRequest => "Solicitud inválida",
                    StatusCodes.Status401Unauthorized => "No autenticado",
                    StatusCodes.Status403Forbidden => "Acceso denegado",
                    StatusCodes.Status404NotFound => "Recurso no encontrado",
                    StatusCodes.Status409Conflict => "Conflicto",
                    _ => "Error interno del servidor"
                },
                Detail = exception is ApiException apiProblem
                    ? apiProblem.Message
                    : statusCode == StatusCodes.Status409Conflict
                        ? "La operación entra en conflicto con una restricción de unicidad."
                        : "Ocurrió un error inesperado.",
                Instance = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(problem, context.RequestAborted);
        }
    }
}
