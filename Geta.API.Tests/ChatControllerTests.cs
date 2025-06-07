using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Geta.API.DTOs.Auth;
using Geta.API.DTOs.Chat;
using Geta.API.Entities;
using Xunit;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace Geta.API.Tests;

public class ChatControllerTests : IClassFixture<GetaWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly GetaWebApplicationFactory<Program> _factory;

    public ChatControllerTests(GetaWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string token, int userId)> RegisterAndGetUser(string username)
    {
        var registerDto = new RegisterDto { Username = username, Email = $"{username}@test.com", Password = "password123" };
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        response.EnsureSuccessStatusCode();
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        return (loginResponse!.Token, loginResponse.User.Id);
    }

    [Fact]
    public async Task GetChatHistory_ComUsuarioAutenticado_DeveRetornarHistoricoDeMensagens()
    {
        // Arrange
        var (user1Token, user1Id) = await RegisterAndGetUser("user1");
        var (_, user2Id) = await RegisterAndGetUser("user2");

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
            context.ChatMessages.Add(new ChatMessage { SenderId = user1Id, ReceiverId = user2Id, Content = "Olá, user2!" });
            context.ChatMessages.Add(new ChatMessage { SenderId = user2Id, ReceiverId = user1Id, Content = "Olá, user1!" });
            await context.SaveChangesAsync();
        }

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);

        // Act
        var response = await _client.GetAsync($"/api/chat/history/{user2Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var messages = await response.Content.ReadFromJsonAsync<List<ChatMessageDto>>();
        Assert.NotNull(messages);
        Assert.Equal(2, messages.Count);
    }
}