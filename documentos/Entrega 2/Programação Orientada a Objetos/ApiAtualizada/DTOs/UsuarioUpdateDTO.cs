using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Servidor_PI.DTOs
{
    public class UsuarioUpdateDTO
    {
        [StringLength(100)]
        public string? Nome { get; set; }

        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        [StringLength(120)]
        public string? Email { get; set; }

        // Se o admin quiser mudar a senha via update (opcional)
        [StringLength(255, MinimumLength = 6)]
        public string? Senha { get; set; }

        public DateTime? DataNascimento { get; set; }
        public string? Endereco { get; set; }
    }
}

