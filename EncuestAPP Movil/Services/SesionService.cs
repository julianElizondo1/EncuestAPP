using System.Text.Json;
using EncuestAPP_Movil.DTOs.Auth;
using Microsoft.Maui.Storage;
using Microsoft.Extensions.Logging;

namespace EncuestAPP_Movil.Services;

public sealed class SesionService(ILogger<SesionService> logger) : ISesionService
{
    private const string StorageKey = "encuestapp.session";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private AuthResponseDto? _sesion;
    private bool _restaurada;

    public bool EstaAutenticado => Token is not null && Usuario is not null;
    public UsuarioAutenticadoDto? Usuario => _sesion?.Usuario;
    public string? Token => _sesion is { Token.Length: > 0 } sesion && sesion.ExpiraEn > DateTime.UtcNow
        ? sesion.Token
        : null;

    public event Action? EstadoSesionCambio;

    public async Task<bool> RestaurarAsync()
    {
        if (_restaurada)
        {
            return EstaAutenticado;
        }

        try
        {
            var contenido = await SecureStorage.Default.GetAsync(StorageKey);
            if (!string.IsNullOrWhiteSpace(contenido))
            {
                _sesion = JsonSerializer.Deserialize<AuthResponseDto>(contenido, JsonOptions);
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "No se pudo restaurar la sesión almacenada de forma segura.");
            _sesion = null;
        }

        _restaurada = true;
        if (!EstaAutenticado)
        {
            await CerrarSesionLocalAsync();
            return false;
        }

        EstadoSesionCambio?.Invoke();
        return true;
    }

    public async Task GuardarSesionAsync(AuthResponseDto sesion)
    {
        if (string.IsNullOrWhiteSpace(sesion.Token) || sesion.Usuario is null || sesion.ExpiraEn <= DateTime.UtcNow)
        {
            throw new ApiClientException("La API devolvió una sesión incompleta o vencida.");
        }

        try
        {
            var contenido = JsonSerializer.Serialize(sesion, JsonOptions);
            await SecureStorage.Default.SetAsync(StorageKey, contenido);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "No se pudo guardar la sesión de forma segura.");
            throw new ApiClientException("No se pudo guardar la sesión en este dispositivo.", innerException: exception);
        }

        _sesion = sesion;
        _restaurada = true;
        EstadoSesionCambio?.Invoke();
    }

    public Task CerrarSesionLocalAsync()
    {
        _sesion = null;
        _restaurada = true;

        try
        {
            SecureStorage.Default.Remove(StorageKey);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "No se pudo eliminar la sesión almacenada de forma segura.");
        }

        EstadoSesionCambio?.Invoke();
        return Task.CompletedTask;
    }
}
