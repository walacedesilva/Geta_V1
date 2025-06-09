using Geta.API.Data;
using Geta.API.DTOs.Chat;
using Geta.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Geta.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        // NOVO MÉTODO: Chamado pelo cliente para enviar uma mensagem
        public async Task SendMessage(string? toUserId, string message)
        {
            var fromUserId = int.Parse(Context.UserIdentifier!);
            var fromUserUsername = Context.User?.FindFirstValue(ClaimTypes.Name) ?? "unknown";

            // 1. Criar a entidade da mensagem
            var chatMessage = new ChatMessage
            {
                SenderId = fromUserId,
                ReceiverId = !String.IsNullOrEmpty(toUserId) ? int.Parse(toUserId) : null,
                Content = message,
                SentAt = DateTime.UtcNow
            };

            // 2. Salvar a mensagem no banco de dados
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // 3. Enviar a mensagem para o destinatário
            if (!String.IsNullOrEmpty(toUserId))
            {
                await Clients.User(toUserId).SendAsync("ReceiveMessage", new ChatMessageDto
                {
                    Id = chatMessage.Id,
                    SenderId = chatMessage.SenderId,
                    ReceiverId = chatMessage.ReceiverId,
                    Content = chatMessage.Content,
                    SentAt = chatMessage.SentAt,
                    SenderUsername = fromUserUsername
                });
            }
            // Opcional: Enviar a mensagem de volta para o remetente para confirmar o envio
            await Clients.Caller.SendAsync("ReceiveMessage", new ChatMessageDto
            {
                Id = chatMessage.Id,
                SenderId = chatMessage.SenderId,
                ReceiverId = chatMessage.ReceiverId,
                Content = chatMessage.Content,
                SentAt = chatMessage.SentAt,
                SenderUsername = fromUserUsername
            });
        }

        // NOVO MÉTODO: Chamado pelo cliente para notificar que está a escrever
        public async Task UserIsTyping(string toUserId)
        {
            var fromUserId = Context.UserIdentifier;
            // Envia a notificação "typing" para todos os usuários conectados, exceto o remetente
            await Clients.User(toUserId).SendAsync("UserTyping", fromUserId);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            // Notifica outros sobre a conexão, se necessário
            // Exemplo: await Clients.All.SendAsync("UserIsOnline", userId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            // Notifica outros sobre a desconexão, se necessário
            // Exemplo: await Clients.All.SendAsync("UserIsOffline", userId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}