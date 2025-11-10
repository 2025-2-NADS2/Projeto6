using System.ComponentModel.DataAnnotations;
using Microsoft.OpenApi.MicrosoftExtensions;

namespace Servidor_PI.Models
{
    public class Usuarios
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = "";

        [Required]
        [StringLength(120)]
        public string Email { get; set; } = "";

        [Required]
        [StringLength(255, MinimumLength = 6)]
        public string Senha { get; set; } = ""; // senha guardada hasheada

        [Required]
        public DateTime DataNascimento { get; set; }

        [StringLength(250)]
        public string? Endereco { get; set; }


        // true = administrador, false = usuário comum
        public bool IsAdmin { get; set; } = false;

        // Relações inversas (opcional)
        public ICollection<Ouvidoria>? Ouvidorias { get; set; }
        public ICollection<Eventos>? Eventos { get; set; }
        public ICollection<Transparencia>? Transparencias { get; set; }

        public ICollection<Atividades>? Atividades { get; set; }
        public ICollection<Doacao>? Doacao { get; set; }

    }
}

