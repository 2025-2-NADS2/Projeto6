using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Servidor_PI.Models;

namespace Servidor_PI.Data.Map
{
    public class UsuariosMap : IEntityTypeConfiguration<Usuarios>
    {
        public void Configure(EntityTypeBuilder<Usuarios> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x=>x.Nome).IsRequired().HasMaxLength(100).HasColumnName("user_nome");
            builder.Property(x=>x.Email).IsRequired().HasMaxLength(120).HasColumnName("user_email");
            builder.Property(x => x.DataNascimento).IsRequired().HasColumnName("user_data_nasc");
            builder.Property(x => x.Senha).HasColumnName("user_senha").IsRequired();
            builder.Property(x => x.IsAdmin).HasColumnName("user_isAdmin").IsRequired();
            builder.Property(x => x.Endereco).HasMaxLength(250).HasColumnName("user_endereco");
        }
    }
}