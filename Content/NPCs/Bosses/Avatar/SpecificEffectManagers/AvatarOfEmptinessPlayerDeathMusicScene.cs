using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;

public class AvatarOfEmptinessPlayerDeathMusicScene : ModSceneEffect
{
    public override SceneEffectPriority Priority => (SceneEffectPriority)9;

    public override int Music => 0;

    public override bool IsSceneEffectActive(Player player) => player.GetModPlayer<AvatarDeathVisualsPlayer>().MusicAtTimeOfDeath is not null;
}
