using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Core.World.GameScenes.RiftEclipse;
using NoxusBoss.Core.World.WorldSaving;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace NoxusBoss.Core.World.CustomWorldIcons;

public class CustomWorldIconManager : ModSystem
{
    public override void OnModLoad()
    {
        Main.QueueMainThreadAction(() =>
        {
            On_AWorldListItem.GetIcon += UseCustomIcons;
            On_UIWorldListItem.ctor += UseCustomBorder;
        });
    }

    private static Asset<Texture2D> UseCustomIcons(On_AWorldListItem.orig_GetIcon orig, AWorldListItem self)
    {
        // Check for tag data.
        if (self.Data.TryGetHeaderData<CustomWorldIconManager>(out TagCompound tag))
        {
            if (tag.ContainsKey("UnlockedNamelessDeityWorldIcon"))
                return GennedAssets.Textures.CustomWorldIcons.IconPostNamelessDeity.Asset;
            if (tag.ContainsKey("AvatarWorld"))
            {
                if (self.Data.HasCrimson)
                    return GennedAssets.Textures.CustomWorldIcons.IconAvatarWorldCrimson.Asset;
                return GennedAssets.Textures.CustomWorldIcons.IconAvatarWorldCorruption.Asset;
            }
        }

        return orig(self);
    }

    private static void UseCustomBorder(On_UIWorldListItem.orig_ctor orig, UIWorldListItem self, WorldFileData data, int orderInList, bool canBePlayed)
    {
        orig(self, data, orderInList, canBePlayed);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_worldIcon")]
        extern static ref UIElement GetWorldIcon(UIWorldListItem item);
        UIElement worldIcon = GetWorldIcon(self);

        if (data.TryGetHeaderData<CustomWorldIconManager>(out TagCompound tag))
        {
            if (tag.ContainsKey("UnlockedNamelessDeityWorldIcon"))
                return;
            if (tag.ContainsKey("AvatarWorld"))
            {
                // Remove the standard post-ML border, assuming it exists.
                worldIcon.RemoveAllChildren();

                Asset<Texture2D> borderTextureAsset = GennedAssets.Textures.CustomWorldIcons.BorderAvatarWorld.Asset;
                UIImage border = new UIImage(borderTextureAsset)
                {
                    HAlign = 0.5f,
                    VAlign = 0.5f,
                    Top = new StyleDimension(-10f, 0f),
                    Left = new StyleDimension(-3f, 0f),
                    IgnoresMouseInteraction = true
                };
                worldIcon.Append(border);
            }
        }
    }

    public override void SaveWorldHeader(TagCompound tag)
    {
        if (BossDownedSaveSystem.HasDefeated<NamelessDeityBoss>())
            tag["UnlockedNamelessDeityWorldIcon"] = true;
        if (RiftEclipseManagementSystem.RiftEclipseOngoing)
            tag["AvatarWorld"] = true;
    }
}
