# Werewolves UI Client: Architecture Document

## 1. Project Overview & Philosophy

### 1.1. Goal
Build a visual "Thin Client" for the Werewolves game engine. This client is a **moderator helper application** that guides a human moderator through the game flow.

### 1.2. Core Principles
*   **Render State, Never Compute It:** The UI renders state from `Werewolves.Core` and collects input. **No game logic is performed in the UI.**
*   **Single Source of Truth:** All game state lives in `IGameSession` from Core. The UI holds no duplicate game state.
*   **Convention Over Configuration:** Asset mappings (audio, icons) use naming conventions to eliminate maintenance overhead.
*   **Reactive by Default:** Components subscribe to state changes and re-render automatically.

### 1.3. Target Platforms
Mobile-First (Android/iOS) via **.NET MAUI Blazor Hybrid**, with Windows and macOS support.

---

## 2. Technology Stack

| Component | Technology | Notes |
|-----------|------------|-------|
| **Framework** | .NET 10 MAUI Blazor Hybrid | Cross-platform native shell with Blazor WebView |
| **UI Library** | MudBlazor | Material Design components |
| **Audio** | `Plugin.Maui.Audio` | Assets in `Resources/Raw/Audio/` |
| **Device Control** | `Microsoft.Maui.Devices.IDeviceDisplay` | Wake Lock management |
| **Core Dependency** | `Werewolves.Core.GameLogic` | Project reference to game engine |

### 2.1. MudBlazor Guidelines
*   **Constraint:** Minimal custom CSS.
*   **Rule:** Use MudBlazor utility classes (`d-flex`, `pa-4`, `gap-3`) for 95% of styling.
*   **Exception:** Custom CSS permitted strictly for layout shims (e.g., `touch-action`, Safe Area Insets) where utility classes fail.

---

## 3. Architecture Pattern: Model-View-Adapter (MVA)

```
┌─────────────────────────────────────────────────────────────────┐
│                           UI Layer                              │
│  ┌─────────────┐    ┌─────────────────────┐    ┌─────────────┐  │
│  │   Blazor    │◄───│  GameClientManager  │───►│   Core      │  │
│  │ Components  │    │     (Adapter)       │    │ GameService │  │
│  │   (View)    │    │                     │    │   (Model)   │  │
│  └─────────────┘    └─────────────────────┘    └─────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### 3.1. The Model (Core)
*   `IGameSession` from `Werewolves.Core.StateModels` – read-only from client perspective.
*   `ModeratorInstruction` – the current prompt for the moderator.
*   `ModeratorResponse` – user input submitted back to Core.

### 3.2. The View (Blazor Components)
*   **State Rule:** No duplication of game state. Components read from `GameClientManager`.
*   **Transient State:** UI-specific state (draft inputs, accordion open/close) lives in the component. This is ephemeral—lost on crash/restart.
*   **Navigation:** `MudTabs` with `KeepPanelsAlive="true"` to maintain timer state across tab switches.

### 3.3. The Adapter (GameClientManager)
*   **Singleton Service** – registered in DI container.
*   **Responsibilities:**
    *   Holds `IGameSession` reference
    *   Exposes `CurrentInstruction` and `StateChanged` event
    *   Manages audio playback (multiple concurrent sounds)
    *   Proxies input to `GameService.ProcessInstruction()`

---

## 4. IDisposable Pattern (Critical)

**Any component subscribing to `GameClientManager.StateChanged` MUST implement `IDisposable`.**

This is mandatory because `MudTabs` uses `KeepPanelsAlive="true"`, meaning components persist in memory across tab switches. Without proper disposal, event handlers accumulate, causing memory leaks and duplicate re-renders.

### 4.1. Required Implementation Pattern
```razor
@implements IDisposable
@inject GameClientManager GameClient

