using System;
using System.ComponentModel.DataAnnotations;

namespace Servidor_PI.Models
{
    public class Eventos
    {
        [Key]
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Descricao { get; set; } = "";
        public DateTime DataPostagem { get; set; } = DateTime.Now;
        public DateTime DataEvento {  get; set; }
        public string LocalEvento { get; set; } = "";
        public string LinkImagem { get; set; } = "";

        // Auditoria (quem criou)
        public int UsuarioId { get; set; }      // FK
        public Usuarios Usuario { get; set; }   // Navegação
    }
}
