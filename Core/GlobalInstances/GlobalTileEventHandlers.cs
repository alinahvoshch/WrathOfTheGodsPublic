using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.World.TileDisabling;
using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace NoxusBoss.Core.GlobalInstances;

[LegacyName("NoxusGlobalTile")]
public class GlobalTileEventHandlers : GlobalTile
{
    public delegate void NearbyEffectsDelegate(int x, int y, int type, bool closer);

    public static event NearbyEffectsDelegate? NearbyEffectsEvent;

    public delegate void RandomUpdateDelegate(int x, int y, int type);

    public static event RandomUpdateDelegate? RandomUpdateEvent;

    public delegate bool TileConditionDelegate(int x, int y, int type);

    public static event TileConditionDelegate? IsTileUnbreakableEvent;

    public delegate bool TileDrawDelegate(int x, int y, int type, SpriteBatch spriteBatch);

    public static event TileDrawDelegate PreDrawEvent;

    public delegate bool ModifyKillSoundDelegate(int x, int y, int type, bool fail);

    public static event ModifyKillSoundDelegate? ModifyKillSoundEvent;

    public delegate void KillTileDelegate(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem);

    public static event KillTileDelegate? KillTileEvent;

    public static event TileConditionDelegate? ShakeTreeEvent;

    public override void Load() => On_WorldGen.ShakeTree += ShakeTreeHook;

    private static void GetTreeShakeInfo(out int numTreeShakes, out int maxTreeShakes, out int[] treeShakeX, out int[] treeShakeY)
    {
        numTreeShakes = (int)(typeof(WorldGen).GetField("numTreeShakes", UniversalBindingFlags)?.GetValue(null) ?? 0);
        maxTreeShakes = (int)(typeof(WorldGen).GetField("maxTreeShakes", UniversalBindingFlags)?.GetValue(null) ?? 0);
        treeShakeX = (int[])(typeof(WorldGen).GetField("treeShakeX", UniversalBindingFlags)?.GetValue(null) ?? Array.Empty<int>());
        treeShakeY = (int[])(typeof(WorldGen).GetField("treeShakeY", UniversalBindingFlags)?.GetValue(null) ?? Array.Empty<int>());
    }

    private void ShakeTreeHook(On_WorldGen.orig_ShakeTree orig, int i, int j)
    {
        if (ShakeTreeEvent is null)
        {
            orig(i, j);
            return;
        }
        WorldGen.GetTreeBottom(i, j, out int x, out int y);
        GetTreeShakeInfo(out int numTreeShakes, out int maxTreeShakes, out int[] treeShakeX, out int[] treeShakeY);

        if (numTreeShakes == maxTreeShakes)
            return;

        TreeTypes treeType = WorldGen.GetTreeType(Main.tile[x, y].TileType);
        if (treeType == TreeTypes.None)
            return;

        for (int k = 0; k < numTreeShakes; k++)
        {
            if (treeShakeX[k] == x && treeShakeY[k] == y)
                return;
        }

        int tileID = Framing.GetTileSafely(x, y).TileType;
        bool result = true;
        foreach (Delegate d in ShakeTreeEvent.GetInvocationList())
            result &= ((TileConditionDelegate)d).Invoke(x, y, tileID);

        if (!result)
            return;

        orig(i, j);
    }

    public override void Unload()
    {
        // Reset all events on mod unload.
        NearbyEffectsEvent = null;
        RandomUpdateEvent = null;
        IsTileUnbreakableEvent = null;
        ModifyKillSoundEvent = null;
        KillTileEvent = null;
    }

    public override void NearbyEffects(int i, int j, int type, bool closer)
    {
        NearbyEffectsEvent?.Invoke(i, j, type, closer);
    }

    public static bool IsTileUnbreakable(int x, int y)
    {
        // Use default behavior if the event has no subscribers.
        if (IsTileUnbreakableEvent is null)
            return false;

        int tileID = Framing.GetTileSafely(x, y).TileType;
        bool result = false;
        foreach (Delegate d in IsTileUnbreakableEvent.GetInvocationList())
            result |= ((TileConditionDelegate)d).Invoke(x, y, tileID);

        return result;
    }

    public override bool PreHitWire(int i, int j, int type) => !TileDisablingSystem.TilesAreUninteractable;

    public override bool CanExplode(int i, int j, int type)
    {
        if (IsTileUnbreakable(i, j))
            return false;

        return true;
    }

    public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
    {
        if (IsTileUnbreakable(i, j))
            return false;

        return true;
    }

    public override bool CanReplace(int i, int j, int type, int tileTypeBeingPlaced)
    {
        if (IsTileUnbreakable(i, j))
            return false;

        return true;
    }

    public override void RandomUpdate(int i, int j, int type) => RandomUpdateEvent?.Invoke(i, j, type);

    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        KillTileEvent?.Invoke(i, j, type, ref fail, ref effectOnly, ref noItem);
    }

    public override bool KillSound(int i, int j, int type, bool fail)
    {
        // Use default behavior if the event has no subscribers.
        if (ModifyKillSoundEvent is null)
            return true;

        bool result = true;
        foreach (Delegate d in ModifyKillSoundEvent.GetInvocationList())
            result &= ((ModifyKillSoundDelegate)d).Invoke(i, j, type, fail);

        return result;
    }

    public override bool PreDraw(int i, int j, int type, SpriteBatch spriteBatch)
    {
        bool result = true;
        foreach (Delegate d in PreDrawEvent.GetInvocationList())
            result &= ((TileDrawDelegate)d).Invoke(i, j, type, spriteBatch);

        return result;
    }
}
