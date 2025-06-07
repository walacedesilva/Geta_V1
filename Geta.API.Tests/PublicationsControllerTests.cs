using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Geta.API.DTOs.Auth;
using Geta.API.DTOs.Publication;
using Geta.API.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

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

    // Método auxiliar para registrar um usuário e obter um token JWT válido
    private async Task<string> GetJwtToken()
    {
        var registerDto = new RegisterDto
        {
            Username = $"testuser_{Guid.NewGuid()}",
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = "password123"
        };
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
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var publicationDto = await response.Content.ReadFromJsonAsync<PublicationDto>();
        Assert.NotNull(publicationDto);
        Assert.Equal("Este é um teste de publicação!", publicationDto.Content);
    }

    [Fact]
    public async Task CreatePublication_SemAutenticacao_DeveRetornarUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;
        var createDto = new CreatePublicationDto { Content = "Conteúdo não autorizado" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/publications", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetPublications_DeveRetornarListaDePublicacoes()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
            var user = new User { Username = "author", Email = "author@test.com", PasswordHash = "hash" };
            context.Users.Add(user);
            context.Publications.Add(new Publication { Content = "Publicação para o teste GET", User = user });
            await context.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync("/api/publications");

        // Assert
        response.EnsureSuccessStatusCode();
        var publications = await response.Content.ReadFromJsonAsync<PublicationDto[]>();
        Assert.NotNull(publications);
        Assert.True(publications.Length > 0);
        Assert.Contains(publications, p => p.Content == "Publicação para o teste GET");
    }
}