using EncuestAPP_Movil.DTOs.Resultados;

namespace EncuestAPP_Movil.Services;

public interface IResultadoService
{
    Task<ActividadParticipacionesDto> ObtenerActividadAsync(int desfaseUtcMinutos, CancellationToken cancellationToken = default);
    Task<ResultadoGeneralDto> ObtenerResumenAsync(int encuestaId, CancellationToken cancellationToken = default);
    Task<ResultadoPreguntaDto> ObtenerPorPreguntaAsync(int encuestaId, int preguntaId, CancellationToken cancellationToken = default);
}
