using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.FormPresets;

public class NamelessDeityFormPreset
{
    /// <summary>
    /// Whether this preset is active based on the player's name.
    /// </summary>
    public bool IsActive => Data.ValidPlayerNames.Any(n => Main.LocalPlayer.name.Equals(n, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// The optional shader overlay effect applied on top of this preset.
    /// </summary>
    public Action<Texture2D>? ShaderOverlayEffect;

    /// <summary>
    /// The loadable data associated with this preset.
    /// </summary>
    public NamelessDeityLoadablePresetData Data;
}
