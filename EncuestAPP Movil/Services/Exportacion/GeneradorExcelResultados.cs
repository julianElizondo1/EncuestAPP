using ClosedXML.Excel;

namespace EncuestAPP_Movil.Services.Exportacion;

public sealed class GeneradorExcelResultados
{
    private const int MaxCaracteresCelda = 32767;
    private const int MaxFilas = 1048576;

    public void Generar(InformeResultados informe, Stream destino, CancellationToken cancellationToken = default)
    {
        using var libro = new XLWorkbook();
        var resumen = libro.AddWorksheet("Resumen");
        Encabezados(resumen, "Dato", "Valor");
        var fila = 2;
        void Dato(string nombre, string? valor)
        {
            // Una descripción extensa también se conserva en continuaciones.
            foreach (var parte in Partes(valor ?? string.Empty))
            {
                ComprobarFila(fila);
                resumen.Cell(fila, 1).Value = nombre;
                resumen.Cell(fila++, 2).Value = parte;
            }
        }
        Dato("Aplicación", "EncuestAPP");
        Dato("Encuesta ID", informe.Encuesta.Id.ToString());
        Dato("Título", informe.Encuesta.Titulo);
        Dato("Descripción", informe.Encuesta.Descripcion);
        Dato("Categoría", informe.Encuesta.Categoria);
        Dato("Estado", informe.Resumen.Estado);
        resumen.Cell(fila, 1).Value = "Total de participaciones";
        resumen.Cell(fila++, 2).Value = informe.Resumen.TotalParticipaciones;
        void Fecha(string nombre, DateTime? fecha)
        {
            if (fecha is null) return;
            resumen.Cell(fila, 1).Value = nombre;
            resumen.Cell(fila, 2).Value = fecha.Value;
            resumen.Cell(fila++, 2).Style.DateFormat.Format = "dd/mm/yyyy hh:mm:ss";
        }
        Fecha("Creación (hora local)", informe.Resumen.FechaCreacion.ToLocalTime());
        Fecha("Publicación (hora local)", informe.Resumen.FechaPublicacion?.ToLocalTime());
        Fecha("Cierre (hora local)", informe.Resumen.FechaCierre?.ToLocalTime());
        Fecha("Datos consultados (hora local)", informe.ConsultadoEn.LocalDateTime);
        Fecha("Informe generado (hora local)", informe.GeneradoEn.LocalDateTime);
        Dato("Zona horaria", TimeZoneInfo.Local.Id);
        Dato("Alcance", "Resultados agregados consultados mediante solicitudes independientes. No constituye una captura transaccional.");
        if (informe.Resumen.TotalParticipaciones == 0) Dato("Observación", "Encuesta sin participaciones.");
        Tabla(resumen, fila - 1, 2, "DatosGenerales");

        var preguntas = libro.AddWorksheet("Preguntas");
        Encabezados(preguntas, "Orden", "Pregunta ID", "Pregunta", "Tipo", "Obligatoria", "Total de respuestas", "Nota de porcentajes");
        var opciones = libro.AddWorksheet("Opciones");
        Encabezados(opciones, "Orden de pregunta", "Pregunta ID", "Pregunta", "Opción ID", "Opción", "Cantidad", "Porcentaje", "Nota del denominador");
        var textos = libro.AddWorksheet("Respuestas abiertas");
        Encabezados(textos, "Orden de pregunta", "Pregunta ID", "Pregunta", "Respuesta número", "Parte", "Texto");
        var fp = 2; var fo = 2; var ft = 2;
        var estructura = informe.Encuesta.Preguntas.ToDictionary(p => p.Id);
        foreach (var p in informe.Preguntas)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var original = estructura[p.PreguntaId];
            ComprobarFila(fp);
            preguntas.Cell(fp, 1).Value = original.Orden;
            preguntas.Cell(fp, 2).Value = p.PreguntaId;
            preguntas.Cell(fp, 3).Value = p.Texto;
            preguntas.Cell(fp, 4).Value = p.Tipo;
            preguntas.Cell(fp, 5).Value = original.Obligatoria ? "Sí" : "No";
            preguntas.Cell(fp, 6).Value = p.TotalRespuestas;
            preguntas.Cell(fp++, 7).Value = p.NotaPorcentajes;
            foreach (var o in p.Opciones)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ComprobarFila(fo);
                opciones.Cell(fo, 1).Value = original.Orden;
                opciones.Cell(fo, 2).Value = p.PreguntaId;
                opciones.Cell(fo, 3).Value = p.Texto;
                opciones.Cell(fo, 4).Value = o.OpcionRespuestaId;
                opciones.Cell(fo, 5).Value = o.Texto;
                opciones.Cell(fo, 6).Value = o.Cantidad;
                opciones.Cell(fo, 7).Value = o.Porcentaje / 100m;
                opciones.Cell(fo, 7).Style.NumberFormat.Format = "0.00%";
                opciones.Cell(fo++, 8).Value = p.NotaPorcentajes;
            }
            for (var i = 0; i < p.RespuestasTextuales.Count; i++)
            {
                var parte = 0;
                foreach (var texto in Partes(p.RespuestasTextuales[i]))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ComprobarFila(ft);
                    textos.Cell(ft, 1).Value = original.Orden;
                    textos.Cell(ft, 2).Value = p.PreguntaId;
                    textos.Cell(ft, 3).Value = p.Texto;
                    textos.Cell(ft, 4).Value = i + 1;
                    textos.Cell(ft, 5).Value = ++parte;
                    // Asignación de texto: nunca FormulaA1, incluso si comienza con =.
                    textos.Cell(ft++, 6).Value = texto;
                }
            }
        }
        Tabla(preguntas, fp - 1, 7, "ListadoPreguntas");
        Tabla(opciones, fo - 1, 8, "ResultadosOpciones");
        Tabla(textos, ft - 1, 6, "RespuestasAbiertas");
        resumen.Column(1).Width = 34; resumen.Column(2).Width = 85;
        preguntas.Column(3).Width = 65; preguntas.Column(7).Width = 75;
        opciones.Column(3).Width = 55; opciones.Column(5).Width = 45; opciones.Column(8).Width = 75;
        textos.Column(3).Width = 55; textos.Column(6).Width = 100;
        cancellationToken.ThrowIfCancellationRequested();
        libro.SaveAs(destino);
        cancellationToken.ThrowIfCancellationRequested();
    }

    private static void Encabezados(IXLWorksheet hoja, params string[] nombres)
    {
        for (var i = 0; i < nombres.Length; i++) hoja.Cell(1, i + 1).Value = nombres[i];
        hoja.Columns(1, nombres.Length).Width = 22;
        hoja.SheetView.FreezeRows(1);
        hoja.Row(1).Height = 32;
        var rango = hoja.Range(1, 1, 1, nombres.Length);
        rango.Style.Fill.BackgroundColor = XLColor.FromHtml("#172554");
        rango.Style.Font.FontColor = XLColor.White;
        rango.Style.Font.Bold = true;
        rango.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
        rango.Style.Border.BottomBorderColor = XLColor.FromHtml("#F59E0B");
    }

    private static void Tabla(IXLWorksheet hoja, int ultimaFila, int columnas, string nombre)
    {
        if (ultimaFila > 1)
        {
            var rango = hoja.Range(1, 1, ultimaFila, columnas);
            rango.CreateTable(nombre).Theme = XLTableTheme.TableStyleMedium2;
            rango.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            rango.Style.Alignment.WrapText = true;
        }
        // Anchos explícitos: no dependemos de fuentes Windows ni de AdjustToContents.
    }

    private static void ComprobarFila(int fila)
    {
        if (fila > MaxFilas) throw new InvalidOperationException("El informe supera el límite de filas de Excel. No se generó un archivo incompleto.");
    }

    private static IEnumerable<string> Partes(string texto)
    {
        if (texto.Length == 0) { yield return string.Empty; yield break; }
        for (var inicio = 0; inicio < texto.Length;)
        {
            var longitud = Math.Min(MaxCaracteresCelda, texto.Length - inicio);
            if (inicio + longitud < texto.Length && char.IsHighSurrogate(texto[inicio + longitud - 1])) longitud--;
            yield return texto.Substring(inicio, longitud);
            inicio += longitud;
        }
    }
}
