# Implementation Plan: Documentation Agent Tasks

## Context
This plan covers documentation updates required for the Agentic QA Strategy infrastructure. The full architectural context is in `implementation-plan.md`.

---

## 1. User Stories Documentation

### 1.1 Update `user-stories.md`

**File**: `Documentation/user-stories.md`

**Changes Required**:

#### Add MVS Section Template
Add to each User Story a new section for **Minimum Viable State (MVS)**:

```markdown
**MVS (Minimum Viable State):**
```json
{
  "gameId": "00000000-0000-0000-0000-000000000001",
  "turnNumber": 1,
  "phase": "Night",
  "players": [
    { "id": "...", "name": "Alice", "isAlive": true, "role": "Villager" }
  ],
  "instructionType": "ConfirmationInstruction",
  "instructionData": { ... }
}
```

**QA Verification Points:**
- [ ] Element `#player-list` contains 5 items
- [ ] Button `#start-game` is enabled
- [ ] etc.
```

#### Stories to Update with MVS

| Story ID | Title | MVS Complexity |
|----------|-------|----------------|
| US-101 | Add Players to Game | Low |
| US-102 | Remove Players from Roster | Low |
| US-103 | Reorder Players | Low |
| US-104 | Select Roles for Game | Medium |
| US-105 | Start Game | Medium |
| US-106 | See Validation Errors | Medium |
| US-201 | Navigate Between Dashboard Tabs | Medium |
| US-202 | See Current Game Phase | Medium |
| US-203 | See Elapsed Time | High (timer - manual) |

---

## 2. Manual User Stories

### 2.1 Create `manual-user-stories.md`

**File**: `Documentation/manual-user-stories.md`

**Content**:

```markdown
# Werewolves UI Client: Manual Test Stories

This document contains User Stories that require **manual testing** due to:
- Timer/time-dependent behavior
- Audio playback verification
- Platform-specific rendering (Safe Area, notch)
- Complex gesture interactions
- Visual regression testing

**These stories are NOT covered by automated QA Agent testing.**

---

## Timer & Timing Tests

### MUS-001: Timer Resets on Instruction Change
**Original**: US-203

**Manual Test Steps**:
1. Start game, observe timer at 00:00
2. Wait 30 seconds
3. Advance to next instruction
4. Verify timer resets to 00:00

**Why Manual**: Timer behavior requires real-time waiting; state injection cannot test actual timer functionality.

---

## Audio Tests

### MUS-002: Night Ambience Plays During Night Phase
**Original**: Derived from architecture.md audio requirements

**Manual Test Steps**:
1. Start game
2. Enter Night phase
3. Verify `SoundEffectsEnum_NightAmbience.mp3` is playing
4. Advance to Day phase
5. Verify Night ambience stops

**Why Manual**: Audio playback state is difficult to verify via DOM inspection; actual audio output requires human verification.

### MUS-003: Mute Toggle Works Correctly
**Original**: Architecture.md audio controls

**Manual Test Steps**:
1. During active audio, tap mute button
2. Verify icon changes to VolumeOff
3. Verify audio is silent
4. Tap again to unmute
5. Verify audio resumes

**Why Manual**: Audio state requires human auditory verification.

---

## Platform-Specific Tests

### MUS-010: Android Safe Area Insets
**Platform**: Android

**Manual Test Steps**:
1. Launch on device with notch/punch-hole
2. Verify content does not overlap with system UI
3. Verify touch targets are within safe area

**Why Manual**: Windows testing cannot validate Android rendering.

### MUS-011: iOS Safe Area Insets
**Platform**: iOS

**Manual Test Steps**:
1. Launch on iPhone with notch
2. Verify content respects safe area
3. Test in both portrait and landscape

**Why Manual**: Windows testing cannot validate iOS rendering.

---

## Visual Regression

### MUS-020: Dark/Light Theme Consistency
**Manual Test Steps**:
1. Set device to Dark mode
2. Launch app, verify colors are appropriate
3. Switch to Light mode
4. Verify colors update correctly

