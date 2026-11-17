using EncuestAPP__API.Data;
using EncuestAPP__API.DTOs.Encuestas;
using EncuestAPP__API.DTOs.Participaciones;
using EncuestAPP__API.Middleware;
using EncuestAPP__API.Models;
using EncuestAPP__API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EncuestAPP__API.Services.Implementations;

public sealed class ParticipacionService(AppDbContext dbContext) : IParticipacionService
{
    public async Task<EncuestaPublicaDto> ObtenerEncuestaPublicaAsync(string tokenPublico, CancellationToken cancellationToken)
    {
        var encuesta = await ObtenerPorTokenAsync(tokenPublico, cancellationToken);
        var publicada = encuesta.EstadoEncuesta.Nombre == "Publicada";
        return new EncuestaPublicaDto(
            encuesta.Titulo,
            encuesta.Descripcion,
            encuesta.CategoriaEncuesta.Nombre,
            encuesta.EstadoEncuesta.Nombre,
            publicada,
            encuesta.EsAnonima,
            encuesta.UnaRespuestaPorDispositivo,
            encuesta.Preguntas.OrderBy(x => x.Orden).Select(x => new PreguntaEncuestaDto(
                x.Id, x.Texto, x.TipoPregunta.Nombre, x.Orden, x.Obligatoria,
                x.OpcionesRespuesta.OrderBy(o => o.Orden)
                    .Select(o => new OpcionEncuestaDto(o.Id, o.Texto, o.Orden)).ToList())).ToList());
    }

    public async Task<ParticipacionCreadaDto> CrearAsync(
        string tokenPublico, CrearParticipacionDto solicitud, CancellationToken cancellationToken)
    {
        var encuesta = await ObtenerPorTokenAsync(tokenPublico, cancellationToken);
        if (encuesta.EstadoEncuesta.Nombre != "Publicada")
        {
            throw new ApiException(StatusCodes.Status409Conflict, "La encuesta no admite nuevas participaciones.");
        }

        var identificador = encuesta.UnaRespuestaPorDispositivo
            ? solicitud.IdentificadorDispositivo?.Trim()
            : null;
        if (encuesta.UnaRespuestaPorDispositivo && string.IsNullOrWhiteSpace(identificador))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Esta encuesta requiere un identificador de dispositivo.");
        }

        if (!string.IsNullOrWhiteSpace(identificador) &&
            await dbContext.Participaciones.AnyAsync(x => x.EncuestaId == encuesta.Id && x.IdentificadorDispositivo == identificador, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Este dispositivo ya participó en la encuesta.");
        }

        var solicitudes = solicitud.Respuestas ?? [];
        if (solicitudes.Select(x => x.PreguntaId).Distinct().Count() != solicitudes.Count)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "No se puede enviar más de una respuesta para la misma pregunta.");
        }

        var preguntas = encuesta.Preguntas.ToDictionary(x => x.Id);
        if (solicitudes.Any(x => !preguntas.ContainsKey(x.PreguntaId)))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Una o más preguntas no pertenecen a esta encuesta.");
        }

        var porPregunta = solicitudes.ToDictionary(x => x.PreguntaId);
        var respuestasValidas = new List<(Pregunta Pregunta, string? Texto, List<OpcionRespuesta> Opciones)>();
        foreach (var pregunta in encuesta.Preguntas)
        {
            porPregunta.TryGetValue(pregunta.Id, out var respuesta);
            var texto = respuesta?.Texto;
            var opcionesIds = respuesta?.OpcionRespuestaIds ?? [];
            if (opcionesIds.Distinct().Count() != opcionesIds.Count)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"La respuesta a la pregunta {pregunta.Id} contiene opciones repetidas.");
            }

            var tipo = pregunta.TipoPregunta.Nombre;
            var esTexto = tipo is "Texto corto" or "Texto largo";
            var esSeleccionUnica = tipo is "Selección única" or "Escala de valoración";
            var esSeleccionMultiple = tipo == "Selección múltiple";
            if (!esTexto && !esSeleccionUnica && !esSeleccionMultiple)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"El tipo de la pregunta {pregunta.Id} no es válido.");
            }

            if (esTexto && opcionesIds.Count > 0)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"La pregunta de texto {pregunta.Id} no admite opciones.");
            }
            if (!esTexto && !string.IsNullOrWhiteSpace(texto))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"La pregunta {pregunta.Id} no admite una respuesta de texto.");
            }

            var tieneTexto = esTexto && !string.IsNullOrWhiteSpace(texto);
            var tieneOpciones = opcionesIds.Count > 0;
            if (!tieneTexto && !tieneOpciones)
            {
                if (pregunta.Obligatoria)
                {
                    throw new ApiException(StatusCodes.Status400BadRequest, $"La pregunta obligatoria {pregunta.Id} debe responderse.");
                }
                continue;
            }

            if (esTexto && !tieneTexto)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"La respuesta de texto de la pregunta {pregunta.Id} no puede estar vacía.");
            }
            if (esSeleccionUnica && opcionesIds.Count != 1)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"La pregunta {pregunta.Id} requiere exactamente una opción.");
            }
            if (esSeleccionMultiple && opcionesIds.Count == 0)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"La pregunta {pregunta.Id} requiere al menos una opción.");
            }

            var opciones = pregunta.OpcionesRespuesta.Where(x => opcionesIds.Contains(x.Id)).ToList();
            if (opciones.Count != opcionesIds.Count)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"Una o más opciones no pertenecen a la pregunta {pregunta.Id}.");
            }

            respuestasValidas.Add((pregunta, esTexto ? texto!.Trim() : null, opciones));
        }

        var participacion = new Participacion
        {
            EncuestaId = encuesta.Id,
            FechaRespuesta = DateTime.UtcNow,
            IdentificadorDispositivo = identificador
        };

        await using var transaccion = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            dbContext.Participaciones.Add(participacion);
            await dbContext.SaveChangesAsync(cancellationToken);

            foreach (var (pregunta, texto, opciones) in respuestasValidas)
            {
                var respuesta = new Respuesta
                {
                    ParticipacionId = participacion.Id,
                    PreguntaId = pregunta.Id,
                    Texto = texto
                };
                dbContext.Respuestas.Add(respuesta);
                foreach (var opcion in opciones)
                {
                    dbContext.RespuestasOpciones.Add(new RespuestaOpcion
                    {
                        Respuesta = respuesta,
                        OpcionRespuestaId = opcion.Id
                    });
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (SqlServerErrorDetector.EsRestriccionUnica(exception))
        {
            await transaccion.RollbackAsync(cancellationToken);
            throw new ApiException(StatusCodes.Status409Conflict,
                "No se pudo registrar la participación; puede existir un envío duplicado.");
        }

        return new ParticipacionCreadaDto(participacion.Id, participacion.FechaRespuesta);
    }

    private async Task<Encuesta> ObtenerPorTokenAsync(string tokenPublico, CancellationToken cancellationToken) =>
        await dbContext.Encuestas.AsNoTracking()
            .Include(x => x.EstadoEncuesta)
            .Include(x => x.CategoriaEncuesta)
            .Include(x => x.Preguntas).ThenInclude(x => x.TipoPregunta)
            .Include(x => x.Preguntas).ThenInclude(x => x.OpcionesRespuesta)
            .SingleOrDefaultAsync(x => x.TokenPublico == tokenPublico, cancellationToken)
        ?? throw new ApiException(StatusCodes.Status404NotFound, "No se encontró una encuesta con ese enlace público.");
}
