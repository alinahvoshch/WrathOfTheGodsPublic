using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;
using NoxusBoss.Core.World.WorldSaving;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace NoxusBoss.Content.MainMenuThemes;

public class AvatarWindMainMenu : ModMenu
{
    public override string DisplayName => Language.GetTextValue($"Mods.{Mod.Name}.MenuThemes.AvatarWindMainMenu.DisplayName");

    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/AvatarRift");

    public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
    {
        if (!ShaderManager.HasFinishedLoading)
            return false;

        Vector2 screenArea = ViewportSize;
        Vector2 textureArea = screenArea / WhitePixel.Size() * 2f;

        Main.spriteBatch.PrepareForShaders(null);
        Main.spriteBatch.Draw(WhitePixel, screenArea * 0.5f, null, Color.Black, 0f, WhitePixel.Size() * 0.5f, textureArea, 0, 0f);
        AvatarOfEmptinessSky.DrawBackground();
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        return false;
    }

    public override bool IsAvailable => GlobalBossDownedSaveSystem.IsDefeated<AvatarOfEmptiness>();
}
