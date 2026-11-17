namespace EncuestAPP_Movil.Configuration;

public static class PublicWebConfiguration
{
    public const string BaseAddress = "https://encuestapp.runasp.net/";

    public static string CrearEnlaceEncuesta(string tokenPublico)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenPublico);

        var tokenCodificado = Uri.EscapeDataString(tokenPublico);
        var baseUri = new Uri(BaseAddress, UriKind.Absolute);
        return new Uri(baseUri, $"encuesta/{tokenCodificado}").AbsoluteUri;
    }
}
