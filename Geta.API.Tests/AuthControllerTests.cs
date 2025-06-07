using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Geta.API.DTOs.Auth;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

namespace Geta.API.Tests;

public class AuthControllerTests : IClassFixture<GetaWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthControllerTests(GetaWebApplicationFactory<Program> factory)
    {
        // Cria um cliente HTTP que faz requisições para a API em memória.
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ComDadosValidos_DeveRetornarOkComTokenEUsuario()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.EnsureSuccessStatusCode(); // Garante status 2xx
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginResponse);
        Assert.NotEmpty(loginResponse.Token);
        Assert.Equal("newuser", loginResponse.User.Username);
    }

    [Fact]
    public async Task Register_ComUsernameExistente_DeveRetornarBadRequest()
    {
        // Arrange - Primeiro, garantimos que um usuário já existe
        await RegisterTestUser("existinguser", "email1@example.com");

        var registerDto = new RegisterDto
        {
            Username = "existinguser",
            Email = "email2@example.com",
            Password = "password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarOkComToken()
    {
        // Arrange
        await RegisterTestUser("loginuser", "login@example.com");
        var loginDto = new LoginDto { Email = "login@example.com", Password = "password123" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginResponse);
        Assert.NotEmpty(loginResponse.Token);
    }

    [Fact]
    public async Task Login_ComSenhaInvalida_DeveRetornarUnauthorized()
    {
        // Arrange
        await RegisterTestUser("loginuser2", "login2@example.com");
        var loginDto = new LoginDto { Email = "login2@example.com", Password = "senhaincorreta" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // Método auxiliar para simplificar o registro nos testes
    private async Task RegisterTestUser(string username, string email)
    {
        var registerDto = new RegisterDto { Username = username, Email = email, Password = "password123" };
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);
    }
}