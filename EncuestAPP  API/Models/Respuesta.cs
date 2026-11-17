namespace EncuestAPP__API.Models;

public class Respuesta
{
    public int Id { get; set; }
    public int ParticipacionId { get; set; }
    public int PreguntaId { get; set; }
    public string? Texto { get; set; }

    public Participacion Participacion { get; set; } = null!;
    public Pregunta Pregunta { get; set; } = null!;
    public ICollection<RespuestaOpcion> RespuestasOpciones { get; set; } = new List<RespuestaOpcion>();
}
