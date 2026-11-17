using EncuestAPP__API.DTOs.Preguntas;
using EncuestAPP__API.Extensions;
using EncuestAPP__API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EncuestAPP__API.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public sealed class PreguntasController(IPreguntaService preguntaService) : ControllerBase
{
    [HttpGet("encuestas/{encuestaId:int}/preguntas")]
    public async Task<ActionResult<IReadOnlyList<PreguntaDto>>> Listar(int encuestaId, CancellationToken cancellationToken) =>
        Ok(await preguntaService.ListarAsync(encuestaId, User.ObtenerUsuarioId(), cancellationToken));

    [HttpPost("encuestas/{encuestaId:int}/preguntas")]
    [ProducesResponseType(typeof(PreguntaDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<PreguntaDto>> Crear(int encuestaId, CrearPreguntaDto solicitud, CancellationToken cancellationToken)
    {
        var pregunta = await preguntaService.CrearAsync(encuestaId, solicitud, User.ObtenerUsuarioId(), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, pregunta);
    }

    [HttpPut("preguntas/{id:int}")]
    public async Task<ActionResult<PreguntaDto>> Actualizar(int id, ActualizarPreguntaDto solicitud, CancellationToken cancellationToken) =>
        Ok(await preguntaService.ActualizarAsync(id, solicitud, User.ObtenerUsuarioId(), cancellationToken));

    [HttpDelete("preguntas/{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        await preguntaService.EliminarAsync(id, User.ObtenerUsuarioId(), cancellationToken);
        return NoContent();
    }

    [HttpPost("preguntas/{preguntaId:int}/opciones")]
    [ProducesResponseType(typeof(OpcionDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<OpcionDto>> CrearOpcion(int preguntaId, CrearOpcionDto solicitud, CancellationToken cancellationToken)
    {
        var opcion = await preguntaService.CrearOpcionAsync(preguntaId, solicitud, User.ObtenerUsuarioId(), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, opcion);
    }

    [HttpPut("opciones/{id:int}")]
    public async Task<ActionResult<OpcionDto>> ActualizarOpcion(int id, ActualizarOpcionDto solicitud, CancellationToken cancellationToken) =>
        Ok(await preguntaService.ActualizarOpcionAsync(id, solicitud, User.ObtenerUsuarioId(), cancellationToken));

    [HttpDelete("opciones/{id:int}")]
    public async Task<IActionResult> EliminarOpcion(int id, CancellationToken cancellationToken)
    {
        await preguntaService.EliminarOpcionAsync(id, User.ObtenerUsuarioId(), cancellationToken);
        return NoContent();
    }
}
