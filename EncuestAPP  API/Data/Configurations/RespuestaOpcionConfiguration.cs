using EncuestAPP__API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EncuestAPP__API.Data.Configurations;

public class RespuestaOpcionConfiguration : IEntityTypeConfiguration<RespuestaOpcion>
{
    public void Configure(EntityTypeBuilder<RespuestaOpcion> builder)
    {
        builder.ToTable("RespuestasOpciones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.RespuestaId).IsRequired();
        builder.Property(x => x.OpcionRespuestaId).IsRequired();
        builder.HasIndex(x => new { x.RespuestaId, x.OpcionRespuestaId }).IsUnique();

        builder.HasOne(x => x.Respuesta)
            .WithMany(x => x.RespuestasOpciones)
            .HasForeignKey(x => x.RespuestaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.OpcionRespuesta)
            .WithMany(x => x.RespuestasOpciones)
            .HasForeignKey(x => x.OpcionRespuestaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
