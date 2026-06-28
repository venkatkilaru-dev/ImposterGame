using ImposterGameV3.Models;

namespace ImposterGameV3.Services;

public class GameService
{
    private readonly object _lock = new();
    private readonly Dictionary<string, GameRoom> _rooms = new();
    public event Action<string>? RoomChanged;

    private readonly string[] _words =
    {
        "Pizza","Beach","Airport","School","Hospital","Movie Theater","Restaurant","Gym",
        "Library","Temple","Mall","Bus Stop","Coffee Shop","Hotel","Bank","Park",
        "Stadium","Train Station","Wedding","College","Office","Supermarket",
        "Police Station","Gas Station","Swimming Pool","Cricket Ground","Birthday Party",
        "Doctor Clinic","Barber Shop","Pharmacy","Museum","Zoo","Concert","Court Room"
    };

    public GameRoom CreateRoom()
    {
        lock (_lock)
        {
            string code;
            do { code = Random.Shared.Next(1000, 9999).ToString(); }
            while (_rooms.ContainsKey(code));

            var room = new GameRoom { RoomCode = code };
            _rooms[code] = room;
            return room;
        }
    }

    public GameRoom? GetRoom(string code)
    {
        lock (_lock)
        {
            _rooms.TryGetValue(code, out var room);
            return room;
        }
    }

    public Player? JoinRoom(string code, string playerName, bool isHost = false)
    {
        lock (_lock)
        {
            var room = GetRoomUnsafe(code);
            if (room is null || room.GameStarted) return null;

            var player = new Player
            {
                Name = playerName.Trim(),
                IsHost = isHost || room.Players.Count == 0
            };

            room.Players.Add(player);
            if (player.IsHost) room.HostPlayerId = player.Id;

            AddSystemMessage(room, $"{player.Name} joined the room.");
            Notify(code);
            return player;
        }
    }

    public void StartGame(string code, string playerId)
    {
        lock (_lock)
        {
            var room = GetRoomUnsafe(code);
            if (room is null || room.HostPlayerId != playerId || room.Players.Count < 3) return;

            room.SecretWord = _words[Random.Shared.Next(_words.Length)];
            room.GameStarted = true;
            room.VotingStarted = false;
            room.GameEnded = false;

            foreach (var p in room.Players)
            {
                p.IsImposter = false;
                p.Votes = 0;
                p.HasVoted = false;
            }

            room.Players[Random.Shared.Next(room.Players.Count)].IsImposter = true;
            AddSystemMessage(room, "Game started.");
            Notify(code);
        }
    }

    public void StartVoting(string code, string playerId)
    {
        lock (_lock)
        {
            var room = GetRoomUnsafe(code);
            if (room is null || room.HostPlayerId != playerId) return;
            room.VotingStarted = true;
            AddSystemMessage(room, "Voting started.");
            Notify(code);
        }
    }

    public void Vote(string code, string voterId, string votedId)
    {
        lock (_lock)
        {
            var room = GetRoomUnsafe(code);
            if (room is null || room.GameEnded) return;

            var voter = room.Players.FirstOrDefault(x => x.Id == voterId);
            var voted = room.Players.FirstOrDefault(x => x.Id == votedId);
            if (voter is null || voted is null || voter.HasVoted) return;

            voted.Votes++;
            voter.HasVoted = true;
            AddSystemMessage(room, $"{voter.Name} voted.");
            Notify(code);
        }
    }

    public void EndGame(string code, string playerId)
    {
        lock (_lock)
        {
            var room = GetRoomUnsafe(code);
            if (room is null || room.HostPlayerId != playerId) return;
            room.GameEnded = true;
            AddSystemMessage(room, "Host revealed the result.");
            Notify(code);
        }
    }

    public void ResetAndStart(string code, string playerId)
    {
        lock (_lock)
        {
            var room = GetRoomUnsafe(code);
            if (room is null || room.HostPlayerId != playerId) return;

            room.SecretWord = "";
            room.GameStarted = false;
            room.VotingStarted = false;
            room.GameEnded = false;

            foreach (var p in room.Players)
            {
                p.IsImposter = false;
                p.Votes = 0;
                p.HasVoted = false;
            }
        }
        StartGame(code, playerId);
    }

    public void SetMedia(string code, string playerId, bool on)
    {
        lock (_lock)
        {
            var room = GetRoomUnsafe(code);
            var player = room?.Players.FirstOrDefault(x => x.Id == playerId);
            if (room is null || player is null) return;

            player.CameraOn = on;
            player.MicOn = on;
            AddSystemMessage(room, $"{player.Name} turned {(on ? "on" : "off")} camera/mic.");
            Notify(code);
        }
    }

    public void AddChat(string code, string playerId, string message)
    {
        lock (_lock)
        {
            var room = GetRoomUnsafe(code);
            var player = room?.Players.FirstOrDefault(x => x.Id == playerId);
            if (room is null || player is null || string.IsNullOrWhiteSpace(message)) return;

            room.Messages.Add(new ChatMessage { PlayerName = player.Name, Message = message.Trim() });
            Notify(code);
        }
    }

    private GameRoom? GetRoomUnsafe(string code)
    {
        _rooms.TryGetValue(code, out var room);
        return room;
    }

    private static void AddSystemMessage(GameRoom room, string message)
    {
        room.Messages.Add(new ChatMessage { PlayerName = "System", Message = message });
    }

    private void Notify(string code) => RoomChanged?.Invoke(code);
}
