using EncuestAPP__API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EncuestAPP__API.Data.Configurations;

public class OpcionRespuestaConfiguration : IEntityTypeConfiguration<OpcionRespuesta>
{
    public void Configure(EntityTypeBuilder<OpcionRespuesta> builder)
    {
        builder.ToTable("OpcionesRespuesta");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Texto).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Orden).IsRequired();
        builder.HasIndex(x => new { x.PreguntaId, x.Orden }).IsUnique();

        builder.HasOne(x => x.Pregunta)
            .WithMany(x => x.OpcionesRespuesta)
            .HasForeignKey(x => x.PreguntaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
