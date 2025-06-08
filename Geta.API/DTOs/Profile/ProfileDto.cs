namespace Geta.API.DTOs.Profile;

public class ProfileDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime MemberSince { get; set; }
}