@code {
    protected override void OnInitialized()
    {
        GameClient.StateChanged += StateHasChanged;
    }

    public void Dispose()
    {
        GameClient.StateChanged -= StateHasChanged;
    }
}
```

### 4.2. Components Using This Pattern
| Component | Location |
|-----------|----------|
| `InstructionRenderer.razor` | `Components/Game/` |
| `PlayerList.razor` | `Components/Game/DashboardTabs/` |
| `GameOverview.razor` | `Components/Game/DashboardTabs/` |
| `Dashboard.razor` | `Components/Pages/` |

---

## 5. Audio System

### 5.1. Architecture
*   **AudioMap** (`Services/AudioMap.cs`) – Maps `SoundEffectsEnum` to filename using convention.
*   **GameClientManager** – Manages `Dictionary<SoundEffectsEnum, IAudioPlayer>` for concurrent sounds.
*   **Instruction-Driven:** Audio is controlled by `ModeratorInstruction.SoundEffects` property.

### 5.2. Convention-Based Naming
```
Audio files: Resources/Raw/Audio/{EnumType}_{EnumValue}.mp3
Example: SoundEffectsEnum_NightAmbience.mp3
```

### 5.3. Audio Reconciliation Flow
```csharp
private void ReconcileAudio()
{
    var desiredSounds = CurrentInstruction?.SoundEffects ?? new List<SoundEffectsEnum>();
    
    // Stop sounds not in desired list
    foreach (var sound in _activeSounds.Keys.Where(s => !desiredSounds.Contains(s)))
        StopAudio(sound);
    
    // Start new sounds
    foreach (var sound in desiredSounds.Where(s => !_activeSounds.ContainsKey(s)))
        StartAudioAsync(sound);
}
```

### 5.4. Audio API
| Method | Description |
|--------|-------------|
| `ReconcileAudio()` | Called after `ProcessInputAsync()` – syncs audio to instruction |
| `StopAudio(SoundEffectsEnum)` | Stops and disposes a specific sound |
| `StopAllAudio()` | Stops all active sounds |
| `ToggleMute()` | Toggles `IsMuted` property, pauses/resumes all sounds |
| `IsMuted` | Property indicating current mute state |

### 5.5. Dashboard Audio Controls
The Dashboard includes a mute/unmute toggle button:
```razor
<MudIconButton 
    Icon="@(GameClient.IsMuted ? Icons.Material.Filled.VolumeOff : Icons.Material.Filled.VolumeUp)" 
    OnClick="@(() => GameClient.ToggleMute())" 
    Color="Color.Primary" />
```

---

## 6. Icon System

### 6.1. Convention-Based Naming
**IconMap** (`Services/IconMap.cs`) replaces the old `ImageMap.cs` with a convention-based approach:

```csharp
public string GetIcon(Enum enumValue)
{
    return $"{enumValue.GetType().Name}_{enumValue}.png";
}
```

### 6.2. File Location & Naming
```
Icons: Resources/Images/{EnumType}_{EnumValue}.png
Examples:
  - MainRoleType_Werewolf.png
  - MainRoleType_Seer.png
  - StatusEffect_Protected.png
```

### 6.3. Validation
Both `AudioMap` and `IconMap` include `ValidateAsync()` methods that:
*   **DEBUG mode:** Throw exceptions for missing assets.
*   **RELEASE mode:** Log warnings and continue.

---

## 7. Wake Lock System

### 7.1. Purpose
Prevents the device screen from turning off during gameplay. Games can last 30+ minutes, so the moderator shouldn't have to repeatedly wake the device.

### 7.2. Implementation
```csharp
// Enable (in GameClientManager.StartGameAsync or Lobby.OnInitializedAsync)
DeviceDisplay.Current.KeepScreenOn = true;

