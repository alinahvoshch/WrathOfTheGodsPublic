using System.Runtime.CompilerServices;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Core.Graphics.FastParticleSystems;
using Terraria.ModLoader;
using MatrixSIMD = System.Numerics.Matrix4x4;
using Vector2SIMD = System.Numerics.Vector2;

namespace NoxusBoss.Core.Graphics.Blossoms;

[Autoload(Side = ModSide.Client)]
public class LeafParticleSystem : FastParticleSystem
{
    public LeafParticleSystem(int maxParticles, Action renderPreparations, ParticleUpdateAction? extraUpdates = null) :
        base(maxParticles, renderPreparations, extraUpdates)
    { }

    protected override void PopulateVertexBufferIndex(VertexPosition2DColorTexture[] vertices, int particleIndex)
    {
        ref FastParticle particle = ref particles[particleIndex];

        Color color = particle.Active ? particle.Color : Color.Transparent;
        Vector2SIMD center = Unsafe.As<Vector2, Vector2SIMD>(ref particle.Position);
        Vector2SIMD size = Unsafe.As<Vector2, Vector2SIMD>(ref particle.Size);
        MatrixSIMD particleRotationMatrix = MatrixSIMD.CreateRotationX(particle.Rotation * 0.7f) *
                                            MatrixSIMD.CreateRotationY(particle.Rotation * 3f) *
                                            MatrixSIMD.CreateRotationZ(particle.Rotation);

        Vector2SIMD topLeftPosition = center + Vector2SIMD.Transform(topLeftOffset * size, particleRotationMatrix);
        Vector2SIMD topRightPosition = center + Vector2SIMD.Transform(topRightOffset * size, particleRotationMatrix);
        Vector2SIMD bottomLeftPosition = center + Vector2SIMD.Transform(bottomLeftOffset * size, particleRotationMatrix);
        Vector2SIMD bottomRightPosition = center + Vector2SIMD.Transform(bottomRightOffset * size, particleRotationMatrix);

        int vertexIndex = particleIndex * 4;
        vertices[vertexIndex] = new(Unsafe.As<Vector2SIMD, Vector2>(ref topLeftPosition), color, Vector2.Zero, 0f);
        vertices[vertexIndex + 1] = new VertexPosition2DColorTexture(Unsafe.As<Vector2SIMD, Vector2>(ref topRightPosition), color, Vector2.UnitX, 0f);
        vertices[vertexIndex + 2] = new VertexPosition2DColorTexture(Unsafe.As<Vector2SIMD, Vector2>(ref bottomRightPosition), color, Vector2.One, 0f);
        vertices[vertexIndex + 3] = new VertexPosition2DColorTexture(Unsafe.As<Vector2SIMD, Vector2>(ref bottomLeftPosition), color, Vector2.UnitY, 0f);
    }
}
