# Implementation Plan: Agentic QA Strategy Infrastructure

## 1. Abstract

This plan defines the infrastructure required to implement an AI-driven Quality Assurance strategy for the Werewolves MAUI Blazor Hybrid application. The approach uses a **"Puppeteer Mock" architecture** where:

1. Mock implementations replace Core dependencies at runtime
2. A JavaScript bridge enables runtime state injection via Chrome DevTools
3. The QA Agent can "teleport" to any User Story scenario instantly by injecting state
4. Automated tests cover snapshot rendering, click handlers, and input validation
5. Manual tests cover timer behavior, audio, and complex interaction patterns

This infrastructure enables efficient, cost-effective UI testing without requiring full game simulations.

---

## 2. Motivation

### 2.1 The Hybrid Complexity Challenge
MAUI Blazor Hybrid applications wrap a WebView in a native shell. Standard web automation tools struggle with native app lifecycle management.

### 2.2 The "Thin Client" Opportunity  
The UI follows a strict MVA architecture where `f(State) = View`. Since Core logic is covered by integration tests, UI tests should verify *presentation* (did the screen update correctly?) rather than *logic* (did the vote count correctly?).

### 2.3 Agentic Efficiency
AI Agents struggle with long sequential tasks. By treating the UI as a pure function of state, we allow the Agent to teleport to any scenario instantly.

### 2.4 User Request
User requested a formal QA strategy and definition of the `qa-agent.md` sub-agent for the orchestration workflow.

---

## 3. Proposed Changes

### 3.1 Solution & Project Structure Changes

#### 3.1.1 Create Solution File
**File**: `Werewolves.UI.Maui.sln`

Create a new solution file that replaces `Werewolves.Client.slnx`:
- Include `Werewolves.UI.Maui.Client` (renamed from `Werewolves.Client`)
- Include `Werewolves.UI.Maui.Tests` (new test project)

#### 3.1.2 Rename Client Project
**Current**: `Werewolves.Client/` → **New**: `Werewolves.UI.Maui.Client/`

Update:
- Folder name
- `.csproj` file name
- Namespace: `Werewolves.Client` → `Werewolves.UI.Maui.Client`
- All `using` statements referencing the old namespace

#### 3.1.3 Create Test Project
**New Project**: `Werewolves.UI.Maui.Tests/`

```
Werewolves.UI.Maui.Tests/
├── Werewolves.UI.Maui.Tests.csproj
├── Mocks/
│   ├── MockGameSession.cs
│   ├── MockGameService.cs
│   ├── MockPlayer.cs
│   └── MockPlayerState.cs
├── TestInstructions/
│   └── (Subclassed ModeratorInstruction types)
├── StateFixtures/
│   └── (JSON state fixture files per User Story)
├── Infrastructure/
│   ├── TestBridgeService.cs
│   └── TestAppBuilder.cs
└── Tests/
    ├── LobbyTests.cs
    ├── DashboardTests.cs
    └── InstructionRenderingTests.cs
```

---

### 3.2 Architectural Changes

#### 3.2.1 Extract IGameService Interface

**Location**: `Werewolves.Core.GameLogic` (Core repository - requires Core change)

Extract interface from `GameService`:
```csharp
public interface IGameService
{
    ModeratorInstruction StartNewGame(GameSessionConfig config);
    ProcessInstructionResult ProcessInstruction(Guid gameId, ModeratorResponse response);
    IGameSession? GetGameStateView(Guid gameId);
}
```

**Impact**: This is a **Core dependency**. The UI project cannot fully implement this strategy until Core exposes `IGameService`.

**Workaround**: UI can define a local `IGameService` interface matching the current `GameService` API. When Core updates, the local interface can be removed.

#### 3.2.2 Update GameClientManager for DI

**File**: `Services/GameClientManager.cs`

Update constructor to accept `IGameService` via DI:
```csharp
public class GameClientManager
{
    private readonly IGameService _gameService;
    
    public GameClientManager(IAudioManager audioManager, AudioMap audioMap, IGameService gameService)
    {
        _audioManager = audioManager;
        _audioMap = audioMap;
        _gameService = gameService;
    }
}
```

#### 3.2.3 JavaScript Bridge for State Injection

**File**: `wwwroot/index.html` (add script block)

```html
<script>
    window.TestBridge = {
        _dotNetRef: null,
        register: (dotNetRef) => { 
            window.TestBridge._dotNetRef = dotNetRef;
            console.log('[TestBridge] Registered .NET reference');
        },
        setState: (jsonString) => {
            if (!window.TestBridge._dotNetRef) {
                console.error('[TestBridge] Not registered. Call register() first.');
                return Promise.reject('TestBridge not registered');
            }
            return window.TestBridge._dotNetRef.invokeMethodAsync('InjectState', jsonString);
        },
        isReady: () => !!window.TestBridge._dotNetRef
    };
</script>
```

