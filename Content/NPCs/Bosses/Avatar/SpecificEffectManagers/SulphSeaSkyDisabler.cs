using CalamityMod.Skies;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;

[JITWhenModsEnabled(CalamityCompatibility.ModName)]
[ExtendsFromMod(CalamityCompatibility.ModName)]
public class SulphSeaSkyDisabler : ModSystem
{
    /// <summary>
    /// Whether the sulphurous sea sky background should be disabled this frame or not.
    /// </summary>
    public static bool DisableSulphSeaSky
    {
        get;
        set;
    }

    public delegate void orig_SulphSeaDrawMethod(SulphurSeaSky self, SpriteBatch spriteBatch, float minDepth, float maxDepth);

    public delegate void hook_SulphSeaDrawMethod(orig_SulphSeaDrawMethod orig, SulphurSeaSky self, SpriteBatch spriteBatch, float minDepth, float maxDepth);

    public override void PostSetupContent()
    {
        MonoModHooks.Add(typeof(SulphurSeaSky).GetMethod("Draw", UniversalBindingFlags), new hook_SulphSeaDrawMethod(DisableSulphSeaRendering));
    }

    public override void PreUpdateEntities() => DisableSulphSeaSky = false;

    public static void DisableSulphSeaRendering(orig_SulphSeaDrawMethod orig, SulphurSeaSky self, SpriteBatch spriteBatch, float minDepth, float maxDepth)
    {
        if (!DisableSulphSeaSky)
            orig(self, spriteBatch, minDepth, maxDepth);
    }
}
