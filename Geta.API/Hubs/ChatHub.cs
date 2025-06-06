// Hubs/ChatHub.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Geta.API.Hubs;

[Authorize] // Exige que o usuário esteja autenticado para se conectar
public class ChatHub : Hub
{
    // Métodos do Hub serão adicionados aqui, como SendPrivateMessage, etc.
}