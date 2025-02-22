using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Core.Graphics.RenderTargets;
using NoxusBoss.Core.Graphics.ScreenShake;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;

public class PortalArmStrike : ModProjectile, IProjOwnedByBoss<AvatarOfEmptiness>
{
    /// <summary>
    /// The data structure that contains information about this striking arm.
    /// </summary>
    public AvatarShadowArm Arm;

    /// <summary>
    /// The hand variant of this striking arm.
    /// </summary>
    public AvatarShadowArm.HandVariant HandVariant;

    /// <summary>
    /// The lifetime ratio of this striking arm.
    /// </summary>
    public float LifetimeRatio => Time / Lifetime;

    /// <summary>
    /// The origin position of this striking arm.
    /// </summary>
    public Vector2 PortalSource
    {
        get => new(Projectile.ai[1], Projectile.ai[2]);
        set
        {
            Projectile.ai[1] = value.X;
            Projectile.ai[2] = value.Y;
        }
    }

    /// <summary>
    /// How long this striking arm has existed for.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    /// <summary>
    /// How long this arm should exist for.
    /// </summary>
    public static int Lifetime => 51;

    /// <summary>
    /// The render target that holds render information for this striking arm.
    /// </summary>
    public static InstancedRequestableTarget ArmTarget
    {
        get;
        private set;
    }

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            ArmTarget = new InstancedRequestableTarget();
            Main.ContentThatNeedsRenderTargets.Add(ArmTarget);

            RenderTargetManager.RenderTargetUpdateLoopEvent += ClearResourcesIfNecessary;
        }
    }

    private void ClearResourcesIfNecessary()
    {
        // Upwards of 100 arm portal strike projectiles can exist at once, each one necessitating an RT instance in the ArmTarget.
        // 324x324x100 = over 10,000,000 pixels
        // While manageable in theory, this is far from what I would consider acceptable.
        // As such, a check is performed to ensure that if no arm strikes are present the targets are reset.
        if (ArmTarget.AnyTargetsAllocated && !AnyProjectiles(Type))
            ArmTarget.Reset();
    }

    public override void SetDefaults()
    {
        Projectile.width = 90;
        Projectile.height = 90;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = Lifetime;
        Projectile.hide = true;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Arm = new(Projectile.Center, Vector2.Zero, true);
        HandVariant = Main.rand.NextFromList(AvatarShadowArm.HandVariant.GrabbingHand1, AvatarShadowArm.HandVariant.GrabbingHand2, AvatarShadowArm.HandVariant.GrabbingHand3);
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        Arm.WriteTo(writer);
        writer.Write((int)HandVariant);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        Arm = AvatarShadowArm.ReadFrom(reader);
        HandVariant = (AvatarShadowArm.HandVariant)reader.ReadInt32();
    }

    public override void AI()
    {
        // Define the portal source as the starting position on the first frame.
        if (PortalSource == Vector2.Zero)
        {
            PortalSource = Projectile.Center;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.netUpdate = true;

            CustomScreenShakeSystem.Start(10, 3f);
        }

        float retractInterpolant = InverseLerp(20f, 0f, Projectile.timeLeft);
        Projectile.scale = Lerp(0.37f, 0.65f, InverseLerp(0f, 10f, Time).Squared() * (1f - retractInterpolant));
        Arm.Scale = Projectile.scale;
        Arm.AnchorOffset = PortalSource - Projectile.Center - Projectile.rotation.ToRotationVector2() * Projectile.scale * 670f;

        float swipeRadius = Lerp(30f, 70f, Projectile.identity / 9f % 1f) * Arm.VerticalFlip.ToDirectionInt();
        float swipeDirection = -Cos((Arm.ForearmEnd - PortalSource).ToRotation()).NonZeroSign();
        Arm.Center = Projectile.Center + (TwoPi * Time / Lifetime * swipeDirection * 0.5f).ToRotationVector2() * swipeRadius;

        Projectile.velocity += Projectile.rotation.ToRotationVector2() * Lerp(0.06f, -14f, retractInterpolant);

        Projectile.frame = (int)Round(Pow(LifetimeRatio, 1.25f) * 4f);

        // Increment time.
        Time++;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float _ = 0f;
        Vector2 start = PortalSource;
        Vector2 end = Projectile.Center;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, Projectile.scale * 40f, ref _);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (PortalSource == Vector2.Zero || ArmTarget is null)
            return false;

        Vector2 renderTargetArea = new Vector2(324f, 324f);
        ArmTarget.Request((int)renderTargetArea.X, (int)renderTargetArea.Y, Projectile.identity, () =>
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            Arm?.Draw(Projectile.Center - renderTargetArea * 0.5f, Projectile, HandVariant, Projectile.frame);
            Main.spriteBatch.End();
        });

        if (!ArmTarget.TryGetTarget(Projectile.identity, out RenderTarget2D? target) || target is null)
            return false;

        Main.spriteBatch.PrepareForShaders();

        // Determine whether this arm is behind its source entirely.
        // If it is, terminate this method immediately and don't draw, for efficiency.
        Vector2 sourceDirection = Projectile.rotation.ToRotationVector2();
        bool behindSource = Vector2.Dot(Projectile.Center - PortalSource, sourceDirection) <= 0f;
        if (behindSource)
            return false;

        float[] blurWeights = new float[7];
        for (int i = 0; i < blurWeights.Length; i++)
            blurWeights[i] = GaussianDistribution(i - blurWeights.Length * 0.5f, 2.475f) / 7f;

        // Prepare the arm shader.
        ManagedShader armShader = ShaderManager.GetShader("NoxusBoss.AvatarShadowArmShader");
        armShader.TrySetParameter("blurWeights", blurWeights);
        armShader.TrySetParameter("blurOffset", 0.004f);
        armShader.TrySetParameter("blurAtCenter", false);
        armShader.TrySetParameter("performPositionCutoff", true);
        armShader.TrySetParameter("forwardDirection", sourceDirection);
        armShader.TrySetParameter("cutoffOrigin", Vector2.Transform(PortalSource - Main.screenPosition, Main.GameViewMatrix.TransformationMatrix));
        armShader.Apply();

        // Draw the arm target with a special shader.
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        DrawData targetData = new DrawData(target, drawPosition, target.Frame(), Projectile.GetAlpha(Color.Red), 0f, target.Size() * 0.5f, 1f, 0, 0f);
        targetData.Draw(Main.spriteBatch);

        Main.spriteBatch.ResetToDefault();

        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);
    }
}
