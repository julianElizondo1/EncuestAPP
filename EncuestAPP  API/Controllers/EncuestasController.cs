using EncuestAPP__API.DTOs.Encuestas;
using EncuestAPP__API.Extensions;
using EncuestAPP__API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EncuestAPP__API.Controllers;

[ApiController]
[Authorize]
[Route("api/encuestas")]
public sealed class EncuestasController(IEncuestaService encuestaService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<EncuestaListaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginaDto<EncuestaListaDto>>> Listar(
        [FromQuery] string? estado,
        [FromQuery] int? categoriaId,
        [FromQuery] string? titulo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await encuestaService.ListarAsync(User.ObtenerUsuarioId(), estado, categoriaId, titulo, page, pageSize, cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EncuestaDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EncuestaDetalleDto>> Obtener(int id, CancellationToken cancellationToken) =>
        Ok(await encuestaService.ObtenerPropiaAsync(id, User.ObtenerUsuarioId(), cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(EncuestaDetalleDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<EncuestaDetalleDto>> Crear(CrearEncuestaDto solicitud, CancellationToken cancellationToken)
    {
        var encuesta = await encuestaService.CrearAsync(solicitud, User.ObtenerUsuarioId(), cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = encuesta.Id }, encuesta);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EncuestaDetalleDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EncuestaDetalleDto>> Actualizar(int id, ActualizarEncuestaDto solicitud, CancellationToken cancellationToken) =>
        Ok(await encuestaService.ActualizarAsync(id, solicitud, User.ObtenerUsuarioId(), cancellationToken));

    [HttpPost("{id:int}/publicar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Publicar(int id, CancellationToken cancellationToken)
    {
        await encuestaService.PublicarAsync(id, User.ObtenerUsuarioId(), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/cerrar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cerrar(int id, CancellationToken cancellationToken)
    {
        await encuestaService.CerrarAsync(id, User.ObtenerUsuarioId(), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/archivar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Archivar(int id, CancellationToken cancellationToken)
    {
        await encuestaService.ArchivarAsync(id, User.ObtenerUsuarioId(), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/nueva-version")]
    [ProducesResponseType(typeof(EncuestaDetalleDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<EncuestaDetalleDto>> CrearNuevaVersion(int id, CancellationToken cancellationToken)
    {
        var encuesta = await encuestaService.CrearNuevaVersionAsync(id, User.ObtenerUsuarioId(), cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = encuesta.Id }, encuesta);
    }
}
