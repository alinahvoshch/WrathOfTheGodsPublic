using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static NoxusBoss.Core.CrossCompatibility.Inbound.ModReferences;

namespace NoxusBoss.Core.CrossCompatibility.Inbound.WikiThis;

public class WikithisCompatibilitySystem : ModSystem
{
    public const string WikiURLPrefix = "https://terrariamods.wiki.gg/wiki/Wrath_of_the_Gods/";

    public const string WikiURL = $"{WikiURLPrefix}{{}}";

    public override void PostSetupContent()
    {
        // Wikithis is client-side, and should not be accessed on servers.
        if (Main.netMode == NetmodeID.Server)
            return;

        // Don't load anything if Wikithis is not enabled.
        if (Wikithis is null)
            return;

        // Register the wiki URL.
        Wikithis.Call("AddModURL", Mod, WikiURL);

        // Register the wiki texture.
        Wikithis.Call("AddWikiTexture", Mod, ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/ModIcons/WikiThisIcon"));

        // Clear up name conflicts.
        ResolveItemRedirects();
        ResolveNPCRedirects();
    }

    private void ResolveItemRedirects()
    {
        List<ModItem> itemsWithRedirects = Mod.GetContent<ModItem>().Where(i =>
        {
            return i is IWikithisNameRedirect;
        }).ToList();

        foreach (ModItem item in itemsWithRedirects)
        {
            IWikithisNameRedirect redirectData = (IWikithisNameRedirect)item;
            Wikithis.Call("ItemIDReplacement", item.Type, $"{WikiURLPrefix}{redirectData.RedirectPageName}");
        }
    }

    private void ResolveNPCRedirects()
    {
        List<ModNPC> npcsWithRedirects = Mod.GetContent<ModNPC>().Where(i =>
        {
            return i is IWikithisNameRedirect;
        }).ToList();

        foreach (ModNPC npc in npcsWithRedirects)
        {
            IWikithisNameRedirect redirectData = (IWikithisNameRedirect)npc;
            Wikithis.Call("NPCIDReplacement", npc.Type, $"{WikiURLPrefix}{redirectData.RedirectPageName}");
        }
    }
}
