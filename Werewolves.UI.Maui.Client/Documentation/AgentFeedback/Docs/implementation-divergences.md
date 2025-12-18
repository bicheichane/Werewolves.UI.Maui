# Implementation Divergences: Agentic QA Strategy Infrastructure

**Audit Date:** December 7, 2025  
**Auditor:** Docs Agent  
**Plan Document:** `implementation-plan.md`, `implementation-plan-tests.md`, `implementation-plan-docs.md`

---

## Summary

This document tracks divergences between the implementation plan and the actual implementation delivered by the Coder Agent for the "Agentic QA Strategy Infrastructure" feature.

| Category | Planned | Implemented | Gap |
|----------|---------|-------------|-----|
| Project Structure | 100% | 100% | ✅ None |
| Mock Classes | 100% | 100% | ✅ None |
| Infrastructure | 100% | 80% | ⚠️ Missing FixtureLoader |
| Test Instructions | 100% | 10% | ❌ Only README |
| State Fixtures | 100% | 10% | ❌ Only README |
| Test Classes | 100% | 10% | ❌ Only README |
| Documentation | 100% | 80% | ⚠️ Missing MVS in user-stories |

---

## Critical Issues

### 1. Test Project Build Failure

**Severity:** 🔴 Critical  
**Status:** Unresolved

**Description:**  
The test project (`Werewolves.UI.Maui.Tests.csproj`) targets `net10.0` but the client project only targets platform-specific frameworks (`net10.0-android`, `net10.0-ios`, `net10.0-windows10.0.19041`). This causes a NuGet restore error:

```
error NU1201: Project Werewolves.UI.Maui.Client is not compatible with net10.0
```

**Impact:** No tests can be compiled or run until this is resolved.

**Recommended Resolution:**  
Option A: Change test project to target `net10.0-windows10.0.19041.0` (Windows-only tests)  
Option B: Extract testable components to a Razor Class Library targeting `netstandard2.0` or `net10.0`  
Option C: Use conditional compilation in the client project

---

## Missing Implementations

### 2. FixtureLoader Utility

**Severity:** 🟡 Medium  
**Plan Reference:** `implementation-plan-tests.md` Section 2.3

**Description:**  
The `Infrastructure/FixtureLoader.cs` utility class was not created. This class is needed to load embedded JSON state fixtures for tests.

**Expected Code:**
```csharp
public static class FixtureLoader
{
    public static TestStateDto Load(string relativePath) { ... }
    public static string LoadRaw(string relativePath) { ... }
}
```

---

### 3. Test Instruction Subclasses

**Severity:** 🟡 Medium  
**Plan Reference:** `implementation-plan-tests.md` Section 3

**Description:**  
The `TestInstructions/` folder contains only a README. No actual instruction subclasses were created. The plan specified:
- `TestConfirmationInstruction.cs`
- `TestAssignRolesInstruction.cs`
- `TestSelectPlayersInstruction.cs`
- `TestSelectOptionsInstruction.cs`
- `InstructionFactory.cs`

**Impact:** Cannot create test scenarios involving specific instruction types.

---

### 4. State Fixture JSON Files

**Severity:** 🟡 Medium  
**Plan Reference:** `implementation-plan-tests.md` Section 2.2

**Description:**  
The `StateFixtures/` folder contains only a README. The plan specified JSON fixtures:
- `LobbyStates/empty-lobby.json`
- `LobbyStates/valid-5-players.json`
- `LobbyStates/invalid-duplicate-names.json`
- `GameplayStates/night-start.json`
- `GameplayStates/day-voting.json`
- `GameplayStates/game-end-village-win.json`

**Impact:** QA Agent has no predefined states to inject for testing.

---

### 5. Test Classes

**Severity:** 🟡 Medium  
**Plan Reference:** `implementation-plan-tests.md` Section 4

**Description:**  
The `Tests/` folder contains only a README. The plan specified:
- `Rendering/LobbyRenderingTests.cs`
- `Rendering/DashboardRenderingTests.cs`
- `Rendering/InstructionRenderingTests.cs`
- `Interactions/LobbyInteractionTests.cs`
- `Interactions/DashboardInteractionTests.cs`

**Impact:** No automated tests exist.

---

## Missing Documentation

### 6. User Stories MVS Sections

**Severity:** 🟠 High  
**Plan Reference:** `implementation-plan-docs.md` Section 1.1

**Description:**  
The `user-stories.md` document does not include **Minimum Viable State (MVS)** JSON definitions for each User Story as specified. These are required for the QA Agent to inject test states.

**Impact:** QA Agent has no predefined states tied to User Stories.

---

## Recommendations

1. **Immediate:** Resolve test project build failure (Critical)
2. **High Priority:** Add MVS sections to User Stories in `user-stories.md`
3. **Medium Priority:** Create state fixture JSON files
4. **Medium Priority:** Create FixtureLoader utility
5. **Medium Priority:** Create test instruction subclasses
6. **Lower Priority:** Create actual test classes

---

## Next Steps

This divergence report should be reviewed by the Orchestrator to determine:
1. Whether to assign the missing items back to Coder Agent
2. Whether to proceed to QA testing with partial infrastructure
3. Whether to update the plan to reduce scope
