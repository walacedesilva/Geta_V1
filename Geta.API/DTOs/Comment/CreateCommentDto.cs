using System.ComponentModel.DataAnnotations;

namespace Geta.API.DTOs.Comment
{
    public class CreateCommentDto
    {
        [Required]
        public int PublicationId { get; set; }

        [Required]
        [MinLength(1)]
        public string Content { get; set; }
    }
}
