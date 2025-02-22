using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.Physics.VerletIntergration;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;

public class DanglingVinesStep : INamelessDeityRenderStep
{
    public int LayerIndex => 10;

    public NamelessDeityRenderComposite Composite
    {
        get;
        set;
    }

    /// <summary>
    /// The left vine's underlying rope.
    /// </summary>
    public VerletSimulatedRope LeftVine
    {
        get;
        set;
    }

    /// <summary>
    /// The right vine's underlying rope.
    /// </summary>
    public VerletSimulatedRope RightVine
    {
        get;
        set;
    }

    /// <summary>
    /// The texture of the vines.
    /// </summary>
    public NamelessDeitySwappableTexture VinesTexture
    {
        get;
        private set;
    }

    public void Initialize()
    {
        VinesTexture = Composite.RegisterSwappableTexture("Vines", 5, Composite.UsedPreset?.Data.PreferredVineTextures).WithAutomaticSwapRule(() =>
        {
            return Composite.Time % 150 == 0;
        });
    }

    /// <summary>
    /// Updates the rotations of the vines.
    /// </summary>
    /// <param name="owner">The owner of the vines.</param>
    public void HandleDanglingVineRotation(Entity owner)
    {
        Vector2 vineOrigin = owner.Center + Vector2.UnitY * 8f;
        Vector2 vineSpacing = Vector2.UnitX * 90f;
        Vector2 leftVinePosition = vineOrigin - vineSpacing;
        Vector2 rightVinePosition = vineOrigin + vineSpacing;

        LeftVine ??= new(leftVinePosition, Vector2.Zero, 6, 700f);
        RightVine ??= new(rightVinePosition, Vector2.Zero, 6, 700f);
        List<VerletSimulatedSegment> leftVineRope = LeftVine.Rope;
        List<VerletSimulatedSegment> rightVineRope = RightVine.Rope;

        // Apply forces to the end of the vines over time.
        Vector2 shakeForce = (Main.GlobalTimeWrappedHourly * 40f).ToRotationVector2() * Clamp(ScreenShakeSystem.OverallShakeIntensity * 2f, 0f, 20f);
        leftVineRope[^3].Position.X += Sin(Main.GlobalTimeWrappedHourly * 2.3f) * 12f - 3.2f;
        rightVineRope[^3].Position.X += Sin(Main.GlobalTimeWrappedHourly * 2.4f + 1.887f) * 12f - 3.2f;
        leftVineRope[^3].Position += shakeForce;
        rightVineRope[^3].Position += shakeForce;

        // Update the vines.
        float vineGravity = Main.LocalPlayer.gravDir * 0.65f;
        LeftVine.Update(leftVinePosition, vineGravity);
        RightVine.Update(rightVinePosition, vineGravity);
    }

    public void Render(Entity owner, Vector2 drawCenter)
    {
        float opacity = 1f;
        if (owner is NPC npc)
            opacity = npc.Opacity;
        else if (owner is Projectile projectile)
            opacity = projectile.Opacity;

        HandleDanglingVineRotation(owner);

        // Draw the vines swaying as Nameless moves.
        Texture2D vine = VinesTexture.UsedTexture;

        // Draw each dangling part separately.
        Vector2 drawOffset = -owner.Center + new Vector2(Main.instance.GraphicsDevice.Viewport.Width * 0.5f, Main.instance.GraphicsDevice.Viewport.Height * 0.5f);
        drawOffset.Y += 120f;

        (int oldScreenWidth, int oldScreenHeight) = (Main.screenWidth, Main.screenHeight);
        (Main.screenWidth, Main.screenHeight) = (Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);
        LeftVine?.DrawProjection(vine, drawOffset, false, _ => Color.White * opacity, Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height, unscaledMatrix: true);
        RightVine?.DrawProjection(vine, drawOffset, true, _ => Color.White * opacity, Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height, unscaledMatrix: true);
        (Main.screenWidth, Main.screenHeight) = (oldScreenWidth, oldScreenHeight);
    }
}
