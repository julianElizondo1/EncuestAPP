using EncuestAPP__API.DTOs.Resultados;

namespace EncuestAPP__API.Services.Interfaces;

public interface IResultadoService
{
    Task<ActividadParticipacionesDto> ObtenerActividadAsync(int usuarioId, int desfaseUtcMinutos, CancellationToken cancellationToken);
    Task<ResultadoGeneralDto> ObtenerResumenAsync(int encuestaId, int usuarioId, CancellationToken cancellationToken);
    Task<ResultadoPreguntaDto> ObtenerPorPreguntaAsync(int encuestaId, int preguntaId, int usuarioId, CancellationToken cancellationToken);
}
