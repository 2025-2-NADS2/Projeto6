namespace Servidor_PI.DTOs
{

    public class EventosUpdateDTO
    {
        public string? Titulo { get; set; }
        public string? Descricao { get; set; }
        public DateTime? DataEvento { get; set; }
        public IFormFile? ImagemArquivo { get; set; }
        public string? LocalEvento { get; set; }

    }
}
