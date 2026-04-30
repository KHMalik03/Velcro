using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace velcro.Hubs;

[Authorize]
public class BoardHub : Hub
{
    public async Task JoinBoard(string boardId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"board:{boardId}");
        await Clients.Group($"board:{boardId}").SendAsync("UserJoined", Context.UserIdentifier);
    }

    public async Task LeaveBoard(string boardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"board:{boardId}");
        await Clients.Group($"board:{boardId}").SendAsync("UserLeft", Context.UserIdentifier);
    }

    public async Task StartEditing(string boardId, string cardId)
    {
        await Clients.Group($"board:{boardId}").SendAsync("UserStartedEditing", Context.UserIdentifier, cardId);
    }

    public async Task StopEditing(string boardId, string cardId)
    {
        await Clients.Group($"board:{boardId}").SendAsync("UserStoppedEditing", Context.UserIdentifier, cardId);
    }
}
