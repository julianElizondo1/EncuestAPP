namespace EncuestAPP__API.Models;

public class OpcionRespuesta
{
    public int Id { get; set; }
    public int PreguntaId { get; set; }
    public string Texto { get; set; } = null!;
    public int Orden { get; set; }

    public Pregunta Pregunta { get; set; } = null!;
    public ICollection<RespuestaOpcion> RespuestasOpciones { get; set; } = new List<RespuestaOpcion>();
}
