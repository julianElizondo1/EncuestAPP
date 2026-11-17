using System.ComponentModel.DataAnnotations;

namespace EncuestAPP__API.DTOs.Participaciones;

public sealed class CrearParticipacionDto
{
    [StringLength(100)]
    public string? IdentificadorDispositivo { get; set; }

    public List<RespuestaParticipanteDto> Respuestas { get; set; } = [];
}

public sealed class RespuestaParticipanteDto
{
    [Range(1, int.MaxValue)]
    public int PreguntaId { get; set; }

    public string? Texto { get; set; }
    public List<int> OpcionRespuestaIds { get; set; } = [];
}

public sealed record ParticipacionCreadaDto(int Id, DateTime FechaRespuesta);
