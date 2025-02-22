using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Emotes;

public class SolynEmote : ModEmoteBubble
{
    public override string Texture => GetAssetPath("Emotes", Name);

    public override void SetStaticDefaults() =>
        AddToCategory(EmoteID.Category.Town);
}
