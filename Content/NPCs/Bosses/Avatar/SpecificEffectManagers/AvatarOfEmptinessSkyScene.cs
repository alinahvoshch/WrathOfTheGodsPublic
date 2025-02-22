using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Core.Graphics.Shaders.Screen;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using static NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers.AvatarOfEmptinessSky;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;

public class AvatarOfEmptinessSkyScene : ModSceneEffect
{
    public static bool NamelessShouldTakePriority => NamelessDeityBoss.Myself is not null && NamelessDeityBoss.Myself_CurrentState != NamelessDeityBoss.NamelessAIType.SavePlayerFromAvatar;

    public override bool IsSceneEffectActive(Player player) => SkyIntensityOverride > 0f || (NPC.AnyNPCs(ModContent.NPCType<AvatarOfEmptiness>()) && !NamelessShouldTakePriority) || InProximityOfMonolith;

    public override void SpecialVisuals(Player player, bool isActive)
    {
        player.ManageSpecialBiomeVisuals(ScreenShaderKey, isActive);
    }

    public override void Load()
    {
        SkyManager.Instance[ScreenShaderKey] = new AvatarOfEmptinessSky();
        Filters.Scene[ScreenShaderKey] = new Filter(new GenericScreenShaderData("FilterMiniTower").UseColor(Color.Transparent).UseOpacity(0f), EffectPriority.VeryHigh);
    }
}
