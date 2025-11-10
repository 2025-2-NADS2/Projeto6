using System;
using System.ComponentModel.DataAnnotations;

namespace Servidor_PI.Models
{
    public class Ouvidoria
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(100, ErrorMessage = "O título pode ter no máximo 100 caracteres.")]
        public string Titulo { get; set; } = "";

        [Required(ErrorMessage = "Descrição obrigatoria")]
        [StringLength(1000, ErrorMessage = "A descrição pode ter no máximo 1000 caracteres.")]
        public string Descricao { get; set; } = "";
        public DateTime DataOuvidoria { get; set; } = DateTime.Now;

        // Auditoria (quem criou)
        public int UsuarioId { get; set; }      // FK
        public Usuarios Usuario { get; set; }   // Navegação

    }
}
