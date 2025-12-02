using System;
using System.Collections.Generic;

namespace Werewolves.Client.Services;

public class AudioMap
{
    private readonly Dictionary<string, string> _instructionAudio;

    public AudioMap()
    {
        _instructionAudio = new Dictionary<string, string>
        {
            // TODO: Populate with actual Instruction IDs and filenames
            { "AssignRoles", "bg_lobby.mp3" },
            { "NightStart", "bg_night.mp3" },
            { "DayStart", "bg_day.mp3" }
        };
    }

    public string GetAudioFile(string instructionId)
    {
        return _instructionAudio.TryGetValue(instructionId, out var file) ? file : "bg_default.mp3";
    }
}
