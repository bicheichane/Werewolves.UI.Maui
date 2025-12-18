# Werewolves UI Client: User Stories

This document defines user stories for the Werewolves moderator helper UI. Stories focus on **UX and interaction patterns**, complementing the Core's integration tests which verify game logic correctness.

**Legend:**
- ✅ Complete - Implemented and working
- 🔧 In Progress - Partially implemented
- ⏳ Blocked - Requires Core dependency
- 📋 Planned - Not yet started

---

## 1. Game Setup

Stories covering the Lobby experience: adding players, selecting roles, and starting a game.

### US-101: Add Players to Game
**As a** moderator,  
**I want to** add player names to the game roster,  
**so that** I can set up a game with the correct participants.

**Acceptance Criteria:**
- [ ] Text input field for player name
- [ ] "Add" button to confirm entry
- [ ] Enter key also adds the player
- [ ] Players appear in a list below the input
- [ ] Minimum 5 players required (error shown if fewer)
- [ ] Maximum player limit enforced (if any)
- [ ] Duplicate names prevented (case-insensitive)

**Status:** ✅ Complete

---

### US-102: Remove Players from Roster
**As a** moderator,  
**I want to** remove players I added by mistake,  
**so that** I can correct the roster before starting.

**Acceptance Criteria:**
- [ ] Each player row has a delete button
- [ ] Clicking delete removes the player immediately
- [ ] No confirmation dialog (quick correction)

**Status:** ✅ Complete

---

### US-103: Reorder Players (Seating Order)
**As a** moderator,  
**I want to** arrange players in their actual seating order,  
**so that** the game flow follows the physical table.

**Acceptance Criteria:**
- [ ] Move Up / Move Down buttons on each player row
- [ ] First player cannot move up (button disabled)
- [ ] Last player cannot move down (button disabled)
- [ ] Changes are immediate and visual

**Status:** ✅ Complete

---

### US-104: Select Roles for Game
**As a** moderator,  
**I want to** choose which roles will be in the game,  
**so that** the game matches the cards I have available.

**Acceptance Criteria:**
- [ ] List of available roles with icons
- [ ] Ability to add/remove roles from selection
- [ ] Role count displayed (e.g., "2 Werewolves")
- [ ] Total role count must match player count (validation)
- [ ] Clear indication of required role balance

**Status:** ✅ Complete

---

### US-105: Start Game
**As a** moderator,  
**I want to** start the game when setup is complete,  
**so that** gameplay can begin.

**Acceptance Criteria:**
- [ ] "Start Game" button visible when valid configuration
- [ ] Button disabled with explanation when invalid
- [ ] Starting navigates to Dashboard
- [ ] Screen stays on (wake lock enabled)

**Status:** ✅ Complete

---

### US-106: See Validation Errors During Setup
**As a** moderator,  
**I want to** see clear error messages when my setup is invalid,  
**so that** I know exactly what to fix.

**Acceptance Criteria:**
- [ ] Errors displayed near the relevant input
- [ ] "Need X more roles" message when roles < players
- [ ] "Duplicate player name" message shown
- [ ] "Minimum 5 players required" message shown

**Status:** ✅ Complete

---

## 2. Navigation & Orientation

Stories covering how the moderator navigates the app and understands current game state.

### US-201: Navigate Between Dashboard Tabs
**As a** moderator,  
**I want to** quickly switch between Players, Action, and Overview tabs,  
**so that** I can access different information during the game.

**Acceptance Criteria:**
- [ ] Three tabs clearly labeled with icons
- [ ] Tab switching is instant (no loading)
- [ ] Current tab is visually highlighted
- [ ] Timer continues running when switching tabs (KeepPanelsAlive)

**Status:** ✅ Complete

---

### US-202: See Current Game Phase
**As a** moderator,  
**I want to** always know what phase the game is in,  
**so that** I can guide players appropriately.

**Acceptance Criteria:**
- [ ] Phase displayed on Overview tab (Night, Dawn, Day)
- [ ] Turn number displayed
- [ ] Phase-appropriate audio plays (if enabled)

**Status:** ✅ Complete

---

### US-203: See Elapsed Time for Current Instruction
**As a** moderator,  
**I want to** see how long the current instruction has been active,  
**so that** I can pace the game appropriately.

**Acceptance Criteria:**
- [ ] Timer displayed as "MM:SS" format
- [ ] Timer resets to 00:00 when instruction changes
- [ ] Timer continues when switching tabs

**Status:** ✅ Complete

---

