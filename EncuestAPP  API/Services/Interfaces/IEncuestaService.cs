using EncuestAPP__API.DTOs.Encuestas;

namespace EncuestAPP__API.Services.Interfaces;

public interface IEncuestaService
{
    Task<PaginaDto<EncuestaListaDto>> ListarAsync(int usuarioId, string? estado, int? categoriaId, string? titulo, int pagina, int tamanoPagina, CancellationToken cancellationToken);
    Task<EncuestaDetalleDto> ObtenerPropiaAsync(int id, int usuarioId, CancellationToken cancellationToken);
    Task<EncuestaDetalleDto> CrearAsync(CrearEncuestaDto solicitud, int usuarioId, CancellationToken cancellationToken);
    Task<EncuestaDetalleDto> ActualizarAsync(int id, ActualizarEncuestaDto solicitud, int usuarioId, CancellationToken cancellationToken);
    Task PublicarAsync(int id, int usuarioId, CancellationToken cancellationToken);
    Task CerrarAsync(int id, int usuarioId, CancellationToken cancellationToken);
    Task ArchivarAsync(int id, int usuarioId, CancellationToken cancellationToken);
    Task<EncuestaDetalleDto> CrearNuevaVersionAsync(int id, int usuarioId, CancellationToken cancellationToken);
}
