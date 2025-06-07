using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Geta.API.DTOs.Auth;
using Xunit;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace Geta.API.Tests;

public class SearchControllerTests : IClassFixture<GetaWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly GetaWebApplicationFactory<Program> _factory;

    public SearchControllerTests(GetaWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndGetToken(string username)
    {
        var registerDto = new RegisterDto { Username = username, Email = $"{username}@test.com", Password = "password123" };
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        response.EnsureSuccessStatusCode();
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        return loginResponse!.Token;
    }

    [Theory]
    [InlineData("test", 2)]
    [InlineData("user", 3)]
    [InlineData("specific", 1)]
    [InlineData("nonexistent", 0)]
    public async Task SearchUsers_ComQueryValida_DeveRetornarUsuariosCorrespondentes(string query, int expectedCount)
    {
        // Arrange
        var token = await RegisterAndGetToken("mainuser");
        await RegisterAndGetToken("testuser1");
        await RegisterAndGetToken("testuser2");
        await RegisterAndGetToken("specific_user");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/search/users?query={query}");

        // Assert
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        Assert.NotNull(users);
        Assert.Equal(expectedCount, users.Count);
    }
}