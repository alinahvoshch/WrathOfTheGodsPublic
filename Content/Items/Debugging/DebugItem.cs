using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Debugging;

public abstract class DebugItem : ModItem
{
    public override string Texture => "NoxusBoss/Assets/Textures/Content/Items/Debugging/DebugItem";

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 0;
}
