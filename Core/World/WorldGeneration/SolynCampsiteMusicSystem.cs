using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Core.Autoloaders;
using NoxusBoss.Core.World.WorldSaving;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.World.WorldGeneration;

public class SolynCampsiteMusicSystem : ModSceneEffect
{
    public override bool IsSceneEffectActive(Player player)
    {
        if (SolynCampsiteWorldGen.TentPosition == Vector2.Zero)
            return false;

        if (!player.WithinRange(SolynCampsiteWorldGen.TentPosition, 1080f))
            return false;

        return BossDownedSaveSystem.HasDefeated<AvatarOfEmptiness>();
    }

    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/StellarRequiem");

    public override void Load()
    {
        string contentPath = GetAssetPath("Content/Items/Placeable/MusicBoxes", "StellarRequiemMusicBox");
        string musicPath = "Assets/Sounds/Music/StellarRequiem";
        MusicBoxAutoloader.Create(Mod, contentPath, musicPath, out _, out _);
    }

    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
}
