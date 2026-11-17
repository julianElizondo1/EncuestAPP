using EncuestAPP_Movil.DTOs.Preguntas;

namespace EncuestAPP_Movil.Services;

public sealed class PreguntaService(ApiClient apiClient) : IPreguntaService
{
    public Task<IReadOnlyList<PreguntaDto>> ObtenerAsync(int encuestaId, CancellationToken cancellationToken = default) =>
        apiClient.ObtenerAsync<IReadOnlyList<PreguntaDto>>($"api/encuestas/{encuestaId}/preguntas", true, cancellationToken);

    public Task<PreguntaDto> CrearAsync(int encuestaId, CrearPreguntaDto solicitud, CancellationToken cancellationToken = default) =>
        apiClient.EnviarConRespuestaAsync<PreguntaDto>(HttpMethod.Post, $"api/encuestas/{encuestaId}/preguntas", solicitud, true, cancellationToken);

    public Task<PreguntaDto> ActualizarAsync(int id, ActualizarPreguntaDto solicitud, CancellationToken cancellationToken = default) =>
        apiClient.EnviarConRespuestaAsync<PreguntaDto>(HttpMethod.Put, $"api/preguntas/{id}", solicitud, true, cancellationToken);

    public Task EliminarAsync(int id, CancellationToken cancellationToken = default) =>
        apiClient.EnviarSinRespuestaAsync(HttpMethod.Delete, $"api/preguntas/{id}", cancellationToken: cancellationToken);

    public Task<OpcionDto> CrearOpcionAsync(int preguntaId, CrearOpcionDto solicitud, CancellationToken cancellationToken = default) =>
        apiClient.EnviarConRespuestaAsync<OpcionDto>(HttpMethod.Post, $"api/preguntas/{preguntaId}/opciones", solicitud, true, cancellationToken);

    public Task<OpcionDto> ActualizarOpcionAsync(int id, ActualizarOpcionDto solicitud, CancellationToken cancellationToken = default) =>
        apiClient.EnviarConRespuestaAsync<OpcionDto>(HttpMethod.Put, $"api/opciones/{id}", solicitud, true, cancellationToken);

    public Task EliminarOpcionAsync(int id, CancellationToken cancellationToken = default) =>
        apiClient.EnviarSinRespuestaAsync(HttpMethod.Delete, $"api/opciones/{id}", cancellationToken: cancellationToken);
}