#### 3.2.4 TestBridgeService (Test Project)

**File**: `Werewolves.UI.Maui.Tests/Infrastructure/TestBridgeService.cs`

```csharp
public class TestBridgeService : IDisposable
{
    private readonly MockGameService _mockGameService;
    private DotNetObjectReference<TestBridgeService>? _dotNetRef;
    
    [JSInvokable]
    public void InjectState(string jsonState)
    {
        var stateDto = JsonSerializer.Deserialize<TestStateDto>(jsonState);
        _mockGameService.ApplyTestState(stateDto);
    }
    
    public async Task RegisterWithJavaScript(IJSRuntime jsRuntime)
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        await jsRuntime.InvokeVoidAsync("TestBridge.register", _dotNetRef);
    }
}
```

---

### 3.3 Mock Implementations

#### 3.3.1 MockGameSession

**File**: `Werewolves.UI.Maui.Tests/Mocks/MockGameSession.cs`

Simple implementation of `IGameSession` with public setters:
- All properties gettable/settable
- `GetPlayers()` returns configurable `MockPlayer` list
- `GameHistoryLog` returns configurable log entries
- No game logic—pure state container

#### 3.3.2 MockPlayer / MockPlayerState

**Files**: `Werewolves.UI.Maui.Tests/Mocks/MockPlayer.cs`, `MockPlayerState.cs`

Simple implementations of `IPlayer` and `IPlayerState`:
- All properties have public get/set
- Constructors accept initial values
- No computed properties or state machine logic

#### 3.3.3 MockGameService

**File**: `Werewolves.UI.Maui.Tests/Mocks/MockGameService.cs`

Implements `IGameService`:
- Holds a `MockGameSession` instance
- `StartNewGame()` returns configurable instruction
- `ProcessInstruction()` returns configurable result
- `ApplyTestState(TestStateDto dto)` method for bridge injection
- Fires state change events

#### 3.3.4 Test Instruction Subclasses

**Location**: `Werewolves.UI.Maui.Tests/TestInstructions/`

Create subclasses for each `ModeratorInstruction` type to expose constructors:
```csharp
public class TestAssignRolesInstruction : AssignRolesInstruction
{
    public TestAssignRolesInstruction(Guid gameGuid, /* params */) 
        : base(gameGuid) // calls internal base constructor
    {
        // Set properties via reflection or exposed setters
    }
}
```

**Note**: This requires analyzing each instruction's internal structure.

---

### 3.4 DI Configuration

#### 3.4.1 Production Configuration (MauiProgram.cs)

```csharp
// Production DI
builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<GameClientManager>();
```

#### 3.4.2 Test Configuration (TestAppBuilder.cs)

```csharp
// Test DI - inject mocks
builder.Services.AddSingleton<MockGameService>();
builder.Services.AddSingleton<IGameService>(sp => sp.GetRequiredService<MockGameService>());
builder.Services.AddSingleton<TestBridgeService>();
builder.Services.AddSingleton<GameClientManager>();
```

---

### 3.5 Documentation Changes

#### 3.5.1 Update `user-stories.md`

Add **Minimum Viable State (MVS)** section to each User Story:

```markdown
**MVS (Minimum Viable State):**
```json
{
  "phase": "Lobby",
  "players": ["Alice", "Bob", "Charlie"],
  "selectedRoles": []
}
```
```

#### 3.5.2 Create `manual-user-stories.md`

**File**: `Documentation/manual-user-stories.md`

Move/copy User Stories that require manual testing:
- Timer behavior tests
- Audio playback tests
- Platform-specific visual tests (Safe Area, notch handling)
- Complex gesture interactions

#### 3.5.3 Update `qa-agent.md`

**File**: `.github/agents/qa-agent.md`

Full agent definition including:
- Role & Responsibilities
- Workspace Context
- Chrome DevTools MCP usage instructions
- Test Workflow (connect → inject state → verify DOM → report)
- User Story verification process
- MVS requirements
- Scope limitations (Windows only, no visual regression)

#### 3.5.4 Update `planner-agent.md`

**File**: `.github/agents/planner-agent.md`

Add instruction to produce `manual-user-stories.md` when planning features with non-automatable acceptance criteria.

#### 3.5.5 Update `architecture.md`

**File**: `Documentation/architecture.md`

Add section documenting:
- Test infrastructure architecture
- Mock implementation patterns
- JS Bridge mechanism
- DI configuration for testing

---

### 3.6 Agent File Updates

#### 3.6.1 `qa-agent.md` - Full Definition

