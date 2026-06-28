namespace ImposterGameV3.Models;

public class Player
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public bool IsHost { get; set; }
    public bool IsImposter { get; set; }
    public int Votes { get; set; }
    public bool HasVoted { get; set; }
    public bool CameraOn { get; set; }
    public bool MicOn { get; set; }
}

public class ChatMessage
{
    public string PlayerName { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime SentAt { get; set; } = DateTime.Now;
}

public class GameRoom
{
    public string RoomCode { get; set; } = "";
    public string HostPlayerId { get; set; } = "";
    public string SecretWord { get; set; } = "";
    public bool GameStarted { get; set; }
    public bool VotingStarted { get; set; }
    public bool GameEnded { get; set; }
    public List<Player> Players { get; set; } = new();
    public List<ChatMessage> Messages { get; set; } = new();
}
