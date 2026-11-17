namespace EncuestAPP__API.DTOs.Resultados;

public sealed record ResultadoGeneralDto(
    int EncuestaId,
    string Titulo,
    int TotalParticipaciones,
    DateTime FechaCreacion,
    DateTime? FechaPublicacion,
    DateTime? FechaCierre,
    string Estado);

public sealed record ResultadoPreguntaDto(
    int PreguntaId,
    string Texto,
    string Tipo,
    int TotalRespuestas,
    string NotaPorcentajes,
    IReadOnlyList<ResultadoOpcionDto> Opciones,
    IReadOnlyList<string> RespuestasTextuales);

public sealed record ResultadoOpcionDto(int OpcionRespuestaId, string Texto, int Cantidad, decimal Porcentaje);

public sealed record ActividadParticipacionesDto(
    DateOnly Desde, DateOnly Hasta, int DesfaseUtcMinutos, DateTimeOffset GeneradoEn,
    int ParticipacionesHoy, int ParticipacionesUltimosSieteDias,
    IReadOnlyList<ActividadDiariaDto> Dias);

public sealed record ActividadDiariaDto(DateOnly Fecha, int Cantidad);
