using Luminance.Core.Cutscenes;
using Terraria.Graphics;

namespace NoxusBoss.Core.World.GameScenes.SolynEventHandlers;

public class SolynEnteringRiftScene : Cutscene
{
    public override int CutsceneLength => SecondsToFrames(7f);

    public override void OnEnd()
    {
    }

    public override void Update()
    {

    }

    public override void ModifyTransformMatrix(ref SpriteViewMatrix transform)
    {
        float zoomInInterpolant = Pow(LifetimeRatio, 0.65f);
        float zoomFactor = Lerp(1f, 1.2f, zoomInInterpolant);
        transform.Zoom *= zoomFactor;
    }
}