// Disable (in GameClientManager.FinishGame)
DeviceDisplay.Current.KeepScreenOn = false;
```

### 7.3. Lifecycle
*   **Enabled:** When game starts (Lobby → Dashboard transition)
*   **Disabled:** When game finishes (`FinishedGameConfirmationInstruction`)
*   **Coverage:** Entire game session (Lobby through gameplay)

---

## 8. Timer System

### 8.1. Purpose
Displays elapsed time since the current instruction was shown. Helps moderators pace the game.

### 8.2. Implementation (Dashboard.razor)
```csharp
private System.Threading.Timer? _timer;
private DateTime _timerStart = DateTime.UtcNow;
private TimeSpan _elapsed = TimeSpan.Zero;
private ModeratorInstruction? _lastInstruction;

protected override void OnInitialized()
{
    _timer = new System.Threading.Timer(
        callback: _ => UpdateTimer(),
        state: null,
        dueTime: TimeSpan.Zero,
        period: TimeSpan.FromSeconds(1));
}

private void OnStateChanged()
{
    // Reset timer when instruction changes
    if (!ReferenceEquals(GameClient.CurrentInstruction, _lastInstruction))
    {
        _timerStart = DateTime.UtcNow;
        _elapsed = TimeSpan.Zero;
        _lastInstruction = GameClient.CurrentInstruction;
    }
}
```

### 8.3. Display Format
```
Elapsed: MM:SS
```

---

## 9. InstructionRenderer Pattern

### 9.1. Purpose
Routes `ModeratorInstruction` subtypes to appropriate View components using pattern matching.

### 9.2. Switch Pattern
```razor
@switch (GameClient.CurrentInstruction)
{
    case AssignRolesInstruction assignRoles:
        <AssignRolesView Instruction="@assignRoles" />
        break;
    case SelectPlayersInstruction selectPlayers:
        <SelectPlayersView Instruction="@selectPlayers" />
        break;
    case SelectOptionsInstruction selectOptions:
        <SelectOptionsView Instruction="@selectOptions" />
        break;
    case ConfirmationInstruction confirmation:
        <ConfirmationView Instruction="@confirmation" />
        break;
    default:
        <MudAlert Severity="Severity.Warning">
            Unknown instruction type: @(GameClient.CurrentInstruction?.GetType().Name)
        </MudAlert>
        break;
}
```

### 9.3. Adding New Instruction Views
1.  Create view in `Components/Game/Views/{InstructionName}View.razor`
2.  Accept instruction as `[Parameter]`
3.  Build appropriate `ModeratorResponse` subtype on submit
4.  Add case to `InstructionRenderer.razor` switch statement

---

## 10. View Implementations

### 10.1. AssignRolesView
*   **Purpose:** Assign roles to players at game start.
*   **Draft Data:** `Dictionary<Guid, MainRoleType?>` (PlayerId → Role)
*   **Validation:** Prevents submission until all roles assigned and counts match.
*   **Delta Feedback:** Displays specific messages (e.g., "Need 1 more Villager").

### 10.2. SelectPlayersView
*   **Purpose:** Select one or more players (e.g., wolf target, seer peek).
*   **Constraints:** Enforces `MinSelections` and `MaxSelections` from instruction.
*   **Validation:** Submit disabled until selection count is within valid range.

### 10.3. SelectOptionsView
*   **Purpose:** Choose from a list of options (e.g., Witch potion choice).
*   **Modes:** Single-select or multi-select based on instruction.
*   **Options:** Reads from `instruction.Options` list.

### 10.4. ConfirmationView
*   **Purpose:** Simple Yes/No or acknowledgment prompts.
*   **Use Case:** Phase transitions, announcements.

---

## 11. PlayerList Features

### 11.1. Visual Styling
| State | Styling |
|-------|---------|
| Alive | Normal opacity, colored avatar |
| Dead | `opacity: 0.5`, disabled text color |

### 11.2. Structure
Each player rendered in `MudExpansionPanel`:
*   **Header:** Avatar, name, alive/dead status
*   **Expanded:** Role icon + name (moderator-only info)

---

## 12. Project Structure

```
Werewolves.Client/
├── MauiProgram.cs                  # DI registration, platform config
├── App.xaml(.cs)                   # Application lifecycle
├── MainPage.xaml(.cs)              # BlazorWebView host
├── Resources/
│   ├── Raw/
│   │   └── Audio/                  # {EnumType}_{EnumValue}.mp3
│   └── Images/                     # {EnumType}_{EnumValue}.png
├── Services/
│   ├── GameClientManager.cs        # Session, Audio Dict<SoundEffectsEnum, IAudioPlayer>, StateChanged
│   ├── AudioMap.cs                 # SoundEffectsEnum → filename (convention-based)
│   └── IconMap.cs                  # Enum → image filename (convention-based)
├── Components/
│   ├── Layout/
│   │   └── MainLayout.razor
│   ├── Pages/
│   │   ├── Lobby.razor             # Game setup, player entry, role selection
│   │   └── Dashboard.razor         # MudTabs(KeepPanelsAlive=true), Timer
│   └── Game/
│       ├── InstructionRenderer.razor   # Routes instructions to views
│       ├── Views/
│       │   ├── AssignRolesView.razor   # Delta validation messages
│       │   ├── SelectPlayersView.razor # Min/max selection enforcement
│       │   ├── SelectOptionsView.razor # Single/multi-select
│       │   └── ConfirmationView.razor  # Yes/No confirmation
│       └── DashboardTabs/
│           ├── PlayerList.razor        # IDisposable, dead player styling
│           └── GameOverview.razor      # IDisposable, phase/turn/counts
└── wwwroot/
    └── css/
        └── app.css                 # Minimal layout shims only
