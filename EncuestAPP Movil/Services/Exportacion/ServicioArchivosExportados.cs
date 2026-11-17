using System.Globalization;
using System.Text;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;

namespace EncuestAPP_Movil.Services.Exportacion;

public sealed partial class ServicioArchivosExportados(GeneradorExcelResultados excel, GeneradorPdfResultados pdf)
{
    private readonly SemaphoreSlim _generacion = new(1, 1);

    public async Task<string> GenerarAsync(InformeResultados informe, bool esPdf, CancellationToken cancellationToken)
    {
        await _generacion.WaitAsync(cancellationToken);
        try { return await GenerarEnDiscoAsync(informe, esPdf, cancellationToken); }
        finally { _generacion.Release(); }
    }

    private async Task<string> GenerarEnDiscoAsync(InformeResultados informe, bool esPdf, CancellationToken cancellationToken)
    {
        var carpeta = Path.Combine(FileSystem.CacheDirectory, "exportaciones");
        Directory.CreateDirectory(carpeta);
        foreach (var archivo in Directory.EnumerateFiles(carpeta))
        {
            try { if (File.GetLastWriteTimeUtc(archivo) < DateTime.UtcNow.AddDays(-7)) File.Delete(archivo); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        var titulo = new string(informe.Encuesta.Titulo.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .Select(c => char.IsAsciiLetterOrDigit(c) ? c : '_').Take(60).ToArray()).Trim('_');
        if (titulo.Length == 0) titulo = "Encuesta";
        var extension = esPdf ? "pdf" : "xlsx";
        var nombre = $"EncuestAPP_Resultados_{informe.Encuesta.Id}_{titulo}_{informe.GeneradoEn:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.{extension}";
        var ruta = Path.Combine(carpeta, nombre);
        try
        {
            await Task.Run(() =>
            {
                using var destino = new FileStream(ruta, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                if (esPdf) pdf.Generar(informe, destino, cancellationToken);
                else excel.Generar(informe, destino, cancellationToken);
            }, cancellationToken);
            return ruta;
        }
        catch
        {
            try { if (File.Exists(ruta)) File.Delete(ruta); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            throw;
        }
    }

    public Task CompartirAsync(string ruta) => MainThread.InvokeOnMainThreadAsync(() =>
        Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Resultados de EncuestAPP",
            File = new ShareFile(ruta, TipoContenido(ruta))
        }));

    public Task<ArchivoGuardado?> GuardarAsync(string ruta, CancellationToken cancellationToken) =>
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var destino = await GuardarEnDestinoAsync(ruta, cancellationToken);
            return destino is null ? null : new ArchivoGuardado(destino, Path.GetFileName(ruta), TipoContenido(ruta));
        });

    public Task<bool> AbrirAsync(ArchivoGuardado archivo) =>
        MainThread.InvokeOnMainThreadAsync(() => AbrirEnDestinoAsync(archivo));

    private partial Task<bool> AbrirEnDestinoAsync(ArchivoGuardado archivo);

#if !ANDROID && !WINDOWS
    private partial Task<bool> AbrirEnDestinoAsync(ArchivoGuardado archivo) =>
        throw new PlatformNotSupportedException("La apertura de informes está disponible en Android y Windows.");
#endif

    private static string TipoContenido(string ruta) => Path.GetExtension(ruta).Equals(".pdf", StringComparison.OrdinalIgnoreCase)
        ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private partial Task<string?> GuardarEnDestinoAsync(string ruta, CancellationToken cancellationToken);
}
