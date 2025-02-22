using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.FormPresets;
using NoxusBoss.Core.Graphics.FastParticleSystems;
using NoxusBoss.Core.World.Subworlds;
using NoxusBoss.Core.World.TileDisabling;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.Blossoms;

[Autoload(Side = ModSide.Client)]
public class LeafVisualsSystem : ModSystem
{
    /// <summary>
    /// The particle system responsible for the rendering of leaves.
    /// </summary>
    public static LeafParticleSystem ParticleSystem
    {
        get;
        private set;
    }

    /// <summary>
    /// The texture of the leaves.
    /// </summary>
    public static LazyAsset<Texture2D> LeafTexture
    {
        get;
        private set;
    }

    public override void OnModLoad()
    {
        LeafTexture = LazyAsset<Texture2D>.FromPath(GetAssetPath("Content/Particles", "Leaf"));
        ParticleSystem = new(8192, PrepareParticleRendering, ExtraParticleUpdates);
        On_Main.DrawRain += RenderLeaves;
    }

    private void RenderLeaves(On_Main.orig_DrawRain orig, Main self)
    {
        orig(self);

        if (ParticleSystem.particles.Any(p => p.Active))
            ParticleSystem.RenderAll();
    }

    private static void PrepareParticleRendering()
    {
        Matrix world = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f);
        Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -100f, 100f);

        Texture2D leaf = LeafTexture.Value;
        if (NamelessDeityFormPresetRegistry.UsingLucillePreset && EternalGardenUpdateSystem.WasInSubworldLastUpdateFrame)
            leaf = GennedAssets.Textures.Particles.LeafAutumnal;

        Main.instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;

        ManagedShader overlayShader = ShaderManager.GetShader("NoxusBoss.BasicPrimitiveOverlayShader");
        overlayShader.TrySetParameter("uWorldViewProjection", world * Main.GameViewMatrix.TransformationMatrix * projection);
        overlayShader.SetTexture(leaf, 1, SamplerState.LinearClamp);
        overlayShader.Apply();
    }

    private static void ExtraParticleUpdates(ref FastParticle particle)
    {
        Vector2 bottom = particle.Position;
        Point bottomPoint = bottom.ToTileCoordinates();

        if (WorldGen.InWorld(bottomPoint.X, bottomPoint.Y))
        {
            Tile checkTile = Main.tile[bottomPoint];
            if ((checkTile.HasUnactuatedTile && Main.tileSolid[checkTile.TileType] || checkTile.LiquidAmount >= 128) && !TileDisablingSystem.TilesAreUninteractable)
            {
                particle.Velocity.X *= 0.8f;
                particle.Velocity.Y = 0f;
            }
            else
                particle.Velocity.X += Cos(particle.Position.Y * 0.015f + particle.Position.X * 0.03f) * 0.06f - 0.012f;

            if (Abs(particle.Velocity.X) >= 4f)
                particle.Velocity.X *= 0.98f;

            particle.Rotation += Cos(particle.Position.X * 0.01f - particle.Position.Y * 0.024f) * particle.Velocity.Length() * 0.008f;

            if (particle.Time >= 1500)
                particle.Color *= 0.98f;
            if (particle.Time >= 1800)
                particle.Active = false;
        }
    }

    public override void PreUpdateEntities()
    {
        ParticleSystem.UpdateAll();
    }
}
