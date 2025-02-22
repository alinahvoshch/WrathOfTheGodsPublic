using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.World.GameScenes.EndCredits;

public class EndCreditsMusicHandler : ModSceneEffect
{
    public override bool IsSceneEffectActive(Player player) => ModContent.GetInstance<EndCreditsScene>().IsActive;

    public override SceneEffectPriority Priority => (SceneEffectPriority)25;

    public override int Music => ModContent.GetInstance<EndCreditsScene>().MusicIDOverride ?? MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/EndCredits");
}
