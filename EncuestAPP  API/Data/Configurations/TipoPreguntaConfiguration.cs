using EncuestAPP__API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EncuestAPP__API.Data.Configurations;

public class TipoPreguntaConfiguration : IEntityTypeConfiguration<TipoPregunta>
{
    public void Configure(EntityTypeBuilder<TipoPregunta> builder)
    {
        builder.ToTable("TiposPregunta");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Nombre).IsUnique();
        builder.HasData(
            new TipoPregunta { Id = 1, Nombre = "Texto corto" },
            new TipoPregunta { Id = 2, Nombre = "Texto largo" },
            new TipoPregunta { Id = 3, Nombre = "Selección única" },
            new TipoPregunta { Id = 4, Nombre = "Selección múltiple" },
            new TipoPregunta { Id = 5, Nombre = "Escala de valoración" });
    }
}
