using Servidor_PI.Models;
using System.ComponentModel.DataAnnotations;

namespace Servidor_PI.DTOs
{
    public class OuvidoriaCreateDTO
    {
        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(100, ErrorMessage = "O título pode ter no máximo 100 caracteres.")]
        public string Titulo { get; set; } = "";

        [Required(ErrorMessage = "Descrição obrigatoria")]
        [StringLength(1000, ErrorMessage = "A descrição pode ter no máximo 1000 caracteres.")]
        public string Descricao { get; set; } = "";

        [Required(ErrorMessage = "O Email do remetente é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        public string EmailRemetente { get; set; } = string.Empty;
    }
}



