using System.Runtime.CompilerServices;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.Physics.VerletIntergration;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon;

public class AttachedRope
{
    private List<VerletSimulatedSegment> ropeSegments
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Rope.Rope;
    }

    /// <summary>
    /// The underlying physics rope that this rope uses.
    /// </summary>
    public readonly VerletSimulatedRope Rope;

    /// <summary>
    /// The starting offset of this rope relative to Mars .
    /// </summary>
    public Vector2 StartingOffset
    {
        get;
        set;
    }

    /// <summary>
    /// The ending offset of this rope relative to Mars.
    /// </summary>
    public Vector2 EndingOffset
    {
        get;
        set;
    }

    public AttachedRope(Vector2 startingOffset, Vector2 endingOffset)
    {
        StartingOffset = startingOffset;
        EndingOffset = endingOffset;
        int segmentCount = (int)(StartingOffset.Distance(EndingOffset) * 0.04f) + 10;
        Rope = new(Vector2.Zero, Vector2.Zero, segmentCount, startingOffset.Distance(endingOffset) * 1.6f);
    }

    /// <summary>
    /// Updates this rope, attaching it to a given NPC.
    /// </summary>
    /// <param name="owner">The owner NPC that this rope should be attached to.</param>
    /// <param name="gravity">The gravity applied to the rope.</param>
    /// <param name="attach">Whether this rope should be attached.</param>
    /// <param name="externalForce">External forces applied to the rope. Defaults to <see cref="Vector2.Zero"/>.</param>
    public void Update(NPC owner, float gravity, bool attach, Vector2 externalForce = default)
    {
        Vector2 startingPosition = owner.Center + StartingOffset.RotatedBy(owner.rotation) * owner.scale;
        Vector2 endingPosition = owner.Center + EndingOffset.RotatedBy(owner.rotation) * owner.scale;

        for (int i = 0; i < ropeSegments.Count; i++)
        {
            float ropeInterpolant = i / (float)(ropeSegments.Count - 1f);
            if (ropeSegments[i].Position.Length() <= 10f)
                ropeSegments[i].Position = ropeSegments[i].OldPosition = Vector2.Lerp(startingPosition, endingPosition, ropeInterpolant);
        }

        ropeSegments[0].Locked = attach;
        if (attach)
        {
            ropeSegments[0].Position = startingPosition;
            ropeSegments[0].OldPosition = startingPosition;
        }

        VerletSimulations.StandardVerletSimulation(ropeSegments, Rope.IdealRopeLength / ropeSegments.Count, externalForce + Vector2.UnitY * gravity, 25);

        for (int i = 1; i < 6; i++)
        {
            if (!attach)
                break;

            ropeSegments[^i].Position = endingPosition;
            ropeSegments[^i].OldPosition = endingPosition;
        }
    }

    /// <summary>
    /// Offsets all points on the rope.
    /// </summary>
    /// <param name="offset">The amount by which all rope points should be offset.</param>
    public void OffsetAllPoints(Vector2 offset)
    {
        for (int i = 0; i < ropeSegments.Count; i++)
            ropeSegments[i].Position += offset;
    }

    /// <summary>
    /// Renders this rope.
    /// </summary>
    /// <param name="colorFunction">The rope's color function.</param>
    /// <param name="affectedByLight">Whether the rope should be affected by natural lighting or not.</param>
    /// <param name="width">The overall width of the rope.</param>
    /// <param name="drawOffset">The draw offset of the rope.</param>
    /// <param name="ui">Whether this rope is being drawn in a UI context or not.</param>
    public void Render(Func<float, Color> colorFunction, bool affectedByLight, float width, Vector2 drawOffset, bool ui)
    {
        Color lightStart = Lighting.GetColor(ropeSegments.First().Position.ToTileCoordinates());
        Color lightEnd = Lighting.GetColor(ropeSegments.Last().Position.ToTileCoordinates());

        Main.instance.GraphicsDevice.BlendState = BlendState.NonPremultiplied;

        ManagedShader projectionShader = ShaderManager.GetShader("NoxusBoss.PrimitiveProjection");
        projectionShader.TrySetParameter("horizontalFlip", false);
        projectionShader.TrySetParameter("heightRatio", 1f);
        projectionShader.TrySetParameter("lengthRatio", Rope.RopeLength / Rope.IdealRopeLength);
        projectionShader.SetTexture(WhitePixel, 1, SamplerState.AnisotropicClamp);

        List<Vector2> ropePositions = ropeSegments.Select(r => r.Position + drawOffset).ToList();

        PrimitiveSettings settings = new PrimitiveSettings(_ => width, new(c =>
        {
            Color light = ui || !affectedByLight ? Color.White : Color.Lerp(lightStart, lightEnd, c);
            return colorFunction(c).MultiplyRGBA(light);
        }), _ => Vector2.Zero, Shader: projectionShader, UseUnscaledMatrix: true, ProjectionAreaWidth: Main.instance.GraphicsDevice.Viewport.Width, ProjectionAreaHeight: Main.instance.GraphicsDevice.Viewport.Height);
        PrimitiveRenderer.RenderTrail(ropePositions, settings, 44);
    }
}
