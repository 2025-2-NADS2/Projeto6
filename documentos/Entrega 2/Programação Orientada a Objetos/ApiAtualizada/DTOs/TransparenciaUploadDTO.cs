using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class TransparenciaUploadDTO
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(100, ErrorMessage = "O título pode ter no máximo 100 caracteres.")]
    public string Titulo { get; set; } = "";

    [StringLength(1000, ErrorMessage = "A descrição pode ter no máximo 1000 caracteres.")]

    public string Descricao { get; set; } = "";

    [Required(ErrorMessage = "O arquivo PDF é obrigatório.")]
    [DataType(DataType.Upload)]
    public IFormFile PdfFile { get; set; }
}
