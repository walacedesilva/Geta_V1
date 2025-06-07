using Geta.API.Data;
using Geta.API.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Geta.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SearchController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/search/users?query=term
    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> SearchUsers([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Ok(new List<UserDto>());
        }

        var users = await _context.Users
            .Where(u => u.Username.Contains(query))
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email, // Considere remover o email dos resultados da busca
                CreatedAt = u.CreatedAt
            })
            .Take(10) // Limita o número de resultados
            .ToListAsync();

        return Ok(users);
    }
}