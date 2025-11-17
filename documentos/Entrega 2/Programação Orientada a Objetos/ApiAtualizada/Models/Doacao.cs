using System;
using System.ComponentModel.DataAnnotations;

namespace Servidor_PI.Models
{
    public class Doacao
    {
        [Key]
        public int Id { get; set; }

        // Valor em reais (use decimal para valores monetários)
        public decimal Valor { get; set; }
        public bool Anonima { get; set; } = false;

        // status local: "pendente", "aprovado", "rejeitado", "cancelado"
        public string Status { get; set; } = "pendente";

        // Id do pagamento no Mercado Pago (quando recebido)
        public string? MercadoPagoPaymentId { get; set; }

        // Link de checkout retornado pelo Mercado Pago
        public string? CheckoutLink { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataPagamento { get; set; }

        // FK para o usuário que realizou a doação
        public int? UsuarioId { get; set; }
        public Usuarios Usuario { get; set; }


    }
}

