using EncuestAPP__API.DTOs.Auth;

namespace EncuestAPP__API.Services.Interfaces;

public interface IAuthService
{
    Task<AuthRespuestaDto> RegistrarAsync(RegistroDto solicitud, CancellationToken cancellationToken);
    Task<AuthRespuestaDto> IniciarSesionAsync(InicioSesionDto solicitud, CancellationToken cancellationToken);
}
