using EncuestAPP__API.DTOs.Auth;
using EncuestAPP__API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EncuestAPP__API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("registro")]
    [ProducesResponseType(typeof(AuthRespuestaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthRespuestaDto>> Registrar(RegistroDto solicitud, CancellationToken cancellationToken)
    {
        var respuesta = await authService.RegistrarAsync(solicitud, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, respuesta);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthRespuestaDto>> IniciarSesion(InicioSesionDto solicitud, CancellationToken cancellationToken) =>
        Ok(await authService.IniciarSesionAsync(solicitud, cancellationToken));

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult CerrarSesion() => NoContent();
}
