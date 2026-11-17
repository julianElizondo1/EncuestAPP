namespace EncuestAPP__API.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime FechaRegistro { get; set; }

    public ICollection<Encuesta> Encuestas { get; set; } = new List<Encuesta>();
}
