using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Geta.API.Entities
{
    public class Like
    {
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public int PublicationId { get; set; }
        [ForeignKey("PublicationId")]
        public Publication Publication { get; set; }
    }
}
