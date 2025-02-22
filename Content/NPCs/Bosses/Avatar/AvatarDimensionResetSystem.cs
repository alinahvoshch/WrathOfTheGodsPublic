using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar;

public class AvatarDimensionResetSystem : ModSystem
{
    public override void OnWorldLoad() => AvatarOfEmptinessSky.Dimension = null;

    public override void OnWorldUnload() => AvatarOfEmptinessSky.Dimension = null;
}
