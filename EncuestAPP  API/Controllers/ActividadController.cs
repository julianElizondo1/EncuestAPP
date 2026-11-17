using System.ComponentModel.DataAnnotations;
using EncuestAPP__API.DTOs.Resultados;
using EncuestAPP__API.Extensions;
using EncuestAPP__API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EncuestAPP__API.Controllers;

[ApiController]
[Authorize]
[Route("api/resultados/actividad")]
public sealed class ActividadController(IResultadoService resultadoService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ActividadParticipacionesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ActividadParticipacionesDto>> Obtener(
        [FromQuery, Required, Range(-840, 840)] int? desfaseUtcMinutos, CancellationToken cancellationToken) =>
        Ok(await resultadoService.ObtenerActividadAsync(User.ObtenerUsuarioId(), desfaseUtcMinutos!.Value, cancellationToken));
}
