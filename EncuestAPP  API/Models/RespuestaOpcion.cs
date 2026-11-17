namespace EncuestAPP__API.Models;

public class RespuestaOpcion
{
    public int Id { get; set; }
    public int RespuestaId { get; set; }
    public int OpcionRespuestaId { get; set; }

    public Respuesta Respuesta { get; set; } = null!;
    public OpcionRespuesta OpcionRespuesta { get; set; } = null!;
}
