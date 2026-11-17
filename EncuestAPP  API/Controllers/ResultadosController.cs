using EncuestAPP__API.DTOs.Resultados;
using EncuestAPP__API.Extensions;
using EncuestAPP__API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EncuestAPP__API.Controllers;

[ApiController]
[Authorize]
[Route("api/encuestas/{id:int}/resultados")]
public sealed class ResultadosController(IResultadoService resultadoService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ResultadoGeneralDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoGeneralDto>> Resumen(int id, CancellationToken cancellationToken) =>
        Ok(await resultadoService.ObtenerResumenAsync(id, User.ObtenerUsuarioId(), cancellationToken));

    [HttpGet("preguntas/{preguntaId:int}")]
    [ProducesResponseType(typeof(ResultadoPreguntaDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPreguntaDto>> PorPregunta(int id, int preguntaId, CancellationToken cancellationToken) =>
        Ok(await resultadoService.ObtenerPorPreguntaAsync(id, preguntaId, User.ObtenerUsuarioId(), cancellationToken));
}
