using Android.App;
using Android.Content;
using Microsoft.Maui.ApplicationModel;

namespace EncuestAPP_Movil.Services.Exportacion;

public sealed partial class ServicioArchivosExportados
{
    private static int _siguienteCodigo = 1414;

    private partial Task<bool> AbrirEnDestinoAsync(ArchivoGuardado archivo)
    {
        var actividad = Platform.CurrentActivity
            ?? throw new InvalidOperationException("No hay una ventana Android disponible.");
        using var uri = Android.Net.Uri.Parse(archivo.Ubicacion)
            ?? throw new IOException("El destino del documento no es válido.");
        if (uri.Scheme != "content")
            throw new IOException("El documento no tiene una URI de acceso válida.");

        using var intent = new Intent(Intent.ActionView);
        intent.SetDataAndType(uri, archivo.TipoContenido);
        intent.AddFlags(ActivityFlags.GrantReadUriPermission);
        // El visor recibe permiso temporal de lectura de este documento concreto.
        intent.ClipData = ClipData.NewRawUri(archivo.Nombre, uri);
        try
        {
            // No usar ResolveActivity: la visibilidad de paquetes puede dar falsos negativos.
            actividad.StartActivity(intent);
            return Task.FromResult(true);
        }
        catch (ActivityNotFoundException)
        {
            return Task.FromResult(false);
        }
    }

    private partial async Task<string?> GuardarEnDestinoAsync(string ruta, CancellationToken cancellationToken)
    {
        var codigoGuardar = Interlocked.Increment(ref _siguienteCodigo) & 0xffff;
        var actividad = Platform.CurrentActivity as MainActivity
            ?? throw new InvalidOperationException("No hay una ventana Android disponible.");
        var respuesta = new TaskCompletionSource<Android.Net.Uri?>(TaskCreationOptions.RunContinuationsAsynchronously);
        void Recibir(int codigo, Result resultado, Intent? datos)
        {
            if (codigo == codigoGuardar) respuesta.TrySetResult(resultado == Result.Ok ? datos?.Data : null);
        }
        actividad.DocumentoSeleccionado += Recibir;
        try
        {
            using var intent = new Intent(Intent.ActionCreateDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType(TipoContenido(ruta));
            intent.PutExtra(Intent.ExtraTitle, Path.GetFileName(ruta));
            actividad.StartActivityForResult(intent, codigoGuardar);
            using var registro = cancellationToken.Register(() => respuesta.TrySetCanceled(cancellationToken));
            var uri = await respuesta.Task;
            if (uri is null) return null;
            try
            {
                using var destino = actividad.ContentResolver!.OpenOutputStream(uri, "wt")
                    ?? throw new IOException("No se pudo abrir el destino elegido.");
                await using var origen = File.OpenRead(ruta);
                await origen.CopyToAsync(destino, cancellationToken);
                await destino.FlushAsync(cancellationToken);
                return uri.ToString();
            }
            catch
            {
                // ACTION_CREATE_DOCUMENT crea un documento nuevo: quitarlo si quedó incompleto.
                try { Android.Provider.DocumentsContract.DeleteDocument(actividad.ContentResolver!, uri); }
                catch (Exception) { /* El proveedor puede no admitir borrado; conservar el error original. */ }
                throw;
            }
        }
        finally { actividad.DocumentoSeleccionado -= Recibir; }
    }
}
