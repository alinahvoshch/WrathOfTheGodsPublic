using System.Reflection;
using CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;
using Luminance.Core.Hooking;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using Terraria;
using Terraria.ModLoader;
using static NoxusBoss.Core.Autoloaders.SolynBooks.SolynBookAutoloader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    /// <summary>
    /// The chance that a slime will spawn with the bible of slime contained within it, taking into account the 1/20 chance for a slime to spawn with an item inside it.
    /// </summary>
    public static int BibleOfSlimeAppearanceChance => 720;

    public delegate void CalamitySlimeChooseRandomItemDelegate(out int dropItem);

    public delegate void CalamitySlimeChooseRandomItemHook(CalamitySlimeChooseRandomItemDelegate orig, out int dropItem);

    private static void LoadBibleOfSlimeObtainment()
    {
        On_NPC.AI_001_Slimes_GenerateItemInsideBody += AddGoozmaBibleToSlimes;
        if (CalamityCompatibility.Enabled)
            AddGoozmaBibleToSlimes_CalamityWrapper();
    }

    private static int AddGoozmaBibleToSlimes(On_NPC.orig_AI_001_Slimes_GenerateItemInsideBody orig, bool isBallooned)
    {
        if (Main.rand.NextBool(BibleOfSlimeAppearanceChance / 20))
            return Books["BibleOfSlime"].Type;

        return orig(isBallooned);
    }

    private static void AddGoozmaBibleToSlimes_Calamity(CalamitySlimeChooseRandomItemDelegate orig, out int dropItem)
    {
        orig(out dropItem);

        if (Main.rand.NextBool(BibleOfSlimeAppearanceChance / 20))
            dropItem = Books["BibleOfSlime"].Type;
    }

    [JITWhenModsEnabled(CalamityCompatibility.ModName)]
    private static void AddGoozmaBibleToSlimes_CalamityWrapper()
    {
        MethodInfo? chooseItemMethod = typeof(SlimeAI).GetMethod("ChooseRandomItem", UniversalBindingFlags);
        if (chooseItemMethod is null)
            return;

        HookHelper.ModifyMethodWithDetour(chooseItemMethod, AddGoozmaBibleToSlimes_Calamity);
    }
}
