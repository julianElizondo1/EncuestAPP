using EncuestAPP__API.Models;
using Microsoft.EntityFrameworkCore;

namespace EncuestAPP__API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Encuesta> Encuestas => Set<Encuesta>();
    public DbSet<EstadoEncuesta> EstadosEncuesta => Set<EstadoEncuesta>();
    public DbSet<CategoriaEncuesta> CategoriasEncuesta => Set<CategoriaEncuesta>();
    public DbSet<TipoPregunta> TiposPregunta => Set<TipoPregunta>();
    public DbSet<Pregunta> Preguntas => Set<Pregunta>();
    public DbSet<OpcionRespuesta> OpcionesRespuesta => Set<OpcionRespuesta>();
    public DbSet<Participacion> Participaciones => Set<Participacion>();
    public DbSet<Respuesta> Respuestas => Set<Respuesta>();
    public DbSet<RespuestaOpcion> RespuestasOpciones => Set<RespuestaOpcion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
