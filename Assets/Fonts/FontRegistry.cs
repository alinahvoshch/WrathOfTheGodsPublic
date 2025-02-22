using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace NoxusBoss.Assets.Fonts;

public class FontRegistry : ModSystem
{
    // Historically Calamity received errors when attempting to load fonts on Linux systems for their MGRR boss HP bar.
    // Out of an abundance of caution, this mod implements the same solution as them and only uses the font on windows operating systems.
    public static bool CanLoadFonts => Environment.OSVersion.Platform == PlatformID.Win32NT;

    public static FontRegistry Instance => ModContent.GetInstance<FontRegistry>();

    public static readonly GameCulture EnglishGameCulture = GameCulture.FromCultureName(GameCulture.CultureName.English);

    public static readonly GameCulture ChineseGameCulture = GameCulture.FromCultureName(GameCulture.CultureName.Chinese);

    public static readonly GameCulture RussianGameCulture = GameCulture.FromCultureName(GameCulture.CultureName.Russian);

    // This font deliberately makes no sense, and does not correspond to a real world language.
    public DynamicSpriteFont DivineLanguageTextText
    {
        get
        {
            if (Main.netMode != NetmodeID.Server && CanLoadFonts)
                return Mod.Assets.Request<DynamicSpriteFont>("Assets/Fonts/DivineLanguageText", AssetRequestMode.ImmediateLoad).Value;

            return FontAssets.MouseText.Value;
        }
    }

    public DynamicSpriteFont NamelessDeityText
    {
        get
        {
            // Chinese characters are not present for this font.
            if (ChineseGameCulture.IsActive)
                return FontAssets.DeathText.Value;

            if (CanLoadFonts)
            {
                if (RussianGameCulture.IsActive)
                    return Mod.Assets.Request<DynamicSpriteFont>("Assets/Fonts/NamelessDeityTextRussian", AssetRequestMode.ImmediateLoad).Value;

                return Mod.Assets.Request<DynamicSpriteFont>("Assets/Fonts/NamelessDeityText", AssetRequestMode.ImmediateLoad).Value;
            }

            return FontAssets.MouseText.Value;
        }
    }

    public DynamicSpriteFont AvatarPoemText
    {
        get
        {
            // Chinese characters are not present for this font.
            if (ChineseGameCulture.IsActive)
                return FontAssets.DeathText.Value;

            if (CanLoadFonts)
                if (RussianGameCulture.IsActive)
                    return Mod.Assets.Request<DynamicSpriteFont>("Assets/Fonts/AvatarPoemTextRussian", AssetRequestMode.ImmediateLoad).Value;

            return Mod.Assets.Request<DynamicSpriteFont>("Assets/Fonts/AvatarPoemText", AssetRequestMode.ImmediateLoad).Value;

            return FontAssets.MouseText.Value;
        }
    }

    public DynamicSpriteFont DraedonText
    {
        get
        {
            // Chinese and Russian characters are not present for this font.
            if (ChineseGameCulture.IsActive || RussianGameCulture.IsActive)
                return FontAssets.DeathText.Value;

            if (CanLoadFonts)
                return Mod.Assets.Request<DynamicSpriteFont>("Assets/Fonts/DraedonText", AssetRequestMode.ImmediateLoad).Value;

            return FontAssets.MouseText.Value;
        }
    }

    public DynamicSpriteFont SolynText
    {
        get
        {
            // Chinese characters are not present for this font.
            if (ChineseGameCulture.IsActive)
                return FontAssets.DeathText.Value;

            if (CanLoadFonts)
            {
                if (RussianGameCulture.IsActive)
                    return Mod.Assets.Request<DynamicSpriteFont>("Assets/Fonts/NamelessDeityTextRussian", AssetRequestMode.ImmediateLoad).Value;

                return Mod.Assets.Request<DynamicSpriteFont>("Assets/Fonts/SolynText", AssetRequestMode.ImmediateLoad).Value;
            }

            return FontAssets.MouseText.Value;
        }
    }

    public DynamicSpriteFont SolynTextItalics
    {
        get
        {
            // Chinese characters are not present for this font.
            if (ChineseGameCulture.IsActive)
                return FontAssets.DeathText.Value;

            if (CanLoadFonts)
                return Mod.Assets.Request<DynamicSpriteFont>("Assets/Fonts/SolynTextItalics", AssetRequestMode.ImmediateLoad).Value;

            return FontAssets.MouseText.Value;
        }
    }

    public DynamicSpriteFont SolynFightDialogue
    {
        get
        {
            // Chinese characters are not present for this font.
            if (ChineseGameCulture.IsActive)
                return FontAssets.DeathText.Value;

            if (CanLoadFonts)
                return Mod.Assets.Request<DynamicSpriteFont>("Assets/Fonts/SolynFightDialogue", AssetRequestMode.ImmediateLoad).Value;

            return FontAssets.MouseText.Value;
        }
    }
}
