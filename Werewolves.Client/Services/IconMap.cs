using System.Diagnostics;
using Werewolves.Core.StateModels.Enums;

namespace Werewolves.Client.Services;

public class IconMap
{
    private readonly List<string> _missingIcons = [];

    /// <summary>
    /// Gets any icons that were found to be missing during validation.
    /// </summary>
    public IReadOnlyList<string> MissingIcons => _missingIcons;

    /// <summary>
    /// Indicates whether icon validation has been performed.
    /// </summary>
    public bool HasValidated { get; private set; }

    /// <summary>
    /// Gets the icon filename for any enum value using convention-based naming.
    /// </summary>
    /// <param name="enumValue">Any enum value (MainRoleType, StatusEffect, etc.)</param>
    /// <returns>Filename in format "{EnumType}_{EnumValue}.png"</returns>
    public string GetIcon(Enum enumValue)
    {
        return $"{enumValue.GetType().Name}_{enumValue}.png";
    }

    /// <summary>
    /// Validates that all required icon files exist in Resources/Images.
    /// Should be called at app startup. In DEBUG mode, throws an exception if icons are missing.
    /// In RELEASE mode, logs warnings and continues.
    /// </summary>
    /// <returns>True if all icons exist, false if any are missing.</returns>
    public async Task<bool> ValidateIconsAsync()
    {
        _missingIcons.Clear();
        HasValidated = true;

        // Check all MainRoleType icons
        foreach (MainRoleType role in Enum.GetValues<MainRoleType>())
        {
            await CheckIconExistsAsync(GetIcon(role));
        }

        // Add other enum types here as needed:
        // foreach (StatusEffect effect in Enum.GetValues<StatusEffect>())
        // {
        //     await CheckIconExistsAsync(GetIcon(effect));
        // }

        if (_missingIcons.Count > 0)
        {
            var missingList = string.Join(", ", _missingIcons);
            var message = $"IconMap validation failed. Missing icons: {missingList}";
            
            Debug.WriteLine($"[IconMap] WARNING: {message}");

#if DEBUG
            // In DEBUG mode, throw to fail fast during development
            throw new InvalidOperationException(message);
#endif
        }

        return _missingIcons.Count == 0;
    }

    private async Task CheckIconExistsAsync(string fileName)
    {
        try
        {
            // MAUI images in Resources/Images are accessed via the filename directly
            // Try to open as an app package file to verify it exists
            using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
        }
        catch (FileNotFoundException)
        {
            _missingIcons.Add(fileName);
        }
    }
}
