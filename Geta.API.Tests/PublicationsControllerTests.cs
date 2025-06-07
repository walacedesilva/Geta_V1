using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Geta.API.DTOs.Auth;
using Geta.API.DTOs.Publication;
using Geta.API.Entities;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System;

namespace Geta.API.Tests;

public class PublicationsControllerTests : IClassFixture<GetaWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly GetaWebApplicationFactory<Program> _factory;

    public PublicationsControllerTests(GetaWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetJwtToken()
    {
        var registerDto = new RegisterDto { Username = $"testuser_{Guid.NewGuid()}", Email = $"test_{Guid.NewGuid()}@example.com", Password = "password123" };
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        response.EnsureSuccessStatusCode();
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        return loginResponse!.Token;
    }

    [Fact]
    public async Task CreatePublication_ComUsuarioAutenticado_DeveRetornarCreated()
    {
        // Arrange
        var token = await GetJwtToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var createDto = new CreatePublicationDto { Content = "Este é um teste de publicação!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/publications", createDto);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var publicationDto = await response.Content.ReadFromJsonAsync<PublicationDto>();
        Assert.NotNull(publicationDto);
    }

    [Fact]
    public async Task GetPublications_QuandoExistemPublicacoes_DeveRetornarListaDePublicacoes()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
            var user = new User { Username = "author", Email = "author@test.com", PasswordHash = "hash" };
            user.Profile = new Profile();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            context.Publications.Add(new Publication { Content = "Publicação para o teste GET", UserId = user.Id });
            await context.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync("/api/publications");

        // Assert
        response.EnsureSuccessStatusCode();
        var publications = await response.Content.ReadFromJsonAsync<PublicationDto[]>();
        Assert.NotNull(publications);
        Assert.Single(publications);
        Assert.Contains(publications, p => p.Content == "Publicação para o teste GET");
    }
}