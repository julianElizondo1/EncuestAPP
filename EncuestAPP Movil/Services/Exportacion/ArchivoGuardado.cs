namespace EncuestAPP_Movil.Services.Exportacion;

// Resultado local del guardado. Ubicacion es una URI de documento en Android,
// no una ruta de almacenamiento que se pueda abrir con File.Open.
public sealed record ArchivoGuardado(string Ubicacion, string Nombre, string TipoContenido);
