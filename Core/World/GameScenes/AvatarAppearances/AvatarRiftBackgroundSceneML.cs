using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.World.GameScenes.AvatarAppearances;

public class AvatarRiftBackgroundSceneML : ModSceneEffect
{
    public override bool IsSceneEffectActive(Player player) => ModContent.GetInstance<PostMLRiftAppearanceSystem>().Ongoing;

    public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

    public override float GetWeight(Player player) => 0.8f;

    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/AvatarWinterStart");
}
