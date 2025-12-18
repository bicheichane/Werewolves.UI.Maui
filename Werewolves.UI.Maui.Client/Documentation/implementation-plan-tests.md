# Implementation Plan: QA/Tests Agent Tasks

## Context
This plan covers test infrastructure and test case implementation for the Agentic QA Strategy. The full architectural context is in `implementation-plan.md`.

---

## 1. Test Project Setup

### 1.1 Project Structure

**Location**: `Werewolves.UI.Maui.Tests/`

```
Werewolves.UI.Maui.Tests/
├── Werewolves.UI.Maui.Tests.csproj
├── Mocks/
│   ├── MockGameSession.cs
│   ├── MockGameService.cs
│   ├── MockPlayer.cs
│   └── MockPlayerState.cs
├── TestInstructions/
│   ├── TestAssignRolesInstruction.cs
│   ├── TestConfirmationInstruction.cs
│   ├── TestSelectPlayersInstruction.cs
│   └── TestSelectOptionsInstruction.cs
├── StateFixtures/
│   ├── LobbyStates/
│   │   ├── empty-lobby.json
│   │   ├── valid-5-players.json
│   │   └── invalid-duplicate-names.json
│   ├── GameplayStates/
│   │   ├── night-start.json
│   │   ├── day-voting.json
│   │   └── game-end-village-win.json
│   └── README.md
├── Infrastructure/
│   ├── TestStateDto.cs
│   ├── TestBridgeService.cs
│   └── TestAppBuilder.cs
└── Tests/
    ├── Rendering/
    │   ├── LobbyRenderingTests.cs
    │   ├── DashboardRenderingTests.cs
    │   └── InstructionRenderingTests.cs
    └── Interactions/
        ├── LobbyInteractionTests.cs
        └── DashboardInteractionTests.cs
```

### 1.2 Test Project Dependencies

**File**: `Werewolves.UI.Maui.Tests.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <!-- Test Framework -->
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageReference Include="FluentAssertions" Version="6.*" />
    
    <!-- Blazor Testing -->
    <PackageReference Include="bunit" Version="1.*" />
    <PackageReference Include="Moq" Version="4.*" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Werewolves.UI.Maui.Client\Werewolves.UI.Maui.Client.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!-- Include JSON fixtures as embedded resources -->
    <EmbeddedResource Include="StateFixtures\**\*.json" />
  </ItemGroup>
</Project>
```

---

## 2. State Fixtures

### 2.1 Fixture Design Principles

State fixtures are JSON files that define the **Minimum Viable State (MVS)** for each test scenario. They must:
1. Be valid according to `TestStateDto` schema
2. Contain only the minimum state needed for the test
3. Be documented with comments (in a sibling `.md` file if needed)

### 2.2 Core Fixture Examples

**File**: `StateFixtures/LobbyStates/valid-5-players.json`
```json
{
  "gameId": "00000000-0000-0000-0000-000000000001",
  "turnNumber": 0,
  "phase": "Setup",
  "players": [
    { "id": "10000000-0000-0000-0000-000000000001", "name": "Alice", "isAlive": true },
    { "id": "10000000-0000-0000-0000-000000000002", "name": "Bob", "isAlive": true },
    { "id": "10000000-0000-0000-0000-000000000003", "name": "Charlie", "isAlive": true },
    { "id": "10000000-0000-0000-0000-000000000004", "name": "Diana", "isAlive": true },
    { "id": "10000000-0000-0000-0000-000000000005", "name": "Eve", "isAlive": true }
  ],
  "instructionType": null,
  "instructionData": null
}
```

