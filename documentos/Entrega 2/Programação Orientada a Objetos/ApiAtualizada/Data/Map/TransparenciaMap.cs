using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Servidor_PI.Models;

namespace Servidor_PI.Data.Map
{
    public class TransparenciaMap : IEntityTypeConfiguration<Transparencia>
    {
        public void Configure(EntityTypeBuilder<Transparencia> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("trans_id");
            builder.Property(x => x.Titulo).IsRequired().HasMaxLength(100).HasColumnName("trans_titulo");
            builder.Property(x => x.Descricao).IsRequired().HasMaxLength(1000).HasColumnName("trans_descricao");
            builder.Property(x => x.DataPublicacao).IsRequired().HasDefaultValueSql("GETDATE()").HasColumnName("trans_data_publicacao");
            builder.Property(x => x.LinkDownload).HasMaxLength(250).HasColumnName("trans_link_download");
            builder.Property(x => x.NomeOriginal).HasMaxLength(100).HasColumnName("trans_nome_original");
        }
    }
}
