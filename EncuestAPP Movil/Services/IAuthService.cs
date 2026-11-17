using EncuestAPP_Movil.DTOs.Auth;


namespace EncuestAPP_Movil.Services;

public interface IAuthService
{
    Task<AuthResponseDto> IniciarSesionAsync(LoginRequestDto solicitud, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RegistrarAsync(RegistroRequestDto solicitud, CancellationToken cancellationToken = default);
    Task CerrarSesionAsync(CancellationToken cancellationToken = default);
}
