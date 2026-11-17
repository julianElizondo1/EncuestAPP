using System.Security.Cryptography;
using EncuestAPP__API.Data;
using EncuestAPP__API.DTOs.Encuestas;
using EncuestAPP__API.Middleware;
using EncuestAPP__API.Models;
using EncuestAPP__API.Services.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace EncuestAPP__API.Services.Implementations;

public sealed class EncuestaService(AppDbContext dbContext) : IEncuestaService
{
    public async Task<PaginaDto<EncuestaListaDto>> ListarAsync(
        int usuarioId, string? estado, int? categoriaId, string? titulo, int pagina, int tamanoPagina,
        CancellationToken cancellationToken)
    {
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Clamp(tamanoPagina, 1, 100);
        var consulta = dbContext.Encuestas.AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId);

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estadoNormalizado = estado.Trim();
            consulta = consulta.Where(x => x.EstadoEncuesta.Nombre == estadoNormalizado);
        }

        if (categoriaId.HasValue)
        {
            consulta = consulta.Where(x => x.CategoriaEncuestaId == categoriaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(titulo))
        {
            var filtro = titulo.Trim();
            consulta = consulta.Where(x => x.Titulo.Contains(filtro));
        }

        var total = await consulta.CountAsync(cancellationToken);
        var encuestas = await consulta
            .OrderByDescending(x => x.FechaCreacion)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Select(x => new EncuestaListaDto(
                x.Id, x.Titulo, x.Descripcion, x.EstadoEncuesta.Nombre, x.CategoriaEncuesta.Nombre,
                x.FechaCreacion, x.FechaPublicacion, x.FechaCierre))
            .ToListAsync(cancellationToken);

        return new PaginaDto<EncuestaListaDto>(encuestas, pagina, tamanoPagina, total,
            (int)Math.Ceiling(total / (double)tamanoPagina));
    }

    public async Task<EncuestaDetalleDto> ObtenerPropiaAsync(int id, int usuarioId, CancellationToken cancellationToken)
    {
        var encuesta = await ConsultaDetalle().SingleOrDefaultAsync(
            x => x.Id == id && x.UsuarioId == usuarioId, cancellationToken);
        return encuesta is null ? throw NoEncontrada() : Mapear(encuesta);
    }

    public async Task<EncuestaDetalleDto> CrearAsync(CrearEncuestaDto solicitud, int usuarioId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(solicitud.Titulo))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "El título de la encuesta es obligatorio.");
        }

        if (!await dbContext.CategoriasEncuesta.AnyAsync(x => x.Id == solicitud.CategoriaEncuestaId, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "La categoría indicada no existe.");
        }

        var borradorId = await ObtenerEstadoIdAsync("Borrador", cancellationToken);
        var encuesta = new Encuesta
        {
            Titulo = solicitud.Titulo.Trim(),
            Descripcion = solicitud.Descripcion,
            FechaCreacion = DateTime.UtcNow,
            TokenPublico = await GenerarTokenAsync(cancellationToken),
            EsAnonima = solicitud.EsAnonima,
            UnaRespuestaPorDispositivo = solicitud.UnaRespuestaPorDispositivo,
            UsuarioId = usuarioId,
            CategoriaEncuestaId = solicitud.CategoriaEncuestaId,
            EstadoEncuestaId = borradorId
        };

        dbContext.Encuestas.Add(encuesta);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await ObtenerPropiaAsync(encuesta.Id, usuarioId, cancellationToken);
    }

    public async Task<EncuestaDetalleDto> ActualizarAsync(
        int id, ActualizarEncuestaDto solicitud, int usuarioId, CancellationToken cancellationToken)
    {
        var encuesta = await ObtenerEncuestaPropiaAsync(id, usuarioId, cancellationToken);
        if (string.IsNullOrWhiteSpace(solicitud.Titulo))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "El título de la encuesta es obligatorio.");
        }

        if (encuesta.EstadoEncuesta.Nombre is not ("Borrador" or "Publicada"))
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Solo se pueden editar encuestas en borrador o publicadas.");
        }

        if (encuesta.EstadoEncuesta.Nombre == "Publicada" &&
            await dbContext.Participaciones.AnyAsync(x => x.EncuestaId == id, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status409Conflict, "No se puede modificar una encuesta publicada que ya recibió participaciones.");
        }

        if (!await dbContext.CategoriasEncuesta.AnyAsync(x => x.Id == solicitud.CategoriaEncuestaId, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "La categoría indicada no existe.");
        }

        encuesta.Titulo = solicitud.Titulo.Trim();
        encuesta.Descripcion = solicitud.Descripcion;
        encuesta.CategoriaEncuestaId = solicitud.CategoriaEncuestaId;
        encuesta.EsAnonima = solicitud.EsAnonima;
        encuesta.UnaRespuestaPorDispositivo = solicitud.UnaRespuestaPorDispositivo;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await ObtenerPropiaAsync(id, usuarioId, cancellationToken);
    }

    public async Task PublicarAsync(int id, int usuarioId, CancellationToken cancellationToken)
    {
        var encuesta = await dbContext.Encuestas
            .Include(x => x.EstadoEncuesta)
            .Include(x => x.Preguntas).ThenInclude(x => x.TipoPregunta)
            .Include(x => x.Preguntas).ThenInclude(x => x.OpcionesRespuesta)
            .SingleOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuarioId, cancellationToken)
            ?? throw NoEncontrada();

        if (encuesta.EstadoEncuesta.Nombre != "Borrador")
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Solo se puede publicar una encuesta en borrador.");
        }

        if (string.IsNullOrWhiteSpace(encuesta.Titulo) || encuesta.Preguntas.Count == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "La encuesta debe tener título y al menos una pregunta.");
        }

        foreach (var pregunta in encuesta.Preguntas)
        {
            var tipo = pregunta.TipoPregunta.Nombre;
            var esTexto = tipo is "Texto corto" or "Texto largo";
            var esSeleccion = tipo is "Selección única" or "Selección múltiple" or "Escala de valoración";
            if (string.IsNullOrWhiteSpace(pregunta.Texto) || (!esTexto && !esSeleccion) ||
                (esSeleccion && pregunta.OpcionesRespuesta.Count == 0) ||
                (esTexto && pregunta.OpcionesRespuesta.Count > 0))
            {
                throw new ApiException(StatusCodes.Status400BadRequest,
                    $"La pregunta {pregunta.Id} no tiene una configuración válida para su tipo.");
            }
        }

        encuesta.EstadoEncuestaId = await ObtenerEstadoIdAsync("Publicada", cancellationToken);
        encuesta.FechaPublicacion = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CerrarAsync(int id, int usuarioId, CancellationToken cancellationToken)
    {
        var encuesta = await ObtenerEncuestaPropiaAsync(id, usuarioId, cancellationToken);
        if (encuesta.EstadoEncuesta.Nombre != "Publicada")
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Solo se puede cerrar una encuesta publicada.");
        }

        encuesta.EstadoEncuestaId = await ObtenerEstadoIdAsync("Cerrada", cancellationToken);
        encuesta.FechaCierre = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ArchivarAsync(int id, int usuarioId, CancellationToken cancellationToken)
    {
        var encuesta = await ObtenerEncuestaPropiaAsync(id, usuarioId, cancellationToken);
        if (encuesta.EstadoEncuesta.Nombre is not ("Borrador" or "Cerrada"))
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Solo se puede archivar una encuesta en borrador o cerrada.");
        }

        encuesta.EstadoEncuestaId = await ObtenerEstadoIdAsync("Archivada", cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<EncuestaDetalleDto> CrearNuevaVersionAsync(int id, int usuarioId, CancellationToken cancellationToken)
    {
        var original = await ConsultaDetalle()
            .SingleOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuarioId, cancellationToken)
            ?? throw NoEncontrada();

        if (original.EstadoEncuesta.Nombre is not ("Publicada" or "Cerrada"))
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Solo se puede versionar una encuesta publicada o cerrada.");
        }

        var nueva = new Encuesta
        {
            Titulo = original.Titulo,
            Descripcion = original.Descripcion,
            FechaCreacion = DateTime.UtcNow,
            TokenPublico = await GenerarTokenAsync(cancellationToken),
            EsAnonima = original.EsAnonima,
            UnaRespuestaPorDispositivo = original.UnaRespuestaPorDispositivo,
            UsuarioId = usuarioId,
            EstadoEncuestaId = await ObtenerEstadoIdAsync("Borrador", cancellationToken),
            CategoriaEncuestaId = original.CategoriaEncuestaId,
            EncuestaAnteriorId = original.Id,
            Preguntas = original.Preguntas.Select(p => new Pregunta
            {
                Texto = p.Texto,
                TipoPreguntaId = p.TipoPreguntaId,
                Orden = p.Orden,
                Obligatoria = p.Obligatoria,
                OpcionesRespuesta = p.OpcionesRespuesta.Select(o => new OpcionRespuesta
                {
                    Texto = o.Texto,
                    Orden = o.Orden
                }).ToList()
            }).ToList()
        };

        dbContext.Encuestas.Add(nueva);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await ObtenerPropiaAsync(nueva.Id, usuarioId, cancellationToken);
    }

    private IQueryable<Encuesta> ConsultaDetalle() => dbContext.Encuestas.AsNoTracking()
        .Include(x => x.EstadoEncuesta)
        .Include(x => x.CategoriaEncuesta)
        .Include(x => x.Preguntas).ThenInclude(x => x.TipoPregunta)
        .Include(x => x.Preguntas).ThenInclude(x => x.OpcionesRespuesta);

    private async Task<Encuesta> ObtenerEncuestaPropiaAsync(int id, int usuarioId, CancellationToken cancellationToken) =>
        await dbContext.Encuestas
            .Include(x => x.EstadoEncuesta)
            .SingleOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuarioId, cancellationToken)
        ?? throw NoEncontrada();

    private async Task<int> ObtenerEstadoIdAsync(string nombre, CancellationToken cancellationToken) =>
        await dbContext.EstadosEncuesta.Where(x => x.Nombre == nombre).Select(x => x.Id)
            .SingleOrDefaultAsync(cancellationToken) is var id && id != 0
            ? id
            : throw new InvalidOperationException($"No existe el estado de encuesta '{nombre}' en el catálogo.");

    private async Task<string> GenerarTokenAsync(CancellationToken cancellationToken)
    {
        string token;
        do
        {
            token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(24));
        } while (await dbContext.Encuestas.AnyAsync(x => x.TokenPublico == token, cancellationToken));

        return token;
    }

    private static EncuestaDetalleDto Mapear(Encuesta encuesta) => new(
        encuesta.Id,
        encuesta.Titulo,
        encuesta.Descripcion,
        encuesta.EstadoEncuesta.Nombre,
        encuesta.CategoriaEncuesta.Nombre,
        encuesta.EsAnonima,
        encuesta.UnaRespuestaPorDispositivo,
        encuesta.TokenPublico,
        encuesta.FechaCreacion,
        encuesta.FechaPublicacion,
        encuesta.FechaCierre,
        encuesta.Preguntas.OrderBy(x => x.Orden).Select(x => new PreguntaEncuestaDto(
            x.Id, x.Texto, x.TipoPregunta.Nombre, x.Orden, x.Obligatoria,
            x.OpcionesRespuesta.OrderBy(o => o.Orden).Select(o => new OpcionEncuestaDto(o.Id, o.Texto, o.Orden)).ToList())).ToList());

    private static ApiException NoEncontrada() => new(StatusCodes.Status404NotFound, "La encuesta no existe o no pertenece al usuario autenticado.");
}
