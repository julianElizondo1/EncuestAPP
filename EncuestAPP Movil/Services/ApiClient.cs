using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EncuestAPP_Movil.Handlers;
using Microsoft.Extensions.Logging;

namespace EncuestAPP_Movil.Services;

public sealed class ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task<TResponse> ObtenerAsync<TResponse>(string ruta, bool requiereAutorizacion = true, CancellationToken cancellationToken = default) =>
        EnviarConRespuestaAsync<TResponse>(HttpMethod.Get, ruta, null, requiereAutorizacion, cancellationToken);

    public async Task<TResponse> EnviarConRespuestaAsync<TResponse>(
        HttpMethod metodo, string ruta, object? contenido, bool requiereAutorizacion = true, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = CrearSolicitud(metodo, ruta, contenido, requiereAutorizacion);
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw await CrearExcepcionAsync(response, cancellationToken);
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            try
            {
                return JsonSerializer.Deserialize<TResponse>(body, JsonOptions)
                    ?? throw new ApiClientException("La API devolvió una respuesta vacía o inválida.", response.StatusCode);
            }
            catch (JsonException exception)
            {
                logger.LogWarning(exception, "La API devolvió una respuesta con formato inesperado.");
                throw new ApiClientException("La respuesta de la API no tiene un formato válido.", response.StatusCode, innerException: exception);
            }
        }
        catch (ApiClientException)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "No se pudo conectar con la API.");
            throw new ApiClientException("No se pudo conectar con el servidor. Revisá tu conexión e intentá nuevamente.", innerException: exception);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "La solicitud a la API agotó el tiempo de espera.");
            throw new ApiClientException("La solicitud tardó demasiado. Intentá nuevamente.", innerException: exception);
        }
    }

    public async Task EnviarSinRespuestaAsync(
        HttpMethod metodo, string ruta, object? contenido = null, bool requiereAutorizacion = true, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = CrearSolicitud(metodo, ruta, contenido, requiereAutorizacion);
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw await CrearExcepcionAsync(response, cancellationToken);
            }
        }
        catch (ApiClientException)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "No se pudo conectar con la API.");
            throw new ApiClientException("No se pudo conectar con el servidor. Revisá tu conexión e intentá nuevamente.", innerException: exception);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "La solicitud a la API agotó el tiempo de espera.");
            throw new ApiClientException("La solicitud tardó demasiado. Intentá nuevamente.", innerException: exception);
        }
    }

    private static HttpRequestMessage CrearSolicitud(HttpMethod metodo, string ruta, object? contenido, bool requiereAutorizacion)
    {
        var request = new HttpRequestMessage(metodo, ruta);
        if (contenido is not null)
        {
            request.Content = JsonContent.Create(contenido, options: JsonOptions);
        }
        if (requiereAutorizacion)
        {
            request.Options.Set(ApiRequestOptions.RequiresAuthorization, true);
        }
        return request;
    }

    private static async Task<ApiClientException> CrearExcepcionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        ApiProblemDetailsDto? problem;
        try
        {
            problem = JsonSerializer.Deserialize<ApiProblemDetailsDto>(body, JsonOptions);
        }
        catch (JsonException exception)
        {
            return new ApiClientException(MensajePara(response.StatusCode), response.StatusCode, innerException: exception);
        }

        var errores = problem?.Errors?.Values.SelectMany(mensajes => mensajes ?? []).ToArray() ?? [];
        var mensaje = !string.IsNullOrWhiteSpace(problem?.Detail)
            ? problem.Detail
            : !string.IsNullOrWhiteSpace(problem?.Title)
                ? problem.Title
                : MensajePara(response.StatusCode);

        return new ApiClientException(mensaje, response.StatusCode, errores);
    }

    private static string MensajePara(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.BadRequest => "La solicitud no es válida. Revisá los datos ingresados.",
        HttpStatusCode.Unauthorized => "La sesión ya no es válida. Iniciá sesión nuevamente.",
        HttpStatusCode.Forbidden => "No tenés permisos para realizar esta operación.",
        HttpStatusCode.NotFound => "No se encontró el recurso solicitado.",
        HttpStatusCode.Conflict => "La operación entra en conflicto con el estado actual del recurso.",
        _ when (int)statusCode >= 500 => "El servidor no pudo completar la operación.",
        _ => "No se pudo completar la solicitud. Intentá nuevamente."
    };

    private sealed class ApiProblemDetailsDto
    {
        public string? Title { get; set; }
        public string? Detail { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
