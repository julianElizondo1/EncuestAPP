using EncuestAPP__API.Data;
using EncuestAPP__API.DTOs.Encuestas;
using EncuestAPP__API.DTOs.Preguntas;
using EncuestAPP__API.Middleware;
using EncuestAPP__API.Models;
using EncuestAPP__API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EncuestAPP__API.Services.Implementations;

public sealed class PreguntaService(AppDbContext dbContext) : IPreguntaService
{
    public async Task<IReadOnlyList<PreguntaDto>> ListarAsync(int encuestaId, int usuarioId, CancellationToken cancellationToken)
    {
        await VerificarPropiedadAsync(encuestaId, usuarioId, cancellationToken);
        var preguntas = await dbContext.Preguntas.AsNoTracking()
            .Where(x => x.EncuestaId == encuestaId)
            .Include(x => x.TipoPregunta)
            .Include(x => x.OpcionesRespuesta)
            .OrderBy(x => x.Orden)
            .ToListAsync(cancellationToken);
        return preguntas.Select(Mapear).ToList();
    }

    public async Task<PreguntaDto> CrearAsync(int encuestaId, CrearPreguntaDto solicitud, int usuarioId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(solicitud.Texto))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "El texto de la pregunta es obligatorio.");
        }

        var encuesta = await VerificarEditableAsync(encuestaId, usuarioId, cancellationToken);
        var tipo = await dbContext.TiposPregunta.SingleOrDefaultAsync(x => x.Id == solicitud.TipoPreguntaId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status400BadRequest, "El tipo de pregunta indicado no existe.");

        var pregunta = new Pregunta
        {
            EncuestaId = encuesta.Id,
            TipoPreguntaId = tipo.Id,
            Texto = solicitud.Texto.Trim(),
            Orden = solicitud.Orden,
            Obligatoria = solicitud.Obligatoria
        };
        dbContext.Preguntas.Add(pregunta);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await ObtenerDtoAsync(pregunta.Id, cancellationToken);
    }

    public async Task<PreguntaDto> ActualizarAsync(int id, ActualizarPreguntaDto solicitud, int usuarioId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(solicitud.Texto))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "El texto de la pregunta es obligatorio.");
        }

        var pregunta = await ObtenerPreguntaAsync(id, usuarioId, cancellationToken);
        await VerificarEditableAsync(pregunta.EncuestaId, usuarioId, cancellationToken);
        var tipo = await dbContext.TiposPregunta.SingleOrDefaultAsync(x => x.Id == solicitud.TipoPreguntaId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status400BadRequest, "El tipo de pregunta indicado no existe.");

        if (EsTexto(tipo.Nombre) && pregunta.OpcionesRespuesta.Count > 0)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Eliminá las opciones antes de cambiar la pregunta a un tipo de texto.");
        }

        pregunta.Texto = solicitud.Texto.Trim();
        pregunta.TipoPreguntaId = solicitud.TipoPreguntaId;
        pregunta.Orden = solicitud.Orden;
        pregunta.Obligatoria = solicitud.Obligatoria;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await ObtenerDtoAsync(id, cancellationToken);
    }

    public async Task EliminarAsync(int id, int usuarioId, CancellationToken cancellationToken)
    {
        var pregunta = await ObtenerPreguntaAsync(id, usuarioId, cancellationToken);
        await VerificarEditableAsync(pregunta.EncuestaId, usuarioId, cancellationToken);
        dbContext.OpcionesRespuesta.RemoveRange(pregunta.OpcionesRespuesta);
        dbContext.Preguntas.Remove(pregunta);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<OpcionDto> CrearOpcionAsync(int preguntaId, CrearOpcionDto solicitud, int usuarioId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(solicitud.Texto))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "El texto de la opción es obligatorio.");
        }

        var pregunta = await ObtenerPreguntaAsync(preguntaId, usuarioId, cancellationToken);
        await VerificarEditableAsync(pregunta.EncuestaId, usuarioId, cancellationToken);
        VerificarAdmiteOpciones(pregunta.TipoPregunta.Nombre);

        var opcion = new OpcionRespuesta
        {
            PreguntaId = pregunta.Id,
            Texto = solicitud.Texto.Trim(),
            Orden = solicitud.Orden
        };
        dbContext.OpcionesRespuesta.Add(opcion);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new OpcionDto(opcion.Id, opcion.PreguntaId, opcion.Texto, opcion.Orden);
    }

    public async Task<OpcionDto> ActualizarOpcionAsync(int id, ActualizarOpcionDto solicitud, int usuarioId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(solicitud.Texto))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "El texto de la opción es obligatorio.");
        }

        var opcion = await ObtenerOpcionAsync(id, usuarioId, cancellationToken);
        await VerificarEditableAsync(opcion.Pregunta.EncuestaId, usuarioId, cancellationToken);
        opcion.Texto = solicitud.Texto.Trim();
        opcion.Orden = solicitud.Orden;
        await dbContext.SaveChangesAsync(cancellationToken);
        return new OpcionDto(opcion.Id, opcion.PreguntaId, opcion.Texto, opcion.Orden);
    }

    public async Task EliminarOpcionAsync(int id, int usuarioId, CancellationToken cancellationToken)
    {
        var opcion = await ObtenerOpcionAsync(id, usuarioId, cancellationToken);
        await VerificarEditableAsync(opcion.Pregunta.EncuestaId, usuarioId, cancellationToken);
        dbContext.OpcionesRespuesta.Remove(opcion);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Pregunta> ObtenerPreguntaAsync(int id, int usuarioId, CancellationToken cancellationToken) =>
        await dbContext.Preguntas
            .Include(x => x.Encuesta).ThenInclude(x => x.EstadoEncuesta)
            .Include(x => x.TipoPregunta)
            .Include(x => x.OpcionesRespuesta)
            .SingleOrDefaultAsync(x => x.Id == id && x.Encuesta.UsuarioId == usuarioId, cancellationToken)
        ?? throw new ApiException(StatusCodes.Status404NotFound, "La pregunta no existe o no pertenece al usuario autenticado.");

    private async Task<OpcionRespuesta> ObtenerOpcionAsync(int id, int usuarioId, CancellationToken cancellationToken) =>
        await dbContext.OpcionesRespuesta
            .Include(x => x.Pregunta).ThenInclude(x => x.Encuesta).ThenInclude(x => x.EstadoEncuesta)
            .Include(x => x.Pregunta).ThenInclude(x => x.TipoPregunta)
            .SingleOrDefaultAsync(x => x.Id == id && x.Pregunta.Encuesta.UsuarioId == usuarioId, cancellationToken)
        ?? throw new ApiException(StatusCodes.Status404NotFound, "La opción no existe o no pertenece al usuario autenticado.");

    private async Task<Encuesta> VerificarEditableAsync(int encuestaId, int usuarioId, CancellationToken cancellationToken)
    {
        var encuesta = await dbContext.Encuestas.Include(x => x.EstadoEncuesta)
            .SingleOrDefaultAsync(x => x.Id == encuestaId && x.UsuarioId == usuarioId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "La encuesta no existe o no pertenece al usuario autenticado.");
        if (encuesta.EstadoEncuesta.Nombre != "Borrador")
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Las preguntas y opciones solo se pueden administrar en encuestas en borrador.");
        }

        if (await dbContext.Participaciones.AnyAsync(x => x.EncuestaId == encuestaId, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status409Conflict, "No se puede modificar la estructura de una encuesta que ya tiene participaciones.");
        }

        return encuesta;
    }

    private async Task VerificarPropiedadAsync(int encuestaId, int usuarioId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Encuestas.AnyAsync(x => x.Id == encuestaId && x.UsuarioId == usuarioId, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status404NotFound, "La encuesta no existe o no pertenece al usuario autenticado.");
        }
    }

    private async Task<PreguntaDto> ObtenerDtoAsync(int id, CancellationToken cancellationToken)
    {
        var pregunta = await dbContext.Preguntas.AsNoTracking()
            .Include(x => x.TipoPregunta).Include(x => x.OpcionesRespuesta)
            .SingleAsync(x => x.Id == id, cancellationToken);
        return Mapear(pregunta);
    }

    private static PreguntaDto Mapear(Pregunta pregunta) => new(
        pregunta.Id, pregunta.EncuestaId, pregunta.TipoPreguntaId, pregunta.TipoPregunta.Nombre,
        pregunta.Texto, pregunta.Orden, pregunta.Obligatoria,
        pregunta.OpcionesRespuesta.OrderBy(x => x.Orden)
            .Select(x => new OpcionEncuestaDto(x.Id, x.Texto, x.Orden)).ToList());

    private static bool EsTexto(string tipo) => tipo is "Texto corto" or "Texto largo";
    private static void VerificarAdmiteOpciones(string tipo)
    {
        if (EsTexto(tipo))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Las preguntas de texto no admiten opciones de respuesta.");
        }
    }
}
