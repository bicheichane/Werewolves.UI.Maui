using Werewolves.Core.GameLogic.Services;
using Werewolves.Core.StateModels.Core;
using Werewolves.Core.StateModels.Models;

namespace Werewolves.Client.Services;

/// <summary>
/// Wraps the Core GameService to implement local IGameService interface.
/// </summary>
public class GameServiceWrapper : IGameService
{
    private readonly GameService _coreService = new();

    public ModeratorInstruction StartNewGame(GameSessionConfig config)
        => _coreService.StartNewGame(config);

    public ProcessInstructionResult ProcessInstruction(Guid gameId, ModeratorResponse response)
        => _coreService.ProcessInstruction(gameId, response);

    public IGameSession? GetGameStateView(Guid gameId)
        => _coreService.GetGameStateView(gameId);
}
