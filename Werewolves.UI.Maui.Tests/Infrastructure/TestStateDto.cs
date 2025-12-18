using System.Text.Json.Serialization;
using Werewolves.Core.StateModels.Enums;
using Werewolves.Core.StateModels.Models;
using Werewolves.UI.Maui.Tests.Mocks;

namespace Werewolves.UI.Maui.Tests.Infrastructure;

/// <summary>
/// DTO for injecting test state via the TestBridge.
/// This is the JSON structure that will be passed from JavaScript.
/// </summary>
public class TestStateDto
{
    public Guid GameId { get; set; } = Guid.NewGuid();
    public int TurnNumber { get; set; } = 1;
    public GamePhase Phase { get; set; } = GamePhase.Night;
    public List<TestPlayerDto> Players { get; set; } = new();
    
    // Instruction will need custom deserialization based on type
    [JsonIgnore]
    public ModeratorInstruction? CurrentInstruction { get; set; }
    
    /// <summary>
    /// The type name of the instruction (e.g., "AssignRolesInstruction")
    /// </summary>
    public string? InstructionType { get; set; }
    
    /// <summary>
    /// Instruction-specific data for reconstruction
    /// </summary>
    public Dictionary<string, object>? InstructionData { get; set; }

    /// <summary>
    /// Converts this DTO to a MockGameSession
    /// </summary>
    public MockGameSession ToMockSession()
    {
        var session = new MockGameSession
        {
            Id = GameId,
            TurnNumber = TurnNumber,
            Phase = Phase
        };
        
        foreach (var playerDto in Players)
        {
            session.AddPlayer(playerDto.ToMockPlayer());
        }
        
        return session;
    }
}

/// <summary>
/// DTO representing a player for test state injection
/// </summary>
public class TestPlayerDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Player";
    public bool IsAlive { get; set; } = true;
    public bool IsSheriff { get; set; }
    public MainRoleType? Role { get; set; }

    /// <summary>
    /// Converts this DTO to a MockPlayer
    /// </summary>
    public MockPlayer ToMockPlayer() => new()
    {
        Id = Id,
        Name = Name,
        State = new MockPlayerState
        {
            IsAlive = IsAlive,
            IsSheriff = IsSheriff,
            Role = Role
        }
    };
}
