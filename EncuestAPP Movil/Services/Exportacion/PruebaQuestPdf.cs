#if DEBUG
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EncuestAPP_Movil.Services.Exportacion;

// Prueba mínima; no es el sistema completo de informes PDF.
public static class PruebaQuestPdf
{
    public static void Generar(string ruta)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        Document.Create(documento => documento.Page(pagina =>
        {
            pagina.Size(PageSizes.A4);
            pagina.Margin(30);
            pagina.Header().Text("EncuestAPP - Prueba QuestPDF").FontSize(20);
            pagina.Content().Column(columna =>
            {
                for (var i = 1; i <= 90; i++)
                    columna.Item().Text($"Pregunta {i}: selección múltiple, valoración y texto. 2 respuestas: 66,67 %.");
            });
            pagina.Footer().AlignCenter().Text(t => { t.Span("Página "); t.CurrentPageNumber(); });
        })).GeneratePdf(ruta);
    }
}
#endif
