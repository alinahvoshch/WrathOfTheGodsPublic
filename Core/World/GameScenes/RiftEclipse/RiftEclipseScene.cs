using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using NoxusBoss.Core.Autoloaders;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.World.GameScenes.RiftEclipse;

public class RiftEclipseScene : ModSceneEffect
{
    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/RiftEclipse");

    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

    public override void Load()
    {
        string contentPath = GetAssetPath("Content/Items/Placeable/MusicBoxes", "RiftEclipseMusicBox");
        string musicPath = "Assets/Sounds/Music/RiftEclipse";
        MusicBoxAutoloader.Create(Mod, contentPath, musicPath, out _, out _, AvatarRift.DrawMusicBoxWithBackRift);
    }

    public override bool IsSceneEffectActive(Player player)
    {
        return !Main.raining && RiftEclipseManagementSystem.RiftEclipseOngoing && player.Center.Y <= Main.worldSurface * 16f;
    }
}
