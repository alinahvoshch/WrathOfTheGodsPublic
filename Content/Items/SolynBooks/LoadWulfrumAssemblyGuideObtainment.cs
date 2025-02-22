using CalamityMod.NPCs.NormalNPCs;
using NoxusBoss.Core.Autoloaders.SolynBooks;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using NoxusBoss.Core.DataStructures.DropRules;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    /// <summary>
    /// The chance that the Wulfrum Assembly Guide will drop from a given supercharged wulfrum enemy.
    /// </summary>
    public static int WulfrumAssemblyGuideDropChance => 20;

    private static void LoadWulfrumAssemblyGuideObtainment_Wrapper()
    {
        if (CalamityCompatibility.Enabled)
            LoadWulfrumAssemblyGuideObtainment();
    }

    [JITWhenModsEnabled(CalamityCompatibility.ModName)]
    private static void LoadWulfrumAssemblyGuideObtainment()
    {
        static void GenerateDropRule<TNPCType>(NPC npc, NPCLoot loot, Func<DropAttemptInfo, bool> condition) where TNPCType : ModNPC
        {
            if (npc.type == ModContent.NPCType<TNPCType>())
            {
                LeadingConditionRule superchargedRule = new LeadingConditionRule(new ArbitraryDropRule(condition));
                {
                    int bookID = SolynBookAutoloader.Books["WulfrumAssemblyGuide"].Type;
                    superchargedRule.OnSuccess(new CommonDrop(bookID, WulfrumAssemblyGuideDropChance));
                }
                loot.Add(superchargedRule);
            }
        }

        GlobalNPCEventHandlers.ModifyNPCLootEvent += (npc, loot) =>
        {
            GenerateDropRule<WulfrumDrone>(npc, loot, d => d.npc.As<WulfrumDrone>().Supercharged);
            GenerateDropRule<WulfrumGyrator>(npc, loot, d => d.npc.As<WulfrumGyrator>().Supercharged);
            GenerateDropRule<WulfrumHovercraft>(npc, loot, d => d.npc.As<WulfrumHovercraft>().Supercharged);
            GenerateDropRule<WulfrumRover>(npc, loot, d => d.npc.As<WulfrumRover>().Supercharged);
        };
    }
}