### US-204: Return to Lobby on Accident
**As a** moderator,  
**I want to** be warned if I accidentally navigate away from the game,  
**so that** I don't lose my progress.

**Acceptance Criteria:**
- [ ] Back navigation shows confirmation dialog
- [ ] Can cancel and stay in game
- [ ] Can confirm and lose progress (no persistence yet)

**Status:** 📋 Planned

---

## 3. Role Assignment

Stories covering the AssignRolesView experience at game start.

### US-301: Assign Roles to Each Player
**As a** moderator,  
**I want to** assign a role to each player,  
**so that** the game knows who has which role.

**Acceptance Criteria:**
- [ ] Each player shown in a list
- [ ] Dropdown/selector for each player's role
- [ ] All selected roles shown in a pool
- [ ] Roles visually indicate when assigned vs. available

**Status:** ✅ Complete

---

### US-302: See Role Assignment Progress
**As a** moderator,  
**I want to** see which roles are still unassigned,  
**so that** I know what's left to distribute.

**Acceptance Criteria:**
- [ ] "Roles to assign" count or list visible
- [ ] Delta messages: "Need 1 more Villager"
- [ ] Submit disabled until all roles assigned

**Status:** ✅ Complete

---

### US-303: Change Role Assignment Before Submit
**As a** moderator,  
**I want to** change a role assignment before submitting,  
**so that** I can correct mistakes.

**Acceptance Criteria:**
- [ ] Can select different role for any player
- [ ] Previous role returns to available pool
- [ ] No confirmation needed for changes

**Status:** ✅ Complete

---

### US-304: See Role Icons During Assignment
**As a** moderator,  
**I want to** see role icons next to role names,  
**so that** I can quickly identify roles visually.

**Acceptance Criteria:**
- [ ] Each role in dropdown has icon + name
- [ ] Icons match the physical cards

**Status:** ✅ Complete

---

## 4. Night Phase Actions

Stories covering moderator interactions during night phase.

### US-401: See Public Announcement Text
**As a** moderator,  
**I want to** see the text I should read aloud to players,  
**so that** I can guide the game correctly.

**Acceptance Criteria:**
- [ ] Large, readable text in announcement area
- [ ] Clear visual distinction from private notes
- [ ] Icon indicates "read this aloud" (e.g., megaphone)

**Status:** ✅ Complete

---

### US-402: See Private Moderator Instructions
**As a** moderator,  
**I want to** see instructions meant only for me,  
**so that** I know what to do but don't read it aloud.

**Acceptance Criteria:**
- [ ] Italicized or visually distinct from public text
- [ ] Lock icon or similar indicates "private"
- [ ] Smaller text than public announcement

**Status:** ✅ Complete

---

### US-403: Select Target Player (Night Actions)
**As a** moderator,  
**I want to** select a player as the target of a night action,  
**so that** I can record who was chosen.

