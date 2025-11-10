using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Servidor_PI.Models;

namespace Servidor_PI.Data.Map
{
    public class AtividadesMap : IEntityTypeConfiguration<Atividades>
    {
        public void Configure(EntityTypeBuilder<Atividades> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("atv_id");
            builder.Property(x => x.Titulo).IsRequired().HasMaxLength(100).HasColumnName("atv_titulo");
            builder.Property(x => x.Descricao).IsRequired().HasMaxLength(1000).HasColumnName("atv_descricao");
            builder.Property(x => x.DataPostagem).IsRequired().HasDefaultValueSql("GETDATE()").HasColumnName("atv_data_publicacao");
            builder.Property(x => x.LinkImagem).HasMaxLength(250).HasColumnName("atv_link_imagem"); 
        }
    }
}
