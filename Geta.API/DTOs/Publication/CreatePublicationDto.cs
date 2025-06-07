using System.ComponentModel.DataAnnotations;

namespace Geta.API.DTOs.Publication;

public class CreatePublicationDto
{
    [Required(ErrorMessage = "O conteúdo da publicação é obrigatório.")]
    [MinLength(1, ErrorMessage = "O conteúdo não pode estar vazio.")]
    public string Content { get; set; } = string.Empty;
}