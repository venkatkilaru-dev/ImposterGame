using ImposterGameClean.Models;

namespace ImposterGameClean.Services;

public class GameService
{
    private readonly object _lock = new();
    private readonly Dictionary<string, GameRoom> _rooms = new();

    private readonly string[] _words =
    {
        "Pizza", "Beach", "Airport", "School", "Hospital", "Movie Theater",
        "Restaurant", "Gym", "Library", "Temple", "Mall", "Bus Stop",
        "Coffee Shop", "Hotel", "Bank", "Park", "Stadium", "Train Station",
        "Wedding", "College", "Office", "Supermarket", "Police Station",
        "Gas Station", "Swimming Pool", "Cricket Ground", "Birthday Party",
        "Doctor Clinic", "Barber Shop", "Pharmacy", "Museum", "Zoo",
        "Concert", "Court Room", "Apartment", "Kitchen", "Garage"
    };

    public GameRoom CreateRoom()
    {
        lock (_lock)
        {
            string code;

            do
            {
                code = Random.Shared.Next(1000, 9999).ToString();
            }
            while (_rooms.ContainsKey(code));

            var room = new GameRoom
            {
                RoomCode = code
            };

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

    public Player? JoinRoom(string code, string playerName)
    {
        lock (_lock)
        {
            var room = GetRoomUnsafe(code);

            if (room is null || room.GameStarted)
                return null;

            var player = new Player
            {
                Name = playerName.Trim()
            };

            room.Players.Add(player);
            AddSystemMessage(room, $"{player.Name} joined the room.");
            return player;
        }
    }

    public void StartGame(string code)
    {
        lock (_lock)
        {
            var room = GetRoomUnsafe(code);

            if (room is null || room.Players.Count < 3)
                return;

            room.SecretWord = _words[Random.Shared.Next(_words.Length)];
            room.GameStarted = true;
            room.VotingStarted = false;
            room.GameEnded = false;

            foreach (var player in room.Players)
            {
                player.IsImposter = false;
                player.Votes = 0;
            }

            var imposterIndex = Random.Shared.Next(room.Players.Count);
            room.Players[imposterIndex].IsImposter = true;

            AddSystemMessage(room, "Game started.");
        }
    }

    public void StartVoting(string code)
    {
        lock (_lock)
        {
            var room = GetRoomUnsafe(code);

            if (room is null)
                return;

            room.VotingStarted = true;
            AddSystemMessage(room, "Voting started.");
        }
    }

    public void Vote(string code, string voterPlayerId, string votedPlayerId)
    {
        lock (_lock)
        {
            var room = GetRoomUnsafe(code);

            if (room is null || room.GameEnded)
                return;

            var votedPlayer = room.Players.FirstOrDefault(x => x.Id == votedPlayerId);
            var voter = room.Players.FirstOrDefault(x => x.Id == voterPlayerId);

            if (votedPlayer is null || voter is null)
                return;

            votedPlayer.Votes++;
            AddSystemMessage(room, $"{voter.Name} voted.");
        }
    }

    public void EndGame(string code)
    {
        lock (_lock)
        {
            var room = GetRoomUnsafe(code);

            if (room is null)
                return;

            room.GameEnded = true;
            AddSystemMessage(room, "Result revealed.");
        }
    }

    public void ResetAndStart(string code)
    {
        lock (_lock)
        {
            var room = GetRoomUnsafe(code);

            if (room is null)
                return;

            room.SecretWord = "";
            room.GameStarted = false;
            room.VotingStarted = false;
            room.GameEnded = false;

            foreach (var player in room.Players)
            {
                player.IsImposter = false;
                player.Votes = 0;
            }
        }

        StartGame(code);
    }

    public void SetMedia(string code, string playerId, bool cameraOn, bool micOn)
    {
        lock (_lock)
        {
            var room = GetRoomUnsafe(code);
            var player = room?.Players.FirstOrDefault(x => x.Id == playerId);

            if (player is null)
                return;

            player.CameraOn = cameraOn;
            player.MicOn = micOn;
        }
    }

    public void AddChat(string code, string playerId, string message)
    {
        lock (_lock)
        {
            var room = GetRoomUnsafe(code);
            var player = room?.Players.FirstOrDefault(x => x.Id == playerId);

            if (room is null || player is null || string.IsNullOrWhiteSpace(message))
                return;

            room.Messages.Add(new ChatMessage
            {
                PlayerName = player.Name,
                Message = message.Trim()
            });
        }
    }

    private GameRoom? GetRoomUnsafe(string code)
    {
        _rooms.TryGetValue(code, out var room);
        return room;
    }

    private static void AddSystemMessage(GameRoom room, string message)
    {
        room.Messages.Add(new ChatMessage
        {
            PlayerName = "System",
            Message = message
        });
    }
}
