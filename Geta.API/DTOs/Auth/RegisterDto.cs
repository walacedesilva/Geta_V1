using System.ComponentModel.DataAnnotations;

namespace Geta.API.DTOs.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "O nome de usuário é obrigatório.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome de usuário deve ter entre 3 e 50 caracteres.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "O formato do email é inválido.")]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres.")]
    public string Password { get; set; } = string.Empty;
}