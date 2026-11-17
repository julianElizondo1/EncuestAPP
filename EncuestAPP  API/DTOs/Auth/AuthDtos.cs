using System.ComponentModel.DataAnnotations;

namespace EncuestAPP__API.DTOs.Auth;

public sealed class RegistroDto
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Contrasena { get; set; } = string.Empty;

    [Required, Compare(nameof(Contrasena))]
    public string ConfirmacionContrasena { get; set; } = string.Empty;
}

public sealed class InicioSesionDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Contrasena { get; set; } = string.Empty;
}

public sealed record UsuarioAutenticadoDto(int Id, string Nombre, string Email);
public sealed record AuthRespuestaDto(UsuarioAutenticadoDto Usuario, string Token, DateTime ExpiraEn);
