namespace Servidor_PI.DTOs
{
    public class UsuarioViewDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsAdmin { get; set; }
    }
}