**File**: `StateFixtures/GameplayStates/night-start.json`
```json
{
  "gameId": "00000000-0000-0000-0000-000000000001",
  "turnNumber": 1,
  "phase": "Night",
  "players": [
    { "id": "10000000-0000-0000-0000-000000000001", "name": "Alice", "isAlive": true, "role": "Werewolf" },
    { "id": "10000000-0000-0000-0000-000000000002", "name": "Bob", "isAlive": true, "role": "Seer" },
    { "id": "10000000-0000-0000-0000-000000000003", "name": "Charlie", "isAlive": true, "role": "Villager" },
    { "id": "10000000-0000-0000-0000-000000000004", "name": "Diana", "isAlive": true, "role": "Villager" },
    { "id": "10000000-0000-0000-0000-000000000005", "name": "Eve", "isAlive": true, "role": "Villager" }
  ],
  "instructionType": "ConfirmationInstruction",
  "instructionData": {
    "publicAnnouncement": "The village falls asleep. Night begins.",
    "privateInstruction": "Call for werewolves to wake."
  }
}
```

**File**: `StateFixtures/GameplayStates/game-end-village-win.json`
```json
{
  "gameId": "00000000-0000-0000-0000-000000000001",
  "turnNumber": 5,
  "phase": "EndGame",
  "players": [
    { "id": "10000000-0000-0000-0000-000000000001", "name": "Alice", "isAlive": false, "role": "Werewolf" },
    { "id": "10000000-0000-0000-0000-000000000002", "name": "Bob", "isAlive": true, "role": "Seer" },
    { "id": "10000000-0000-0000-0000-000000000003", "name": "Charlie", "isAlive": true, "role": "Villager" }
  ],
  "instructionType": "FinishedGameConfirmationInstruction",
  "instructionData": {
    "publicAnnouncement": "The Village has won! All werewolves have been eliminated.",
    "winningTeam": "Village"
  }
}
```

### 2.3 Fixture Loader Utility

**File**: `Infrastructure/FixtureLoader.cs`

```csharp
using System.Reflection;
using System.Text.Json;

namespace Werewolves.UI.Maui.Tests.Infrastructure;

public static class FixtureLoader
{
    public static TestStateDto Load(string relativePath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"Werewolves.UI.Maui.Tests.StateFixtures.{relativePath.Replace('/', '.')}";
        
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Fixture not found: {relativePath}");
        
        return JsonSerializer.Deserialize<TestStateDto>(stream, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException($"Failed to deserialize fixture: {relativePath}");
    }
    
    public static string LoadRaw(string relativePath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"Werewolves.UI.Maui.Tests.StateFixtures.{relativePath.Replace('/', '.')}";
        
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Fixture not found: {relativePath}");
        using var reader = new StreamReader(stream);
        
        return reader.ReadToEnd();
    }
}
```

---

## 3. Test Instruction Subclasses

Since `ModeratorInstruction` subclasses have `internal` constructors, we create test-friendly subclasses.

### 3.1 Analysis Required

**Task**: Analyze each instruction type in `Werewolves.Core.StateModels/Models/Instructions/`:
- Identify constructor parameters
- Identify settable properties
- Determine subclassing strategy

### 3.2 Example Implementation

**File**: `TestInstructions/TestConfirmationInstruction.cs`

```csharp
using Werewolves.StateModels.Models.Instructions;

namespace Werewolves.UI.Maui.Tests.TestInstructions;

/// <summary>
/// Test-friendly subclass that exposes construction capabilities
/// </summary>
public class TestConfirmationInstruction : ConfirmationInstruction
{
    public TestConfirmationInstruction(
        Guid gameGuid,
        string? publicAnnouncement = null,
        string? privateInstruction = null)
        : base(gameGuid) // Calls internal base constructor
    {
        // Use reflection or init-only setters if available
        SetPublicAnnouncement(publicAnnouncement);
        SetPrivateInstruction(privateInstruction);
    }
    
    private void SetPublicAnnouncement(string? value)
    {
        // Strategy depends on base class structure
        // Option 1: If property has protected setter
        // Option 2: Use reflection
        var prop = typeof(ModeratorInstruction).GetProperty("PublicAnnouncement");
        prop?.SetValue(this, value);
    }
    
    private void SetPrivateInstruction(string? value)
    {
        var prop = typeof(ModeratorInstruction).GetProperty("PrivateInstruction");
        prop?.SetValue(this, value);
    }
}
```

