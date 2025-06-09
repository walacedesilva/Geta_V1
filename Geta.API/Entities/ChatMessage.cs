using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Geta.API.Entities;

public class ChatMessage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int SenderId { get; set; }

    public int? ReceiverId { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; } = false;

    // Propriedades de Navegação
    [ForeignKey("SenderId")]
    public User Sender { get; set; } = null!;

    public User Receiver { get; set; } = null!;
}