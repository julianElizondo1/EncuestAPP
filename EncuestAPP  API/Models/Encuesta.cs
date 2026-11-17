namespace EncuestAPP__API.Models;

public class Encuesta
{
    public int Id { get; set; }
    public string Titulo { get; set; } = null!;
    public string? Descripcion { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaPublicacion { get; set; }
    public DateTime? FechaCierre { get; set; }
    public string TokenPublico { get; set; } = null!;
    public bool EsAnonima { get; set; }
    public bool UnaRespuestaPorDispositivo { get; set; }
    public int UsuarioId { get; set; }
    public int EstadoEncuestaId { get; set; }
    public int CategoriaEncuestaId { get; set; }
    public int? EncuestaAnteriorId { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public EstadoEncuesta EstadoEncuesta { get; set; } = null!;
    public CategoriaEncuesta CategoriaEncuesta { get; set; } = null!;
    public Encuesta? EncuestaAnterior { get; set; }
    public ICollection<Encuesta> EncuestasPosteriores { get; set; } = new List<Encuesta>();
    public ICollection<Pregunta> Preguntas { get; set; } = new List<Pregunta>();
    public ICollection<Participacion> Participaciones { get; set; } = new List<Participacion>();
}
