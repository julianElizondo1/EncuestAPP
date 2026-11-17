namespace EncuestAPP_Web.Contracts;

public sealed record EncuestaPublicaDto(
    string Titulo,
    string? Descripcion,
    string Categoria,
    string Estado,
    bool AdmiteRespuestas,
    bool EsAnonima,
    bool UnaRespuestaPorDispositivo,
    IReadOnlyList<PreguntaEncuestaDto> Preguntas);

public sealed record PreguntaEncuestaDto(
    int Id,
    string Texto,
    string Tipo,
    int Orden,
    bool Obligatoria,
    IReadOnlyList<OpcionEncuestaDto> Opciones);

public sealed record OpcionEncuestaDto(int Id, string Texto, int Orden);

public sealed class CrearParticipacionDto
{
    public string? IdentificadorDispositivo { get; set; }
    public List<RespuestaParticipanteDto> Respuestas { get; set; } = [];
}

public sealed class RespuestaParticipanteDto
{
    public int PreguntaId { get; set; }
    public string? Texto { get; set; }
    public List<int> OpcionRespuestaIds { get; set; } = [];
}

public sealed record ParticipacionCreadaDto(int Id, DateTime FechaRespuesta);
