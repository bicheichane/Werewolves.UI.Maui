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
**Related**: US-203 (See Elapsed Time for Current Instruction)

**Manual Test Steps**:
1. Start a game, observe timer displays 00:00
2. Wait 30 seconds, observe timer at ~00:30
3. Advance to next instruction (e.g., confirm a phase transition)
4. Verify timer resets to 00:00

**Why Manual**: Timer behavior requires real-time waiting; state injection cannot test actual timer functionality.

---

### MUS-002: Timer Continues Across Tab Switches
**Related**: US-201 (Navigate Between Dashboard Tabs)

**Manual Test Steps**:
1. Start a game, wait 10 seconds
2. Switch to Players tab
3. Switch back to Action tab
4. Verify timer shows elapsed time (not reset)

**Why Manual**: Requires real-time observation of timer persistence.

---

## Audio Tests

### MUS-003: Night Ambience Plays During Night Phase
**Related**: US-701 (Hear Phase Audio Cues)

**Manual Test Steps**:
1. Start game and assign roles
2. Proceed to first Night phase
3. Verify `SoundEffectsEnum_NightAmbience.mp3` is playing (audio is audible)
4. Advance to Day phase
5. Verify Night ambience stops and Day ambience starts

**Why Manual**: Audio playback state requires human auditory verification; cannot be verified via DOM.

---

### MUS-004: Mute Toggle Works Correctly
**Related**: US-702 (Mute/Unmute Audio)

**Manual Test Steps**:
1. Start a game, proceed to a phase with active audio
2. Tap the mute button
3. Verify:
   - Icon changes to VolumeOff
   - Audio is silent
4. Tap the unmute button
5. Verify:
   - Icon changes to VolumeUp
   - Audio resumes

**Why Manual**: Audio state requires human auditory verification.

---

### MUS-005: Multiple Concurrent Audio Tracks
**Related**: US-703 (Multiple Concurrent Audio Tracks)

**Manual Test Steps**:
1. Navigate to a game state where multiple sounds should play
2. Verify all expected sounds are audible simultaneously
3. Advance to a state where some sounds should stop
4. Verify correct sounds stop and others continue

**Why Manual**: Multiple concurrent audio requires human ear to distinguish tracks.

---

## Platform-Specific Tests

### MUS-010: Android Safe Area Insets
**Platform**: Android

**Manual Test Steps**:
1. Launch app on device with notch/punch-hole camera
2. Verify header content does not overlap with system UI
3. Verify touch targets are within safe area
4. Test in both portrait and landscape orientations

**Why Manual**: Windows testing cannot validate Android rendering.

---

### MUS-011: iOS Safe Area Insets (Notch)
**Platform**: iOS

**Manual Test Steps**:
1. Launch app on iPhone with Face ID notch
2. Verify content respects safe area insets
3. Test in both portrait and landscape orientations
4. Verify home indicator does not overlap with bottom content

**Why Manual**: Windows testing cannot validate iOS rendering.

---

### MUS-012: iOS Safe Area Insets (Dynamic Island)
**Platform**: iOS

**Manual Test Steps**:
1. Launch app on iPhone 14 Pro or newer with Dynamic Island
2. Verify content respects Dynamic Island safe area
3. Test with Dynamic Island expanded (e.g., during call)

**Why Manual**: Windows testing cannot validate iOS rendering.

---

## Visual Regression Tests

### MUS-020: Dark/Light Theme Consistency
**Manual Test Steps**:
1. Set device to Dark mode
2. Launch app, verify colors are appropriate (dark background, light text)
3. Switch device to Light mode
4. Verify colors update correctly (light background, dark text)
5. Check contrast ratios are readable in both modes

**Why Manual**: Visual appearance requires human judgment.

---

### MUS-021: Player List Dead Player Styling
**Related**: US-601 (See All Players and Their Status)

**Manual Test Steps**:
1. Inject state with some dead players
2. Verify dead players display with:
   - Reduced opacity (0.5)
   - Appropriate visual distinction (strikethrough, gray text, etc.)
3. Verify alive players display normally

**Why Manual**: Visual distinction quality requires human judgment.

---

### MUS-022: Role Icon Visibility
**Related**: US-304 (See Role Icons During Assignment)

**Manual Test Steps**:
1. Navigate to role assignment view
2. Verify each role icon is:
   - Clearly visible against background
   - Appropriately sized
   - Correctly positioned next to role name
3. Compare to physical card artwork (if applicable)

**Why Manual**: Visual quality assessment requires human judgment.

---

## Wake Lock Tests

### MUS-030: Screen Stays On During Game
**Related**: US-105 (Start Game), architecture.md Section 7

**Manual Test Steps**:
1. Start a new game
2. Wait 5-10 minutes without touching the device
3. Verify screen remains on (wake lock active)

**Why Manual**: Wake lock behavior requires real-time waiting.

---

### MUS-031: Wake Lock Releases After Game End
**Related**: US-1002 (Return to Lobby After Game)

**Manual Test Steps**:
1. Complete a game until game end screen
2. Confirm the game end (return to lobby)
3. Set device aside without touching
4. Verify screen dims/turns off normally (wake lock released)

**Why Manual**: Wake lock behavior requires real-time observation.

---

## Summary

| Test ID | Description | Platform | Blocking |
|---------|-------------|----------|----------|
| MUS-001 | Timer resets | All | No |
| MUS-002 | Timer persistence | All | No |
| MUS-003 | Night audio | All | No |
| MUS-004 | Mute toggle | All | No |
| MUS-005 | Concurrent audio | All | No |
| MUS-010 | Android safe area | Android | No |
| MUS-011 | iOS notch | iOS | No |
| MUS-012 | iOS Dynamic Island | iOS | No |
| MUS-020 | Theme consistency | All | No |
| MUS-021 | Dead player styling | All | No |
| MUS-022 | Role icons | All | No |
| MUS-030 | Wake lock active | All | No |
| MUS-031 | Wake lock release | All | No |
