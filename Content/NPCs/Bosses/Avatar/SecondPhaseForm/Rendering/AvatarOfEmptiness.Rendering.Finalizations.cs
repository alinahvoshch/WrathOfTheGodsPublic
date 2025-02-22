using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.FormPresets;
using NoxusBoss.Core.Graphics.RenderTargets;
using Terraria;
using Terraria.ID;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    internal static InstancedRequestableTarget FinalRenderTarget;

    /// <summary>
    /// An optional post-processing action that should be performed on the Avatar.
    /// </summary>
    public Action? PostProcessingAction
    {
        get;
        set;
    }

    /// <summary>
    /// Whether the Avatar should use his final render target for post-processing purposes.
    /// </summary>
    public bool UsingFinalTarget => PostProcessingAction is not null;

    internal void LoadTargets_Final()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        FinalRenderTarget = new InstancedRequestableTarget();
        Main.ContentThatNeedsRenderTargets.Add(FinalRenderTarget);
    }

    private void ProcessTargets_Final()
    {
        Vector2 targetSize = new Vector2(TargetDownscaleFactor * 4200f);
        FinalRenderTarget.Request((int)targetSize.X, (int)targetSize.Y, RenderTargetIdentifier, () =>
        {
            if (!TargetsShouldBeProcessed || !UsingFinalTarget)
                return;

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            Vector2 drawOffset = NPC.Center - targetSize * 0.5f;
            DrawFinalParts(drawOffset, targetSize * 0.5f);
            Main.spriteBatch.End();
        });
    }

    private void DrawFinalParts(Vector2 drawOffset, Vector2 shadowPartCenter)
    {
        RenderRift(drawOffset);
        RenderShadowPartsFromTarget(shadowPartCenter);

        Matrix transform = NPC.IsABestiaryIconDummy ? Main.UIScaleMatrix : Main.GameViewMatrix.TransformationMatrix;
        if (UsingFinalTarget)
            transform = Matrix.Identity;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, CullOnlyScreen, null, transform);
        RenderBodyWithPostProcessing(drawOffset);
    }

    private void DefinePostProcessingStep()
    {
        PostProcessingAction = null;
        if (AvatarFormPresetRegistry.UsingMoonburnPreset)
            PostProcessingAction = PostProcess_MoonburnPreset;
    }

    private static void PostProcess_MoonburnPreset()
    {
        float[] blurWeights = new float[7];
        for (int i = 0; i < blurWeights.Length; i++)
            blurWeights[i] = GaussianDistribution(i - blurWeights.Length * 0.5f, 2.475f) / 7f;

        ManagedShader blueShader = ShaderManager.GetShader("NoxusBoss.MoonburnOverlayShader");
        blueShader.TrySetParameter("blurWeights", blurWeights);
        blueShader.TrySetParameter("blurOffset", 0.00018f);
        blueShader.TrySetParameter("additiveColor", new Vector3(-1f, 0.2f, 1f));
        blueShader.TrySetParameter("biasRedTowards", new Vector3(0f, 0f, 1f));
        blueShader.Apply();
    }

    private void RenderFromFinalTarget(Vector2 screenPos)
    {
        DefinePostProcessingStep();

        if (!UsingFinalTarget)
        {
            DrawFinalParts(screenPos, NPC.Center - screenPos);
            Main.spriteBatch.ResetToDefault();
            return;
        }

        if (!FinalRenderTarget.TryGetTarget(RenderTargetIdentifier, out RenderTarget2D? target) || target is null)
            return;

        Main.spriteBatch.PrepareForShaders(null, NPC.IsABestiaryIconDummy);
        PostProcessingAction?.Invoke();
        Main.spriteBatch.Draw(target, NPC.Center - screenPos, null, Color.White, 0f, target.Size() * 0.5f, 1f, 0, 0f);
        Main.spriteBatch.ResetToDefault();
    }
}
