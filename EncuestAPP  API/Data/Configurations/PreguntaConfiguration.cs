using EncuestAPP__API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EncuestAPP__API.Data.Configurations;

public class PreguntaConfiguration : IEntityTypeConfiguration<Pregunta>
{
    public void Configure(EntityTypeBuilder<Pregunta> builder)
    {
        builder.ToTable("Preguntas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.EncuestaId).IsRequired();
        builder.Property(x => x.TipoPreguntaId).IsRequired();
        builder.Property(x => x.Texto).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Orden).IsRequired();
        builder.Property(x => x.Obligatoria).HasDefaultValue(false).IsRequired();
        builder.HasIndex(x => new { x.EncuestaId, x.Orden }).IsUnique();

        builder.HasOne(x => x.Encuesta)
            .WithMany(x => x.Preguntas)
            .HasForeignKey(x => x.EncuestaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.TipoPregunta)
            .WithMany(x => x.Preguntas)
            .HasForeignKey(x => x.TipoPreguntaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