**Acceptance Criteria:**
- [ ] List of selectable players
- [ ] Dead players not shown (or clearly disabled)
- [ ] Invalid targets excluded (e.g., Wolves can't target wolves)
- [ ] Single-tap to select
- [ ] Selection highlighted before confirm

**Status:** ✅ Complete

---

### US-404: Select Multiple Targets (When Allowed)
**As a** moderator,  
**I want to** select multiple players when the action allows it,  
**so that** I can record group selections.

**Acceptance Criteria:**
- [ ] Multi-select mode when instruction allows
- [ ] Min/max selection limits enforced
- [ ] Count displayed: "2 of 3 selected"
- [ ] Submit disabled until within valid range

**Status:** ✅ Complete

---

### US-405: Confirm Night Action Selection
**As a** moderator,  
**I want to** confirm my selection before proceeding,  
**so that** I don't accidentally submit the wrong target.

**Acceptance Criteria:**
- [ ] Submit/Confirm button clearly visible
- [ ] Button disabled until valid selection made
- [ ] Processing feedback shown after submit

**Status:** ✅ Complete

---

### US-406: Choose from Options (Night Decisions)
**As a** moderator,  
**I want to** select from a list of options,  
**so that** I can record decisions like Witch potion usage.

**Acceptance Criteria:**
- [ ] Options displayed as buttons or list
- [ ] Single-select or multi-select based on instruction
- [ ] Clear visual feedback on selection
- [ ] Can change selection before submit

**Status:** ✅ Complete

---

## 5. Day Phase Actions

Stories covering moderator interactions during day phase.

### US-501: Record Vote Outcome
**As a** moderator,  
**I want to** record who the village voted to eliminate,  
**so that** the game state updates correctly.

**Acceptance Criteria:**
- [ ] Player selection list
- [ ] Option for "No elimination" (tie)
- [ ] Dead players excluded from list

**Status:** ✅ Complete

---

### US-502: Confirm Phase Transitions
**As a** moderator,  
**I want to** confirm when I'm ready to proceed to the next phase,  
**so that** I control the game pace.

**Acceptance Criteria:**
- [ ] Clear "Continue" or "Confirm" button
- [ ] Public announcement visible before proceeding
- [ ] No accidental skips

**Status:** ✅ Complete

---

## 6. Player List & Status

Stories covering the Players tab and player information display.

### US-601: See All Players and Their Status
**As a** moderator,  
**I want to** see all players with their alive/dead status,  
**so that** I know who is still in the game.

**Acceptance Criteria:**
- [ ] Player list on Players tab
- [ ] Alive players clearly distinguishable
- [ ] Dead players visually faded (opacity: 0.5) with strikethrough or indicator
- [ ] Dead players sorted to bottom or clearly grouped

**Status:** ✅ Complete (sorting not yet implemented)

---

### US-602: See Player's Role (Moderator View)
**As a** moderator,  
**I want to** see each player's role,  
**so that** I can verify game state and guide night actions.

**Acceptance Criteria:**
- [ ] Expandable panel for each player
- [ ] Role icon + name shown when expanded
- [ ] Role only visible to moderator (device)

**Status:** ✅ Complete

---

### US-603: See Player's Status Effects
**As a** moderator,  
**I want to** see active status effects on each player,  
**so that** I know about infections, protection, etc.

**Acceptance Criteria:**
- [ ] Status effect icons in player detail
- [ ] Tooltip or label explaining each effect
- [ ] Effects update when game state changes

**Status:** ⏳ Blocked (Core: `GetActiveStatusEffects(playerId)`)

---

**MVS (Minimum Viable State) Template & Examples**

The QA Agent requires a small JSON object (MVS) per User Story that includes the minimal fields necessary to render the UI for that story. Add an **MVS** section to each story following this template.

MVS Template:
```json
{
	"gameId": "00000000-0000-0000-0000-000000000001",
	"turnNumber": 1,
	"phase": "Night",
	"players": [
		{ "id": "10000000-0000-0000-0000-000000000001", "name": "Alice", "isAlive": true, "role": "Villager" }
	],
	"instructionType": "ConfirmationInstruction",
	"instructionData": { }
}
```

Examples (add one `**MVS**` under the corresponding story):

- **US-101: Add Players to Game**
```json
{
	"players": [
		{ "id": "10000000-0000-0000-0000-000000000001", "name": "Alice", "isAlive": true },
		{ "id": "10000000-0000-0000-0000-000000000002", "name": "Bob", "isAlive": true },
		{ "id": "10000000-0000-0000-0000-000000000003", "name": "Charlie", "isAlive": true },
		{ "id": "10000000-0000-0000-0000-000000000004", "name": "Dana", "isAlive": true },
		{ "id": "10000000-0000-0000-0000-000000000005", "name": "Eve", "isAlive": true }
	]
}
```

- **US-105: Start Game**
```json
{
	"phase": "Lobby",
	"players": [ /* 5 players as above */ ],
	"instructionType": "StartGameConfirmationInstruction",
	"instructionData": { "canStart": true }
}
```

Notes:
- Add an `**MVS**` block to each User Story in this file.
- If a story requires more complex instruction payloads (SelectPlayers, SelectOptions, AssignRoles), include `instructionType` and `instructionData` fields that reflect the minimum props needed by the component.
- For stories that are intentionally manual (see `manual-user-stories.md`), mark them as manual and include an `MVS` only if useful for partial automation.

---

## 7. Audio & Feedback

Stories covering audio cues and feedback systems.

### US-701: Hear Phase Audio Cues
**As a** moderator,  
**I want to** hear audio that matches the current game phase,  
**so that** players know when phases change.

**Acceptance Criteria:**
- [ ] Night ambience plays during night phase
- [ ] Day ambience plays during day phase
- [ ] Audio changes automatically with instructions

**Status:** ✅ Complete

---

### US-702: Mute/Unmute Audio
**As a** moderator,  
**I want to** mute the audio without stopping the game,  
**so that** I can play in quiet environments.

**Acceptance Criteria:**
- [ ] Mute toggle button visible on Action tab
- [ ] Icon changes between volume/muted states
- [ ] Mute state persists within session
- [ ] Unmuting resumes current audio

**Status:** ✅ Complete

---

### US-703: Multiple Concurrent Audio Tracks
**As a** moderator,  
**I want to** hear multiple sound effects at once when appropriate,  
**so that** overlapping events are all audible.

**Acceptance Criteria:**
- [ ] Instruction can specify multiple SoundEffects
- [ ] All specified sounds play simultaneously
- [ ] Sounds stop when no longer in instruction's SoundEffects list

**Status:** ✅ Complete

---

## 8. Session Persistence

Stories covering saving and resuming games.

### US-801: Resume Game After App Close
**As a** moderator,  
**I want to** resume my game if the app is closed,  
**so that** interruptions don't end the game.

**Acceptance Criteria:**
- [ ] Game state saved automatically after each action
- [ ] App launch checks for saved game
- [ ] Option to resume or start new game
- [ ] Resumed game at correct instruction

**Status:** ⏳ Blocked (Core: `IGameSession.Serialize()`, `GameSessionFactory.Deserialize()`)

---

### US-802: Abandon Current Game
**As a** moderator,  
**I want to** intentionally abandon the current game,  
**so that** I can start fresh.

**Acceptance Criteria:**
- [ ] "End Game" or "Abandon" option accessible
- [ ] Confirmation dialog prevents accidents
- [ ] Returns to Lobby after confirmation
- [ ] Saved state cleared

**Status:** ⏳ Blocked (depends on persistence)

---

## 9. Error Handling & Recovery

Stories covering error states and user guidance.

### US-901: See Friendly Error Messages
**As a** moderator,  
**I want to** see helpful error messages when something goes wrong,  
**so that** I know how to recover.

**Acceptance Criteria:**
- [ ] Validation errors shown inline
- [ ] Snackbar for transient errors
- [ ] No cryptic technical messages

**Status:** ✅ Complete

---

### US-902: Recover from Unknown Instruction
**As a** moderator,  
**I want to** see guidance when the app doesn't recognize an instruction,  
**so that** I know the game encountered an unexpected state.

**Acceptance Criteria:**
- [ ] Warning alert for unknown instruction type
- [ ] Instruction type name displayed for debugging
- [ ] Does not crash the app

**Status:** ✅ Complete

---

## 10. Game End

Stories covering the end-of-game experience.

### US-1001: See Victory Announcement
**As a** moderator,  
**I want to** see which team won and why,  
**so that** I can announce the result to players.

**Acceptance Criteria:**
- [ ] Victory message displayed prominently
- [ ] Winning team indicated (Village, Werewolves, etc.)
- [ ] Reason for victory shown

**Status:** 🔧 In Progress

---

### US-1002: Return to Lobby After Game
**As a** moderator,  
**I want to** return to the Lobby after a game ends,  
**so that** I can start a new game.

**Acceptance Criteria:**
- [ ] "New Game" or "Return to Lobby" button
- [ ] Audio stopped
- [ ] Wake lock disabled
- [ ] Previous game state cleared

**Status:** ✅ Complete

---

## Summary: Implementation Status

| Category | Total | ✅ Complete | 🔧 In Progress | ⏳ Blocked | 📋 Planned |
|----------|-------|-------------|----------------|------------|------------|
| Game Setup | 6 | 6 | 0 | 0 | 0 |
| Navigation & Orientation | 4 | 3 | 0 | 0 | 1 |
| Role Assignment | 4 | 4 | 0 | 0 | 0 |
| Night Phase Actions | 6 | 6 | 0 | 0 | 0 |
| Day Phase Actions | 2 | 2 | 0 | 0 | 0 |
| Player List & Status | 3 | 2 | 0 | 1 | 0 |
| Audio & Feedback | 3 | 3 | 0 | 0 | 0 |
| Session Persistence | 2 | 0 | 0 | 2 | 0 |
| Error Handling & Recovery | 2 | 2 | 0 | 0 | 0 |
| Game End | 2 | 1 | 1 | 0 | 0 |
| **TOTAL** | **34** | **29** | **1** | **3** | **1** |

---

## Blocked Dependencies

The following Core APIs are needed to unblock stories:

| Story | Core API Required | Description |
|-------|-------------------|-------------|
| US-603 | `IGameSession.GetActiveStatusEffects(playerId)` | Query active status effects for display |
| US-801 | `IGameSession.Serialize()` | Serialize game state to JSON |
| US-801 | `GameSessionFactory.Deserialize()` | Reconstruct game from JSON |
| US-802 | (same as above) | Depends on persistence system |
