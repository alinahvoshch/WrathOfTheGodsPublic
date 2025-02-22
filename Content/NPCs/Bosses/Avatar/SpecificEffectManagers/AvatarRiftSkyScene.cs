using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using NoxusBoss.Core.Graphics.Shaders.Screen;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using static NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers.AvatarRiftSky;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;

public class AvatarRiftSkyScene : ModSceneEffect
{
    public override bool IsSceneEffectActive(Player player) => AvatarRift.Myself is not null && AvatarRift.Myself.As<AvatarRift>().CurrentAttack != AvatarRift.RiftAttackType.KillOldDuke;

    public override void SpecialVisuals(Player player, bool isActive)
    {
        player.ManageSpecialBiomeVisuals(ScreenShaderKey, isActive);
    }

    public override void Load()
    {
        Filters.Scene[ScreenShaderKey] = new Filter(new GenericScreenShaderData("FilterMiniTower").UseColor(Color.Transparent).UseOpacity(0f), EffectPriority.VeryHigh);
        SkyManager.Instance[ScreenShaderKey] = new AvatarRiftSky();
    }
}
