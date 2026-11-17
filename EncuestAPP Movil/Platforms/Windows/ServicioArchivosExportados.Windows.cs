using Windows.Storage.Pickers;
using Microsoft.Maui.ApplicationModel;

namespace EncuestAPP_Movil.Services.Exportacion;

public sealed partial class ServicioArchivosExportados
{
    private partial async Task<bool> AbrirEnDestinoAsync(ArchivoGuardado archivo)
    {
        var documento = await Windows.Storage.StorageFile.GetFileFromPathAsync(archivo.Ubicacion);
        return await Windows.System.Launcher.LaunchFileAsync(documento);
    }

    private partial async Task<string?> GuardarEnDestinoAsync(string ruta, CancellationToken cancellationToken)
    {
        var ventana = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView
            ?? throw new InvalidOperationException("No hay una ventana Windows disponible.");
        var selector = new FileSavePicker { SuggestedFileName = Path.GetFileNameWithoutExtension(ruta) };
        var esPdf = Path.GetExtension(ruta).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
        selector.FileTypeChoices.Add(esPdf ? "PDF" : "Excel", new[] { esPdf ? ".pdf" : ".xlsx" });
        WinRT.Interop.InitializeWithWindow.Initialize(selector, WinRT.Interop.WindowNative.GetWindowHandle(ventana));
        var destino = await selector.PickSaveFileAsync();
        cancellationToken.ThrowIfCancellationRequested();
        if (destino is null) return null;
        await using var origen = File.OpenRead(ruta);
        using var transaccion = await destino.OpenTransactedWriteAsync();
        using var salida = transaccion.Stream.AsStreamForWrite();
        salida.SetLength(0);
        await origen.CopyToAsync(salida, cancellationToken);
        await salida.FlushAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        await transaccion.CommitAsync();
        return destino.Path;
    }
}
