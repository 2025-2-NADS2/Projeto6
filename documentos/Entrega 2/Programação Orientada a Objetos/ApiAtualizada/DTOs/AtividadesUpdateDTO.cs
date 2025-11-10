namespace Servidor_PI.DTOs
{

    public class AtividadesUpdateDTO
    {
        public string? Titulo { get; set; }
        public string? Descricao { get; set; }
        public IFormFile? ImagemArquivo { get; set; }
    }
}