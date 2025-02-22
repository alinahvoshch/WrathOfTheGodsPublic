using Microsoft.Xna.Framework;
using NoxusBoss.Core.Graphics.Shaders.Screen;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using static NoxusBoss.Core.Graphics.GenesisEffects.GenesisSky;

namespace NoxusBoss.Core.Graphics.GenesisEffects;

public class Genesis : ModSceneEffect
{
    public override SceneEffectPriority Priority => (SceneEffectPriority)12;

    public override int Music => 0;

    public override bool IsSceneEffectActive(Player player) => GenesisVisualsSystem.EffectActive;

    public override void SpecialVisuals(Player player, bool isActive) =>
        player.ManageSpecialBiomeVisuals(ScreenShaderKey, isActive);

    public override void Load()
    {
        SkyManager.Instance[ScreenShaderKey] = new GenesisSky();
        Filters.Scene[ScreenShaderKey] = new Filter(new GenericScreenShaderData("FilterMiniTower").UseColor(Color.Transparent).UseOpacity(0f), EffectPriority.VeryHigh);
    }
}
