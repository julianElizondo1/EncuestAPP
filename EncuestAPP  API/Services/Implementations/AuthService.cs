using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EncuestAPP__API.Data;
using EncuestAPP__API.DTOs.Auth;
using EncuestAPP__API.Middleware;
using EncuestAPP__API.Models;
using EncuestAPP__API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EncuestAPP__API.Services.Implementations;

public sealed class AuthService(
    AppDbContext dbContext,
    IPasswordHasher<Usuario> passwordHasher,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public async Task<AuthRespuestaDto> RegistrarAsync(RegistroDto solicitud, CancellationToken cancellationToken)
    {
        var email = solicitud.Email.Trim().ToLowerInvariant();
        if (await dbContext.Usuarios.AnyAsync(x => x.Email == email, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Ya existe un usuario registrado con ese email.");
        }

        var usuario = new Usuario
        {
            Nombre = solicitud.Nombre.Trim(),
            Email = email,
            FechaRegistro = DateTime.UtcNow
        };
        usuario.PasswordHash = passwordHasher.HashPassword(usuario, solicitud.Contrasena);

        dbContext.Usuarios.Add(usuario);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (SqlServerErrorDetector.EsRestriccionUnica(exception))
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Ya existe un usuario registrado con ese email.");
        }

        return CrearRespuesta(usuario);
    }

    public async Task<AuthRespuestaDto> IniciarSesionAsync(InicioSesionDto solicitud, CancellationToken cancellationToken)
    {
        var email = solicitud.Email.Trim().ToLowerInvariant();
        var usuario = await dbContext.Usuarios.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
        if (usuario is null)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Email o contraseña incorrectos.");
        }

        var resultado = passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, solicitud.Contrasena);
        if (resultado == PasswordVerificationResult.Failed)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Email o contraseña incorrectos.");
        }

        if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.PasswordHash = passwordHasher.HashPassword(usuario, solicitud.Contrasena);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return CrearRespuesta(usuario);
    }

    private AuthRespuestaDto CrearRespuesta(Usuario usuario)
    {
        var secret = Encoding.UTF8.GetBytes(_jwt.Secret);
        if (secret.Length < 32)
        {
            throw new InvalidOperationException("Configure Jwt:Secret con al menos 32 bytes mediante User Secrets o una variable de entorno.");
        }

        var expira = DateTime.UtcNow.AddMinutes(Math.Clamp(_jwt.ExpirationMinutes, 1, 1440));
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Name, usuario.Nombre),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expira,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256));

        return new AuthRespuestaDto(
            new UsuarioAutenticadoDto(usuario.Id, usuario.Nombre, usuario.Email),
            new JwtSecurityTokenHandler().WriteToken(token),
            expira);
    }
}
