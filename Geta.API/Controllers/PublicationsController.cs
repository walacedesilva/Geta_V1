using System.Security.Claims;
using Geta.API.Data;
using Geta.API.DTOs.Publication;
using Geta.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Geta.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Protege todos os endpoints neste controller
public class PublicationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PublicationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/publications
    [HttpGet]
    [AllowAnonymous] // Permite que qualquer um veja as publicações
    public async Task<ActionResult<IEnumerable<PublicationDto>>> GetPublications()
    {
        var publications = await _context.Publications
            .Include(p => p.User) // Inclui dados do autor
            .ThenInclude(u => u.Profile)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PublicationDto
            {
                Id = p.Id,
                UserId = p.UserId,
                Username = p.User.Username,
                UserAvatarUrl = p.User.Profile != null ? p.User.Profile.AvatarUrl : null,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        return Ok(publications);
    }

    // POST: api/publications
    [HttpPost]
    public async Task<ActionResult<PublicationDto>> CreatePublication(CreatePublicationDto createDto)
    {
        // Pega o ID do usuário a partir do token JWT
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var publication = new Publication
        {
            Content = createDto.Content,
            UserId = userId
        };

        _context.Publications.Add(publication);
        await _context.SaveChangesAsync();

        // Recarregar a publicação com os dados do usuário para retornar o DTO completo
        var createdPublication = await _context.Publications
            .Include(p => p.User)
            .ThenInclude(u => u.Profile)
            .Where(p => p.Id == publication.Id)
            .Select(p => new PublicationDto
            {
                Id = p.Id,
                UserId = p.UserId,
                Username = p.User.Username,
                UserAvatarUrl = p.User.Profile != null ? p.User.Profile.AvatarUrl : null,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstAsync();

        return CreatedAtAction(nameof(GetPublication), new { id = publication.Id }, createdPublication);
    }

    // GET: api/publications/5
    // Endpoint auxiliar para o CreatedAtAction funcionar
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<PublicationDto>> GetPublication(int id)
    {
        var publication = await _context.Publications
            .Include(p => p.User)
            .ThenInclude(u => u.Profile)
            .Where(p => p.Id == id)
            .Select(p => new PublicationDto
            {
                Id = p.Id,
                UserId = p.UserId,
                Username = p.User.Username,
                UserAvatarUrl = p.User.Profile != null ? p.User.Profile.AvatarUrl : null,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (publication == null)
        {
            return NotFound();
        }

        return Ok(publication);
    }
}