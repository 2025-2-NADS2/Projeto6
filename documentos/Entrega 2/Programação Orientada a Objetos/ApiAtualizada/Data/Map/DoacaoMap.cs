using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Servidor_PI.Models;

namespace Servidor_PI.Data.Map
{
    public class DoacaoMap : IEntityTypeConfiguration<Doacao>
    {
        public void Configure(EntityTypeBuilder<Doacao> builder)
        {
            builder.HasKey(d => d.Id);
            builder.Property(d => d.Id).HasColumnName("doacao_id");
            builder.Property(d => d.Valor).HasColumnType("decimal(18,2)").IsRequired().HasColumnName("doacao_valor");
            builder.Property(d => d.Status).HasMaxLength(50).IsRequired().HasColumnName("doacao_status");
            builder.Property(d => d.CheckoutLink).HasMaxLength(500).IsRequired(false);
            builder.Property(d => d.MercadoPagoPaymentId).HasMaxLength(100).IsRequired(false);
            builder.Property(d => d.DataCriacao).IsRequired().HasColumnName("doacao_data_criacao");
            builder.Property(d => d.DataPagamento).IsRequired(false).HasColumnName("doacao_data_pagamento");

            // relacionamento com usuário
            builder.HasOne(d => d.Usuario)
                   .WithMany() // ou .WithMany(u => u.Doacoes) se você criar coleção em Usuarios
                   .HasForeignKey(d => d.UsuarioId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
