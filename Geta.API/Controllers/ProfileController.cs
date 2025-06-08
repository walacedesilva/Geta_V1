using System.Security.Claims;
using Geta.API.Data;
using Geta.API.DTOs.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Geta.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProfileController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/profile/{userId}
    [HttpGet("{userId:int}")]
    [AllowAnonymous] // Permite que qualquer um veja perfis
    public async Task<ActionResult<ProfileDto>> GetProfile(int userId)
    {
        var userProfile = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new ProfileDto
            {
                UserId = u.Id,
                Username = u.Username,
                Email = u.Email, // Considere remover o email de um endpoint público
                Bio = u.Profile != null ? u.Profile.Bio : null,
                Location = u.Profile != null ? u.Profile.Location : null,
                AvatarUrl = u.Profile != null ? u.Profile.AvatarUrl : null,
                MemberSince = u.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (userProfile == null)
        {
            return NotFound("Perfil não encontrado.");
        }

        return Ok(userProfile);
    }

    // PUT: api/profile
    [HttpPut]
    public async Task<IActionResult> UpdateProfile(UpdateProfileDto updateDto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            return NotFound("Perfil não encontrado para o usuário autenticado.");
        }

        // Atualiza os campos do perfil
        profile.Bio = updateDto.Bio;
        profile.Location = updateDto.Location;
        profile.AvatarUrl = updateDto.AvatarUrl;
        profile.UpdatedAt = DateTime.UtcNow;

        _context.Entry(profile).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Profiles.Any(p => p.UserId == userId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent(); // Retorna 204 No Content, indicando sucesso sem corpo de resposta.
    }
}