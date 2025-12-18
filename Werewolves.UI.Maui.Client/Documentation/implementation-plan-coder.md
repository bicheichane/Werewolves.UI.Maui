# Implementation Plan: Coder Agent Tasks

## Context
This plan covers the code implementation required for the Agentic QA Strategy infrastructure. The full architectural context is in `implementation-plan.md`.

---

## 1. Solution & Project Structure

### 1.1 Create Solution File
**File**: `/Werewolves.UI.Maui.sln`

Create a standard .NET solution file containing:
- `Werewolves.UI.Maui.Client/Werewolves.UI.Maui.Client.csproj`
- `Werewolves.UI.Maui.Tests/Werewolves.UI.Maui.Tests.csproj`

### 1.2 Rename Client Project

**Folder**: `Werewolves.Client/` → `Werewolves.UI.Maui.Client/`

**Files to rename**:
- `Werewolves.Client.csproj` → `Werewolves.UI.Maui.Client.csproj`
- Update `<RootNamespace>` in csproj to `Werewolves.UI.Maui.Client`
- Update `<AssemblyName>` in csproj

**Namespace changes** (all `.cs` and `.razor` files):
- `namespace Werewolves.Client` → `namespace Werewolves.UI.Maui.Client`
- `using Werewolves.Client` → `using Werewolves.UI.Maui.Client`

**Files affected**:
- `App.xaml.cs`
- `MainPage.xaml.cs`
- `MauiProgram.cs`
- `Services/*.cs`
- `Components/**/*.razor`
- `Components/_Imports.razor`
- `Platforms/**/*.cs`

### 1.3 Create Test Project

**New folder**: `Werewolves.UI.Maui.Tests/`

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
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Werewolves.UI.Maui.Client\Werewolves.UI.Maui.Client.csproj" />
  </ItemGroup>
</Project>
```

**Directory structure**:
```
Werewolves.UI.Maui.Tests/
├── Mocks/
├── TestInstructions/
├── StateFixtures/
├── Infrastructure/
└── Tests/
```

---

## 2. Interface Definitions

### 2.1 IGameService Interface (Local)

**File**: `Werewolves.UI.Maui.Client/Services/IGameService.cs`

```csharp
using Werewolves.StateModels.Core;
using Werewolves.StateModels.Models;

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
```

### 2.2 GameService Wrapper

**File**: `Werewolves.UI.Maui.Client/Services/GameServiceWrapper.cs`

```csharp
using Werewolves.GameLogic.Services;
using Werewolves.StateModels.Core;
using Werewolves.StateModels.Models;

namespace Werewolves.UI.Maui.Client.Services;

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
```

---

## 3. GameClientManager Updates

**File**: `Werewolves.UI.Maui.Client/Services/GameClientManager.cs`

### 3.1 Update Constructor

**Change**:
```csharp
// Before
private readonly GameService _gameService = new();

// After
private readonly IGameService _gameService;

public GameClientManager(IAudioManager audioManager, AudioMap audioMap, IGameService gameService)
{
    _audioManager = audioManager;
    _audioMap = audioMap;
    _gameService = gameService;
}
```

### 3.2 Add StateChanged Invocation Method (for testing)

```csharp
/// <summary>
/// Forces a StateChanged event. Used by test infrastructure for state injection.
/// </summary>
internal void NotifyStateChanged()
{
    StateChanged?.Invoke();
}

/// <summary>
/// Allows test infrastructure to set the current instruction directly.
/// </summary>
internal void SetCurrentInstruction(ModeratorInstruction instruction)
{
    CurrentInstruction = instruction;
    NotifyStateChanged();
}

/// <summary>
/// Allows test infrastructure to set the active session directly.
/// </summary>
internal void SetActiveSession(IGameSession session)
{
    ActiveSession = session;
}
```

---

## 4. DI Configuration Updates

**File**: `Werewolves.UI.Maui.Client/MauiProgram.cs`

### 4.1 Update Service Registration

```csharp
// Game Services (updated for DI)
builder.Services.AddSingleton<AudioMap>();
builder.Services.AddSingleton<IconMap>();
builder.Services.AddSingleton<IGameService, GameServiceWrapper>();
builder.Services.AddSingleton<GameClientManager>();
```

---

## 5. JavaScript Bridge

**File**: `Werewolves.UI.Maui.Client/wwwroot/index.html`

Add before closing `</body>` tag:

```html
<!-- Test Bridge for QA Agent state injection -->
<script>
    window.TestBridge = {
        _dotNetRef: null,
        register: function(dotNetRef) { 
            window.TestBridge._dotNetRef = dotNetRef;
            console.log('[TestBridge] Registered .NET reference');
        },
        setState: function(jsonString) {
            if (!window.TestBridge._dotNetRef) {
                console.error('[TestBridge] Not registered. Call register() first.');
                return Promise.reject('TestBridge not registered');
            }
            return window.TestBridge._dotNetRef.invokeMethodAsync('InjectState', jsonString);
        },
        isReady: function() { 
            return !!window.TestBridge._dotNetRef; 
        }
    };
</script>
```

---

## 6. Mock Implementations (Test Project)

### 6.1 MockPlayerState

**File**: `Werewolves.UI.Maui.Tests/Mocks/MockPlayerState.cs`

```csharp
using Werewolves.StateModels.Core;
using Werewolves.StateModels.Enums;

namespace Werewolves.UI.Maui.Tests.Mocks;

public class MockPlayerState : IPlayerState
{
    public bool IsAlive { get; set; } = true;
    public bool IsSheriff { get; set; }
    public bool IsImmuneToLynching { get; set; }
    public bool IsInfected { get; set; }
    public MainRoleType? Role { get; set; }
    // Add other IPlayerState properties as needed
}
```

### 6.2 MockPlayer

**File**: `Werewolves.UI.Maui.Tests/Mocks/MockPlayer.cs`

```csharp
using Werewolves.StateModels.Core;

