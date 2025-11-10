using System.ComponentModel.DataAnnotations;

namespace Servidor_PI.Models.DTOs
{
    public class DoacaoCreateDTO
    {
        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
        public decimal Valor { get; set; }

        public string? Descricao { get; set; }
    }
}

