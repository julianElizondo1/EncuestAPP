using EncuestAPP_Movil.DTOs.Encuestas;
using EncuestAPP_Movil.DTOs.Resultados;

namespace EncuestAPP_Movil.Services.Exportacion;

// Modelo interno: no modifica ni agrega contratos de la API.
public sealed record InformeResultados(
    EncuestaDetalleDto Encuesta,
    ResultadoGeneralDto Resumen,
    IReadOnlyList<ResultadoPreguntaDto> Preguntas,
    DateTimeOffset ConsultadoEn,
    DateTimeOffset GeneradoEn)
{
    public static InformeResultados Crear(EncuestaDetalleDto encuesta, ResultadoGeneralDto resumen,
        IReadOnlyDictionary<int, ResultadoPreguntaDto> resultados, DateTimeOffset consultadoEn)
    {
        if (encuesta.Id != resumen.EncuestaId || encuesta.Preguntas.Any(p => !resultados.ContainsKey(p.Id)))
            throw new InvalidOperationException("Los resultados no están completos para exportar.");

        var preguntas = encuesta.Preguntas.OrderBy(p => p.Orden).Select(p =>
        {
            var resultado = resultados[p.Id];
            return resultado with
            {
                Opciones = resultado.Opciones.ToArray(),
                RespuestasTextuales = resultado.RespuestasTextuales.ToArray()
            };
        }).ToArray();
        return new(encuesta, resumen, preguntas, consultadoEn, DateTimeOffset.Now);
    }
}