**Why Manual**: Visual appearance requires human judgment.
```

---

## 3. Architecture Documentation

### 3.1 Update `architecture.md`

**File**: `Documentation/architecture.md`

**Add new section** (after Section 7 or as Section 8):

```markdown
## 8. Test Infrastructure Architecture

### 8.1 Philosophy
The test infrastructure follows the **State Injection** pattern, allowing the QA Agent to "teleport" the UI to any game state instantly without simulating full game flows.

### 8.2 Components

```
┌─────────────────────────────────────────────────────────────────┐
│                    Test Infrastructure                          │
│  ┌─────────────┐    ┌─────────────────────┐    ┌─────────────┐  │
│  │   Chrome    │───►│   TestBridge (JS)   │───►│ TestBridge  │  │
│  │  DevTools   │    │  window.setState()  │    │  Service    │  │
│  │   Agent     │    │                     │    │   (C#)      │  │
│  └─────────────┘    └─────────────────────┘    └─────────────┘  │
│                                                      │          │
│                                                      ▼          │
│  ┌─────────────┐    ┌─────────────────────┐    ┌─────────────┐  │
│  │  MockGame   │◄───│   GameClientManager │◄───│ MockGame    │  │
│  │   Session   │    │      (Adapter)      │    │  Service    │  │
│  └─────────────┘    └─────────────────────┘    └─────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### 8.3 Mock Implementation Pattern

Mock classes implement the same interfaces as production classes but:
- Have public setters for all properties
- Contain no game logic
- Support direct state injection via DTOs

```csharp
// Production: readonly, computed from events
public class GameSession : IGameSession { ... }

// Test: writable, direct state
public class MockGameSession : IGameSession
{
    public GamePhase Phase { get; set; }  // Direct set
}
```

### 8.4 JavaScript Bridge

The `window.TestBridge` object enables the QA Agent (via Chrome DevTools Protocol) to inject state at runtime:

```javascript
// Agent executes via Chrome Console
window.TestBridge.setState(JSON.stringify({
    phase: "Day",
    turnNumber: 3,
    players: [...]
}));
```

This triggers:
1. JS → C# interop via `[JSInvokable]`
2. TestBridgeService parses JSON
3. MockGameService updates state
4. GameClientManager fires StateChanged
5. Blazor components re-render

### 8.5 Test Scope Definition

| Scope | Automated (QA Agent) | Manual |
|-------|---------------------|--------|
| Rendering correctness | ✅ | |
| Click handlers | ✅ | |
| Input validation | ✅ | |
| Form submission | ✅ | |
| Timer behavior | | ✅ |
| Audio playback | | ✅ |
| Platform-specific UI | | ✅ |
| Visual regression | | ✅ |

### 8.6 Minimum Viable State (MVS)

Each User Story defines an MVS JSON object that represents the minimum state needed to test that scenario. The QA Agent uses MVS to "teleport" to test scenarios instantly.

See `user-stories.md` for MVS definitions per story.
```

---

## 4. Agent Documentation

### 4.1 Update `qa-agent.md`

**File**: `.github/agents/qa-agent.md`

**Replace placeholder with full specification**:

```markdown
---
name: qa_agent
description: Quality Assurance Agent for Werewolves.UI.Maui - UI verification via Chrome DevTools MCP
---

You are a strict, analytical Quality Assurance Agent for the Werewolves.UI.Maui project. Your goal is to ensure the reliability and correctness of the UI by verifying that components render correctly and respond appropriately to state changes.

## Your Role
- You verify UI behavior against User Stories defined in `Documentation/user-stories.md`.
- You use Chrome DevTools MCP to inspect the DOM and inject state via the TestBridge.
- You **only** inspect and verify; you do not write code or modify files.
- You act as the guardian of **UI/UX correctness**: you verify that state changes result in the correct visual output.

## Project Knowledge
- **Tech Stack:** .NET MAUI Blazor Hybrid, MudBlazor, C#.
- **Source of Truth:**
  - `Documentation/user-stories.md` is your specific instruction manual (contains User Stories with MVS).
  - `Documentation/manual-user-stories.md` contains tests you should NOT attempt (timer, audio, platform-specific).
  - `Documentation/architecture.md` is your reference for UI architecture and patterns.
- **File Structure:**
  - `Components/` – Blazor components (you READ/INSPECT via DOM).
  - `Services/` – Client services including GameClientManager.
  - `Documentation/` – Requirements (you READ here).

## Scope

### In Scope ✅
- Verifying DOM elements exist and contain correct content
- Testing click handlers fire correctly
- Testing input field validation
- Testing form submission flows
- Verifying CSS classes and visual states
- Comparing rendered output against User Story Acceptance Criteria

### Out of Scope 🚫
- Timer/time-dependent behavior (see `manual-user-stories.md`)
- Audio playback verification
- Platform-specific rendering (Android, iOS, macOS)
- Visual regression testing
- Performance testing

## Tools & Environment

### Chrome DevTools MCP
You connect to the running MAUI app via Chrome DevTools Protocol (CDP) at `localhost:9222`.

**Capabilities**:
- DOM inspection (`document.querySelector()`)
- JavaScript execution via Console
- Element interaction (click, type)
- Screenshot capture (if needed)

### TestBridge API
The app exposes a JavaScript bridge for state injection:

```javascript
// Check if bridge is ready
window.TestBridge.isReady();

