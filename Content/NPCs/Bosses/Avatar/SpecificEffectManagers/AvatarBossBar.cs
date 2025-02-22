using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Core.Data;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;

public class AvatarBossBar : ModBossBar
{
    public override string Texture => GetAssetPath("Content/NPCs/Bosses/Avatar/SecondPhaseForm", "BossBar");

    public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
    {
        if (npc.type == ModContent.NPCType<AvatarRift>() && npc.As<AvatarRift>().HideBar)
            return false;
        if (npc.type == ModContent.NPCType<AvatarOfEmptiness>() && npc.As<AvatarOfEmptiness>().ParadiseReclaimedIsOngoing)
            return false;
        if (npc.type == ModContent.NPCType<AvatarOfEmptiness>() && npc.As<AvatarOfEmptiness>().HideBar)
            return false;

        (Texture2D barTexture, Vector2 barCenter, _, _, Color iconColor, float life, float lifeMax, float shield, float shieldMax, float iconScale, bool showText, Vector2 textOffset) = drawParams;
        Texture2D iconTexture = npc.type == ModContent.NPCType<AvatarRift>() ? AvatarRiftMapIconLayer.RiftIconTarget : TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[npc.type]].Value;
        Rectangle iconFrame = iconTexture.Frame();

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

    private void DrawBar(NPC npc, Texture2D barTexture, Vector2 barTopLeft, Rectangle barFrame, float scale)
    {
        float huePower = 4.3f;
        string paletteFilePath = $"{this.GetModRelativeDirectory()}Palettes.json";
        float paletteSwapInterpolant = 0f;
        Dictionary<string, Vector3[]> palettes = LocalDataManager.Read<Vector3[]>(paletteFilePath);
        Vector3[] phase1Palette = palettes["Phase1"];
        Vector3[] phase2Palette = palettes["Phase2"];
        Vector3[] palette = new Vector3[phase1Palette.Length];
        if (npc.type == ModContent.NPCType<AvatarOfEmptiness>())
            paletteSwapInterpolant = InverseLerp(0f, 180f, npc.As<AvatarOfEmptiness>().FightTimer);

        for (int i = 0; i < palette.Length; i++)
            palette[i] = Vector3.Lerp(phase1Palette[i], phase2Palette[i], paletteSwapInterpolant);

        huePower = Lerp(huePower, 2.9f, paletteSwapInterpolant);

        ManagedShader barShader = ShaderManager.GetShader("NoxusBoss.AvatarBossBarShader");
        barShader.TrySetParameter("gradient", palette);
        barShader.TrySetParameter("gradientCount", palette.Length);
        barShader.TrySetParameter("huePower", huePower);
        barShader.TrySetParameter("imageSize", barTexture.Size());
        barShader.TrySetParameter("horizontalSquish", npc.life / (float)npc.lifeMax);
        barShader.TrySetParameter("sourceRectangle", new Vector4(barFrame.X, barFrame.Y, barFrame.Width, barFrame.Height));
        barShader.Apply();

        // Bar.
        Vector2 stretchScale = new Vector2(scale / barFrame.Width, 1f);
        Main.spriteBatch.Draw(barTexture, barTopLeft, barFrame, Color.White, 0f, Vector2.Zero, stretchScale, 0, 0f);
    }
}
