using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Servidor_PI.Models;
namespace Servidor_PI.Data.Map
{
    public class OuvidoriaMap : IEntityTypeConfiguration<Ouvidoria>
    {
        public void Configure(EntityTypeBuilder<Ouvidoria> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("ouv_Id");    
            builder.Property(x => x.Titulo).IsRequired().HasMaxLength(100).HasColumnName("ouv_titulo");
            builder.Property(x => x.Descricao).IsRequired().HasMaxLength(1000).HasColumnName("ouv_descricao");
            builder.Property(x => x.DataOuvidoria).IsRequired().HasDefaultValueSql("GETDATE()").HasColumnName("ouv_data_publicacao");
        }
    }
}