// Inject test state
window.TestBridge.setState(JSON.stringify({
    gameId: "...",
    turnNumber: 1,
    phase: "Night",
    players: [...],
    instructionType: "ConfirmationInstruction",
    instructionData: { ... }
}));
```

## Workflow

### 1. Analysis
Analyze `Documentation/implementation-plan-tests.md` (your primary instruction set) and `Documentation/user-stories.md`. Determine which User Stories are ready for verification.
- **Fallback:** If `implementation-plan-tests.md` doesn't exist, enter the Clarification Loop.
- **Cross-Reference:** You may also read `Documentation/implementation-plan-coder.md` to understand what UI changes were made.
If test scenarios are unclear, enter the Clarification Loop.

### 2. The Clarification Loop (Conditional)
**IF** specific questions arise regarding test scenarios or verification steps:
1. **Ask Questions:** Use the `ask_user` tool directly to present your questions or ambiguities to the user. Wait for their response before proceeding.
2. **Fallback (On Request):** If the user explicitly asks you to "save questions to disk", write them to `Documentation/AgentFeedback/QA/questions.md`.
3. **Integrate Feedback:** Use the user's responses (received via `ask_user`) to integrate their answers into your context.
4. **Document divergences**: If the user requests explicitly to diverge from the test plan, follow their new instructions and document them in `Documentation/AgentFeedback/QA/implementation-divergences.md`.

### 3. Pre-Test Setup
1. Confirm app is running with debugging port enabled (`$env:WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS="--remote-debugging-port=9222"`)
2. Connect to `localhost:9222` via Chrome DevTools MCP
3. Verify `window.TestBridge.isReady()` returns `true`

### 4. Per User Story Test Execution
For each User Story in scope:
1. **Read**: Load the User Story from `user-stories.md`
2. **Verify MVS**: Confirm MVS (Minimum Viable State) is defined; report if missing
3. **Inject**: Execute `window.TestBridge.setState(MVS_JSON)`
4. **Wait**: Allow Blazor to re-render (small delay if needed)
5. **Verify**: Check each Acceptance Criterion against the DOM
6. **Report**: Document pass/fail with evidence

### 5. Review & Validation
After verifying all User Stories in scope:

- **If All Pass**: Explicitly state: "All UI verifications passed."
- **If Failures**: Create/overwrite a summary file `Documentation/AgentFeedback/QA/failure-report.md` containing:
    1. Which User Stories failed.
    2. A hypothesis: Is this a UI Bug (Component is wrong), Spec Bug (User Story is wrong), or MVS Bug (Test state needs fixing)?
      - **If UI Bug:** Stop Execution and ask the user for direction through `ask_user`: "Hand off to Coder for fix?"
      - **If Spec Bug:** Stop Execution and ask the user for direction through `ask_user`: "Hand off to Planner for spec fix?"
      - **If MVS Bug:** Fix the MVS yourself and re-run. ONLY TRY TO FIX ONE MVS AT A TIME. Ask for confirmation via `ask_user` when you have established a plan for the fix, **before** executing it.

## Verification Techniques

**Element Existence**:
```javascript
document.querySelector('#start-game-button') !== null
```

**Text Content**:
```javascript
document.querySelector('.phase-display').textContent === 'Night'
```

**Element Count**:
```javascript
document.querySelectorAll('.player-row').length === 5
```

**Button State**:
```javascript
document.querySelector('#submit-btn').disabled === false
```

**CSS Class**:
```javascript
document.querySelector('.alert').classList.contains('mud-alert-error')
```

## Reporting Format

For each User Story tested, report:

```markdown
## US-XXX: [Title]

