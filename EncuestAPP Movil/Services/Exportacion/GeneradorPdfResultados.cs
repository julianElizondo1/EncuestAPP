using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using EncuestAPP_Movil.DTOs.Resultados;

namespace EncuestAPP_Movil.Services.Exportacion;

public sealed class GeneradorPdfResultados
{
    private const string Marino = "#172554";
    private const string Naranja = "#F59E0B";
    private const string Gris = "#64748B";

    public void Generar(InformeResultados informe, Stream destino, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Proyecto académico: licencia Community. No requiere llamadas de activación.
        QuestPDF.Settings.License = LicenseType.Community;
        var estructura = informe.Encuesta.Preguntas.ToDictionary(p => p.Id);
        Document.Create(documento => documento.Page(pagina =>
        {
            pagina.Size(PageSizes.A4);
            pagina.Margin(35);
            pagina.DefaultTextStyle(e => e.FontSize(10).FontColor(Marino));
            // Un bloque de texto con numeración se recompone en cada página,
            // evitando que la versión histórica reutilice fragmentos estáticos incompletos.
            pagina.Header().PaddingBottom(12).BorderBottom(2).BorderColor(Naranja).Text(texto =>
            {
                texto.CurrentPageNumber().Format(numero => $"EncuestAPP | INFORME DE RESULTADOS | {numero}")
                    .Bold().FontSize(13).FontColor(Marino);
            });
            pagina.Content().PaddingVertical(15).Column(columna =>
            {
                columna.Spacing(8);
                columna.Item().Text(informe.Encuesta.Titulo).Bold().FontSize(21);
                columna.Item().Text($"Categoría: {informe.Encuesta.Categoria}   |   Estado: {informe.Resumen.Estado}").FontColor(Gris);
                if (!string.IsNullOrWhiteSpace(informe.Encuesta.Descripcion))
                    columna.Item().Text(informe.Encuesta.Descripcion);
                columna.Item().Background("#F1F5F9").Padding(12)
                    .Text($"Total de participaciones: {informe.Resumen.TotalParticipaciones}").Bold().FontSize(14);
                columna.Item().Text($"Creación: {Fecha(informe.Resumen.FechaCreacion)}").FontColor(Gris);
                if (informe.Resumen.FechaPublicacion is { } publicacion)
                    columna.Item().Text($"Publicación: {Fecha(publicacion)}").FontColor(Gris);
                if (informe.Resumen.FechaCierre is { } cierre)
                    columna.Item().Text($"Cierre: {Fecha(cierre)}").FontColor(Gris);
                columna.Item().Text($"Datos consultados: {informe.ConsultadoEn:dd/MM/yyyy HH:mm:ss zzz}").FontColor(Gris);
                columna.Item().Text($"Informe generado: {informe.GeneradoEn:dd/MM/yyyy HH:mm:ss zzz}").FontColor(Gris);
                columna.Item().Text("Datos agregados obtenidos mediante solicitudes independientes. No constituye una captura transaccional.").FontSize(8).FontColor(Gris);
                if (informe.Resumen.TotalParticipaciones == 0)
                    columna.Item().PaddingVertical(8).Text("Esta encuesta todavía no tiene participaciones.").Italic();
                if (informe.Preguntas.Count == 0)
                    columna.Item().Text("Esta encuesta no tiene preguntas.");
                foreach (var pregunta in informe.Preguntas)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    columna.Item().EnsureSpace(140).Column(seccion =>
                    {
                        seccion.Spacing(8);
                        // Reservar inicio sin mantener indivisible toda una pregunta o respuesta extensa.
                        seccion.Item().PaddingTop(12).Text($"{estructura[pregunta.PreguntaId].Orden}. {pregunta.Texto}").Bold().FontSize(13);
                        seccion.Item().Text($"{pregunta.Tipo}   |   {pregunta.TotalRespuestas} respuestas").FontSize(9).FontColor(Gris);
                        if (pregunta.Tipo is "Texto corto" or "Texto largo")
                        {
                            if (pregunta.RespuestasTextuales.Count == 0)
                                seccion.Item().Text("No se recibieron respuestas de texto.").Italic().FontColor(Gris);
                            for (var indice = 0; indice < pregunta.RespuestasTextuales.Count; indice++)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                var texto = pregunta.RespuestasTextuales[indice];
                                seccion.Item().EnsureSpace(35).PaddingTop(4).Text($"Respuesta {indice + 1}").Bold().FontSize(9);
                                seccion.Item().Text(string.IsNullOrWhiteSpace(texto) ? "(Respuesta vacía)" : texto);
                            }
                        }
                        else
                        {
                            if (pregunta.TotalRespuestas == 0)
                                seccion.Item().Text("Todavía no se recibieron respuestas para esta pregunta.").Italic().FontColor(Gris);
                            foreach (var opcion in pregunta.Opciones)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                seccion.Item().ShowEntire().PaddingTop(4).Column(bloque =>
                                {
                                    bloque.Spacing(4);
                                    bloque.Item().Text(opcion.Texto);
                                    bloque.Item().Text($"{opcion.Cantidad} respuesta(s) - {opcion.Porcentaje.ToString("0.##", CultureInfo.GetCultureInfo("es-AR"))}%").FontSize(9).FontColor(Gris);
                                    // Solo el dibujo se acota; el valor de la API permanece intacto.
                                    var ancho = (float)Math.Clamp(opcion.Porcentaje, 0m, 100m);
                                    bloque.Item().Height(7).Background("#E2E8F0").Row(barra =>
                                    {
                                        if (ancho > 0) barra.RelativeItem(ancho).Background(Naranja);
                                        if (ancho < 100) barra.RelativeItem(100 - ancho);
                                    });
                                });
                            }
                            if (!string.IsNullOrWhiteSpace(pregunta.NotaPorcentajes))
                                seccion.Item().Text(pregunta.NotaPorcentajes).FontSize(8).FontColor(Gris);
                        }
                    });
                }
            });
            pagina.Footer().BorderTop(1).BorderColor("#E2E8F0").PaddingTop(8).AlignCenter().Text(texto =>
            {
                texto.DefaultTextStyle(e => e.FontSize(8).FontColor(Gris));
                texto.CurrentPageNumber().Format(numero => $"EncuestAPP | Encuesta {informe.Encuesta.Id} | Página {numero}");
                texto.TotalPages().Format(total => $" de {total}");
            });
        })).GeneratePdf(destino);
        cancellationToken.ThrowIfCancellationRequested();
    }

    private static string Fecha(DateTime valor) => valor.ToLocalTime().ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("es-AR"));
}
