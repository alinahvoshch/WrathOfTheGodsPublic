using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Dyes;

public class GoodDye : BaseDye
{
    public override void RegisterShader()
    {
        ManagedShader dyeShader = ShaderManager.GetShader("NoxusBoss.GoodDyeShader");
        dyeShader.TrySetParameter("uColor", new Vector3(0.94f, 0.85f, 0.51f));
        dyeShader.CreateDyeBindings(Type);
    }

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.rare = ModContent.RarityType<NamelessDeityRarity>();
        Item.value = Item.sellPrice(0, 5, 0, 0);
    }

    public override void AddRecipes()
    {
        CreateRecipe(2).
            AddTile(TileID.DyeVat).
            AddIngredient(ItemID.BottledWater, 2).
            AddIngredient<GoodApple>().
            Register();
    }
}
