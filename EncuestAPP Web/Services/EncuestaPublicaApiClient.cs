using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EncuestAPP_Web.Contracts;

namespace EncuestAPP_Web.Services;

public sealed class EncuestaPublicaApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<EncuestaPublicaDto> ObtenerEncuestaAsync(string tokenPublico, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(
            () => httpClient.GetAsync(RutaEncuesta(tokenPublico), cancellationToken),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await CrearExcepcionAsync(response, cancellationToken);
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<EncuestaPublicaDto>(JsonOptions, cancellationToken)
                ?? throw new EncuestaPublicaApiException("La API devolvió una respuesta vacía o inválida.");
        }
        catch (JsonException exception)
        {
            throw new EncuestaPublicaApiException("La respuesta de la API no tiene un formato válido.", exception);
        }
    }

    public async Task EnviarParticipacionAsync(
        string tokenPublico, CrearParticipacionDto participacion, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(
            () => httpClient.PostAsJsonAsync(
                $"{RutaEncuesta(tokenPublico)}/participaciones", participacion, JsonOptions, cancellationToken),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await CrearExcepcionAsync(response, cancellationToken);
        }

        try
        {
            _ = await response.Content.ReadFromJsonAsync<ParticipacionCreadaDto>(JsonOptions, cancellationToken)
                ?? throw new EncuestaPublicaApiException("La API confirmó el envío con una respuesta inválida.");
        }
        catch (JsonException exception)
        {
            throw new EncuestaPublicaApiException("La API confirmó el envío con una respuesta inválida.", exception);
        }
    }

    private static string RutaEncuesta(string tokenPublico) =>
        $"api/publico/encuestas/{Uri.EscapeDataString(tokenPublico)}";

    private static async Task<HttpResponseMessage> SendAsync(
        Func<Task<HttpResponseMessage>> send, CancellationToken cancellationToken)
    {
        try
        {
            return await send();
        }
        catch (HttpRequestException exception)
        {
            throw new EncuestaPublicaApiException("No se pudo conectar con el servidor. Revisá tu conexión e intentá nuevamente.", exception);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new EncuestaPublicaApiException("La solicitud tardó demasiado. Intentá nuevamente.", exception);
        }
    }

    private static async Task<EncuestaPublicaApiException> CrearExcepcionAsync(
        HttpResponseMessage response, CancellationToken cancellationToken)
    {
        ApiProblemDetails? problem = null;
        try
        {
            problem = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
        }

        var mensaje = response.StatusCode switch
        {
            HttpStatusCode.NotFound => "No se encontró una encuesta con ese enlace.",
            HttpStatusCode.Conflict when !string.IsNullOrWhiteSpace(problem?.Detail) => problem.Detail,
            HttpStatusCode.BadRequest when !string.IsNullOrWhiteSpace(problem?.Detail) => problem.Detail,
            HttpStatusCode.Conflict => "La encuesta ya no admite respuestas o este navegador ya participó.",
            HttpStatusCode.BadRequest => "No se pudo validar la respuesta. Revisá los datos e intentá nuevamente.",
            _ when (int)response.StatusCode >= 500 => "El servidor no pudo completar la solicitud. Intentá nuevamente más tarde.",
            _ => "No se pudo completar la solicitud. Intentá nuevamente."
        };

        return new EncuestaPublicaApiException(mensaje);
    }

    private sealed class ApiProblemDetails
    {
        public string? Detail { get; set; }
    }
}

public sealed class EncuestaPublicaApiException(string message, Exception? innerException = null)
    : Exception(message, innerException);
