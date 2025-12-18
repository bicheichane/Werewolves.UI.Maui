using System.Diagnostics;
using Werewolves.Core.StateModels.Enums;

namespace Werewolves.UI.Maui.Client.Services;

/// <summary>
/// Maps SoundEffectsEnum values to audio filenames using convention-based naming.
/// Audio files should be located in Resources/Raw/Audio/ with format: {EnumType}_{EnumValue}.mp3
/// Example: SoundEffectsEnum_NightAmbience.mp3
/// </summary>
public class AudioMap
{
    private readonly List<string> _missingAudioFiles = [];

    /// <summary>
    /// Gets any audio files that were found to be missing during validation.
    /// </summary>
    public IReadOnlyList<string> MissingAudioFiles => _missingAudioFiles;

    /// <summary>
    /// Indicates whether audio validation has been performed.
    /// </summary>
    public bool HasValidated { get; private set; }

    /// <summary>
    /// Gets the audio filename for any enum value using convention-based naming.
    /// </summary>
    /// <param name="enumValue">Any enum value (SoundEffectsEnum, etc.)</param>
    /// <returns>Filename in format "{EnumType}_{EnumValue}.mp3"</returns>
    public string GetAudioFile(Enum enumValue)
    {
        return $"{enumValue.GetType().Name}_{enumValue}.mp3";
    }

    /// <summary>
    /// Gets the audio filename for a sound effect, or null if None.
    /// </summary>
    /// <param name="soundEffect">The sound effect enum value.</param>
    /// <returns>The audio filename, or null if SoundEffectsEnum.None.</returns>
    public string? GetAudioFile(SoundEffectsEnum soundEffect)
    {
        if (soundEffect == SoundEffectsEnum.None)
            return null;

        return GetAudioFile((Enum)soundEffect);
    }

    /// <summary>
    /// Validates that all audio files exist in Resources/Raw/Audio.
    /// Should be called at app startup. In DEBUG mode, throws an exception if files are missing.
    /// In RELEASE mode, logs warnings and continues.
    /// </summary>
    /// <returns>True if all audio files exist, false if any are missing.</returns>
    public async Task<bool> ValidateAudioFilesAsync()
    {
        _missingAudioFiles.Clear();
        HasValidated = true;

        // Check all SoundEffectsEnum audio files (except None)
        foreach (SoundEffectsEnum soundEffect in Enum.GetValues<SoundEffectsEnum>())
        {
            if (soundEffect == SoundEffectsEnum.None)
                continue;

            await CheckAudioExistsAsync(GetAudioFile((Enum)soundEffect));
        }

        if (_missingAudioFiles.Count > 0)
        {
            var missingList = string.Join(", ", _missingAudioFiles);
            var message = $"AudioMap validation failed. Missing audio files: {missingList}";

            Debug.WriteLine($"[AudioMap] WARNING: {message}");

#if DEBUG
            // In DEBUG mode, throw to fail fast during development
            throw new InvalidOperationException(message);
#endif
        }

        return _missingAudioFiles.Count == 0;
    }

    private async Task CheckAudioExistsAsync(string fileName)
    {
        try
        {
            // Audio files are in Resources/Raw/Audio/
            using var stream = await FileSystem.OpenAppPackageFileAsync($"Audio/{fileName}");
        }
        catch (FileNotFoundException)
        {
            _missingAudioFiles.Add($"Audio/{fileName}");
        }
    }
}
