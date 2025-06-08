using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Geta.API.DTOs.Auth;
using Xunit;

namespace Geta.API.Tests;

public class AuthControllerTests : IClassFixture<GetaWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthControllerTests(GetaWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ComDadosValidos_DeveRetornarOkComTokenEUsuario()
    {
        // Arrange
        var registerDto = new RegisterDto { Username = "newuser", Email = "newuser@example.com", Password = "password123" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginResponse);
        Assert.NotEmpty(loginResponse.Token);
    }

    [Fact]
    public async Task Register_ComUsernameExistente_DeveRetornarBadRequest()
    {
        // Arrange
        await RegisterTestUser("existinguser", "email1@example.com");
        var registerDto = new RegisterDto { Username = "existinguser", Email = "email2@example.com", Password = "password123" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task RegisterTestUser(string username, string email)
    {
        var registerDto = new RegisterDto { Username = username, Email = email, Password = "password123" };
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        response.EnsureSuccessStatusCode();
    }
}