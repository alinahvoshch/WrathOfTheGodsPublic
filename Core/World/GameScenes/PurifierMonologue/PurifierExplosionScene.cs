using NoxusBoss.Content.Projectiles.Typeless;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.World.GameScenes.PurifierMonologue;

public class PurifierExplosionScene : ModSceneEffect
{
    public override int Music
    {
        get
        {
            if (PurifierMonologueDrawer.TimeSinceMonologueBegan >= 210)
                return MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/worldgen");

            return 0;
        }
    }

    public override SceneEffectPriority Priority => (SceneEffectPriority)100;

    public override bool IsSceneEffectActive(Player player)
    {
        if (player.ownedProjectileCounts[ModContent.ProjectileType<ThePurifierProj>()] >= 1)
            return true;
        if (!Main.gameMenu && WorldGen.generatingWorld)
            return true;

        return false;
    }
}
