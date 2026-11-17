using EncuestAPP__API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EncuestAPP__API.Data.Configurations;

public class CategoriaEncuestaConfiguration : IEntityTypeConfiguration<CategoriaEncuesta>
{
    public void Configure(EntityTypeBuilder<CategoriaEncuesta> builder)
    {
        builder.ToTable("CategoriasEncuesta");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descripcion).HasMaxLength(200);
        builder.HasIndex(x => x.Nombre).IsUnique();
        builder.HasData(
            new CategoriaEncuesta { Id = 1, Nombre = "Académica" },
            new CategoriaEncuesta { Id = 2, Nombre = "Comercial" },
            new CategoriaEncuesta { Id = 3, Nombre = "Satisfacción" },
            new CategoriaEncuesta { Id = 4, Nombre = "Investigación" },
            new CategoriaEncuesta { Id = 5, Nombre = "RRHH" },
            new CategoriaEncuesta { Id = 6, Nombre = "Otras" });
    }
}
