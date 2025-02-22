using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon.SpecificEffectManagers;

public class MarsBossBar : ModBossBar
{
    public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
    {
        (Texture2D barTexture, Vector2 barCenter, _, _, Color iconColor, float life, float lifeMax, float shield, float shieldMax, float iconScale, bool showText, Vector2 textOffset) = drawParams;
        Texture2D iconTexture = TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[npc.type]].Value;
        Rectangle iconFrame = iconTexture.Frame();

        int forcefieldIndex = NPC.FindFirstNPC(ModContent.NPCType<TrappingHolographicForcefield>());
        NPC? forcefield = null;
        if (forcefieldIndex != -1)
        {
            forcefield = Main.npc[forcefieldIndex];
            life = forcefield.life;
            lifeMax = forcefield.lifeMax;
        }

        Point barSize = new Point(456, 22);
        Point topLeftOffset = new Point(32, 24);
        int frameCount = 6;

        Rectangle bgFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 3);
        Color bgColor = Color.White * 0.2f;

        int scale = (int)(barSize.X * life / lifeMax);
        scale -= scale % 2;
        Rectangle barFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 2);
        barFrame.X += topLeftOffset.X;
        barFrame.Y += topLeftOffset.Y;
        barFrame.Width = 2;
        barFrame.Height = barSize.Y;

        int shieldScale = (int)(barSize.X * shield / shieldMax);
        shieldScale -= shieldScale % 2;

        Rectangle barShieldFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 5);
        barShieldFrame.X += topLeftOffset.X;
        barShieldFrame.Y += topLeftOffset.Y;
        barShieldFrame.Width = 2;
        barShieldFrame.Height = barSize.Y;

        Rectangle tipShieldFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 4);
        tipShieldFrame.X += topLeftOffset.X;
        tipShieldFrame.Y += topLeftOffset.Y;
        tipShieldFrame.Width = 2;
        tipShieldFrame.Height = barSize.Y;

        Rectangle barPosition = Utils.CenteredRectangle(barCenter, barSize.ToVector2());
        Vector2 barTopLeft = barPosition.TopLeft();
        Vector2 topLeft = barTopLeft - topLeftOffset.ToVector2();

        // Background.
        spriteBatch.Draw(barTexture, topLeft, bgFrame, bgColor, 0f, Vector2.Zero, 1f, 0, 0f);

        Main.spriteBatch.PrepareForShaders(null, true);
        if (forcefield is not null)
            DrawForcefieldBar(forcefield, barTexture, barTopLeft, barFrame, scale);
        else
            DrawBar(npc, barTexture, barTopLeft, barFrame, scale);
        Main.spriteBatch.ResetToDefaultUI();

        // Bar itself (shield).
        if (shield > 0f)
        {
            Vector2 stretchScale = new Vector2(shieldScale / barFrame.Width, 1f);
            spriteBatch.Draw(barTexture, barTopLeft, barShieldFrame, Color.White, 0f, Vector2.Zero, stretchScale, 0, 0f);
            spriteBatch.Draw(barTexture, barTopLeft + new Vector2(shieldScale - 2, 0f), tipShieldFrame, Color.White, 0f, Vector2.Zero, 1f, 0, 0f);
        }

        // Frame.
        Rectangle frameFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 0);
        spriteBatch.Draw(barTexture, topLeft, frameFrame, Color.White, 0f, Vector2.Zero, 1f, 0, 0f);

        // Icon.
        Vector2 iconOffset = new Vector2(4f, 20f);
        Vector2 iconSize = new Vector2(26f, 28f);
        Vector2 iconPosition = iconOffset + iconSize * 0.5f;
        spriteBatch.Draw(iconTexture, topLeft + iconPosition, iconFrame, iconColor, 0f, iconFrame.Size() / 2f, iconScale, 0, 0f);

        // Health text.
        if (BigProgressBarSystem.ShowText && showText)
        {
            if (shield > 0f)
                BigProgressBarHelper.DrawHealthText(spriteBatch, barPosition, textOffset, shield, shieldMax);
            else
                BigProgressBarHelper.DrawHealthText(spriteBatch, barPosition, textOffset, life, lifeMax);
        }
        return false;
    }

    private static void DrawForcefieldBar(NPC npc, Texture2D barTexture, Vector2 barTopLeft, Rectangle barFrame, float scale)
    {
        ManagedShader barShader = ShaderManager.GetShader("NoxusBoss.MarsForcefieldBossBarShader");
        barShader.TrySetParameter("imageSize", barTexture.Size());
        barShader.TrySetParameter("sourceRectangle", new Vector4(barFrame.X, barFrame.Y, barFrame.Width, barFrame.Height));
        barShader.SetTexture(HexagonalLattice, 1, SamplerState.LinearWrap);
        barShader.Apply();

        Vector2 stretchScale = new Vector2(scale / barFrame.Width, 1f);
        Main.spriteBatch.Draw(barTexture, barTopLeft, barFrame, new Color(120, 199, 255), 0f, Vector2.Zero, stretchScale, 0, 0f);
    }

    private static void DrawBar(NPC npc, Texture2D barTexture, Vector2 barTopLeft, Rectangle barFrame, float scale)
    {
        float impactInterpolant = npc.As<MarsBody>().HealthBarImpactVisualInterpolant;
        Color energyColorA = new Color(250, 205, 112);
        Color energyColorB = new Color(255, 33, 10);
        Color metalColor = new Color(0, 0, 20);

        energyColorA = Color.Lerp(energyColorA, Color.White, impactInterpolant);
        energyColorB = Color.Lerp(energyColorB, Color.White, impactInterpolant);
        metalColor = Color.Lerp(metalColor, Color.White, impactInterpolant);

        ManagedShader barShader = ShaderManager.GetShader("NoxusBoss.MarsBossBarShader");
        barShader.TrySetParameter("energyColorA", energyColorA.ToVector3());
        barShader.TrySetParameter("energyColorB", energyColorB.ToVector3());
        barShader.TrySetParameter("metalColor", metalColor.ToVector3());
        barShader.TrySetParameter("imageSize", barTexture.Size());
        barShader.TrySetParameter("horizontalSquish", npc.life / (float)npc.lifeMax);
        barShader.TrySetParameter("sourceRectangle", new Vector4(barFrame.X, barFrame.Y, barFrame.Width, barFrame.Height));
        barShader.SetTexture(WatercolorNoiseA, 1, SamplerState.LinearWrap);
        barShader.Apply();

        Vector2 stretchScale = new Vector2(scale / barFrame.Width, 1f);
        Main.spriteBatch.Draw(barTexture, barTopLeft, barFrame, Color.White, 0f, Vector2.Zero, stretchScale, 0, 0f);
    }
}
