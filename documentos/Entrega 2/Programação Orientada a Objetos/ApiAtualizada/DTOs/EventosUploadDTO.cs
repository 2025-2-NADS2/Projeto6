using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
public class EventosUploadDTO
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(100, ErrorMessage = "O título pode ter no máximo 100 caracteres.")]
    public string Titulo { get; set; } = "";

    [Required(ErrorMessage ="Descrição é Obrigatória")]
    [StringLength(1000,  ErrorMessage="Descrição o limite de tamanho")]
    public string Descricao { get; set; } = "";

    [Required (ErrorMessage ="Local do Evento Obrigatório")]
    [StringLength (100, ErrorMessage = "Local do evento pode ter no máximo 100 caracteres")]
    public string LocalEvento { get; set; } = "";

    public DateTime DataEvento { get; set; } 


    [Required(ErrorMessage = "A imagem é Obrigatoria.")]
    [DataType(DataType.Upload)]
    public IFormFile ImagemArquivo { get; set; }
}

