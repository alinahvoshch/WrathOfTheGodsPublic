using Luminance.Core.Graphics;
using NoxusBoss.Core.CrossCompatibility.Inbound.WikiThis;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Dyes;

public abstract class BaseDye : ModItem, IWikithisNameRedirect
{
    public int DyeID
    {
        get;
        protected set;
    }

    public override string Texture => $"NoxusBoss/Assets/Textures/Content/Items/Dyes/{Name}";

    public string RedirectPageName => "Dyes";

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 3;
        if (Main.netMode != NetmodeID.Server)
            ShaderManager.PostShaderLoadActions.Enqueue(RegisterShader);
    }

    public abstract void RegisterShader();

    public override void SetDefaults()
    {
        // Cache and restore the dye ID.
        // This is necessary because CloneDefaults will automatically reset the dye ID in accordance with whatever it's copied, when in reality the BindShader
        // call already determined what ID this dye should use.
        DyeID = Item.dye;
        Item.CloneDefaults(ItemID.AcidDye);
        Item.dye = DyeID;
    }
}
