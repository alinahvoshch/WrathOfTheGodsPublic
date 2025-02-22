using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;
using NoxusBoss.Core.World.WorldSaving;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace NoxusBoss.Content.MainMenuThemes;

public class AvatarRiftSkyMainMenu : ModMenu
{
    public override string DisplayName => Language.GetTextValue($"Mods.{Mod.Name}.MenuThemes.AvatarRiftSkyMainMenu.DisplayName");

    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/AvatarRift");

    public override bool IsAvailable => GlobalBossDownedSaveSystem.IsDefeated(NPCID.MoonLordCore);

    public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
    {
        Main.dayTime = false;
        Main.time = 7200D;
        return false;
    }

    public override void SetStaticDefaults() => On_Main.DoUpdate += UpdateSky;

    private void UpdateSky(On_Main.orig_DoUpdate orig, Main self, ref GameTime gameTime)
    {
        orig(self, ref gameTime);

        if (!Main.gameMenu || !IsSelected || SkyManager.Instance[AvatarRiftSky.ScreenShaderKey] is null)
            return;
        if (!ShaderManager.HasFinishedLoading)
            return;

        SkyManager.Instance.Activate(AvatarRiftSky.ScreenShaderKey);
        SkyManager.Instance[AvatarRiftSky.ScreenShaderKey].Update(gameTime);
    }
}
