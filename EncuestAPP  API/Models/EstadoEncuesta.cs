namespace EncuestAPP__API.Models;

public class EstadoEncuesta
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;

    public ICollection<Encuesta> Encuestas { get; set; } = new List<Encuesta>();
}
