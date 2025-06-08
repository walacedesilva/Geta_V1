using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Geta.API.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Geta.API.Services;

public class TokenService
{
    private readonly IConfiguration _config;
    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            // O tipo da claim "NameIdentifier" é mapeado para o ID do usuário no HttpContext.
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email)
        };

        var jwtKey = _config["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey)) throw new InvalidOperationException("Chave JWT não configurada.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds,
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}