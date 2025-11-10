using System;
using System.ComponentModel.DataAnnotations;

namespace Servidor_PI.Models
{
    public class Transparencia
    {
        [Key]
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Descricao { get; set; } = "";
        public string LinkDownload { get; set; } = "";
        public string NomeOriginal { get; set; } = "";
        public DateTime DataPublicacao { get; set; } = DateTime.Now;

        // Auditoria (quem criou)
        public int UsuarioId { get; set; }      // FK
        public Usuarios Usuario { get; set; }   // Navegação
    }
}
