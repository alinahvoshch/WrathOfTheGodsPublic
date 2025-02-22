using Luminance.Common.Easings;
using Microsoft.Xna.Framework;
using Terraria;

namespace NoxusBoss.Core.Graphics.TentInterior.Cutscenes;

public class IdealizedFutureCutscene : SolynTentVisualCutscene
{
    public override int StandardDuration => SecondsToFrames(99999f);

    public override void Render()
    {
        EasingCurves.Curve animationCurve = EasingCurves.Quartic;

        int relativeTime = Time - 60;

        int sunAppearTime = 105;
        int riftAppearDelay = 60;
        int riftAppearTime = 90;
        int seedAppearDelay = 25;
        int seedAppearTime = 132;
        int seedAnimationTime = seedAppearDelay + seedAppearTime;
        int glowAppearTime = 54;
        int glowLingerTime = 120;
        int glowFadeOutTime = 90;
        int namelessAppearDelay = 50;
        float riftAppearInterpolant = animationCurve.Evaluate(EasingType.InOut, InverseLerp(0f, riftAppearTime, relativeTime - sunAppearTime - riftAppearDelay));
        float sunAppearInterpolant = animationCurve.Evaluate(EasingType.InOut, InverseLerp(0f, sunAppearTime, relativeTime)) * (1 - riftAppearInterpolant);
        float constellationFadeOut = InverseLerp(20f, 0f, relativeTime - sunAppearTime - riftAppearDelay - riftAppearTime - seedAnimationTime * 3 - 300 - glowAppearTime);
        Vector2 sunCenter = ViewportSize * new Vector2(0.5f, 0.18f);
        DrawConstellation("SymbolicSun", sunCenter, sunAppearInterpolant * constellationFadeOut, 0f, Vector2.One * 400f);
        DrawConstellation("SymbolicRift", sunCenter, riftAppearInterpolant * constellationFadeOut, Main.GlobalTimeWrappedHourly * 0.08f, Vector2.One * 400f, 0.02f);

        relativeTime -= sunAppearTime + riftAppearDelay + riftAppearTime + 60;

        // Render the antiseed.
        float firstSeedAppearInterpolant = animationCurve.Evaluate(EasingType.InOut, InverseLerp(seedAppearDelay, seedAppearDelay + seedAppearTime, relativeTime));
        Vector2 firstSeedCenter = ViewportSize * new Vector2(0.25f, 0.5f) + Vector2.UnitY * 60f;
        DrawConstellation("TheAntiseed", firstSeedCenter, firstSeedAppearInterpolant * constellationFadeOut, 0f, new Vector2(130f, 400f));

        // Then the synthetic seedling.
        float secondSeedAppearInterpolant = animationCurve.Evaluate(EasingType.InOut, InverseLerp(seedAppearDelay, seedAppearDelay + seedAppearTime, relativeTime - seedAnimationTime));
        Vector2 secondSeedCenter = ViewportSize * new Vector2(0.5f, 0.7f);
        DrawConstellation("SyntheticSeedling", secondSeedCenter, secondSeedAppearInterpolant * constellationFadeOut, 0f, Vector2.One * 400f);

        // And then the blazing bud.
        float thirdSeedInterpolant = animationCurve.Evaluate(EasingType.InOut, InverseLerp(seedAppearDelay, seedAppearDelay + seedAppearTime, relativeTime - seedAnimationTime * 2f));
        Vector2 thirdSeedCenter = ViewportSize * new Vector2(0.75f, 0.5f);
        DrawConstellation("BlazingBud", thirdSeedCenter, thirdSeedInterpolant * constellationFadeOut, 0f, new Vector2(250f, 420f));

        relativeTime -= seedAnimationTime * 3;

        float textMaterializeInterpolant = Sqrt(EasingCurves.Quintic.Evaluate(EasingType.InOut, InverseLerpBump(45f, 150f, 210f, 255f, relativeTime)));
        RenderText("Mods.NoxusBoss.Dialog.DarkRiftVaniquishText", DialogColorRegistry.SolynTextColor * textMaterializeInterpolant);

        relativeTime -= 285;

        float glowScale = SmoothStep(0f, 75f, InverseLerp(0f, glowAppearTime, relativeTime));
        float glowOpacity = InverseLerp(glowFadeOutTime, 0f, relativeTime - glowLingerTime);
        for (int i = 0; i < 2; i++)
            Main.spriteBatch.Draw(BloomCircleSmall, ViewportSize * 0.5f, null, Color.White with { A = 0 } * glowOpacity, 0f, BloomCircleSmall.Size() * 0.5f, glowScale, 0, 0f);

        relativeTime -= glowLingerTime + glowFadeOutTime;

        // Render Nameless' eye.
        float eyeOpacity = InverseLerp(540f, 360f, relativeTime - namelessAppearDelay);
        float eyeInterpolant = animationCurve.Evaluate(EasingType.InOut, InverseLerp(0f, 120f, relativeTime - namelessAppearDelay) * eyeOpacity);
        float eyeScale = eyeInterpolant.Squared() * 0.75f + Clamp(relativeTime - namelessAppearDelay - 25f, 0f, 10000f) * 0.00091f;
        Vector2 eyeCenter = ViewportSize * new Vector2(0.5f, 0.3f);
        DrawConstellation("XerocEye", eyeCenter, eyeInterpolant, 0f, new Vector2(840f, 1000f) * eyeScale, 0.09f, eyeOpacity);

        BackgroundFadeOut = 1f - Sqrt(eyeOpacity);

        if (BackgroundFadeOut >= 1f)
            End();
    }
}
