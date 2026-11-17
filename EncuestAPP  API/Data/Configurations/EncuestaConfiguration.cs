using EncuestAPP__API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EncuestAPP__API.Data.Configurations;

public class EncuestaConfiguration : IEntityTypeConfiguration<Encuesta>
{
    public void Configure(EntityTypeBuilder<Encuesta> builder)
    {
        builder.ToTable("Encuestas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Titulo).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnType("nvarchar(max)");
        builder.Property(x => x.FechaCreacion).HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.FechaPublicacion).HasColumnType("datetime2");
        builder.Property(x => x.FechaCierre).HasColumnType("datetime2");
        builder.Property(x => x.TokenPublico).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EsAnonima).HasDefaultValue(true).IsRequired();
        builder.Property(x => x.UnaRespuestaPorDispositivo).HasDefaultValue(false).IsRequired();
        builder.HasIndex(x => x.TokenPublico).IsUnique();

        builder.HasOne(x => x.Usuario)
            .WithMany(x => x.Encuestas)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.EstadoEncuesta)
            .WithMany(x => x.Encuestas)
            .HasForeignKey(x => x.EstadoEncuestaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CategoriaEncuesta)
            .WithMany(x => x.Encuestas)
            .HasForeignKey(x => x.CategoriaEncuestaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.EncuestaAnterior)
            .WithMany(x => x.EncuestasPosteriores)
            .HasForeignKey(x => x.EncuestaAnteriorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
