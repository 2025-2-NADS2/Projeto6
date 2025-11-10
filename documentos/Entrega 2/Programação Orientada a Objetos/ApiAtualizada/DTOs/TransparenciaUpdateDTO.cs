namespace Servidor_PI.DTOs
{
    public class TransparenciaUpdateDTO
    {
        public string? Titulo { get; set; }
        public string? Descricao { get; set; }
        public IFormFile? PdfFile { get; set; }
    }
}