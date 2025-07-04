using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Geta.API.Entities
{
    public class Follow
    {
        [Key]
        [Column(Order = 0)]
        public int FollowerId { get; set; }
        public User Follower { get; set; }

        [Key]
        [Column(Order = 1)]
        public int FollowingId { get; set; }
        public User Following { get; set; }
    }
}