using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
public class AtividadesUploadDTO
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(100, ErrorMessage = "O título pode ter no máximo 100 caracteres.")]
    public string Titulo { get; set; } = "";

    [Required(ErrorMessage ="A descrição é obrigatória")]
    [StringLength(1000, ErrorMessage = "A descrição excede o limite de tamanho")]
    public string Descricao { get; set; } = "";

    [Required(ErrorMessage = "A imagem é obrigatória.")]
    [DataType(DataType.Upload)]
    public IFormFile ImagemArquivo { get; set; }    
}
