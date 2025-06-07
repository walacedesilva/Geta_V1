using System.ComponentModel.DataAnnotations;

namespace Geta.API.DTOs.Profile;

public class UpdateProfileDto
{
    [StringLength(500, ErrorMessage = "A biografia não pode exceder 500 caracteres.")]
    public string? Bio { get; set; }

    [StringLength(100, ErrorMessage = "A localização não pode exceder 100 caracteres.")]
    public string? Location { get; set; }

    [Url(ErrorMessage = "O URL do avatar deve ser um URL válido.")]
    [StringLength(255)]
    public string? AvatarUrl { get; set; }
}