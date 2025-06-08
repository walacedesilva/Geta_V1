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
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponseDto>> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.Username, registerDto.Email))
        {
            return BadRequest("Nome de usuário ou email já existe.");
        }

        var user = CreateUser(registerDto);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreateLoginResponse(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return Unauthorized("Email ou senha inválidos.");
        }

        return CreateLoginResponse(user);
    }

    private async Task<bool> UserExists(string username, string email)
    {
        return await _context.Users.AnyAsync(u => u.Username == username) ||
               await _context.Users.AnyAsync(u => u.Email == email);
    }

    private User CreateUser(RegisterDto registerDto)
    {
        return new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            Profile = new Profile()
        };
    }

    private LoginResponseDto CreateLoginResponse(User user)
    {
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
