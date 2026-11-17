namespace EncuestAPP_Movil.Handlers;

public static class ApiRequestOptions
{
    public static readonly HttpRequestOptionsKey<bool> RequiresAuthorization = new("RequiresAuthorization");
}
