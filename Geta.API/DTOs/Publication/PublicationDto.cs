namespace Geta.API.DTOs.Publication;

public class PublicationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty; // Nome do autor
    public string? UserAvatarUrl { get; set; } // Avatar do autor
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int CommentCount { get; set; } // Será implementado a seguir
    public int LikesCount { get; set; }
    public bool HasLiked { get; set; }
}