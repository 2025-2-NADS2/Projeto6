using System;
using System.ComponentModel.DataAnnotations;

namespace Servidor_PI.Models
{
    public class Atividades
    {
        [Key]
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Descricao { get; set; } = "";
        public DateTime DataPostagem { get; set; } = DateTime.Now;
        public string LinkImagem { get; set; } = "";

        // Auditoria (quem criou)
        public int UsuarioId { get; set; }      // FK
        public Usuarios Usuario { get; set; }   // Navegação
    }
}
