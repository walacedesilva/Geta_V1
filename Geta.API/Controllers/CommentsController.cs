using Geta.API.Data;
using Geta.API.DTOs.Comment;
using Geta.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Geta.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/publications/{publicationId}/comments
        [HttpGet("/api/publications/{publicationId}/comments")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsForPublication(int publicationId)
        {
            var comments = await _context.Comments
                .Include(c => c.User).ThenInclude(u => u.Profile)
                .Where(c => c.PublicationId == publicationId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UserId = c.UserId,
                    Username = c.User.Username,
                    UserAvatarUrl = c.User.Profile.AvatarUrl,
                    PublicationId = c.PublicationId
                })
                .ToListAsync();

            return Ok(comments);
        }

        // POST: api/comments
        [HttpPost]
        public async Task<ActionResult<CommentDto>> CreateComment(CreateCommentDto createCommentDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var publicationExists = await _context.Publications.AnyAsync(p => p.Id == createCommentDto.PublicationId);
            if (!publicationExists)
            {
                return BadRequest("Publicação não encontrada.");
            }

            var comment = new Comment
            {
                Content = createCommentDto.Content,
                PublicationId = createCommentDto.PublicationId,
                UserId = userId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Carregar dados do utilizador para o DTO de retorno
            await _context.Entry(comment).Reference(c => c.User).LoadAsync();
            await _context.Entry(comment.User).Reference(u => u.Profile).LoadAsync();

            var commentDto = new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UserId = comment.UserId,
                Username = comment.User.Username,
                UserAvatarUrl = comment.User.Profile?.AvatarUrl,
                PublicationId = comment.PublicationId
            };

            return CreatedAtAction(nameof(GetCommentsForPublication), new { publicationId = comment.PublicationId }, commentDto);
        }

        // PUT: api/comments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, UpdateCommentDto updateCommentDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            if (comment.UserId != userId)
            {
                return Forbid(); // O utilizador não é o autor do comentário
            }

            comment.Content = updateCommentDto.Content;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/comments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            if (comment.UserId != userId)
            {
                // Adicionar lógica para permitir que o dono da publicação também apague
                var publication = await _context.Publications.FindAsync(comment.PublicationId);
                if (publication?.UserId != userId)
                {
                    return Forbid();
                }
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
