using EncuestAPP__API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EncuestAPP__API.Data.Configurations;

public class EstadoEncuestaConfiguration : IEntityTypeConfiguration<EstadoEncuesta>
{
    public void Configure(EntityTypeBuilder<EstadoEncuesta> builder)
    {
        builder.ToTable("EstadosEncuesta");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Nombre).HasMaxLength(30).IsRequired();
        builder.HasIndex(x => x.Nombre).IsUnique();
        builder.HasData(
            new EstadoEncuesta { Id = 1, Nombre = "Borrador" },
            new EstadoEncuesta { Id = 2, Nombre = "Publicada" },
            new EstadoEncuesta { Id = 3, Nombre = "Cerrada" },
            new EstadoEncuesta { Id = 4, Nombre = "Archivada" });
    }
}
