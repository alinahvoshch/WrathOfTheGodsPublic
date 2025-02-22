using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public class PerversePurseSpawnPreventionSystem : ModSystem
{
    public static readonly List<Item> ActivePurses = [];

    public override void OnModLoad() => On_Main.UpdateTime_SpawnTownNPCs += DisableTownNPCSpawns;

    public override void OnWorldUnload() => ActivePurses.Clear();

    private void DisableTownNPCSpawns(On_Main.orig_UpdateTime_SpawnTownNPCs orig)
    {
        orig();

        ActivePurses.RemoveAll(p => p.ModItem is not PerversePurse);

        foreach (Item purse in ActivePurses)
        {
            if (purse.ModItem is PerversePurse p && p.VictimIDs.Contains(WorldGen.prioritizedTownNPCType))
            {
                WorldGen.prioritizedTownNPCType = NPCID.None;
                break;
            }
        }
    }
}
