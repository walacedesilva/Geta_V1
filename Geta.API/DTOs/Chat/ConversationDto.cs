namespace Geta.API.DTOs.Chat;

public class ConversationDto
{
    public int ContactId { get; set; } // O ID do outro usuário na conversa
    public string ContactUsername { get; set; } = string.Empty;
    public string? ContactAvatarUrl { get; set; }
    public string LastMessage { get; set; } = string.Empty;
    public DateTime LastMessageTimestamp { get; set; }
    public int UnreadCount { get; set; } // Contagem de mensagens não lidas
}