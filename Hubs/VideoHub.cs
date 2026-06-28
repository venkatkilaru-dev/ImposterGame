using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ImposterGameV3.Hubs;

public class VideoUser
{
    public string ConnectionId { get; set; } = "";
    public string PlayerId { get; set; } = "";
    public string PlayerName { get; set; } = "";
    public string RoomCode { get; set; } = "";
}

public class VideoHub : Hub
{
    private static readonly ConcurrentDictionary<string, VideoUser> Users = new();

    public async Task JoinVideoRoom(string roomCode, string playerId, string playerName)
    {
        var current = new VideoUser
        {
            ConnectionId = Context.ConnectionId,
            PlayerId = playerId,
            PlayerName = playerName,
            RoomCode = roomCode
        };

        Users[Context.ConnectionId] = current;

        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

        var existingUsers = Users.Values
            .Where(x => x.RoomCode == roomCode && x.ConnectionId != Context.ConnectionId)
            .Select(x => new
            {
                connectionId = x.ConnectionId,
                playerId = x.PlayerId,
                playerName = x.PlayerName
            })
            .ToList();

        await Clients.Caller.SendAsync("ExistingUsers", existingUsers);

        await Clients.OthersInGroup(roomCode).SendAsync("UserJoined", new
        {
            connectionId = current.ConnectionId,
            playerId = current.PlayerId,
            playerName = current.PlayerName
        });
    }

    public Task SendOffer(string targetConnectionId, object offer)
    {
        var sender = GetCurrentUser();
        return Clients.Client(targetConnectionId).SendAsync("ReceiveOffer", Context.ConnectionId, sender?.PlayerName ?? "Player", offer);
    }

    public Task SendAnswer(string targetConnectionId, object answer)
    {
        return Clients.Client(targetConnectionId).SendAsync("ReceiveAnswer", Context.ConnectionId, answer);
    }

    public Task SendIceCandidate(string targetConnectionId, object candidate)
    {
        return Clients.Client(targetConnectionId).SendAsync("ReceiveIceCandidate", Context.ConnectionId, candidate);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Users.TryRemove(Context.ConnectionId, out var user))
        {
            await Clients.OthersInGroup(user.RoomCode).SendAsync("UserLeft", Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, user.RoomCode);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private VideoUser? GetCurrentUser()
    {
        Users.TryGetValue(Context.ConnectionId, out var user);
        return user;
    }
}
