using EncuestAPP__API.DTOs.Preguntas;

namespace EncuestAPP__API.Services.Interfaces;

public interface IPreguntaService
{
    Task<IReadOnlyList<PreguntaDto>> ListarAsync(int encuestaId, int usuarioId, CancellationToken cancellationToken);
    Task<PreguntaDto> CrearAsync(int encuestaId, CrearPreguntaDto solicitud, int usuarioId, CancellationToken cancellationToken);
    Task<PreguntaDto> ActualizarAsync(int id, ActualizarPreguntaDto solicitud, int usuarioId, CancellationToken cancellationToken);
    Task EliminarAsync(int id, int usuarioId, CancellationToken cancellationToken);
    Task<OpcionDto> CrearOpcionAsync(int preguntaId, CrearOpcionDto solicitud, int usuarioId, CancellationToken cancellationToken);
    Task<OpcionDto> ActualizarOpcionAsync(int id, ActualizarOpcionDto solicitud, int usuarioId, CancellationToken cancellationToken);
    Task EliminarOpcionAsync(int id, int usuarioId, CancellationToken cancellationToken);
}
