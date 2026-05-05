using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace velcro.Hubs;

// SignalR Hub : classe qui gère les connexions WebSocket des clients
// [Authorize] : seuls les utilisateurs avec un JWT valide peuvent se connecter
[Authorize]
public class BoardHub : Hub
{
    // Le client appelle JoinBoard pour s'abonner aux événements d'un board spécifique
    // Groups : mécanisme SignalR qui permet d'envoyer un message à un sous-ensemble de clients
    public async Task JoinBoard(string boardId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"board:{boardId}");
        await Clients.Group($"board:{boardId}").SendAsync("UserJoined", Context.UserIdentifier);
    }

    // Retire le client du groupe → il ne reçoit plus les événements du board
    public async Task LeaveBoard(string boardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"board:{boardId}");
        await Clients.Group($"board:{boardId}").SendAsync("UserLeft", Context.UserIdentifier);
    }

    // Notifie les autres membres qu'un utilisateur est en train d'éditer une carte
    public async Task StartEditing(string boardId, string cardId)
    {
        await Clients.Group($"board:{boardId}").SendAsync("UserStartedEditing", Context.UserIdentifier, cardId);
    }

    // Notifie les autres membres que l'édition est terminée
    public async Task StopEditing(string boardId, string cardId)
    {
        await Clients.Group($"board:{boardId}").SendAsync("UserStoppedEditing", Context.UserIdentifier, cardId);
    }
}
