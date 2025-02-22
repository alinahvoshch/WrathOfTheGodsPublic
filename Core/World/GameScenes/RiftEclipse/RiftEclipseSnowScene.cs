using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using NoxusBoss.Core.Autoloaders;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace NoxusBoss.Core.World.GameScenes.RiftEclipse;

public class RiftEclipseSnowScene : ModSceneEffect
{
    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/RiftEclipseBlizzard");

    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

    public override void Load()
    {
        string contentPath = GetAssetPath("Content/Items/Placeable/MusicBoxes", "RiftEclipseBlizzardMusicBox");
        string musicPath = "Assets/Sounds/Music/RiftEclipseBlizzard";
        MusicBoxAutoloader.Create(Mod, contentPath, musicPath, out _, out _, AvatarRift.DrawMusicBoxWithBackRift);
    }

    public override bool IsSceneEffectActive(Player player)
    {
        return Main.raining && RiftEclipseManagementSystem.RiftEclipseOngoing && player.Center.Y <= Main.worldSurface * 16f;
    }

    public override void SpecialVisuals(Player player, bool isActive)
    {
        if (isActive && RiftEclipseSnowSystem.IntensityInterpolant >= 0.6f && Main.UseStormEffects && !AnyBosses())
        {
            float stormShaderObstruction = (float)(typeof(Player).GetField("_stormShaderObstruction", UniversalBindingFlags)?.GetValue(player) ?? 0f);
            float opacity = Math.Min(1f, Main.cloudAlpha * 2f) * stormShaderObstruction;
            float intensity = stormShaderObstruction * 0.4f * Math.Min(1f, Main.cloudAlpha * 2f) * 0.9f + 0.1f;
            Filters.Scene["Blizzard"].GetShader().UseIntensity(intensity);
            Filters.Scene["Blizzard"].GetShader().UseOpacity(opacity * 1.25f);
            ((SimpleOverlay)Overlays.Scene["Blizzard"]).GetShader().UseOpacity(1f - opacity);

            player.ManageSpecialBiomeVisuals("Blizzard", true);
        }
    }
}
