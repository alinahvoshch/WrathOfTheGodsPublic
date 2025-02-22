using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Buffs;

public class NilkDebuff : ModBuff
{
    public override string Texture => GetAssetPath("Content/Buffs", Name);

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;

        // The nurse cannot save you from your bad trip.
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }
}
