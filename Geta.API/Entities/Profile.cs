using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Geta.API.Entities;

public class Profile
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    public string? Bio { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    [MaxLength(255)]
    public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Propriedade de Navegação
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}