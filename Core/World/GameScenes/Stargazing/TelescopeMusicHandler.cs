using NoxusBoss.Core.Autoloaders;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.World.GameScenes.Stargazing;

public class TelescopeMusicHandler : ModSceneEffect
{
    public override bool IsSceneEffectActive(Player player) => StargazingScene.IsActive;

    public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/TheTelescopeSong");

    public override void Load()
    {
        string contentPath = GetAssetPath("Content/Items/Placeable/MusicBoxes", "TheTelescopeSongMusicBox");
        string musicPath = "Assets/Sounds/Music/TheTelescopeSong";
        MusicBoxAutoloader.Create(Mod, contentPath, musicPath, out _, out _);
    }
}
