using System.Security.Claims;
using Geta.API.Data;
using Geta.API.DTOs.Chat;
using Geta.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Geta.API.Hubs;

[Authorize] // Apenas usuários autenticados podem se conectar a este hub.
public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;
    // Para rastrear usuários online. Em produção, use uma solução distribuída como Redis.
    private static readonly Dictionary<int, string> OnlineUsers = new();

    public ChatHub(ApplicationDbContext context)
    {
        _context = context;
    }

    // Método chamado quando um cliente se conecta.
    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return;

        // Adiciona ou atualiza o usuário na lista de online com a nova connectionId.
        OnlineUsers[userId] = Context.ConnectionId;

        // Notifica outros clientes que este usuário está online.
        await Clients.Others.SendAsync("UserConnected", userId);

        await base.OnConnectedAsync();
        Console.WriteLine($"--> Usuário conectado: {userId} com ConnectionId: {Context.ConnectionId}");
    }

    // Método chamado quando um cliente se desconecta.
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetCurrentUserId();
        if (userId != 0)
        {
            OnlineUsers.Remove(userId);
            // Notifica outros clientes que este usuário ficou offline.
            await Clients.Others.SendAsync("UserDisconnected", userId);
            Console.WriteLine($"--> Usuário desconectado: {userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Método que o cliente chamará para enviar uma mensagem privada.
    public async Task SendPrivateMessage(int receiverId, string content)
    {
        var senderId = GetCurrentUserId();
        if (senderId == 0 || receiverId == 0 || string.IsNullOrWhiteSpace(content))
        {
            // Poderia enviar um erro de volta ao cliente se necessário.
            // await Clients.Caller.SendAsync("ReceiveError", "Dados da mensagem inválidos.");
            return;
        }

        var message = new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content,
            SentAt = DateTime.UtcNow
        };

        // Salva a mensagem no banco de dados.
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        var messageDto = new ChatMessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            ReceiverId = message.ReceiverId,
            Content = message.Content,
            SentAt = message.SentAt,
            IsRead = message.IsRead
        };

        // Envia a mensagem para o destinatário (se estiver online).
        // Graças ao IUserIdProvider, podemos usar o ID do usuário diretamente.
        await Clients.User(receiverId.ToString()).SendAsync("ReceivePrivateMessage", messageDto);

        // Envia a mensagem de volta para o remetente também, para que a UI dele seja atualizada.
        await Clients.Caller.SendAsync("ReceivePrivateMessage", messageDto);
    }

    // Método auxiliar para obter o ID do usuário logado a partir do contexto do Hub.
    private int GetCurrentUserId()
    {
        var userIdClaim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim) : 0;
    }
}