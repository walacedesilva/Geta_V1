using System.Security.Claims;
using Geta.API.Data;
using Geta.API.DTOs.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Geta.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ChatController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/chat/history/{contactId}
    [HttpGet("history/{contactId:int}")]
    public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetChatHistory(int contactId)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var messages = await _context.ChatMessages
            .Where(m => (m.SenderId == currentUserId && m.ReceiverId == contactId) ||
                        (m.SenderId == contactId && m.ReceiverId == currentUserId))
            .OrderBy(m => m.SentAt)
            .Select(m => new ChatMessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ReceiverId = (int)m.ReceiverId,
                Content = m.Content,
                SentAt = m.SentAt,
                IsRead = m.IsRead
            })
            .ToListAsync();

        return Ok(messages);
    }

    // GET: api/chat/history/{contactId}
    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetChatHistory()
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var messages = await _context.ChatMessages
            .Where(m => (m.SenderId == currentUserId) ||
                        (m.ReceiverId == currentUserId))
            .OrderBy(m => m.SentAt)
            .Select(m => new ChatMessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ReceiverId = (int)m.ReceiverId,
                Content = m.Content,
                SentAt = m.SentAt,
                IsRead = m.IsRead
            })
            .ToListAsync();

        return Ok(messages);
    }

    // GET: api/chat/conversations
    [HttpGet("conversations")]
    public async Task<ActionResult<IEnumerable<ConversationDto>>> GetConversations()
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Esta query é complexa e pode ser otimizada com SQL puro ou uma view no banco de dados.
        // Agrupa todas as mensagens por parceiro de conversa.
        var conversations = await _context.ChatMessages
            .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
            .GroupBy(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
            .Select(g => new
            {
                ContactId = g.Key,
                LastMessage = g.OrderByDescending(m => m.SentAt).FirstOrDefault()
            })
            .Join(
                _context.Users.Include(u => u.Profile), // Junta com usuários para obter nomes e avatares
                convo => convo.ContactId,
                user => user.Id,
                (convo, user) => new ConversationDto
                {
                    ContactId = (int)convo.ContactId,
                    ContactUsername = user.Username,
                    ContactAvatarUrl = user.Profile != null ? user.Profile.AvatarUrl : null,
                    LastMessage = convo.LastMessage != null ? convo.LastMessage.Content : "",
                    LastMessageTimestamp = convo.LastMessage != null ? convo.LastMessage.SentAt : DateTime.MinValue,
                    UnreadCount = _context.ChatMessages.Count(m => m.ReceiverId == currentUserId && m.SenderId == convo.ContactId && !m.IsRead)
                }
            )
            .OrderByDescending(c => c.LastMessageTimestamp)
            .ToListAsync();

        return Ok(conversations);
    }
}