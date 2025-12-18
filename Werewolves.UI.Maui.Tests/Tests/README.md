# Tests

This folder contains the xUnit test classes.

## Organization

- `LobbyTests.cs` - Tests for the Lobby page (US-101 through US-106)
- `DashboardTests.cs` - Tests for the Dashboard page
- `InstructionRenderingTests.cs` - Tests for instruction view components

## Test Pattern

```csharp
public class LobbyTests
{
    private readonly MockGameService _mockService;
    private readonly GameClientManager _gameClient;
    
    public LobbyTests()
    {
        _mockService = new MockGameService();
        _gameClient = new GameClientManager(
            audioManager: null!, // Mock in actual tests
            audioMap: new AudioMap(),
            gameService: _mockService
        );
    }
    
    [Fact]
    public void Lobby_Should_Display_Empty_State()
    {
        // Arrange - inject state
        // Act - render component
        // Assert - verify DOM
    }
}
```
