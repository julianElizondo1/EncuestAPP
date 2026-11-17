namespace EncuestAPP__API.DTOs.Encuestas;

public sealed record EncuestaPublicaDto(
    string Titulo,
    string? Descripcion,
    string Categoria,
    string Estado,
    bool AdmiteRespuestas,
    bool EsAnonima,
    bool UnaRespuestaPorDispositivo,
    IReadOnlyList<PreguntaEncuestaDto> Preguntas);