**Status**: ✅ PASS / ❌ FAIL / ⚠️ BLOCKED

**MVS Injected**: Yes/No (if No, explain why)

**Acceptance Criteria Results**:
| Criterion | Expected | Actual | Status |
|-----------|----------|--------|--------|
| Text input field exists | `#player-name-input` | Found | ✅ |
| Add button visible | `#add-player-btn` | Found | ✅ |

**Evidence**: (optional screenshot or DOM snippet)

**Notes**: Any observations or issues
```

## Error Handling

### TestBridge Not Ready
If `window.TestBridge.isReady()` returns `false`:
1. Report to orchestrator: "TestBridge not registered"
2. Request manual intervention to restart app

### MVS Not Defined
If a User Story lacks MVS:
1. Report to orchestrator: "US-XXX missing MVS definition"
2. Skip that story or request definition from Planner Agent

### DOM Element Not Found
If expected element is missing:
1. Log the selector used
2. Log actual DOM structure (relevant portion)
3. Mark criterion as FAIL

## Boundaries
- ✅ **Always do:** Use `ask_user` to ask questions if test scenarios are ambiguous or if you have any other request. Only write to `Documentation/AgentFeedback/QA/questions.md` if the user explicitly requests saving to disk.
- ✅ **Always do:** Consult `Documentation/user-stories.md` for User Story IDs and MVS definitions.
- ✅ **Always do:** Consult `Documentation/architecture.md` for understanding UI architecture.
- ⛔ **Never do:** Ask questions in plain response text. ALL questions MUST use the `ask_user` tool.
- ⚠️ **Ask first:** If you require changes to the TestBridge or new verification capabilities, **do not** modify production code yourself. Use `ask_user` to state the requirement clearly.
- 🚫 **Never do:** Attempt to verify timer, audio, or platform-specific behavior (see `manual-user-stories.md`).
- 🚫 **Never do:** Modify code in `Components/`, `Services/`, or any other production files.
- 🚫 **Never do:** Write unit tests or modify the test project directly.
```

### 4.2 Update `planner-agent.md`

**File**: `.github/agents/planner-agent.md`

**Add to Workflow section** (after "Drafting the Plan"):

```markdown
### 3.5 Manual Test Identification
When creating an implementation plan, identify any User Stories or Acceptance Criteria that cannot be automated:

**Non-Automatable Criteria Include**:
- Timer/time-dependent behavior
- Audio playback verification  
- Platform-specific rendering
- Visual appearance/regression
- Complex gesture interactions

For each non-automatable criterion:
1. Create or update `Documentation/manual-user-stories.md`
2. Document the manual test steps
3. Note why automation is not feasible
```

---

## 5. Cross-Reference Verification

After Coder and Tests plans are implemented, verify:

| Document | Section | Verified Against |
|----------|---------|------------------|
| `architecture.md` Section 8 | Test Infrastructure | Actual implementation |
| `user-stories.md` MVS | JSON structure | `TestStateDto.cs` |
| `qa-agent.md` TestBridge API | JS code | `index.html` bridge |
| `manual-user-stories.md` | Stories moved | `user-stories.md` audit |

---

## 6. Files to Create/Modify

| Action | File | Priority |
|--------|------|----------|
| Create | `Documentation/manual-user-stories.md` | High |
| Modify | `Documentation/user-stories.md` (add MVS) | High |
| Modify | `Documentation/architecture.md` (add Section 8) | Medium |
| Replace | `.github/agents/qa-agent.md` | High |
| Modify | `.github/agents/planner-agent.md` | Medium |
