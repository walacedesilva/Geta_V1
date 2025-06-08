using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Geta.API.Entities;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Propriedade de Navegação
    public Profile? Profile { get; set; }
    public ICollection<Publication> Publications { get; set; } = new List<Publication>();
    public ICollection<ChatMessage> MessagesSent { get; set; } = new List<ChatMessage>();
    public ICollection<ChatMessage> MessagesReceived { get; set; } = new List<ChatMessage>();
}
