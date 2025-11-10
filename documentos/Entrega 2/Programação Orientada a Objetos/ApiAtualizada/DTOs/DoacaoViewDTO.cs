namespace Servidor_PI.Models.DTOs
{
    public class DoacaoViewDTO
    {
        public int Id { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; } = "";
        public string? CheckoutLink { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataPagamento { get; set; }
    }
}

