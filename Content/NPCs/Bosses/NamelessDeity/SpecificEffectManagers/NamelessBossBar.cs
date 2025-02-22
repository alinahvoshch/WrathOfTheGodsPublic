using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using ReLogic.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;

public class NamelessBossBar : ModBossBar
{
    private static Vector2 barSize => new Vector2(TestOfResolveSystem.IsActive ? 4000f : 456f, 22f);

    /// <summary>
    /// The draw data from the previous frame for the rendering of this bar.
    /// </summary>
    public static BossBarDrawParams DrawDataLastFrame
    {
        get;
        private set;
    }

    /// <summary>
    /// The render target that holds the contents of the boss bar.
    /// </summary>
    public static ManagedRenderTarget BarTarget
    {
        get;
        private set;
    }

    public override string Texture => GetAssetPath("Content/NPCs/Bosses/NamelessDeity", "BossBar");

    public override void Load()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        RenderTargetManager.RenderTargetUpdateLoopEvent += DrawToTargetWrapper;
        BarTarget = new ManagedRenderTarget(false, (width, _2) =>
        {
            return new RenderTarget2D(Main.instance.GraphicsDevice, width, 350);
        });
    }

    private void DrawToTargetWrapper()
    {
        if (NamelessDeityBoss.Myself is null)
            return;

        GraphicsDevice gd = Main.instance.GraphicsDevice;
        gd.SetRenderTarget(BarTarget);
        gd.Clear(Color.Transparent);

        Main.spriteBatch.Begin();
        DrawToTarget(Main.spriteBatch);
        Main.spriteBatch.End();

        gd.SetRenderTarget(null);
    }

    private static void DrawToTarget(SpriteBatch spriteBatch)
    {
        (Texture2D barTexture, _, _, _, _, float life, float lifeMax, float shield, float shieldMax, _, bool showText, Vector2 textOffset) = DrawDataLastFrame;
        if (barTexture is null)
            return;

        if (AvatarOfEmptiness.Myself is not null)
            return;

        Vector2 barCenter = BarTarget.Size() * 0.5f;

        Point topLeftOffset = new Point(32, 24);
        int frameCount = 9;

        Rectangle bgFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 3);
        Color bgColor = Color.White * 0.2f;

        int scale = (int)(barSize.X * life / lifeMax);
        scale -= scale % 2;
        Rectangle barFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 2);
        barFrame.X += topLeftOffset.X;
        barFrame.Y += topLeftOffset.Y;
        barFrame.Width = 2;
        barFrame.Height = (int)barSize.Y;

        Rectangle tipFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 1);
        tipFrame.X += topLeftOffset.X;
        tipFrame.Y += topLeftOffset.Y;
        tipFrame.Width = 2;
        tipFrame.Height = (int)barSize.Y;

        Rectangle barShieldFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 5);
        barShieldFrame.X += topLeftOffset.X;
        barShieldFrame.Y += topLeftOffset.Y;
        barShieldFrame.Width = 2;
        barShieldFrame.Height = (int)barSize.Y;

        Rectangle tipShieldFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 4);
        tipShieldFrame.X += topLeftOffset.X;
        tipShieldFrame.Y += topLeftOffset.Y;
        tipShieldFrame.Width = 2;
        tipShieldFrame.Height = (int)barSize.Y;

        Rectangle barPosition = new Rectangle((int)barCenter.X - 228, (int)(barCenter.Y - barSize.Y * 0.5f), (int)barSize.X, (int)barSize.Y);
        Vector2 barTopLeft = barPosition.TopLeft();
        Vector2 topLeft = barTopLeft - topLeftOffset.ToVector2();

        // Background.
        spriteBatch.Draw(barTexture, topLeft, bgFrame, bgColor, 0f, Vector2.Zero, 1f, 0, 0f);

        // Bar itself.
        if (TestOfResolveSystem.IsActive)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            ManagedShader overlayShader = ShaderManager.GetShader("NoxusBoss.TestOfResolveHPBarShader");
            overlayShader.TrySetParameter("imageSize", barTexture.Size());
            overlayShader.TrySetParameter("sourceRectangle", new Vector4(barFrame.X, barFrame.Y, barFrame.Width, barFrame.Height));
            if (NamelessDeityBoss.Myself is not null)
                overlayShader.TrySetParameter("time", Pow(NamelessDeityBoss.Myself.As<NamelessDeityBoss>().FightTimer / 75f, 1.16f));

            overlayShader.SetTexture(GennedAssets.Textures.Extra.Cosmos, 1, SamplerState.LinearWrap);
            overlayShader.Apply();
        }

        Vector2 stretchScale = new Vector2(scale / barFrame.Width, 1f);
        Main.spriteBatch.Draw(barTexture, barTopLeft, barFrame, Color.White, 0f, Vector2.Zero, stretchScale, 0, 0f);
        if (TestOfResolveSystem.IsActive)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin();
        }

        // Tip.
        Color barColor = Color.White;
        spriteBatch.Draw(barTexture, barTopLeft + new Vector2(scale - 2, 0f), tipFrame, barColor, 0f, Vector2.Zero, 1f, 0, 0f);

        // Frame.
        Rectangle frameFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 0);
        spriteBatch.Draw(barTexture, topLeft, frameFrame, Color.White, 0f, Vector2.Zero, 1f, 0, 0f);

        frameFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 6);
        spriteBatch.Draw(barTexture, topLeft + Vector2.UnitX * 186f, frameFrame, Color.White, 0f, Vector2.Zero, 1f, 0, 0f);

        frameFrame = new Rectangle(34, 482, 1, 30);
        stretchScale = new Vector2((barSize.X - 456f) / frameFrame.Width, 1f);
        spriteBatch.Draw(barTexture, topLeft + new Vector2(330f, 18f), frameFrame, Color.White, 0f, Vector2.Zero, stretchScale, 0, 0f);

        frameFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 7);
        spriteBatch.Draw(barTexture, topLeft + Vector2.UnitX * (330f + barSize.X - 456f), frameFrame, Color.White, 0f, Vector2.Zero, 1f, 0, 0f);

        // Health text.
        if (BigProgressBarSystem.ShowText && showText)
        {
            barPosition = Utils.CenteredRectangle(barCenter, barSize);
            if (TestOfResolveSystem.IsActive)
            {
                Vector2 baseDrawPosition = barPosition.Center.ToVector2() + textOffset;
                RenderTimeElapsedText(baseDrawPosition);

                DynamicSpriteFont font = FontAssets.DeathText.Value;
                string text = "∞";
                Vector2 textSize = font.MeasureString(text);
                Utils.DrawBorderStringFourWay(Main.spriteBatch, font, text, baseDrawPosition.X, baseDrawPosition.Y + 8f, Color.White, Color.Black, textSize * 0.5f, 1f);
            }
            else if (shield > 0f)
                BigProgressBarHelper.DrawHealthText(spriteBatch, barPosition, textOffset, shield, shieldMax);
            else
                BigProgressBarHelper.DrawHealthText(spriteBatch, barPosition, textOffset, life, lifeMax);
        }
    }

    internal static void RenderTimeElapsedText(Vector2 center)
    {
        if (NamelessDeityBoss.Myself is not null)
        {
            int time = NamelessDeityBoss.Myself.As<NamelessDeityBoss>().FightTimer;
            int milliseconds = (int)(time % 60 * 16.66667f);
            int seconds = time / 60 % 60;
            int minutes = time / 3600 % 60;
            int hours = time / 216000;

            DynamicSpriteFont font = FontAssets.DeathText.Value;
            center.Y += 8f;

            TimeSpan timeElapsed = new TimeSpan(0, hours, minutes, seconds, milliseconds);

            string text = timeElapsed.ToString(@"mm\:ss\:ff", Language.ActiveCulture.CultureInfo.DateTimeFormat);
            if (hours >= 1)
                text = timeElapsed.ToString(@"hh\:mm\:ss\:ff", Language.ActiveCulture.CultureInfo.DateTimeFormat);

            Vector2 textSize = font.MeasureString(text);
            Utils.DrawBorderStringFourWay(Main.spriteBatch, font, text, center.X, center.Y - 28f, Color.White, Color.Black, textSize * 0.5f, 0.5f);
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
    {
        if (AvatarOfEmptiness.Myself is not null)
            return false;
        if (NamelessDeityBoss.Myself_CurrentState == NamelessDeityBoss.NamelessAIType.Awaken ||
            NamelessDeityBoss.Myself_CurrentState == NamelessDeityBoss.NamelessAIType.OpenScreenTear)
        {
            return false;
        }

        DrawDataLastFrame = drawParams;

        Main.spriteBatch.PrepareForShaders(null, true);

        Vector2 barDrawPosition = drawParams.BarCenter;
        if (TestOfResolveSystem.IsActive)
            barDrawPosition += Main.rand.NextVector2Circular(4f, 3f);

        ManagedShader barShader = ShaderManager.GetShader("NoxusBoss.NamelessBossBarShader");
        barShader.TrySetParameter("textureSize", BarTarget.Size());
        barShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * (TestOfResolveSystem.IsActive ? 5f : 0.8f));
        barShader.TrySetParameter("chromaticAberrationOffset", ScreenShakeSystem.OverallShakeIntensity * 0.3f + TestOfResolveSystem.IsActive.ToInt() * 0.3f);
        barShader.SetTexture(PerlinNoise, 1, SamplerState.LinearWrap);
        barShader.Apply();

        Main.spriteBatch.Draw(BarTarget, barDrawPosition, null, Color.White, 0f, BarTarget.Size() * 0.5f, 1f, 0, 0f);
        Main.spriteBatch.ResetToDefaultUI();

        // Icon.
        Point topLeftOffset = new Point(32, 24);
        Vector2 barTopLeft = barDrawPosition - new Vector2(228f, barSize.Y * 0.5f);
        Vector2 topLeft = barTopLeft - topLeftOffset.ToVector2();

        Texture2D iconTexture = TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<NamelessDeityBoss>()]].Value;
        Rectangle iconFrame = iconTexture.Frame();
        Vector2 iconOffset = new Vector2(4f, 20f);
        Vector2 iconSize = new Vector2(26f, 28f);
        Vector2 iconPosition = iconOffset + iconSize * 0.5f;
        spriteBatch.Draw(iconTexture, topLeft + iconPosition, iconFrame, drawParams.IconColor, 0f, iconFrame.Size() / 2f, drawParams.IconScale, 0, 0f);

        return false;
    }
}
