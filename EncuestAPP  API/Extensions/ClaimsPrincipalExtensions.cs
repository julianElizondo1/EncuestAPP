using System.Security.Claims;
using EncuestAPP__API.Middleware;

namespace EncuestAPP__API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int ObtenerUsuarioId(this ClaimsPrincipal principal)
    {
        var valor = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(valor, out var usuarioId)
            ? usuarioId
            : throw new ApiException(StatusCodes.Status401Unauthorized, "El token no contiene un identificador de usuario válido.");
    }
}
