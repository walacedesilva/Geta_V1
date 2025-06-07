using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Geta.API.DTOs.Auth;
using Geta.API.DTOs.Profile;
using Xunit;

namespace Geta.API.Tests;

public class ProfileControllerTests : IClassFixture<GetaWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProfileControllerTests(GetaWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<(string token, int userId)> RegisterAndGetUser()
    {
        var registerDto = new RegisterDto { Username = $"profileuser_{Guid.NewGuid()}", Email = $"profile_{Guid.NewGuid()}@example.com", Password = "password123" };
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        response.EnsureSuccessStatusCode();
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        return (loginResponse!.Token, loginResponse.User.Id);
    }

    [Fact]
    public async Task GetProfile_ComUserIdValido_DeveRetornarOkEPerfil()
    {
        // Arrange
        var (_, userId) = await RegisterAndGetUser();

        // Act
        var response = await _client.GetAsync($"/api/profile/{userId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
        Assert.NotNull(profile);
        Assert.Equal(userId, profile.UserId);
    }

    [Fact]
    public async Task UpdateProfile_ComUsuarioAutenticado_DeveRetornarNoContent()
    {
        // Arrange
        var (token, _) = await RegisterAndGetUser();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var updateDto = new UpdateProfileDto { Bio = "Nova biografia.", Location = "Cidade Teste" };

        // Act
        var response = await _client.PutAsJsonAsync("/api/profile", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}