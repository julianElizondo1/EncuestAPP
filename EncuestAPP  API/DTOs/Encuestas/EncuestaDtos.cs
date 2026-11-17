using System.ComponentModel.DataAnnotations;

namespace EncuestAPP__API.DTOs.Encuestas;

public sealed class CrearEncuestaDto
{
    [Required, StringLength(150)]
    public string Titulo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoriaEncuestaId { get; set; }

    public bool EsAnonima { get; set; } = true;
    public bool UnaRespuestaPorDispositivo { get; set; }
}

public sealed class ActualizarEncuestaDto
{
    [Required, StringLength(150)]
    public string Titulo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoriaEncuestaId { get; set; }

    public bool EsAnonima { get; set; }
    public bool UnaRespuestaPorDispositivo { get; set; }
}

public sealed record EncuestaListaDto(
    int Id,
    string Titulo,
    string? Descripcion,
    string Estado,
    string Categoria,
    DateTime FechaCreacion,
    DateTime? FechaPublicacion,
    DateTime? FechaCierre);

public sealed record EncuestaDetalleDto(
    int Id,
    string Titulo,
    string? Descripcion,
    string Estado,
    string Categoria,
    bool EsAnonima,
    bool UnaRespuestaPorDispositivo,
    string TokenPublico,
    DateTime FechaCreacion,
    DateTime? FechaPublicacion,
    DateTime? FechaCierre,
    IReadOnlyList<PreguntaEncuestaDto> Preguntas);

public sealed record PreguntaEncuestaDto(
    int Id,
    string Texto,
    string Tipo,
    int Orden,
    bool Obligatoria,
    IReadOnlyList<OpcionEncuestaDto> Opciones);

public sealed record OpcionEncuestaDto(int Id, string Texto, int Orden);
public sealed record PaginaDto<T>(IReadOnlyList<T> Elementos, int Pagina, int TamanoPagina, int TotalElementos, int TotalPaginas);
