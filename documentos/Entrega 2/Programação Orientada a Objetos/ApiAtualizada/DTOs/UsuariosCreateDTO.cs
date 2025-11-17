using System;
using System.ComponentModel.DataAnnotations;

namespace Servidor_PI.DTOs
{
    public class UsuarioCreateDTO
    {
        [Required(ErrorMessage = "Nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "Nome ultrapasse o limite de caracteres")]
        public string Nome { get; set; } = "";

        [Required(ErrorMessage = "E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        [StringLength(120)]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Senha é obrigatória.")]
        [StringLength(255, MinimumLength = 6)]
        public string Senha { get; set; } = "";

        [Required]
        public DateTime? DataNascimento { get; set; }
        public string? Endereco { get; set; }
        public bool IsAdmin { get; set; } = false;
    }
}

