using Microsoft.Xna.Framework;
using NoxusBoss.Core.Autoloaders;
using NoxusBoss.Core.World.WorldGeneration;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.World;

public class PermafrostKeepMusicSceneManager : ModSceneEffect
{
    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/Svalbard");

    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

    public override void Load()
    {
        string contentPath = GetAssetPath("Content/Items/Placeable/MusicBoxes", "SvalbardMusicBox");
        string musicPath = "Assets/Sounds/Music/Svalbard";
        MusicBoxAutoloader.Create(Mod, contentPath, musicPath, out _, out _);
    }

    public override bool IsSceneEffectActive(Player player)
    {
        if (PermafrostKeepWorldGen.KeepArea == Rectangle.Empty)
            return false;

        Rectangle keepArea = PermafrostKeepWorldGen.KeepArea;
        keepArea.X *= 16;
        keepArea.Y *= 16;
        keepArea.Width *= 16;
        keepArea.Height *= 16;

        keepArea.Inflate(972, 972);

        return player.Hitbox.Intersects(keepArea);
    }
}
