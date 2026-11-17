using EncuestAPP__API.DTOs.Resultados;

namespace EncuestAPP__API.Services.Implementations;

// Seven calendar days in the caller's supplied fixed UTC offset, including today.
internal sealed record PeriodoActividad(DateOnly Desde, DateOnly Hasta, DateTime InicioUtc,
    DateTimeOffset GeneradoEn, int DesfaseUtcMinutos)
{
    public static PeriodoActividad Crear(DateTimeOffset ahora, int desfaseUtcMinutos)
    {
        if (desfaseUtcMinutos is < -840 or > 840) throw new ArgumentOutOfRangeException(nameof(desfaseUtcMinutos));
        var desfase = TimeSpan.FromMinutes(desfaseUtcMinutos);
        var hoy = DateOnly.FromDateTime(ahora.ToOffset(desfase).DateTime);
        var desde = hoy.AddDays(-6);
        return new(desde, hoy, new DateTimeOffset(desde.ToDateTime(TimeOnly.MinValue), desfase).UtcDateTime, ahora, desfaseUtcMinutos);
    }

    public ActividadParticipacionesDto Completar(IReadOnlyDictionary<DateOnly, int> cantidades)
    {
        var dias = Enumerable.Range(0, 7)
            .Select(i => new ActividadDiariaDto(Desde.AddDays(i), cantidades.GetValueOrDefault(Desde.AddDays(i))))
            .ToArray();
        return new(Desde, Hasta, DesfaseUtcMinutos, GeneradoEn, dias[^1].Cantidad, dias.Sum(x => x.Cantidad), dias);
    }
}
