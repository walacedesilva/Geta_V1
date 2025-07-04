using System.ComponentModel.DataAnnotations;

namespace Geta.API.DTOs.Comment
{
    public class UpdateCommentDto
    {
        [Required]
        [MinLength(1)]
        public string Content { get; set; }
    }
}
