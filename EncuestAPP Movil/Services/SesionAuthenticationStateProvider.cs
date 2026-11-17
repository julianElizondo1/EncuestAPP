using System.Security.Claims;
using EncuestAPP_Movil.DTOs.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace EncuestAPP_Movil.Services;

public sealed class SesionAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly ISesionService _sesionService;

    public SesionAuthenticationStateProvider(ISesionService sesionService)
    {
        _sesionService = sesionService;
        _sesionService.EstadoSesionCambio += OnEstadoSesionCambio;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(new AuthenticationState(CrearPrincipal(_sesionService.EstaAutenticado ? _sesionService.Usuario : null)));

    public void Dispose() => _sesionService.EstadoSesionCambio -= OnEstadoSesionCambio;

    private void OnEstadoSesionCambio() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    private static ClaimsPrincipal CrearPrincipal(UsuarioAutenticadoDto? usuario)
    {
        if (usuario is null)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Email, usuario.Email)
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "EncuestAPP"));
    }
}
