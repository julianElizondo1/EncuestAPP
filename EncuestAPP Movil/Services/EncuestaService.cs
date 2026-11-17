using System.Net;
using EncuestAPP_Movil.DTOs;
using EncuestAPP_Movil.DTOs.Encuestas;

namespace EncuestAPP_Movil.Services;

public sealed class EncuestaService(ApiClient apiClient) : IEncuestaService
{
    public async Task<PaginaDto<EncuestaListaDto>> ObtenerPropiasAsync(
        string? estado, int? categoriaId, string? titulo, int pagina, int tamanoPagina, CancellationToken cancellationToken = default)
    {
        var parametros = new List<string>
        {
            $"page={Math.Max(1, pagina)}",
            $"pageSize={Math.Clamp(tamanoPagina, 1, 100)}"
        };
        if (!string.IsNullOrWhiteSpace(estado))
        {
            parametros.Add($"estado={Uri.EscapeDataString(estado.Trim())}");
        }
        if (categoriaId.HasValue)
        {
            parametros.Add($"categoriaId={categoriaId.Value}");
        }
        if (!string.IsNullOrWhiteSpace(titulo))
        {
            parametros.Add($"titulo={Uri.EscapeDataString(titulo.Trim())}");
        }

        return await apiClient.ObtenerAsync<PaginaDto<EncuestaListaDto>>(
            $"api/encuestas?{string.Join("&", parametros)}", true, cancellationToken);
    }

    public Task<IReadOnlyList<CatalogoDto>> ObtenerEstadosAsync(CancellationToken cancellationToken = default) =>
        apiClient.ObtenerAsync<IReadOnlyList<CatalogoDto>>("api/catalogos/estados", false, cancellationToken);

    public Task<IReadOnlyList<CatalogoDto>> ObtenerCategoriasAsync(CancellationToken cancellationToken = default) =>
        apiClient.ObtenerAsync<IReadOnlyList<CatalogoDto>>("api/catalogos/categorias", false, cancellationToken);

    public Task<IReadOnlyList<CatalogoDto>> ObtenerTiposPreguntaAsync(CancellationToken cancellationToken = default) =>
        apiClient.ObtenerAsync<IReadOnlyList<CatalogoDto>>("api/catalogos/tipos-pregunta", false, cancellationToken);

    public Task<EncuestaDetalleDto> ObtenerDetalleAsync(int id, CancellationToken cancellationToken = default) =>
        apiClient.ObtenerAsync<EncuestaDetalleDto>($"api/encuestas/{id}", true, cancellationToken);

    public Task<EncuestaDetalleDto> CrearAsync(CrearEncuestaDto solicitud, CancellationToken cancellationToken = default) =>
        apiClient.EnviarConRespuestaAsync<EncuestaDetalleDto>(HttpMethod.Post, "api/encuestas", solicitud, true, cancellationToken);

    public Task<EncuestaDetalleDto> ActualizarAsync(int id, ActualizarEncuestaDto solicitud, CancellationToken cancellationToken = default) =>
        apiClient.EnviarConRespuestaAsync<EncuestaDetalleDto>(HttpMethod.Put, $"api/encuestas/{id}", solicitud, true, cancellationToken);

    public Task PublicarAsync(int id, CancellationToken cancellationToken = default) =>
        apiClient.EnviarSinRespuestaAsync(HttpMethod.Post, $"api/encuestas/{id}/publicar", cancellationToken: cancellationToken);

    public Task CerrarAsync(int id, CancellationToken cancellationToken = default) =>
        apiClient.EnviarSinRespuestaAsync(HttpMethod.Post, $"api/encuestas/{id}/cerrar", cancellationToken: cancellationToken);

    public Task ArchivarAsync(int id, CancellationToken cancellationToken = default) =>
        apiClient.EnviarSinRespuestaAsync(HttpMethod.Post, $"api/encuestas/{id}/archivar", cancellationToken: cancellationToken);

    public Task<EncuestaDetalleDto> CrearNuevaVersionAsync(int id, CancellationToken cancellationToken = default) =>
        apiClient.EnviarConRespuestaAsync<EncuestaDetalleDto>(HttpMethod.Post, $"api/encuestas/{id}/nueva-version", null, true, cancellationToken);
}
