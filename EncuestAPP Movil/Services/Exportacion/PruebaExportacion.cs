#if DEBUG
using ClosedXML.Excel;
using EncuestAPP_Movil.DTOs.Encuestas;
using EncuestAPP_Movil.DTOs.Resultados;

namespace EncuestAPP_Movil.Services.Exportacion;

public static class PruebaExportacion
{
    public static void Generar(string carpeta)
    {
        foreach (var total in new[] { 0, 3 })
        {
            var preguntas = new[]
            {
                new PreguntaEncuestaDto(1, "Selección única", "Selección única", 1, true, []),
                new PreguntaEncuestaDto(2, "Selección múltiple", "Selección múltiple", 2, true, []),
                new PreguntaEncuestaDto(3, "Escala", "Escala de valoración", 3, false, []),
                new PreguntaEncuestaDto(4, "Comentarios extensos", "Texto largo", 4, false, [])
            };
            var encuesta = new EncuestaDetalleDto(42, "Verificación de exportación", null, "Publicada", "Académica", true, false,
                "token-no-exportable", DateTime.UtcNow, DateTime.UtcNow, null, preguntas);
            var resumen = new ResultadoGeneralDto(42, encuesta.Titulo, total, DateTime.UtcNow, DateTime.UtcNow, null, "Publicada");
            var largo = "=SUM(A1:A2) " + new string('a', 70000);
            var resultados = preguntas.ToDictionary(p => p.Id, p => new ResultadoPreguntaDto(p.Id, p.Texto, p.Tipo, total,
                p.Id == 2 ? "Los porcentajes pueden sumar más del 100%." : "Porcentajes sobre las respuestas recibidas.",
                p.Id == 4 ? [] : new[]
                {
                    new ResultadoOpcionDto(1, "A", total == 0 ? 0 : 2, total == 0 ? 0 : 66.67m),
                    new ResultadoOpcionDto(2, "B", total == 0 ? 0 : p.Id == 2 ? 2 : 1, total == 0 ? 0 : p.Id == 2 ? 66.67m : 33.33m)
                }, p.Id == 4 && total > 0 ? new[] { largo, "Respuesta adicional." } : []));
            var informe = InformeResultados.Crear(encuesta, resumen, resultados, DateTimeOffset.Now);
            var excel = Path.Combine(carpeta, $"prueba-resultados-{total}.xlsx");
            using (var destino = File.Create(excel)) new GeneradorExcelResultados().Generar(informe, destino);
            using (var libro = new XLWorkbook(excel))
            {
                if (libro.Worksheets.Count != 4) throw new InvalidOperationException("Excel incompleto.");
                if (total > 0)
                {
                    var hoja = libro.Worksheet("Respuestas abiertas");
                    var texto = string.Concat(hoja.RowsUsed().Skip(1).Where(f => f.Cell(4).GetValue<int>() == 1).Select(f => f.Cell(6).GetString()));
                    if (texto != largo || hoja.Cell(2, 6).HasFormula) throw new InvalidOperationException("Texto alterado.");
                }
            }
            using var pdf = File.Create(Path.Combine(carpeta, $"prueba-informe-{total}.pdf"));
            new GeneradorPdfResultados().Generar(informe, pdf);
        }
    }
}
#endif
