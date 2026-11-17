using EncuestAPP__API.DTOs.Encuestas;
using EncuestAPP__API.DTOs.Participaciones;
using EncuestAPP__API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EncuestAPP__API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/publico/encuestas")]
public sealed class ParticipacionesController(IParticipacionService participacionService) : ControllerBase
{
    [HttpGet("{tokenPublico}")]
    [ProducesResponseType(typeof(EncuestaPublicaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EncuestaPublicaDto>> ObtenerEncuesta(string tokenPublico, CancellationToken cancellationToken) =>
        Ok(await participacionService.ObtenerEncuestaPublicaAsync(tokenPublico, cancellationToken));

    [HttpPost("{tokenPublico}/participaciones")]
    [ProducesResponseType(typeof(ParticipacionCreadaDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ParticipacionCreadaDto>> Crear(
        string tokenPublico, CrearParticipacionDto solicitud, CancellationToken cancellationToken)
    {
        var resultado = await participacionService.CrearAsync(tokenPublico, solicitud, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, resultado);
    }
}
