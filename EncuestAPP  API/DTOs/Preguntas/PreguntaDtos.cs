using System.ComponentModel.DataAnnotations;
using EncuestAPP__API.DTOs.Encuestas;

namespace EncuestAPP__API.DTOs.Preguntas;

public class CrearPreguntaDto
{
    [Required, StringLength(500)]
    public string Texto { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int TipoPreguntaId { get; set; }

    [Range(1, int.MaxValue)]
    public int Orden { get; set; }

    public bool Obligatoria { get; set; }
}

public sealed class ActualizarPreguntaDto : CrearPreguntaDto { }

public class CrearOpcionDto
{
    [Required, StringLength(200)]
    public string Texto { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Orden { get; set; }
}

public sealed class ActualizarOpcionDto : CrearOpcionDto { }
public sealed record PreguntaDto(int Id, int EncuestaId, int TipoPreguntaId, string Tipo, string Texto, int Orden, bool Obligatoria, IReadOnlyList<OpcionEncuestaDto> Opciones);
public sealed record OpcionDto(int Id, int PreguntaId, string Texto, int Orden);
