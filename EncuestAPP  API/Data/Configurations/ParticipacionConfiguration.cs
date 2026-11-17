using EncuestAPP__API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EncuestAPP__API.Data.Configurations;

public class ParticipacionConfiguration : IEntityTypeConfiguration<Participacion>
{
    public void Configure(EntityTypeBuilder<Participacion> builder)
    {
        builder.ToTable("Participaciones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.EncuestaId).IsRequired();
        builder.Property(x => x.FechaRespuesta).HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.IdentificadorDispositivo).HasMaxLength(100);
        builder.HasIndex(x => new { x.EncuestaId, x.IdentificadorDispositivo })
            .IsUnique()
            .HasFilter("[IdentificadorDispositivo] IS NOT NULL");

        builder.HasOne(x => x.Encuesta)
            .WithMany(x => x.Participaciones)
            .HasForeignKey(x => x.EncuestaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
