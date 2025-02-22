using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using NoxusBoss.Core.DataStructures.ShapeCurves;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;

public abstract class ConstellationProjectile : ModProjectile
{
    private int convergeTime;

    /// <summary>
    /// A cached value of <see cref="constellationShape"/>.
    /// </summary>
    /// <remarks>
    /// This stores the constellationShape property in a field for performance reasons every frame, since the underlying getter method used there can be straining when done
    /// many times per frame, due to looping.
    /// </remarks>
    public ShapeCurve ConstellationShape;

    /// <summary>
    /// The shape curve that composes the constellation's shape.
    /// </summary>
    protected abstract ShapeCurve constellationShape
    {
        get;
    }

    /// <summary>
    /// How long it takes for the constellation to completely converge.
    /// </summary>
    public abstract int ConvergeTime
    {
        get;
    }

    /// <summary>
    /// The factor by which stars are randomly offset from the positions of the points in the <see cref="constellationShape"/>.
    /// </summary>
    public abstract float StarRandomOffsetFactor
    {
        get;
    }

    /// <summary>
    /// This determines how much the star draw loop is incremented. Higher values result in better efficiency but less detail.
    /// </summary>
    /// <remarks>
    /// By default his can typically be set to 1 without issue.
    /// </remarks>
    public abstract int StarDrawIncrement
    {
        get;
    }

    /// <summary>
    /// The animation time factor for star convergence.
    /// </summary>
    public abstract float StarConvergenceSpeed
    {
        get;
    }

    /// <summary>
    /// Decides the primary bloom flare color.
    /// </summary>
    /// <param name="colorVariantInterpolant">The color variant interpolant.</param>
    public abstract Color DecidePrimaryBloomFlareColor(float colorVariantInterpolant);

    /// <summary>
    /// Decides the secondary bloom flare color.
    /// </summary>
    /// <param name="colorVariantInterpolant">The color variant interpolant.</param>
    public abstract Color DecideSecondaryBloomFlareColor(float colorVariantInterpolant);

    /// <summary>
    /// The scale factor of the stars that compose the shape.
    /// </summary>
    public virtual float StarScaleFactor => Utils.Remap(Time, 0f, ConvergeTime, 0.54f, 2.6f);

    /// <summary>
    /// The amount of time, in frames, it has been since the constellation was created.
    /// </summary>
    public ref float Time => ref Projectile.localAI[1];

    public override string Texture => "Terraria/Images/Extra_89";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 15;
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
    }

    public override void AI()
    {
        // Die if Nameless is not present.
        if (NamelessDeityBoss.Myself is null)
        {
            Projectile.Kill();
            return;
        }

        if (convergeTime == 0)
            convergeTime = ConvergeTime;

        // Store the constellation shape.
        ConstellationShape = constellationShape;

        // Increment the time.
        if (Projectile.IsFinalExtraUpdate())
            Time++;
    }

    /// <summary>
    /// Gets the movement interpolant from scattered to focused for a given star.
    /// </summary>
    /// <param name="index">The star's index.</param>
    public float GetStarMovementInterpolant(int index)
    {
        int starPrepareStartTime = (int)(index * convergeTime * StarConvergenceSpeed) + 10;
        float interpolant = Pow(InverseLerp(starPrepareStartTime, starPrepareStartTime + 56f, Time), 0.41f);
        return SmoothStep(0f, 1f, SmoothStep(0f, 1f, interpolant));
    }

    /// <summary>
    /// Calculates a given star's draw position.
    /// </summary>
    /// <param name="index">The star's index.</param>
    public Vector2 GetStarPosition(int index)
    {
        // Calculate the seed for the starting spots of the stars. This is randomized based on both projectile index and star index, so it should be
        // pretty unique across the fight.
        ulong starSeed = (ulong)Projectile.identity * 113uL + (ulong)index * 602uL + 54uL;

        // Orient the stars in such a way that they come from the background in random spots.
        Vector2 starDirectionFromCenter = (ConstellationShape.ShapePoints[index] - ConstellationShape.Center).SafeNormalize(Vector2.UnitY);
        Vector2 randomOffset = new Vector2(Lerp(-1350f, 1350f, Utils.RandomFloat(ref starSeed)), Lerp(-920f, 920f, Utils.RandomFloat(ref starSeed)));
        Vector2 startingSpot = Main.ScreenSize.ToVector2() * 0.5f + starDirectionFromCenter * 500f + randomOffset;
        Vector2 starPosition = ConstellationShape.ShapePoints[index] + Projectile.Center - Main.screenPosition;

        // Apply a tiny, random offset to the star position.
        Vector2 starOffset = Lerp(-TwoPi, TwoPi, Utils.RandomFloat(ref starSeed)).ToRotationVector2() * Lerp(1.5f, 5.3f, Utils.RandomFloat(ref starSeed)) * StarRandomOffsetFactor;
        return Vector2.Lerp(startingSpot, starPosition, GetStarMovementInterpolant(index)) + starOffset;
    }

    // Refer to ConstellationProjectileRenderer.
    public override bool PreDraw(ref Color lightColor) => false;
}
