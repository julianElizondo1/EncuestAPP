using System.ComponentModel.DataAnnotations;

namespace EncuestAPP_Movil.DTOs.Auth;

public sealed class RegistroRequestDto
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

public sealed class LoginRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Contrasena { get; set; } = string.Empty;
}

public sealed record UsuarioAutenticadoDto(int Id, string Nombre, string Email);
public sealed record AuthResponseDto(UsuarioAutenticadoDto Usuario, string Token, DateTime ExpiraEn);
