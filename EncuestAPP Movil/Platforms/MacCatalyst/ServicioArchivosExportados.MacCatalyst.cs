namespace EncuestAPP_Movil.Services.Exportacion;
public sealed partial class ServicioArchivosExportados
{
    private partial Task<string?> GuardarEnDestinoAsync(string ruta, CancellationToken cancellationToken) =>
        throw new PlatformNotSupportedException("El guardado de informes está disponible en Android y Windows.");
}
