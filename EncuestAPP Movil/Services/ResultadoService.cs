using EncuestAPP_Movil.DTOs.Resultados;

namespace EncuestAPP_Movil.Services;

public sealed class ResultadoService(ApiClient apiClient) : IResultadoService
{
    public Task<ActividadParticipacionesDto> ObtenerActividadAsync(int desfaseUtcMinutos, CancellationToken cancellationToken = default) =>
        apiClient.ObtenerAsync<ActividadParticipacionesDto>(
            $"api/resultados/actividad?desfaseUtcMinutos={desfaseUtcMinutos}", true, cancellationToken);

    public Task<ResultadoGeneralDto> ObtenerResumenAsync(int encuestaId, CancellationToken cancellationToken = default) =>
        apiClient.ObtenerAsync<ResultadoGeneralDto>($"api/encuestas/{encuestaId}/resultados", true, cancellationToken);

    public Task<ResultadoPreguntaDto> ObtenerPorPreguntaAsync(int encuestaId, int preguntaId, CancellationToken cancellationToken = default) =>
        apiClient.ObtenerAsync<ResultadoPreguntaDto>(
            $"api/encuestas/{encuestaId}/resultados/preguntas/{preguntaId}", true, cancellationToken);
}
