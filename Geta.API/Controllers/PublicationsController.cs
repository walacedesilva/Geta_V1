using Geta.API.Data;
using Geta.API.DTOs.Publication;
using Geta.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Geta.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PublicationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PublicationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/users/{userId}/publications?page=1&pageSize=10
        [HttpGet("{userId}/publications")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<PublicationDto>>> GetUserPublications(
            int userId, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var query = _context.Publications
                .Where(p => p.UserId == userId)
                .Include(p => p.User).ThenInclude(u => u.Profile)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PublicationDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    UserAvatarUrl = p.User.Profile != null ? p.User.Profile.AvatarUrl : null,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    LikesCount = p.Likes.Count,
                    HasLiked = p.Likes.Any(l => l.UserId == currentUserId),
                    CommentCount = 0 // Placeholder
                })
                .ToListAsync();

            var result = new PagedResult<PublicationDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Ok(result);
        }

        // GET: api/publications
        [HttpGet]
        public async Task<ActionResult<PagedResult<PublicationDto>>> GetPublications([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var query = _context.Publications
                .Include(p => p.User).ThenInclude(u => u.Profile)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var dtos = items.Select(p => new PublicationDto
            {
                Id = p.Id,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                UserId = p.UserId,
                Username = p.User.Username,
                UserAvatarUrl = p.User.Profile?.AvatarUrl,
                LikesCount = p.Likes.Count,
                HasLiked = p.Likes.Any(l => l.UserId == currentUserId),
                CommentCount = 0 // Placeholder
            }).ToList();

            return Ok(new PagedResult<PublicationDto> { Items = dtos, TotalCount = totalCount });
        }

        // GET: api/publications/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PublicationDto>> GetPublication(int id)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var publication = await _context.Publications
                .Include(p => p.User).ThenInclude(u => u.Profile)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (publication == null)
            {
                return NotFound();
            }

            var dto = new PublicationDto
            {
                Id = publication.Id,
                Content = publication.Content,
                CreatedAt = publication.CreatedAt,
                UserId = publication.UserId,
                Username = publication.User.Username,
                UserAvatarUrl = publication.User.Profile?.AvatarUrl,
                LikesCount = publication.Likes.Count,
                HasLiked = publication.Likes.Any(l => l.UserId == currentUserId),
                CommentCount = publication.Comments.Count
            };

            return Ok(dto);
        }

        // POST: api/publications
        [HttpPost]
        public async Task<ActionResult<PublicationDto>> CreatePublication(CreatePublicationDto createPublicationDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return Unauthorized();
            }

            var publication = new Publication
            {
                Content = createPublicationDto.Content,
                UserId = userId
            };

            _context.Publications.Add(publication);
            await _context.SaveChangesAsync();

            var publicationDto = new PublicationDto
            {
                Id = publication.Id,
                Content = publication.Content,
                CreatedAt = publication.CreatedAt,
                UserId = publication.UserId,
                Username = user.Username,
                UserAvatarUrl = user.Profile?.AvatarUrl,
                LikesCount = 0,
                HasLiked = false,
                CommentCount = 0
            };

            return CreatedAtAction(nameof(GetPublications), new { id = publication.Id }, publicationDto);
        }

        // POST: api/publications/{id}/like
        [HttpPost("{id}/like")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Like(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var publication = await _context.Publications.FindAsync(id);
            if (publication == null)
            {
                return NotFound("Publicação não encontrada.");
            }

            var alreadyLiked = await _context.Likes.AnyAsync(l => l.PublicationId == id && l.UserId == userId);
            if (alreadyLiked)
            {
                return BadRequest("Já gostou desta publicação.");
            }

            var like = new Like
            {
                UserId = userId,
                PublicationId = id
            };

            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/publications/{id}/like
        [HttpDelete("{id}/like")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Unlike(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var like = await _context.Likes.FirstOrDefaultAsync(l => l.PublicationId == id && l.UserId == userId);
            if (like == null)
            {
                return NotFound("Não gostou desta publicação.");
            }

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
