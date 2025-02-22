using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Biomes;

// This isn't a real biome, it just exists so that the Avatar can be marked as being in a special biome.
public class DeadUniverseBiome : ModBiome
{
    public override Color? BackgroundColor => Color.Black;

    public override string BestiaryIcon => GetAssetPath("BiomeIcons", Name);

    public override bool IsBiomeActive(Player player) => false;

    public override float GetWeight(Player player) => 0f;
}
