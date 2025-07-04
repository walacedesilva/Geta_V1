using Geta.API.Data;
using Geta.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Geta.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/users/{id}/follow
        [HttpPost("{id}/follow")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Follow(int id)
        {
            var followerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (followerId == id)
            {
                return BadRequest("Não pode seguir-se a si mesmo.");
            }

            var userToFollow = await _context.Users.FindAsync(id);
            if (userToFollow == null)
            {
                return NotFound("Utilizador a seguir não encontrado.");
            }

            var alreadyFollowing = await _context.Follows
                .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == id);

            if (alreadyFollowing)
            {
                return BadRequest("Já está a seguir este utilizador.");
            }

            var follow = new Follow
            {
                FollowerId = followerId,
                FollowingId = id
            };

            _context.Follows.Add(follow);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/users/{id}/unfollow
        [HttpPost("{id}/unfollow")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Unfollow(int id)
        {
            var followerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == id);

            if (follow == null)
            {
                return NotFound("Não está a seguir este utilizador.");
            }

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
