using Microsoft.Xna.Framework;
using NoxusBoss.Core.World.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Biomes;

public class EternalGardenBiome : ModBiome
{
    public const string SkyKey = "NoxusBoss:EternalGarden";

    public override ModWaterStyle WaterStyle => ModContent.Find<ModWaterStyle>("NoxusBoss/EternalGardenWater");

    public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.Find<ModSurfaceBackgroundStyle>("NoxusBoss/EternalGardenSurfaceBGStyle");

    public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>("NoxusBoss/EternalGardenBGStyle");

    public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;

    public override Color? BackgroundColor => Color.White;

    public override string BestiaryIcon => GetAssetPath("BiomeIcons", Name);

    public override string BackgroundPath => GetAssetPath("Map", "EternalGardenBG");

    public override string MapBackground => GetAssetPath("Map", "EternalGardenBG");

    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/EternalGarden");

    public override bool IsBiomeActive(Player player) => SubworldSystem.IsActive<EternalGardenNew>();

    public override float GetWeight(Player player) => 0.96f;

    public override void SpecialVisuals(Player player, bool isActive)
    {
        if (SkyManager.Instance[SkyKey] is not null && isActive != SkyManager.Instance[SkyKey].IsActive())
        {
            if (isActive)
                SkyManager.Instance.Activate(SkyKey);
            else
                SkyManager.Instance.Deactivate(SkyKey);
        }
    }
}
