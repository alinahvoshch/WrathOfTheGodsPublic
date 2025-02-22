using Luminance.Assets;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.Automators;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;

// The non-inclusion of IProjOwnedByBoss<AvatarOfEmptiness> is deliberate, so that it doesn't get abruptly deleted.
public class FrostIllusion : ModProjectile, IDrawsWithShader
{
    public enum FrostIllusionVariant
    {
        Undecided,
        FrozenPlayer,
        Skull1,
        Skull2,
        LaRuga,
    }

    /// <summary>
    /// The variant that this frost illusion is.
    /// </summary>
    public FrostIllusionVariant Variant
    {
        get => (FrostIllusionVariant)Projectile.localAI[0];
        set => Projectile.localAI[0] = (int)value;
    }

    /// <summary>
    /// How long this illusion has existed so far, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    /// <summary>
    /// How long this illusion should exist for, in frames.
    /// </summary>
    public static int Lifetime => SecondsToFrames(8f);

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = Lifetime;
    }

    public override void AI()
    {
        if (Variant == FrostIllusionVariant.Undecided)
        {
            DecideVariant();
            PerformInitializations();
        }

        Projectile.Opacity = InverseLerpBump(0f, 6f, Lifetime - 60f, Lifetime - 1f, Time);
        Projectile.scale = InverseLerp(0f, 4f, Time) + Log(Clamp(Time - 60f, 1f, 1000f), 6f) * 0.1f;

        Player closest = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
        if (Projectile.timeLeft >= 120 && Projectile.WithinRange(closest.Center, 270f))
        {
            Projectile.timeLeft -= 15;
            Time += 15f;
        }

        Time++;
    }

    public void DecideVariant()
    {
        WeightedRandom<FrostIllusionVariant> variantRNG = new WeightedRandom<FrostIllusionVariant>(Main.rand);
        variantRNG.Add(FrostIllusionVariant.Skull1, 1D);
        variantRNG.Add(FrostIllusionVariant.Skull2, 1D);
        variantRNG.Add(FrostIllusionVariant.LaRuga, 0.006614552);

        Variant = variantRNG;

        // Take a snapshot of the player in a render target if the frozen player variant was selected.
        if (Variant == FrostIllusionVariant.FrozenPlayer && CountProjectiles(Type) <= 1)
            LocalPlayerDrawManager.TakePlayerSnapshot();
    }

    public void PerformInitializations()
    {
        // Choose a random rotation and direction.
        Projectile.rotation = Main.rand.NextFloatDirection() * 0.4f;
        Projectile.spriteDirection = Main.rand.NextFromList(-1, 1);

        // Look at the player if frozen.
        if (Variant == FrostIllusionVariant.FrozenPlayer)
            Projectile.spriteDirection = (int)Projectile.HorizontalDirectionTo(Main.LocalPlayer.Center);
    }

    public void DrawWithShader(SpriteBatch spriteBatch)
    {
        float rotation = Projectile.rotation;
        float scale = Projectile.scale;
        float windScatterInterpolant = InverseLerp(120f, 0f, Projectile.timeLeft).Squared();
        SpriteEffects direction = Projectile.spriteDirection.ToSpriteDirection();
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;

        void drawWithFrozenDissipationShader(Texture2D texture)
        {
            ManagedShader frozenShader = ShaderManager.GetShader("NoxusBoss.FrozenDissipiationShader");
            frozenShader.TrySetParameter("identity", Projectile.identity * 1.65f % 0.03f + 1f);
            frozenShader.TrySetParameter("disintegrationFactor", windScatterInterpolant * 9f);
            frozenShader.TrySetParameter("pixelationFactor", Vector2.One * 1.5f / texture.Size());
            frozenShader.TrySetParameter("scatterDirectionBias", Vector2.UnitX * Main.windSpeedTarget.NonZeroSign() * -Projectile.spriteDirection);
            frozenShader.SetTexture(CrackedNoiseA, 1, SamplerState.PointWrap);
            frozenShader.SetTexture(DendriticNoiseZoomedOut, 2, SamplerState.PointWrap);
            frozenShader.Apply();

            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(Color.White) * 0.75f, rotation, texture.Size() * 0.5f, scale, direction, 0f);
        }

        switch (Variant)
        {
            case FrostIllusionVariant.LaRuga:
                Texture2D texture = GennedAssets.Textures.Projectiles.FrostIllusion_LaRuga.Value;
                Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(Color.White) * 0.6f, rotation, texture.Size() * 0.5f, scale, direction, 0f);
                break;

            case FrostIllusionVariant.FrozenPlayer:
                drawWithFrozenDissipationShader(LocalPlayerDrawManager.PlayerSnapshotTarget);
                break;

            case FrostIllusionVariant.Skull1:
                drawWithFrozenDissipationShader(GennedAssets.Textures.Projectiles.FrostIllusion_Skull.Value);
                break;

            case FrostIllusionVariant.Skull2:
                drawWithFrozenDissipationShader(GennedAssets.Textures.Projectiles.FrostIllusion_Skull2.Value);
                break;
        }
    }
}
