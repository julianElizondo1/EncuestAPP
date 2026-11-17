using System.Net;


namespace EncuestAPP_Movil.Services;

public sealed class ApiClientException : Exception
{
    public ApiClientException(string message, HttpStatusCode? statusCode = null, IReadOnlyList<string>? validationErrors = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ValidationErrors = validationErrors ?? [];
    }

    public HttpStatusCode? StatusCode { get; }
    public IReadOnlyList<string> ValidationErrors { get; }
    public string MensajeConValidaciones => ValidationErrors.Count == 0
        ? Message : $"{Message} {string.Join(" ", ValidationErrors)}";
}
