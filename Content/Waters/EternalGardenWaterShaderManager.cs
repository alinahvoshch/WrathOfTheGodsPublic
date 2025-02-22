using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.FormPresets;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Waters;

[Autoload(Side = ModSide.Client)]
public class EternalGardenWaterShaderManager : ModSystem
{
    /// <summary>
    /// The cosmic texture used by the water effect.
    /// </summary>
    public static LazyAsset<Texture2D> CosmicTexture
    {
        get;
        private set;
    }

    public override void OnModLoad() =>
        CosmicTexture = LazyAsset<Texture2D>.FromPath($"{ModContent.GetInstance<EternalGardenWater>().Texture}Cosmos");

    public override void PostUpdatePlayers()
    {
        bool shouldBeActive = Main.waterStyle == ModContent.GetInstance<EternalGardenWater>().Slot && NamelessDeityBoss.Myself is null;
        ManagedScreenFilter cosmicShader = ShaderManager.GetFilter("NoxusBoss.CosmicWaterShader");
        if (cosmicShader.Opacity <= 0f && !shouldBeActive)
            return;

        float brightnessFactor = 1f;
        Vector4 generalColor = Vector4.One;
        if (NamelessDeityFormPresetRegistry.UsingLucillePreset)
        {
            brightnessFactor = 2.1f;
            generalColor = new Vector4(2f, 0.5f, 0.04f, 1f);
        }

        cosmicShader.TrySetParameter("targetSize", Main.ScreenSize.ToVector2());
        cosmicShader.TrySetParameter("generalColor", generalColor);
        cosmicShader.TrySetParameter("brightnessFactor", brightnessFactor);
        cosmicShader.TrySetParameter("oldScreenPosition", Main.screenPosition);
        cosmicShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
        cosmicShader.SetTexture(CosmicTexture.Value, 1, SamplerState.LinearWrap);
        cosmicShader.SetTexture(SmudgeNoise, 2, SamplerState.LinearWrap);
        cosmicShader.SetTexture(TileTargetManagers.LiquidTarget, 3);
        cosmicShader.SetTexture(TileTargetManagers.LiquidSlopesTarget, 4);
        if (shouldBeActive)
            cosmicShader.Activate();
    }
}
