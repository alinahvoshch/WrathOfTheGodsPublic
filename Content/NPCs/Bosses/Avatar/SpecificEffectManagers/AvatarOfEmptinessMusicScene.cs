using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;

public class AvatarOfEmptinessMusicScene : ModSceneEffect
{
    public override SceneEffectPriority Priority => (SceneEffectPriority)9;

    public override int Music => AvatarOfEmptiness.Myself?.ModNPC.Music ?? 0;

    public override bool IsSceneEffectActive(Player player) => AvatarOfEmptiness.Myself is not null && AvatarOfEmptiness.Myself.ModNPC.Music != 0;
}
