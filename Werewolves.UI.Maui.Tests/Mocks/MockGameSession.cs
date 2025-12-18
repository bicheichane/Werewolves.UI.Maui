using Werewolves.Core.StateModels.Core;
using Werewolves.Core.StateModels.Enums;
using Werewolves.Core.StateModels.Log;

namespace Werewolves.UI.Maui.Tests.Mocks;

/// <summary>
/// Mock implementation of IGameSession for testing.
/// Provides simple state container without any game logic.
/// </summary>
public class MockGameSession : IGameSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int TurnNumber { get; set; } = 1;
    public GamePhase Phase { get; set; } = GamePhase.Night;
    
    private readonly List<MockPlayer> _players = new();
    private readonly List<GameLogEntryBase> _historyLog = new();
    private readonly Dictionary<MainRoleType, int> _rolesInPlay = new();

    public IEnumerable<GameLogEntryBase> GameHistoryLog => _historyLog;

    public GamePhase GetCurrentPhase() => Phase;
    
    public IEnumerable<IPlayer> GetPlayers() => _players;
    
    public IPlayer GetPlayer(Guid playerId) => 
        _players.First(p => p.Id == playerId);
    
    public IPlayerState GetPlayerState(Guid playerId) => 
        GetPlayer(playerId).State;
    
    public int RoleInPlayCount(MainRoleType type) => 
        _rolesInPlay.GetValueOrDefault(type, 0);
    
    public string Serialize() => 
        System.Text.Json.JsonSerializer.Serialize(this);

    // Test helper methods
    public void AddPlayer(MockPlayer player) => _players.Add(player);
    public void AddLogEntry(GameLogEntryBase entry) => _historyLog.Add(entry);
    public void SetRoleCount(MainRoleType role, int count) => _rolesInPlay[role] = count;
    public void ClearPlayers() => _players.Clear();
}
