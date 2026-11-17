using System.Net;
using System.Net.Http.Headers;
using EncuestAPP_Movil.Services;

namespace EncuestAPP_Movil.Handlers;

public sealed class BearerTokenHandler(ISesionService sesionService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Options.TryGetValue(ApiRequestOptions.RequiresAuthorization, out var requiereAutorizacion) || !requiereAutorizacion)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        await sesionService.RestaurarAsync();
        var token = sesionService.Token;
        if (string.IsNullOrWhiteSpace(token))
        {
            await sesionService.CerrarSesionLocalAsync();
            throw new ApiClientException("La sesión no está disponible. Iniciá sesión nuevamente.", HttpStatusCode.Unauthorized);
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await sesionService.CerrarSesionLocalAsync();
        }

        return response;
    }
}
