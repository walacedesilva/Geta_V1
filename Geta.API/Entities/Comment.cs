using System.ComponentModel.DataAnnotations;

namespace Geta.API.Entities
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        public User User { get; set; }

        public int PublicationId { get; set; }
        public Publication Publication { get; set; }
    }
}
