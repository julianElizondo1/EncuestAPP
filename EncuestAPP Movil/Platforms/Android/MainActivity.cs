using Android.App;
using Android.Content.PM;
using Android.OS;

namespace EncuestAPP_Movil
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        internal event Action<int, Result, Android.Content.Intent?>? DocumentoSeleccionado;

        protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            DocumentoSeleccionado?.Invoke(requestCode, resultCode, data);
        }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

#if DEBUG
            Android.Webkit.WebView.SetWebContentsDebuggingEnabled(true);
            if (Intent?.GetBooleanExtra("encuestapp_export_probe", false) == true)
            {
                _ = Task.Run(() =>
                {
                    try
                    {
                        var ruta = System.IO.Path.Combine(CacheDir!.AbsolutePath, "questpdf-prueba.pdf");
                        Services.Exportacion.PruebaQuestPdf.Generar(ruta);
                        Services.Exportacion.PruebaExportacion.Generar(CacheDir!.AbsolutePath);
                        Android.Util.Log.Info("EncuestAPP.ExportProbe", $"OK: {ruta}");
                    }
                    catch (Exception exception)
                    {
                        Android.Util.Log.Error("EncuestAPP.ExportProbe", exception.ToString());
                    }
                });
            }
#endif
        }
    }
}
