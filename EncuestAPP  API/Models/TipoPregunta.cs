namespace EncuestAPP__API.Models;

public class TipoPregunta
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;

    public ICollection<Pregunta> Preguntas { get; set; } = new List<Pregunta>();
}
