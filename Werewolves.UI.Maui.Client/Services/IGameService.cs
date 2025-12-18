using Werewolves.Core.StateModels.Core;
using Werewolves.Core.StateModels.Models;

namespace Werewolves.UI.Maui.Client.Services;

/// <summary>
/// Interface for game service operations.
/// This local interface allows DI substitution for testing.
/// TODO: Remove when Core exposes IGameService officially.
/// </summary>
public interface IGameService
{
    ModeratorInstruction StartNewGame(GameSessionConfig config);
    ProcessInstructionResult ProcessInstruction(Guid gameId, ModeratorResponse response);
    IGameSession? GetGameStateView(Guid gameId);
}
