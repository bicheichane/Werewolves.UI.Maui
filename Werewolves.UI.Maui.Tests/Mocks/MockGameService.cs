using Werewolves.Core.StateModels.Core;
using Werewolves.Core.StateModels.Models;
using Werewolves.UI.Maui.Client.Services;
using Werewolves.UI.Maui.Tests.Infrastructure;

namespace Werewolves.UI.Maui.Tests.Mocks;

/// <summary>
/// Mock implementation of IGameService for testing.
/// Allows state injection and configurable responses.
/// </summary>
public class MockGameService : IGameService
{
    public MockGameSession CurrentSession { get; private set; } = new();
    public ModeratorInstruction? LastInstruction { get; set; }
    public ProcessInstructionResult? NextResult { get; set; }

    public event Action? StateChanged;

    public ModeratorInstruction StartNewGame(GameSessionConfig config)
    {
        CurrentSession = new MockGameSession { Id = Guid.NewGuid() };
        
        // Add players from config
        foreach (var playerName in config.PlayerNames)
        {
            CurrentSession.AddPlayer(new MockPlayer { Name = playerName });
        }
        
        // Return a mock instruction if set, otherwise throw
        if (LastInstruction != null)
            return LastInstruction;
            
        throw new NotImplementedException("Set LastInstruction before calling StartNewGame, or use ApplyTestState for test scenarios");
    }

    public ProcessInstructionResult ProcessInstruction(Guid gameId, ModeratorResponse response)
    {
        if (NextResult != null)
            return NextResult;
        
        throw new NotImplementedException("Set NextResult before calling ProcessInstruction");
    }

    public IGameSession? GetGameStateView(Guid gameId)
    {
        return CurrentSession.Id == gameId ? CurrentSession : null;
    }

    /// <summary>
    /// Apply a test state from DTO (used by TestBridge)
    /// </summary>
    public void ApplyTestState(TestStateDto state)
    {
        CurrentSession = state.ToMockSession();
        LastInstruction = state.CurrentInstruction;
        StateChanged?.Invoke();
    }

    /// <summary>
    /// Manually trigger a state change event
    /// </summary>
    public void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
