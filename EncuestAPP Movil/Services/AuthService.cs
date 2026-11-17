using System.Net;
using EncuestAPP_Movil.DTOs.Auth;

namespace EncuestAPP_Movil.Services;

public sealed class AuthService(
    ApiClient apiClient,
    ISesionService sesionService) : IAuthService
{
    public async Task<AuthResponseDto> IniciarSesionAsync(LoginRequestDto solicitud, CancellationToken cancellationToken = default)
    {
        var respuesta = await apiClient.EnviarConRespuestaAsync<AuthResponseDto>(
            HttpMethod.Post, "api/auth/login", solicitud, false, cancellationToken);
        await sesionService.GuardarSesionAsync(respuesta);
        return respuesta;
    }

    public async Task<AuthResponseDto> RegistrarAsync(RegistroRequestDto solicitud, CancellationToken cancellationToken = default)
    {
        var respuesta = await apiClient.EnviarConRespuestaAsync<AuthResponseDto>(
            HttpMethod.Post, "api/auth/registro", solicitud, false, cancellationToken);
        await sesionService.GuardarSesionAsync(respuesta);
        return respuesta;
    }

    public async Task CerrarSesionAsync(CancellationToken cancellationToken = default)
    {
        await sesionService.RestaurarAsync();
        if (!sesionService.EstaAutenticado)
        {
            await sesionService.CerrarSesionLocalAsync();
            return;
        }

        try
        {
            await apiClient.EnviarSinRespuestaAsync(
                HttpMethod.Post, "api/auth/logout", requiereAutorizacion: true, cancellationToken: cancellationToken);
        }
        catch (ApiClientException exception) when (exception.StatusCode == HttpStatusCode.Unauthorized)
        {
        }
        finally
        {
            await sesionService.CerrarSesionLocalAsync();
        }
    }
}
