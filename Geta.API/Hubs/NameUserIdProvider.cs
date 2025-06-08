// Este serviço ajuda o SignalR a identificar usuários pelo seu ID único (claim "NameIdentifier" do token JWT).
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Geta.API.Hubs;

public class NameUserIdProvider : IUserIdProvider
{
    public virtual string? GetUserId(HubConnectionContext connection)
    {
        // Retorna o valor da claim "NameIdentifier", que definimos como o ID do usuário no TokenService.
        return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}