using EncuestAPP__API.DTOs.Encuestas;
using EncuestAPP__API.DTOs.Participaciones;

namespace EncuestAPP__API.Services.Interfaces;

public interface IParticipacionService
{
    Task<EncuestaPublicaDto> ObtenerEncuestaPublicaAsync(string tokenPublico, CancellationToken cancellationToken);
    Task<ParticipacionCreadaDto> CrearAsync(string tokenPublico, CrearParticipacionDto solicitud, CancellationToken cancellationToken);
}
