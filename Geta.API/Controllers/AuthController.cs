using Geta.API.Data;
using Geta.API.DTOs.Auth;
using Geta.API.Entities;
using Geta.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Geta.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(ApplicationDbContext context, IConfiguration config, TokenService tokenService)
    {
        _context = context;
        _tokenService = new TokenService(config);
        _tokenService = tokenService;
    }

    public AuthController(ApplicationDbContext context, IConfigurationRoot configuration)
    {
        _context = context;
        _tokenService = new TokenService(configuration);
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponseDto>> Register(RegisterDto registerDto)
    {
        if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
        {
            return BadRequest("Nome de usuário já existe.");
        }

        if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
        {
            return BadRequest("Email já cadastrado.");
        }

        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)
        };

        // Cria um perfil vazio para o usuário
        user.Profile = new Profile();

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };

        return new LoginResponseDto
        {
            Token = _tokenService.CreateToken(user),
            User = userDto
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return Unauthorized("Email ou senha inválidos.");
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };

        return new LoginResponseDto
        {
            Token = _tokenService.CreateToken(user),
            User = userDto
        };
    }
}