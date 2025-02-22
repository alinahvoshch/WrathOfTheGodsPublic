using Luminance.Core.Hooking;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using NoxusBoss.Assets;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Content.Tiles;
using NoxusBoss.Core.World.GameScenes.Cattail;
using NoxusBoss.Core.World.WorldSaving;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Placeable;

public class Cattail : ModItem
{
    public override string Texture => GetAssetPath("Content/Items/Placeable", Name);

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 50;

        new ManagedILEdit("Play Special Sound for Cattail", Mod, edit =>
        {
            IL_Player.PlaceThing_Tiles_PlaceIt += edit.SubscriptionWrapper;
        }, edit =>
        {
            IL_Player.PlaceThing_Tiles_PlaceIt -= edit.SubscriptionWrapper;
        }, PlayAwesomeSoundForCattail).Apply();
    }

    private static void PlayAwesomeSoundForCattail(ILContext context, ManagedILEdit edit)
    {
        ILCursor cursor = new ILCursor(context);

        if (!cursor.TryGotoNext(i => i.MatchCallOrCallvirt<Player>("PlaceThing_Tiles_PlaceIt_KillGrassForSolids")))
        {
            edit.LogFailure("The Player.PlaceThing_Tiles_PlaceIt_KillGrassForSolids call could not be found.");
            return;
        }

        cursor.Emit(OpCodes.Ldarg_3);
        cursor.EmitDelegate((int tileType) =>
        {
            if (tileType == ModContent.TileType<CattailTile>() && !WorldSaveSystem.HasPlacedCattail)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.Item.CattailPlacementCelebration with { SoundLimitBehavior = SoundLimitBehavior.IgnoreNew });
                CattailAnimationSystem.StartAnimation();
                WorldSaveSystem.HasPlacedCattail = true;
            }
        });
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<CattailTile>());
        Item.width = 16;
        Item.height = 10;
        Item.rare = ModContent.RarityType<NamelessDeityRarity>();
        Item.value = Item.sellPrice(100, 0, 0, 0);
        Item.consumable = true;
    }
}

