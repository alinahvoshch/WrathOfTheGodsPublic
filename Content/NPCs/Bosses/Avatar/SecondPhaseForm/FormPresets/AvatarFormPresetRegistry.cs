using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.FormPresets;

public class AvatarFormPresetRegistry : ModSystem
{
    /// <summary>
    /// This preset replaces the Avatar's mask with the mask he used prior to the death of his universe.
    /// </summary>
    public static bool UsingLucillePreset => Main.LocalPlayer.name.Equals("Lucille", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// This preset makes the Avatar blue.
    /// </summary>
    public static bool UsingMoonburnPreset => Main.LocalPlayer.name.Equals("Moonburn", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// This preset makes the Avatar fucking HUGE.
    /// </summary>
    public static bool UsingDarknessFallsPreset => Main.LocalPlayer.name.Equals("Darkness Falls", StringComparison.OrdinalIgnoreCase);
}