### 3.3 Instruction Factory

**File**: `TestInstructions/InstructionFactory.cs`

```csharp
using System.Text.Json;
using Werewolves.StateModels.Models;

namespace Werewolves.UI.Maui.Tests.TestInstructions;

public static class InstructionFactory
{
    public static ModeratorInstruction? Create(string? type, Dictionary<string, object>? data, Guid gameId)
    {
        if (type == null) return null;
        
        return type switch
        {
            "ConfirmationInstruction" => CreateConfirmation(data, gameId),
            "AssignRolesInstruction" => CreateAssignRoles(data, gameId),
            "SelectPlayersInstruction" => CreateSelectPlayers(data, gameId),
            "SelectOptionsInstruction" => CreateSelectOptions(data, gameId),
            _ => throw new NotSupportedException($"Unknown instruction type: {type}")
        };
    }
    
    private static TestConfirmationInstruction CreateConfirmation(Dictionary<string, object>? data, Guid gameId)
    {
        return new TestConfirmationInstruction(
            gameId,
            data?.GetValueOrDefault("publicAnnouncement")?.ToString(),
            data?.GetValueOrDefault("privateInstruction")?.ToString()
        );
    }
    
    // Additional factory methods for other instruction types...
}
```

---

## 4. Test Categories

### 4.1 Rendering Tests (Unit Tests with bUnit)

These tests verify that components render correctly given specific state.

**File**: `Tests/Rendering/InstructionRenderingTests.cs`

```csharp
using Bunit;
using FluentAssertions;
using Werewolves.UI.Maui.Client.Components.Game;
using Werewolves.UI.Maui.Tests.Mocks;

namespace Werewolves.UI.Maui.Tests.Tests.Rendering;

public class InstructionRenderingTests : TestContext
{
    [Fact]
    public void ConfirmationView_RendersPublicAnnouncement()
    {
        // Arrange
        var mockService = new MockGameService();
        Services.AddSingleton<IGameService>(mockService);
        Services.AddSingleton<GameClientManager>();
        
        var instruction = new TestConfirmationInstruction(
            Guid.NewGuid(),
            publicAnnouncement: "The village awakens!"
        );
        
        // Act
        var cut = RenderComponent<InstructionRenderer>(parameters => 
            parameters.Add(p => p.Instruction, instruction));
        
        // Assert
        cut.Find(".mud-alert-info").TextContent.Should().Contain("The village awakens!");
    }
    
    [Fact]
    public void InstructionRenderer_ShowsWaitingMessage_WhenNoInstruction()
    {
        // Arrange
        var mockGameClient = new MockGameClientManager();
        mockGameClient.CurrentInstruction = null;
        Services.AddSingleton(mockGameClient);
        
        // Act
        var cut = RenderComponent<InstructionRenderer>();
        
        // Assert
        cut.Find(".mud-alert-info").TextContent.Should().Contain("Waiting for instruction");
    }
}
```

### 4.2 Integration Tests (QA Agent Workflow)

These tests simulate what the QA Agent does via Chrome DevTools.

**File**: `Tests/Integration/StateInjectionTests.cs`

```csharp
using FluentAssertions;
using Werewolves.UI.Maui.Tests.Infrastructure;

namespace Werewolves.UI.Maui.Tests.Tests.Integration;

public class StateInjectionTests
{
    [Fact]
    public void TestStateDto_DeserializesCorrectly()
    {
        // Arrange
        var json = FixtureLoader.LoadRaw("LobbyStates/valid-5-players.json");
        
        // Act
        var state = System.Text.Json.JsonSerializer.Deserialize<TestStateDto>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        // Assert
        state.Should().NotBeNull();
        state!.Players.Should().HaveCount(5);
        state.Players[0].Name.Should().Be("Alice");
    }
    
    [Fact]
    public void MockGameSession_CanBeCreatedFromDto()
    {
        // Arrange
        var state = FixtureLoader.Load("GameplayStates/night-start.json");
        
        // Act
        var session = state.ToMockSession();
        
        // Assert
        session.TurnNumber.Should().Be(1);
        session.GetPlayers().Should().HaveCount(5);
        session.GetCurrentPhase().Should().Be(GamePhase.Night);
    }
}
```

