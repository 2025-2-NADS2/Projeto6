using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Servidor_PI.Models;

namespace Servidor_PI.Data.Map
{
    public class EventosMap : IEntityTypeConfiguration<Eventos>
    {
        public void Configure(EntityTypeBuilder<Eventos> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("event_id");
            builder.Property(x => x.Titulo).IsRequired().HasMaxLength(100).HasColumnName("event_titulo");
            builder.Property(x => x.Descricao).IsRequired().HasMaxLength(1000).HasColumnName("event_descricao");
            builder.Property(x => x.DataPostagem).IsRequired().HasDefaultValueSql("GETDATE()").HasColumnName("event_data_publicacao");
            builder.Property(x => x.DataEvento).IsRequired().HasColumnName("event_data_evento");
            builder.Property(x => x.LocalEvento).HasMaxLength(100).IsRequired().HasColumnName("event_local_evento");
            builder.Property(x => x.LinkImagem).HasMaxLength(250).HasColumnName("event_link_imagem");
        }
    }
}
