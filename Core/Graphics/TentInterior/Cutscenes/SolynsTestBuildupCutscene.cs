using Microsoft.Xna.Framework;

namespace NoxusBoss.Core.Graphics.TentInterior.Cutscenes;

public class SolynsTestBuildupCutscene : SolynTentVisualCutscene
{
    public override int StandardDuration => SecondsToFrames(99999f);

    public override void Render()
    {
        int relativeTime = Time - 135;
        int textFadeInTime = 124;
        int textLingerTime = 150;
        int textFadeOutTime = 60;
        int animationTime = textFadeInTime + textLingerTime + textFadeOutTime;
        float textScale = 0.85f;
        Vector2 drawPosition = new Vector2(0.5f, 0.3f);

        float opacity = InverseLerpBump(0f, textFadeInTime, textFadeInTime + textLingerTime, animationTime, relativeTime);
        RenderText("Mods.NoxusBoss.Dialog.SolynTestBuildup1", DialogColorRegistry.SolynTextColor * opacity, textScale, drawPosition);

        relativeTime -= animationTime;
        opacity = InverseLerpBump(0f, textFadeInTime, textFadeInTime + textLingerTime, animationTime, relativeTime);
        RenderText("Mods.NoxusBoss.Dialog.SolynTestBuildup2", DialogColorRegistry.SolynTextColor * opacity, textScale, drawPosition);

        relativeTime -= animationTime;
        opacity = InverseLerpBump(0f, textFadeInTime, textFadeInTime + textLingerTime, animationTime, relativeTime);
        RenderText("Mods.NoxusBoss.Dialog.SolynTestBuildup3", DialogColorRegistry.SolynTextColor * opacity, textScale, drawPosition);

        relativeTime -= animationTime;
        opacity = InverseLerpBump(0f, textFadeInTime, textFadeInTime + textLingerTime, animationTime, relativeTime);
        RenderText("Mods.NoxusBoss.Dialog.SolynTestBuildup4", DialogColorRegistry.SolynTextColor * opacity, textScale, drawPosition);
        relativeTime -= (int)(animationTime * 0.56f);
        opacity = InverseLerpBump(0f, textFadeInTime, textFadeInTime + textLingerTime, animationTime, relativeTime);
        RenderText("Mods.NoxusBoss.Dialog.SolynTestBuildup5", DialogColorRegistry.SolynTextColor * opacity, textScale, drawPosition + Vector2.UnitY * 0.05f);

        relativeTime -= animationTime;
        opacity = InverseLerpBump(0f, textFadeInTime, textFadeInTime + textLingerTime, animationTime, relativeTime);
        RenderText("Mods.NoxusBoss.Dialog.SolynTestBuildup6", DialogColorRegistry.SolynTextColor * opacity, textScale, drawPosition);

        relativeTime -= animationTime;
        opacity = InverseLerpBump(0f, textFadeInTime, textFadeInTime + textLingerTime, animationTime, relativeTime);
        RenderText("Mods.NoxusBoss.Dialog.SolynTestBuildup7", DialogColorRegistry.SolynTextColor * opacity, textScale, drawPosition);

        relativeTime -= animationTime;
        opacity = InverseLerpBump(0f, textFadeInTime, textFadeInTime + textLingerTime, animationTime, relativeTime);
        RenderText("Mods.NoxusBoss.Dialog.SolynTestBuildup8", DialogColorRegistry.SolynTextColor * opacity, textScale, drawPosition);
        relativeTime -= (int)(animationTime * 0.54f);
        opacity = InverseLerpBump(0f, textFadeInTime, textFadeInTime + textLingerTime, animationTime, relativeTime);
        RenderText("Mods.NoxusBoss.Dialog.SolynTestBuildup9", DialogColorRegistry.SolynTextColor * opacity, textScale, drawPosition + Vector2.UnitY * 0.04f);
    }
}
