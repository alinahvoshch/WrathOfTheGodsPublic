using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.World.GameScenes.Cattail;

public class CattailMusicEffect : ModSceneEffect
{
    public override bool IsSceneEffectActive(Player player) => CattailAnimationSystem.AnimationTimer >= 1;

    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

    public override float GetWeight(Player player) => 0.75f;

    public override int Music => 0;
}