### 4.3 User Story Verification Matrix

Map test files to User Stories:

| User Story | Test File | Test Methods |
|------------|-----------|--------------|
| US-101 | `LobbyRenderingTests.cs` | `AddPlayer_AppearsInList`, `PlayerInput_AcceptsName` |
| US-102 | `LobbyRenderingTests.cs` | `DeleteButton_RemovesPlayer` |
| US-103 | `LobbyRenderingTests.cs` | `MoveUp_ReordersPlayers`, `MoveDown_ReordersPlayers` |
| US-104 | `LobbyRenderingTests.cs` | `RoleList_DisplaysAvailableRoles` |
| US-105 | `LobbyRenderingTests.cs` | `StartButton_EnabledWhenValid` |
| US-106 | `LobbyRenderingTests.cs` | `ValidationError_DisplaysMessage` |
| US-201 | `DashboardRenderingTests.cs` | `Tabs_SwitchPanels` |
| US-202 | `DashboardRenderingTests.cs` | `PhaseDisplay_ShowsCurrentPhase` |

---

## 5. Test Helpers

### 5.1 MockGameClientManager

**File**: `Mocks/MockGameClientManager.cs`

A test double that allows direct property manipulation:

```csharp
namespace Werewolves.UI.Maui.Tests.Mocks;

public class MockGameClientManager
{
    public event Action? StateChanged;
    public ModeratorInstruction? CurrentInstruction { get; set; }
    public IGameSession? ActiveSession { get; set; }
    public bool IsMuted { get; set; }
    
    public void TriggerStateChanged() => StateChanged?.Invoke();
}
```

### 5.2 TestContext Extensions

**File**: `Infrastructure/TestContextExtensions.cs`

```csharp
using Bunit;
using Microsoft.Extensions.DependencyInjection;

namespace Werewolves.UI.Maui.Tests.Infrastructure;

public static class TestContextExtensions
{
    public static void ConfigureTestServices(this TestContext ctx, Action<IServiceCollection>? configure = null)
    {
        // Register all mock services
        ctx.Services.AddSingleton<MockGameService>();
        ctx.Services.AddSingleton<IGameService>(sp => sp.GetRequiredService<MockGameService>());
        ctx.Services.AddSingleton<MockGameClientManager>();
        // MudBlazor services
        ctx.Services.AddMudServices();
        
        configure?.Invoke(ctx.Services);
    }
}
```

---

## 6. CI/CD Considerations

### 6.1 Test Execution

Tests can run in two modes:

1. **Unit Tests (CI)**: bUnit tests run without a real browser
2. **Integration Tests (Agent)**: QA Agent runs against live app

### 6.2 GitHub Actions (Optional)

If CI is desired:

```yaml
# .github/workflows/test.yml
name: UI Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet test Werewolves.UI.Maui.Tests/
```

---

## 7. Implementation Order

1. Create test project structure and csproj
2. Implement mock classes (MockPlayer, MockPlayerState, MockGameSession)
3. Implement MockGameService
4. Implement TestStateDto and FixtureLoader
5. Create initial state fixtures (Lobby scenarios)
6. Analyze instruction classes for subclassing
7. Implement test instruction subclasses
8. Write first rendering tests (InstructionRenderer)
9. Write Lobby rendering tests
10. Write Dashboard rendering tests
11. Document User Story → Test mapping

---

## 8. Test Coverage Goals

### Phase 1: Core Infrastructure
- [ ] All mock classes compile and pass basic tests
- [ ] State fixtures load correctly
- [ ] InstructionRenderer tests pass

### Phase 2: Lobby Tests
- [ ] US-101 through US-106 have passing tests
- [ ] MVS defined for each story

### Phase 3: Dashboard Tests
- [ ] US-201, US-202 have passing tests
- [ ] Phase display tests pass
- [ ] Tab navigation tests pass
