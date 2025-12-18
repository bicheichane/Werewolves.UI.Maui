using Werewolves.Core.StateModels.Core;

namespace Werewolves.UI.Maui.Tests.Mocks;

/// <summary>
/// Mock implementation of IPlayer for testing.
/// All properties have public get/set for easy test configuration.
/// </summary>
public class MockPlayer : IPlayer, IEquatable<IPlayer>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "TestPlayer";
    public IPlayerState State { get; set; } = new MockPlayerState();

    public bool Equals(IPlayer? other) => other?.Id == Id;
    public override bool Equals(object? obj) => obj is IPlayer player && Equals(player);
    public override int GetHashCode() => Id.GetHashCode();
}
