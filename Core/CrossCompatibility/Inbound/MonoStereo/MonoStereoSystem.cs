using Terraria.ModLoader;

namespace NoxusBoss.Core.CrossCompatibility.Inbound.MonoStereo;

public class MonoStereoSystem : ModSystem
{
    /// <summary>
    /// The internal name of MonoStereo.
    /// </summary>
    public const string ModName = "MonoStereoMod";

    /// <summary>
    /// The MonoStereo mod's <see cref="Mod"/> instance.
    /// </summary>
    public static Mod MonoStereo
    {
        get;
        private set;
    }

    /// <summary>
    /// Whether the MonoStereo is enabled.
    /// </summary>
    public static bool Enabled => ModLoader.TryGetMod(ModName, out _);

    public override void PostSetupContent()
    {
        if (ModLoader.TryGetMod(ModName, out Mod stereo))
            MonoStereo = stereo;
    }
}
