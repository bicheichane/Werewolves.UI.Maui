using System;
using System.Collections.Generic;

namespace Werewolves.Client.Services;

public class ImageMap
{
    private readonly Dictionary<string, string> _roleImages;

    public ImageMap()
    {
        _roleImages = new Dictionary<string, string>
        {
            // TODO: Populate with actual Role IDs and filenames
            { "Werewolf", "icon_werewolf.png" },
            { "Villager", "icon_villager.png" },
            { "Seer", "icon_seer.png" }
        };
    }

    public string GetRoleImage(string roleId)
    {
        return _roleImages.TryGetValue(roleId, out var file) ? file : "icon_unknown.png";
    }
}
