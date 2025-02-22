using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Core.World.WorldSaving;
using ReLogic.Content;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace NoxusBoss.Content.MainMenuThemes;

// I hate this internal name but it's necessary to ensure that the name is ordered after VisceralDimensionMainMenu.
public class XNamelessDeityDimensionMainMenu : ModMenu
{
    public static ModMenu Instance
    {
        get;
        private set;
    }

    public override void Load()
    {
        Instance = this;
    }

    public override string DisplayName => Language.GetTextValue($"Mods.{Mod.Name}.MenuThemes.NamelessDeityDimensionMainMenu.DisplayName");

    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/EternalGarden");

    public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>(GetAssetPath("MainMenuThemes", "NamelessDeityLogo"));

    public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
    {
        if (!ShaderManager.HasFinishedLoading)
            return false;

        Rectangle area = new Rectangle(0, 0, Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);
        Main.spriteBatch.Draw(NamelessDeityDimensionSkyGenerator.NamelessDeityDimensionTarget, area, Color.White);
        return true;
    }

    public override bool IsAvailable => GlobalBossDownedSaveSystem.IsDefeated<NamelessDeityBoss>();
}
