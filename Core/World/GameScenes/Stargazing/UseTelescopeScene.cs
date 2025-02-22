using Luminance.Core.Cutscenes;
using Microsoft.Xna.Framework;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using Terraria.Graphics;

namespace NoxusBoss.Core.World.GameScenes.Stargazing;

public class UseTelescopeScene : Cutscene
{
    public override int CutsceneLength => SecondsToFrames(1.1f);

    public override void OnEnd()
    {
        StargazingScene.SolynIsPresent = true;
        StargazingScene.IsActive = true;
    }

    public override void Update()
    {
        TotalScreenOverlaySystem.OverlayInterpolant = InverseLerp(0.7f, 0.85f, LifetimeRatio);
        if (TotalScreenOverlaySystem.OverlayInterpolant > 0f)
            TotalScreenOverlaySystem.OverlayColor = Color.Black;
    }

    public override void ModifyTransformMatrix(ref SpriteViewMatrix transform)
    {
        float zoomInInterpolant = Pow(LifetimeRatio, 0.65f);
        float zoomFactor = Lerp(1f, 2.3f, zoomInInterpolant);
        transform.Zoom *= zoomFactor;
    }
}
