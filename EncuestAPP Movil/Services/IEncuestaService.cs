using EncuestAPP_Movil.DTOs;
using EncuestAPP_Movil.DTOs.Encuestas;

namespace EncuestAPP_Movil.Services;

public interface IEncuestaService
{
    Task<PaginaDto<EncuestaListaDto>> ObtenerPropiasAsync(
        string? estado, int? categoriaId, string? titulo, int pagina, int tamanoPagina, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogoDto>> ObtenerEstadosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogoDto>> ObtenerCategoriasAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogoDto>> ObtenerTiposPreguntaAsync(CancellationToken cancellationToken = default);
    Task<EncuestaDetalleDto> ObtenerDetalleAsync(int id, CancellationToken cancellationToken = default);
    Task<EncuestaDetalleDto> CrearAsync(CrearEncuestaDto solicitud, CancellationToken cancellationToken = default);
    Task<EncuestaDetalleDto> ActualizarAsync(int id, ActualizarEncuestaDto solicitud, CancellationToken cancellationToken = default);
    Task PublicarAsync(int id, CancellationToken cancellationToken = default);
    Task CerrarAsync(int id, CancellationToken cancellationToken = default);
    Task ArchivarAsync(int id, CancellationToken cancellationToken = default);
    Task<EncuestaDetalleDto> CrearNuevaVersionAsync(int id, CancellationToken cancellationToken = default);
}
