# Test Instructions

This folder contains subclassed `ModeratorInstruction` types for testing.

## Purpose

Core's `ModeratorInstruction` types have internal constructors.
These test subclasses expose constructors for creating test instances.

## Example

```csharp
public class TestAssignRolesInstruction : AssignRolesInstruction
{
    public TestAssignRolesInstruction(Guid gameGuid, /* params */) 
        : base(gameGuid)
    {
        // Set properties via reflection or exposed setters
    }
}
```

## Implementation Notes

Each instruction type should be analyzed to determine:
1. Required constructor parameters
2. Properties that need to be set
3. Any dependencies on other Core types
