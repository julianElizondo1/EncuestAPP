using EncuestAPP_Movil.DTOs.Auth;


namespace EncuestAPP_Movil.Services;

public interface ISesionService
{
    bool EstaAutenticado { get; }
    UsuarioAutenticadoDto? Usuario { get; }
    string? Token { get; }
    event Action? EstadoSesionCambio;
    Task<bool> RestaurarAsync();
    Task GuardarSesionAsync(AuthResponseDto sesion);
    Task CerrarSesionLocalAsync();
}
