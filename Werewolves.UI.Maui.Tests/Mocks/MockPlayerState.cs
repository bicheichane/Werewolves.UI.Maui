using Werewolves.Core.StateModels.Core;
using Werewolves.Core.StateModels.Enums;

namespace Werewolves.UI.Maui.Tests.Mocks;

/// <summary>
/// Mock implementation of IPlayerState for testing.
/// All properties have public get/set for easy test configuration.
/// </summary>
public class MockPlayerState : IPlayerState
{
    public bool IsAlive { get; set; } = true;
    public bool IsSheriff { get; set; }
    public bool IsImmuneToLynching { get; set; }
    public bool IsInfected { get; set; }
    public MainRoleType? Role { get; set; }
}
