using EncuestAPP_Movil.DTOs.Preguntas;

namespace EncuestAPP_Movil.Services;

public interface IPreguntaService
{
    Task<IReadOnlyList<PreguntaDto>> ObtenerAsync(int encuestaId, CancellationToken cancellationToken = default);
    Task<PreguntaDto> CrearAsync(int encuestaId, CrearPreguntaDto solicitud, CancellationToken cancellationToken = default);
    Task<PreguntaDto> ActualizarAsync(int id, ActualizarPreguntaDto solicitud, CancellationToken cancellationToken = default);
    Task EliminarAsync(int id, CancellationToken cancellationToken = default);
    Task<OpcionDto> CrearOpcionAsync(int preguntaId, CrearOpcionDto solicitud, CancellationToken cancellationToken = default);
    Task<OpcionDto> ActualizarOpcionAsync(int id, ActualizarOpcionDto solicitud, CancellationToken cancellationToken = default);
    Task EliminarOpcionAsync(int id, CancellationToken cancellationToken = default);
}
