using Microsoft.Xna.Framework;

namespace NoxusBoss.Core.Graphics.TentInterior.Cutscenes;

public class WhoSolynIsCutscene : SolynTentVisualCutscene
{
    public override int StandardDuration => SecondsToFrames(99999f);

    public override void Render()
    {
        /*
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, SubtractiveBlending);

        float fadeIn = InverseLerp(0f, 150f, Time);
        float scale = EasingCurves.Quintic.Evaluate(EasingType.InOut, 0f, 2.1f, InverseLerp(0f, 0.7f, fadeIn));
        float rotation = 0.16f;
        float glowOpacity = 0.15f;
        Vector2 constellationPosition = ViewportSize * 0.5f - Vector2.UnitY * 350f;
        for (int i = 0; i < 5; i++)
        {
            float glowScale = scale * Lerp(0.17f, 0.21f, i / 4f);
            Texture2D constellationBackglow = GennedAssets.Textures.NamelessDeity.NamelessDeityEyeFull;
            Vector2 glowPosition = constellationPosition + new Vector2(-5f, -70f).RotatedBy(rotation) * scale;
            Main.spriteBatch.Draw(constellationBackglow, glowPosition, null, Color.White * glowOpacity, rotation, constellationBackglow.Size() * 0.5f, glowScale, 0, 0f);
        }

        Main.spriteBatch.End();
        Main.spriteBatch.Begin();

        ConstellationsRegistry.Constellations["XerocEye"].Render(constellationPosition, scale, fadeIn, rotation);
        */

        int relativeTime = Time - 135;
        int textFadeInTime = 124;
        int textLingerTime = 150;
        int textFadeOutTime = 60;
        int animationTime = textFadeInTime + textLingerTime + textFadeOutTime;
        float textScale = 0.93f;
        Vector2 drawPosition = new Vector2(0.5f, 0.3f);

        float opacity = InverseLerpBump(0f, textFadeInTime, textFadeInTime + textLingerTime, animationTime, relativeTime);
        RenderText("Mods.NoxusBoss.Dialog.SolynBackstory1", DialogColorRegistry.SolynTextColor * opacity, textScale, drawPosition);

        relativeTime -= animationTime;
        opacity = InverseLerpBump(0f, textFadeInTime, textFadeInTime + textLingerTime, animationTime, relativeTime);
        RenderText("Mods.NoxusBoss.Dialog.SolynBackstory2", DialogColorRegistry.SolynTextColor * opacity, textScale, drawPosition);
        relativeTime -= (int)(animationTime * 0.67f);
        opacity = InverseLerpBump(0f, textFadeInTime, textFadeInTime + textLingerTime, animationTime, relativeTime);
        RenderText("Mods.NoxusBoss.Dialog.SolynBackstory3", DialogColorRegistry.SolynTextColor * opacity, textScale, drawPosition + Vector2.UnitY * 0.05f);

        relativeTime -= animationTime;
        opacity = InverseLerpBump(0f, textFadeInTime, textFadeInTime + textLingerTime, animationTime, relativeTime);
        RenderText("Mods.NoxusBoss.Dialog.SolynBackstory4", DialogColorRegistry.SolynTextColor * opacity, textScale, drawPosition);
        relativeTime -= animationTime / 2;
        opacity = InverseLerpBump(0f, textFadeInTime, textFadeInTime + textLingerTime, animationTime, relativeTime);
        RenderText("Mods.NoxusBoss.Dialog.SolynBackstory5", DialogColorRegistry.SolynTextColor * opacity, textScale, drawPosition + Vector2.UnitY * 0.05f);

        relativeTime -= animationTime;
        opacity = InverseLerpBump(0f, textFadeInTime, textFadeInTime + textLingerTime, animationTime, relativeTime);
        RenderText("Mods.NoxusBoss.Dialog.SolynBackstory6", DialogColorRegistry.SolynTextColor * opacity, textScale, drawPosition);

        relativeTime -= animationTime;
        opacity = InverseLerpBump(0f, textFadeInTime, textFadeInTime + textLingerTime, animationTime, relativeTime);
        RenderText("Mods.NoxusBoss.Dialog.SolynBackstory7", DialogColorRegistry.SolynTextColor * opacity, textScale, drawPosition);

        relativeTime -= animationTime;
        opacity = InverseLerpBump(0f, textFadeInTime, textFadeInTime + textLingerTime, animationTime, relativeTime);
        RenderText("Mods.NoxusBoss.Dialog.SolynBackstory8", DialogColorRegistry.SolynTextColor * opacity, textScale, drawPosition);

        relativeTime -= animationTime;
        opacity = InverseLerpBump(0f, textFadeInTime, textFadeInTime + textLingerTime, animationTime, relativeTime);
        RenderText("Mods.NoxusBoss.Dialog.SolynBackstory9", DialogColorRegistry.SolynTextColor * opacity, textScale, drawPosition);
    }
}
