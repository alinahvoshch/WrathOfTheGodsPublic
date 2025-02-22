using Microsoft.Xna.Framework;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using NoxusBoss.Core.Graphics.Shaders.Screen;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace NoxusBoss.Core.World.GameScenes.AvatarUniverseExploration;

[JITWhenModsEnabled(CalamityCompatibility.ModName)]
[ExtendsFromMod(CalamityCompatibility.ModName)]
public class AvatarUniverseExplorationSkyScene : ModSceneEffect
{
    public override bool IsSceneEffectActive(Player player) => AvatarUniverseExplorationSystem.InAvatarUniverse;

    public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/RiftEclipseFog");

    public override string MapBackground => "NoxusBoss/Assets/Textures/Map/AvatarUniverseExplorationMapBackground";

    public override float GetWeight(Player player) => 0.85f;

    public override void SpecialVisuals(Player player, bool isActive)
    {
        player.ManageSpecialBiomeVisuals(AvatarUniverseExplorationSky.ScreenShaderKey, isActive);
    }

    public override void Load()
    {
        Filters.Scene[AvatarUniverseExplorationSky.ScreenShaderKey] = new Filter(new GenericScreenShaderData("FilterMiniTower").UseColor(Color.Transparent).UseOpacity(0f), EffectPriority.VeryHigh);
        SkyManager.Instance[AvatarUniverseExplorationSky.ScreenShaderKey] = new AvatarUniverseExplorationSky();
    }
}
