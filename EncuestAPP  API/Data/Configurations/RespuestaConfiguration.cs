using EncuestAPP__API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EncuestAPP__API.Data.Configurations;

public class RespuestaConfiguration : IEntityTypeConfiguration<Respuesta>
{
    public void Configure(EntityTypeBuilder<Respuesta> builder)
    {
        builder.ToTable("Respuestas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Texto).HasColumnType("nvarchar(max)");
        builder.HasIndex(x => new { x.ParticipacionId, x.PreguntaId }).IsUnique();

        builder.HasOne(x => x.Participacion)
            .WithMany(x => x.Respuestas)
            .HasForeignKey(x => x.ParticipacionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Pregunta)
            .WithMany(x => x.Respuestas)
            .HasForeignKey(x => x.PreguntaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