namespace Werewolves.UI.Maui.Tests.Mocks;

public class MockPlayer : IPlayer, IEquatable<IPlayer>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "TestPlayer";
    public IPlayerState State { get; set; } = new MockPlayerState();

    public bool Equals(IPlayer? other) => other?.Id == Id;
    public override bool Equals(object? obj) => obj is IPlayer player && Equals(player);
    public override int GetHashCode() => Id.GetHashCode();
}
```

### 6.3 MockGameSession

**File**: `Werewolves.UI.Maui.Tests/Mocks/MockGameSession.cs`

```csharp
using Werewolves.StateModels.Core;
using Werewolves.StateModels.Enums;
using Werewolves.StateModels.Log;

namespace Werewolves.UI.Maui.Tests.Mocks;

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
}
```

### 6.4 MockGameService

**File**: `Werewolves.UI.Maui.Tests/Mocks/MockGameService.cs`

```csharp
using Werewolves.StateModels.Core;
using Werewolves.StateModels.Models;
using Werewolves.UI.Maui.Client.Services;

namespace Werewolves.UI.Maui.Tests.Mocks;

public class MockGameService : IGameService
{
    public MockGameSession CurrentSession { get; private set; } = new();
    public ModeratorInstruction? LastInstruction { get; set; }
    public ProcessInstructionResult? NextResult { get; set; }

    public event Action? StateChanged;

    public ModeratorInstruction StartNewGame(GameSessionConfig config)
    {
        CurrentSession = new MockGameSession { Id = Guid.NewGuid() };
        // Return a mock instruction - will need TestStartGameConfirmation
        throw new NotImplementedException("Use ApplyTestState for test scenarios");
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
    /// Apply a test state from JSON (used by TestBridge)
    /// </summary>
    public void ApplyTestState(TestStateDto state)
    {
        CurrentSession = state.ToMockSession();
        LastInstruction = state.CurrentInstruction;
        StateChanged?.Invoke();
    }
}
```

### 6.5 TestStateDto

**File**: `Werewolves.UI.Maui.Tests/Infrastructure/TestStateDto.cs`

```csharp
using System.Text.Json.Serialization;
using Werewolves.StateModels.Enums;
using Werewolves.StateModels.Models;

namespace Werewolves.UI.Maui.Tests.Infrastructure;

/// <summary>
/// DTO for injecting test state via the TestBridge
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
    
    public string? InstructionType { get; set; }
    public Dictionary<string, object>? InstructionData { get; set; }

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

public class TestPlayerDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Player";
    public bool IsAlive { get; set; } = true;
    public bool IsSheriff { get; set; }
    public MainRoleType? Role { get; set; }

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
```

### 6.6 TestBridgeService

**File**: `Werewolves.UI.Maui.Tests/Infrastructure/TestBridgeService.cs`

```csharp
using Microsoft.JSInterop;
using System.Text.Json;

namespace Werewolves.UI.Maui.Tests.Infrastructure;

/// <summary>
/// Service that bridges JavaScript TestBridge calls to C# mock services
/// </summary>
public class TestBridgeService : IDisposable
{
    private readonly MockGameService _mockGameService;
    private readonly GameClientManager _gameClientManager;
    private DotNetObjectReference<TestBridgeService>? _dotNetRef;

    public TestBridgeService(MockGameService mockGameService, GameClientManager gameClientManager)
    {
        _mockGameService = mockGameService;
        _gameClientManager = gameClientManager;
    }

    [JSInvokable]
    public void InjectState(string jsonState)
    {
        var stateDto = JsonSerializer.Deserialize<TestStateDto>(jsonState, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        if (stateDto == null)
            throw new ArgumentException("Failed to deserialize test state", nameof(jsonState));

        _mockGameService.ApplyTestState(stateDto);
        _gameClientManager.SetActiveSession(_mockGameService.CurrentSession);
        
        if (stateDto.CurrentInstruction != null)
            _gameClientManager.SetCurrentInstruction(stateDto.CurrentInstruction);
        
        _gameClientManager.NotifyStateChanged();
    }

    public async Task RegisterWithJavaScript(IJSRuntime jsRuntime)
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        await jsRuntime.InvokeVoidAsync("TestBridge.register", _dotNetRef);
    }

    public void Dispose()
    {
        _dotNetRef?.Dispose();
    }
}
```

---

## 7. Architectural Considerations

### 7.1 Namespace Migration Strategy
Perform namespace rename atomically using IDE refactoring tools or a script. Verify all files compile after rename.

### 7.2 InternalsVisibleTo
Consider adding to `Werewolves.UI.Maui.Client.csproj`:
```xml
<ItemGroup>
  <AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleTo">
    <_Parameter1>Werewolves.UI.Maui.Tests</_Parameter1>
  </AssemblyAttribute>
</ItemGroup>
```

This allows test project to access `internal` members like `SetCurrentInstruction()`.

### 7.3 Interface Segregation
`IPlayerState` interface may have many properties. Only implement the ones actually read by UI components. Use `NotImplementedException` for unused properties during initial development, then implement as needed.

---

## 8. Implementation Order

1. Create `IGameService.cs` and `GameServiceWrapper.cs`
2. Update `GameClientManager.cs` constructor and add test methods
3. Update `MauiProgram.cs` DI registration
4. Add JS bridge to `index.html`
5. Create solution file `Werewolves.UI.Maui.sln`
6. Rename folder and project files
7. Update all namespaces (bulk find/replace)
8. Create test project and folder structure
9. Implement mock classes
10. Implement `TestBridgeService`
11. Verify solution builds and app runs
