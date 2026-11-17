using EncuestAPP__API.Data;
using EncuestAPP__API.DTOs.Resultados;
using EncuestAPP__API.Middleware;
using EncuestAPP__API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EncuestAPP__API.Services.Implementations;

public sealed class ResultadoService(AppDbContext dbContext) : IResultadoService
{
    public async Task<ActividadParticipacionesDto> ObtenerActividadAsync(
        int usuarioId, int desfaseUtcMinutos, CancellationToken cancellationToken)
    {
        if (desfaseUtcMinutos is < -840 or > 840)
            throw new ApiException(StatusCodes.Status400BadRequest, "El desfase horario debe estar entre -840 y 840 minutos.");

        var periodo = PeriodoActividad.Crear(DateTimeOffset.UtcNow, desfaseUtcMinutos);
        var inicioUtc = periodo.InicioUtc;
        var finUtc = periodo.GeneradoEn.UtcDateTime;

        // Ownership and the UTC range are filtered before grouping in SQL Server.
        var cantidades = await dbContext.Participaciones.AsNoTracking()
            .Where(x => x.Encuesta.UsuarioId == usuarioId && x.FechaRespuesta >= inicioUtc && x.FechaRespuesta <= finUtc)
            .GroupBy(x => x.FechaRespuesta.AddMinutes(desfaseUtcMinutos).Date)
            .Select(g => new { Fecha = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => DateOnly.FromDateTime(x.Fecha), x => x.Cantidad, cancellationToken);

        return periodo.Completar(cantidades);
    }

    public async Task<ResultadoGeneralDto> ObtenerResumenAsync(int encuestaId, int usuarioId, CancellationToken cancellationToken)
    {
        var encuesta = await dbContext.Encuestas.AsNoTracking()
            .Where(x => x.Id == encuestaId && x.UsuarioId == usuarioId)
            .Select(x => new
            {
                x.Id,
                x.Titulo,
                x.FechaCreacion,
                x.FechaPublicacion,
                x.FechaCierre,
                Estado = x.EstadoEncuesta.Nombre,
                Total = x.Participaciones.Count
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw NoEncontrada();

        return new ResultadoGeneralDto(encuesta.Id, encuesta.Titulo, encuesta.Total,
            encuesta.FechaCreacion, encuesta.FechaPublicacion, encuesta.FechaCierre, encuesta.Estado);
    }

    public async Task<ResultadoPreguntaDto> ObtenerPorPreguntaAsync(
        int encuestaId, int preguntaId, int usuarioId, CancellationToken cancellationToken)
    {
        var pregunta = await dbContext.Preguntas.AsNoTracking()
            .Include(x => x.TipoPregunta)
            .Include(x => x.OpcionesRespuesta)
            .SingleOrDefaultAsync(x => x.Id == preguntaId && x.EncuestaId == encuestaId && x.Encuesta.UsuarioId == usuarioId, cancellationToken)
            ?? throw NoEncontrada();

        var total = await dbContext.Respuestas.CountAsync(x => x.PreguntaId == preguntaId, cancellationToken);
        var esTexto = pregunta.TipoPregunta.Nombre is "Texto corto" or "Texto largo";
        var textos = esTexto
            ? await dbContext.Respuestas.AsNoTracking()
                .Where(x => x.PreguntaId == preguntaId && x.Texto != null)
                .OrderBy(x => x.Id)
                .Select(x => x.Texto!)
                .ToListAsync(cancellationToken)
            : [];

        var opciones = new List<ResultadoOpcionDto>();
        if (!esTexto)
        {
            var cantidades = await dbContext.RespuestasOpciones.AsNoTracking()
                .Where(x => x.OpcionRespuesta.PreguntaId == preguntaId)
                .GroupBy(x => x.OpcionRespuestaId)
                .Select(g => new { OpcionId = g.Key, Cantidad = g.Count() })
                .ToDictionaryAsync(x => x.OpcionId, x => x.Cantidad, cancellationToken);

            opciones = pregunta.OpcionesRespuesta.OrderBy(x => x.Orden)
                .Select(x =>
                {
                    var cantidad = cantidades.GetValueOrDefault(x.Id);
                    var porcentaje = total == 0 ? 0 : Math.Round(cantidad * 100m / total, 2);
                    return new ResultadoOpcionDto(x.Id, x.Texto, cantidad, porcentaje);
                }).ToList();
        }

        var nota = pregunta.TipoPregunta.Nombre == "Selección múltiple"
            ? "Porcentajes calculados sobre las participaciones que respondieron esta pregunta; pueden sumar más del 100%."
            : "Porcentajes calculados sobre las respuestas recibidas para esta pregunta.";

        return new ResultadoPreguntaDto(
            pregunta.Id, pregunta.Texto, pregunta.TipoPregunta.Nombre, total, nota, opciones, textos);
    }

    private static ApiException NoEncontrada() =>
        new(StatusCodes.Status404NotFound, "La encuesta o pregunta no existe o no pertenece al usuario autenticado.");
}
