namespace EncuestAPP__API.Models;

public class CategoriaEncuesta
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }

    public ICollection<Encuesta> Encuestas { get; set; } = new List<Encuesta>();
}
