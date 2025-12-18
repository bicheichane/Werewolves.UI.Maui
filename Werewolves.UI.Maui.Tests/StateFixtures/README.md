# State Fixtures

This folder contains JSON state fixtures for each User Story.

## Naming Convention

Files should be named: `US-{ID}_{description}.json`

Example: `US-101_lobby_empty.json`

## Structure

Each fixture follows the `TestStateDto` structure:

```json
{
  "gameId": "guid-here",
  "turnNumber": 1,
  "phase": "Night",
  "players": [
    {
      "id": "guid-here",
      "name": "Alice",
      "isAlive": true,
      "isSheriff": false,
      "role": null
    }
  ],
  "instructionType": "AssignRolesInstruction",
  "instructionData": {}
}
```