Replace placeholder with complete agent specification:

```markdown
# QA Agent

## Role & Responsibility
You are the **Quality Assurance Agent** for the Werewolves.UI.Maui project.
Your role is to verify that UI components render correctly and respond appropriately to state changes.

## Scope
- **In Scope**: Snapshot rendering, click handlers, input validation, DOM verification
- **Out of Scope**: Timer behavior, audio playback, visual regression, Android/iOS testing

## Tools
- Chrome DevTools MCP for DOM inspection and JS execution
- `window.TestBridge.setState(json)` for state injection

## Workflow
1. Connect to running app via `localhost:9222`
2. Read User Story and its MVS (Minimum Viable State)
3. Inject MVS via `window.TestBridge.setState()`
4. Verify DOM matches Acceptance Criteria
5. Report pass/fail with evidence

## MVS Requirement
Each User Story must have an MVS JSON definition. If missing, report to orchestrator.
```

---

## 4. Impact Analysis

### 4.1 Benefits

| Benefit | Description |
|---------|-------------|
| **Test Isolation** | UI tests never fail due to Core logic bugs |
| **Speed** | State injection is instant vs. 20+ minute game simulations |
| **Cost Efficiency** | Reduced token usage for AI Agent (no complex reasoning) |
| **Stability** | No async race conditions from timers or network |
| **Maintainability** | Clear separation between what's tested where |

### 4.2 Considerations & Mitigations

| Consideration | Risk | Mitigation |
|---------------|------|------------|
| **Mock Drift** | Mocks may diverge from real implementation | Use shared DTOs; compiler errors when interface changes |
| **Core Dependency** | `IGameService` extraction requires Core change | Define local interface as workaround; remove when Core updates |
| **Platform Specifics** | Windows testing misses Android/iOS bugs | Document scope; manual testing for platform-specific issues |
| **Instruction Internals** | Subclassing instructions may break on Core updates | Version-lock dependencies; test subclasses in CI |
| **Namespace Rename** | Renaming `Werewolves.Client` is invasive | Perform as atomic operation; update all references |

### 4.3 Core Dependencies

| Dependency | Status | Workaround |
|------------|--------|------------|
| `IGameService` interface | **Not yet extracted** | Define local interface in UI project |
| `GameSession.Serialize()` | **Implemented** | No workaround needed |
| `ModeratorInstruction` constructors | **Internal** | Subclass in test project |

---

## 5. Implementation Phases

### Phase 1: Infrastructure Setup
1. Create `Werewolves.UI.Maui.sln`
2. Rename `Werewolves.Client` → `Werewolves.UI.Maui.Client`
3. Create `Werewolves.UI.Maui.Tests` project
4. Define local `IGameService` interface
5. Update `GameClientManager` for DI

### Phase 2: Mock Implementation
1. Create `MockGameSession`, `MockPlayer`, `MockPlayerState`
2. Create `MockGameService`
3. Create test instruction subclasses
4. Add JS bridge to `index.html`
5. Create `TestBridgeService`

### Phase 3: Documentation & Agent Definition
1. Update `user-stories.md` with MVS sections
2. Create `manual-user-stories.md`
3. Write full `qa-agent.md` specification
4. Update `planner-agent.md`
5. Update `architecture.md`

### Phase 4: First Test Suite
1. Implement Lobby tests (US-101 through US-106)
2. Validate end-to-end agent workflow
3. Iterate on state fixture design

---

## 6. Resolved Decisions

| Decision | Resolution |
|----------|------------|
| **Namespace** | Confirmed: `Werewolves.UI.Maui.Client` |
| **Core PR** | Yes - document `IGameService` extraction as blocking issue for Core team |
| **CI Integration** | Agent-only testing (no GitHub Actions) |
| **Instruction Subclassing** | Will be analyzed during implementation |

---

## 7. Files to Create/Modify Summary

| Action | Path |
|--------|------|
| **Create** | `Werewolves.UI.Maui.sln` |
| **Rename** | `Werewolves.Client/` → `Werewolves.UI.Maui.Client/` |
| **Create** | `Werewolves.UI.Maui.Tests/` (project + structure) |
| **Modify** | `MauiProgram.cs` (DI changes) |
| **Modify** | `GameClientManager.cs` (IGameService injection) |
| **Modify** | `wwwroot/index.html` (JS bridge) |
| **Modify** | `Documentation/user-stories.md` (MVS sections) |
| **Create** | `Documentation/manual-user-stories.md` |
| **Modify** | `Documentation/architecture.md` (testing section) |
| **Modify** | `.github/agents/qa-agent.md` (full spec) |
| **Modify** | `.github/agents/planner-agent.md` (manual stories output) |
