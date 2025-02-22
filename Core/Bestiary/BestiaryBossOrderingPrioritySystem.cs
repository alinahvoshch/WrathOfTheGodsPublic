using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Bestiary;

public class BestiaryBossOrderingPrioritySystem : ModSystem
{
    /// <summary>
    /// The priority of boss entries in the bestiary.
    /// </summary>
    public static int[] Priority
    {
        get;
        private set;
    } = NPCID.Sets.Factory.CreateIntSet();

    public override void PostSetupContent()
    {
        NPCID.Sets.BossBestiaryPriority = NPCID.Sets.BossBestiaryPriority.OrderBy(index => Priority[index]).ToList();
    }
}
