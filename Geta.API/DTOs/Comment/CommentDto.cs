namespace Geta.API.DTOs.Comment
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string UserAvatarUrl { get; set; }
        public int PublicationId { get; set; }
    }
}
