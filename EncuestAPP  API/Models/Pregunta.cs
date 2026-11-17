namespace EncuestAPP__API.Models;

public class Pregunta
{
    public int Id { get; set; }
    public int EncuestaId { get; set; }
    public int TipoPreguntaId { get; set; }
    public string Texto { get; set; } = null!;
    public int Orden { get; set; }
    public bool Obligatoria { get; set; }

    public Encuesta Encuesta { get; set; } = null!;
    public TipoPregunta TipoPregunta { get; set; } = null!;
    public ICollection<OpcionRespuesta> OpcionesRespuesta { get; set; } = new List<OpcionRespuesta>();
    public ICollection<Respuesta> Respuestas { get; set; } = new List<Respuesta>();
}
