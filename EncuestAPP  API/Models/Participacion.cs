namespace EncuestAPP__API.Models;

public class Participacion
{
    public int Id { get; set; }
    public int EncuestaId { get; set; }
    public DateTime FechaRespuesta { get; set; }
    public string? IdentificadorDispositivo { get; set; }

    public Encuesta Encuesta { get; set; } = null!;
    public ICollection<Respuesta> Respuestas { get; set; } = new List<Respuesta>();
}