```

---

## 13. Implementation Guidelines

### 13.1. Component Development
*   **MudBlazor First:** Always use MudBlazor components before custom HTML.
*   **CSS:** Only write custom CSS for layout glitches (Safe Areas, touch-action).
*   **CSS Units:** Use standard CSS units (`px`, `rem`). WebView handles DPI scaling.
*   **Reactivity:** Subscribe to `StateChanged` for live updates; implement `IDisposable`.

### 13.2. Input Submission
All user input flows through `GameClientManager.ProcessInputAsync()`:
```csharp
var response = new AssignRolesResponse(Instruction.Id, _assignments);
await GameClient.ProcessInputAsync(response);
```

### 13.3. Error Handling
*   **User Errors:** Display via `MudSnackbar` (e.g., validation failures).
*   **Exceptions:** Allow to propagate for crash reporting.

---

## 14. Decision Log

Key architectural decisions made during development:

| Decision | Rationale |
|----------|-----------|
| **Audio reconciliation is instruction-driven** | `ModeratorInstruction.SoundEffects` property drives audio state. No manual audio triggers in components. |
| **Convention-based asset naming** | Eliminates mapping maintenance. Add asset file, enum value auto-maps. |
| **IDisposable is mandatory for reactive components** | `KeepPanelsAlive="true"` causes components to persist. Without disposal, event handlers leak. |
| **Wake Lock covers entire game session** | Enabled at game start, disabled at game end. Covers Lobby through gameplay. |
| **Timer resets on instruction change** | Reference equality check on `CurrentInstruction` triggers reset. |
| **Single AudioPlayer per sound effect** | `Dictionary<SoundEffectsEnum, IAudioPlayer>` allows multiple concurrent sounds. |
| **IconMap replaced ImageMap** | Old `ImageMap.cs` used explicit mappings; new `IconMap.cs` uses conventions. |

---

## 15. Build & Run

### 15.1. Development Commands
```powershell
# Windows target (default for local dev)
dotnet build Werewolves.Client\Werewolves.UI.MobileClient.csproj -f net10.0-windows10.0.19041.0

# Android
dotnet build -f net10.0-android

# iOS (requires Mac)
dotnet build -f net10.0-ios
```

### 16.2. Key Dependencies
*   `Werewolves.Core.GameLogic` (project reference)
*   `MudBlazor` (NuGet)
*   `Plugin.Maui.Audio` (NuGet)