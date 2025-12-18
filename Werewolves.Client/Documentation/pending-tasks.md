# Validation Report Resolutions

**Created:** December 3, 2025  
**Purpose:** Track user decisions on issues identified in validation reports.

---

## Component Validation Report

### Clarification Questions

#### Q1: Drag-and-Drop Enhancement
**Issue:** Requirements mention Drag-and-Drop as a "secondary/progressive enhancement" for player reordering. Primary "Move Up/Move Down" buttons are implemented.

**Decision:** **Defer** - Mark as "Known Gap" and don't implement now since primary controls work.

---

#### Q2: Wake Lock Location
**Issue:** Requirements say "Screen must remain on during setup" (Lobby), but Wake Lock is only enabled in `GameClientManager.StartGameAsync()` (when game starts, not during setup).

**Decision:** **Fix it** - Enable Wake Lock when entering Lobby, disable when leaving (if game not started).

---

#### Q3: Auto-Timer & Audio Controls Location
**Issue:** Tab B (Dashboard's Pending Instruction tab) should have Auto-Timer (MM:SS) in header and Audio Control buttons (Mute/Unmute) in footer. Neither are implemented.

**Decision:** **Place in Dashboard.razor wrapping InstructionRenderer** - Dashboard provides the chrome (timer header + audio footer), InstructionRenderer just shows instructions.

**Rationale:** 
- Separation of concerns: InstructionRenderer focuses on routing/displaying instructions
- State ownership: Timer and audio controls need GameClientManager access, which Dashboard has
- Consistent layout: Tab B structure (header → content → footer) visible in one place

---

#### Q4: Validation Feedback in AssignRolesView
**Issue:** Requirements say "Must show validation feedback (delta: 'Need 1 more Villager')". Current implementation shows color coding but not specific delta messages.

**Decision:** **Add delta messages** - Explicitly show what's needed (e.g., "Need 1 more Villager", "Remove 2 Werewolves").

---

### Critical Bugs

#### IDisposable Memory Leaks
**Issue:** Three components (`InstructionRenderer.razor`, `PlayerList.razor`, `GameOverview.razor`) subscribe to `StateChanged` and have `Dispose()` methods, but Blazor won't call them without `@implements IDisposable`.

**Decision:** **Create task to fix** - Delegate to sub-agent to add `@implements IDisposable` to all three components.

**Status:** 🟡 Pending - Task created

---

### Missing Features

#### #4: Alive vs Dead Player Appearance
**Issue:** `PlayerList.razor` shows "Alive" hardcoded for all players. Should show dead players greyed out.

**Decision:** **Implement** - Check player health via `player.State.Health` and apply styling.

**Implementation Details:**
- Access: `IGameSession.GetPlayerState(guid).Health` or `player.State.Health`
- Dead players: greyed out appearance
- Maintain original player order (don't sort dead to bottom)

**Status:** 🟡 Pending - Task to create

---

#### #5: Tap to Expand Player Details
**Issue:** PlayerList has no expand/collapse for showing role and status effects.

**Decision:** **Implement** with the following approach:

**UI Implementation:**
- Use MudBlazor's `MudExpansionPanel` for expand/collapse
- Role icon visible only when panel is expanded (even for dead players)

**Core Enhancement Required:**
- Add `List<StatusEffect> GetActiveStatusEffects()` method to `IPlayerState`
- UI should not need to understand individual status effect properties
- UI maps status effect enum values to icons via a new StatusEffectMap service

**Current Status Effects (to include in enum):**
- IsInfected
- HasUsedElderExtraLife
- (extensible for future effects)

**Status:** 🟡 Pending - Requires Core change + UI task

**Core Dependency:** See `core-resolutions.md` - StatusEffect enum and GetActiveStatusEffects() method

---

#### #6 & #7: Public Announcement & Private Note
**Issue:** InstructionRenderer doesn't display `PublicAnnouncement` (large text) or `PrivateInstruction` (italicized, moderator-only).

**Decision:** **Implement** - Display at top of InstructionRenderer, before instruction-specific view.

**Layout:**
```
┌─────────────────────────────────┐
│ 📢 Public Announcement (large)  │  ← Only if not null
├─────────────────────────────────┤
│ 🔒 Private Note (italic)        │  ← Only if not null
├─────────────────────────────────┤
│ [Instruction-specific View]     │
└─────────────────────────────────┘
```

**Core Model:** `ModeratorInstruction.PublicAnnouncement` and `ModeratorInstruction.PrivateInstruction` (both nullable)

**Status:** 🟡 Pending - Task to create

---

#### #8: Dynamic Turn Count in GameOverview
**Issue:** GameOverview shows hardcoded "Day 1" and "Dead: 0" instead of actual game state.

**Decision:** **Implement** - Pull data from IGameSession.

**Implementation:**
```csharp
var phase = GameClient.ActiveSession?.GetCurrentPhase();  // GamePhase enum
var turn = GameClient.ActiveSession?.TurnNumber ?? 0;
var deadCount = GameClient.ActiveSession?.GetPlayers()
    .Count(p => p.State.Health == Health.Dead) ?? 0;
```

**Status:** 🟡 Pending - Task to create

---

### Incomplete Implementations

#### #1: SelectPlayersView.razor
**Issue:** Empty submit handler, no instruction parameter, no Min/Max validation, doesn't use SelectablePlayerIds.

**Decision:** **Fix** - Build its own player selection list (don't reuse PlayerList component).

**Rationale:** Different purpose (input vs. monitoring), different data source (filtered vs. all), different interactions (checkboxes vs. expand).

**Implementation:**
1. Accept `[Parameter] SelectPlayersInstruction Instruction`
2. Show only players from `Instruction.SelectablePlayerIds`
3. Use checkboxes for multi-select or radio for single-select (based on `CountConstraint`)
4. Show validation status based on `CountConstraint`
5. Submit via `Instruction.CreateResponse(selectedIds)` + `GameClient.ProcessInputAsync()`

**Status:** 🟡 Pending - Task to create

---

#### #2: SelectOptionsView.razor
**Issue:** Empty submit handler, no instruction parameter, hardcoded options.

**Decision:** **Fix** - Wire up to SelectOptionsInstruction properly.

**Implementation:**
1. Accept `[Parameter] SelectOptionsInstruction Instruction`
2. Display options from `Instruction.SelectableOptions`
3. Use Radio buttons if `SelectionRange` is exactly 1, Checkboxes otherwise
4. Submit via `Instruction.CreateResponse()` + `GameClient.ProcessInputAsync()`

**Status:** 🟡 Pending - Task to create

---

#### #3: ConfirmationView.razor
**Issue:** Empty submit handler, no instruction parameter, hardcoded text.

**Decision:** **Fix** - Wire up to ConfirmationInstruction properly.

**Implementation:**
1. Accept `[Parameter] ConfirmationInstruction Instruction`
2. Announcement text handled by InstructionRenderer (already decided)
3. Single "Proceed" button calls `Instruction.CreateResponse(true)` + `GameClient.ProcessInputAsync()`

**Status:** 🟡 Pending - Task to create

---

#### #4: InstructionRenderer.razor - Missing Switch Cases
**Issue:** Only handles `AssignRolesInstruction`, missing other instruction types.

**Decision:** **Fix** - Add switch cases for all instruction types.

**Implementation:**
```csharp
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
    case ConfirmationInstruction confirmation:  // Catches derived types too
        <ConfirmationView Instruction="@confirmation" />
        break;
    default:
        <MudAlert Severity="Severity.Warning">Unknown Instruction</MudAlert>
        break;
}
```

**Note:** `StartGameConfirmationInstruction` and `FinishedGameConfirmationInstruction` use `ConfirmationView` for now. May change for FinishedGame in future.

**Status:** 🟡 Pending - Task to create

---

## Service Layer Validation Report

### Issue #1: Persistence Not Implemented
**Issue:** `SaveStateAsync()` is a stub. No load implementation exists.

**Decision:** **Add Core dependency** - Blocked on Core implementing `Serialize()`/`Deserialize()`.

**Core Dependency:** See `core-resolutions.md` - Session Serialization/Deserialization

**Status:** 🔴 Blocked on Core

---

### Issue #2: Audio Reconciliation Not Implemented
**Issue:** No automatic audio sync on state change. `PlayAudio()` must be called explicitly.

**Decision:** **Implement** based on `ModeratorInstruction.SoundEffects` property.

**Core Model:** `ModeratorInstruction.SoundEffects` is a `List<SoundEffectsEnum>`.

**Reconciliation Logic:**
For each `SoundEffectsEnum` value:
- **If in list AND not playing:** Start it
- **If in list AND playing:** Do nothing (continue)
- **If NOT in list AND playing:** Stop it
- **If NOT in list AND not playing:** Do nothing

**Implementation:**
1. `AudioMap` should map `SoundEffectsEnum` → audio filename (not instruction type)
2. `GameClientManager` reconciles audio in `ProcessInputAsync()` after state change
3. Also reconcile on `App.OnResume`
4. `GameClientManager` needs to track currently playing sounds

**Status:** 🟡 Pending - Task to create

---

### Issue #3: No StopAudio() Public Method
**Issue:** Only `PlayAudio()` exposed. Can't stop individual sounds for reconciliation.

**Decision:** **Implement** - Add `StopAudio(SoundEffectsEnum)` method.

**Implementation:**
1. Add `StopAudio(SoundEffectsEnum soundEffect)` method
2. Track active sounds via `Dictionary<SoundEffectsEnum, IAudioPlayer>`
3. Reconciliation uses this to stop sounds no longer in instruction's `SoundEffects` list

**Status:** 🟡 Pending - Part of audio reconciliation task

---

### Issue #4: AudioMap/ImageMap Key Semantics
**Issue:** AudioMap maps by instruction type string (wrong). ImageMap uses MainRoleType but may need expansion.

**Decisions:**

**AudioMap:**
- Should map `SoundEffectsEnum` → audio filename (not instruction type)
- Current implementation is incorrect; needs refactoring

**IconMap (replaces ImageMap):**
- Single `IconMap` class with `Dictionary<Enum, string>`
- Handles all icon types: roles, status effects, titles (Sheriff, Lover, etc.)
- **Convention-based file naming:** `{EnumType}_{EnumValue}.png`
  - e.g., `MainRoleType_SimpleWerewolf.png`, `StatusEffect_Infected.png`
- Implementation: `GetIcon(Enum key) => $"{key.GetType().Name}_{key}.png"`
- No explicit mapping dictionary needed; relies on file naming convention

**Status:** 🟡 Pending - Task to create (refactor AudioMap, replace ImageMap with IconMap)

---

## Validation Report (Unique Items)

### Issue #1: Background Audio - Android Configuration
**Issue:** AndroidManifest.xml missing foreground service permissions for background audio playback.

**Decision:** **Defer** - Audio stops when app is backgrounded. Implement background audio support later.

**Status:** 🔵 Deferred

---

### Issue #2: Background Audio - iOS Configuration
**Issue:** Info.plist missing UIBackgroundModes for audio.

**Decision:** **Defer** - Consistent with Android decision. Background audio support deferred.

**Status:** 🔵 Deferred

---

### Q5: Timer Behavior Details
**Issue:** Timer behavior was undefined.

**Decisions:**
- Timer counts **up** from 0 (not countdown)
- Timer **resets on new instruction**
- Persists while switching tabs (state in Dashboard or GameClientManager)

**Implementation Notes:**
- Start timer at 00:00 when new instruction received
- Display format: MM:SS
- Timer continues while switching between Dashboard tabs
- No action triggered (visual/informational only)

**Status:** ✅ Clarified - Part of Dashboard timer implementation

---




