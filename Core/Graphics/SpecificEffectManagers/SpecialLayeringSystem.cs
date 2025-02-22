using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.SpecificEffectManagers;

[Autoload(Side = ModSide.Client)]
public class SpecialLayeringSystem : ModSystem
{
    public static List<int> DrawCacheAfterBlack
    {
        get;
        private set;
    } = new(Main.maxNPCs);

    public static List<int> DrawCacheFrontLayer
    {
        get;
        private set;
    } = new(Main.maxNPCs);

    public static List<int> DrawCacheOverTent
    {
        get;
        private set;
    } = new(Main.maxNPCs);

    public static List<int> DrawCacheAfterBlack_Proj
    {
        get;
        private set;
    } = new(Main.maxProjectiles);

    internal static void DrawOverBlackNPCCache(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchCall<ScreenDarkness>("DrawBack")))
            return;

        cursor.EmitDelegate(() =>
        {
            if (Main.gameMenu)
                return;

            EmptyDrawCache_Projectile(DrawCacheAfterBlack_Proj);
            EmptyDrawCache_NPC(DrawCacheAfterBlack);
        });
    }

    internal static void DrawToFrontLayer(On_MoonlordDeathDrama.orig_DrawWhite orig, SpriteBatch spriteBatch)
    {
        orig(spriteBatch);
        EmptyDrawCache_NPC(DrawCacheFrontLayer);
    }

    public static void EmptyDrawCache_Projectile(List<int> cache)
    {
        for (int i = 0; i < cache.Count; i++)
        {
            try
            {
                Main.instance.DrawProj(cache[i]);
            }
            catch (Exception e)
            {
                TimeLogger.DrawException(e);
                Main.projectile[cache[i]].active = false;
            }
        }
        cache.Clear();
    }

    public static void EmptyDrawCache_NPC(List<int> cache)
    {
        for (int i = 0; i < cache.Count; i++)
        {
            try
            {
                Main.instance.DrawNPC(cache[i], false);
            }
            catch (Exception e)
            {
                TimeLogger.DrawException(e);
                Main.npc[cache[i]].active = false;
            }
        }
        cache.Clear();
    }

    public override void OnModLoad()
    {
        Main.QueueMainThreadAction(() =>
        {
            IL_Main.DoDraw += DrawOverBlackNPCCache;
            On_MoonlordDeathDrama.DrawWhite += DrawToFrontLayer;
        });
    }

    public override void OnModUnload()
    {
        Main.QueueMainThreadAction(() =>
        {
            IL_Main.DoDraw -= DrawOverBlackNPCCache;
            On_MoonlordDeathDrama.DrawWhite -= DrawToFrontLayer;
        });
    }
}
