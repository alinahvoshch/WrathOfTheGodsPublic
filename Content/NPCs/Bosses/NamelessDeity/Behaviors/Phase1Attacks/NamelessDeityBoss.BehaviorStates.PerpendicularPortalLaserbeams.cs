using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity;

public partial class NamelessDeityBoss : ModNPC
{
    /// <summary>
    /// Whether Nameless has prepared one of his laserbeams' shoot sounds during his Perpendicular Portal Laserbeams attack.
    /// </summary>
    public bool PerpendicularPortalLaserbeams_HasPreparedLaserSound
    {
        get;
        set;
    }

    /// <summary>
    /// How long Nameless spends getting close to the player during his Perpendicular Portal Laserbeams attack.
    /// </summary>
    public static int PerpendicularPortalLaserbeams_CloseRedirectTime => GetAIInt("PerpendicularPortalLaserbeams_CloseRedirectTime");

    /// <summary>
    /// How long Nameless moving away from the player after getting close during his Perpendicular Portal Laserbeams attack.
    /// </summary>
    public static int PerpendicularPortalLaserbeams_FarRedirectTime => GetAIInt("PerpendicularPortalLaserbeams_FarRedirectTime");

    /// <summary>
    /// How long Nameless spends performing dashes during his Perpendicular Portal Laserbeams attack.
    /// </summary>
    public static int PerpendicularPortalLaserbeams_ChargeTime => GetAIInt("PerpendicularPortalLaserbeams_ChargeTime");

    /// <summary>
    /// How long Nameless' portals exist during his Perpendicular Portal Laserbeams attack.
    /// </summary>
    public static int PerpendicularPortalLaserbeams_PortalExistTime => GetAIInt("PerpendicularPortalLaserbeams_PortalExistTime");

    /// <summary>
    /// How long Nameless' portals spend firing lasers during his Perpendicular Portal Laserbeams attack.
    /// </summary>
    public static int PerpendicularPortalLaserbeams_LaserShootTime => GetAIInt("PerpendicularPortalLaserbeams_LaserShootTime");

    /// <summary>
    /// The rate at which Nameless summons portals while dashing horizontally during his Perpendicular Portal Laserbeams attack.
    /// </summary>
    public static int PerpendicularPortalLaserbeams_HorizontalPortalReleaseRate => GetAIInt("PerpendicularPortalLaserbeams_HorizontalPortalReleaseRate");

    /// <summary>
    /// The rate at which Nameless summons portals while dashing vertically during his Perpendicular Portal Laserbeams attack.
    /// </summary>
    public static int PerpendicularPortalLaserbeams_VerticalPortalReleaseRate => GetAIInt("PerpendicularPortalLaserbeams_VerticalPortalReleaseRate");

    /// <summary>
    /// The amount of dashes Nameless performs during his Perpendicular Portal Laserbeams attack.
    /// </summary>
    public static int PerpendicularPortalLaserbeams_ChargeCount => GetAIInt("PerpendicularPortalLaserbeams_ChargeCount");

    /// <summary>
    /// The general speed factor of Nameless' dashes during his Perpendicular Portal Laserbeams attack.
    /// </summary>
    public static float PerpendicularPortalLaserbeams_ChargeSpeedFactor => GetAIFloat("PerpendicularPortalLaserbeams_ChargeSpeedFactor");

    /// <summary>
    /// The signed direction of Nameless' dashes during his Perpendicular Portal Laserbeams attack.
    /// </summary>
    public ref float PerpendicularPortalLaserbeams_ChargeDirectionSign => ref NPC.ai[2];

    /// <summary>
    /// The amount of dashes Nameless has performed so far during his Perpendicular Portal Laserbeams attack.
    /// </summary>
    public ref float PerpendicularPortalLaserbeams_ChargeCounter => ref NPC.ai[3];

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_PerpendicularPortalLaserbeams()
    {
        // Load the transition from PerpendicularPortalLaserbeams to the next in the cycle.
        StateMachine.RegisterTransition(NamelessAIType.PerpendicularPortalLaserbeams, null, false, () =>
        {
            return PerpendicularPortalLaserbeams_ChargeCounter >= PerpendicularPortalLaserbeams_ChargeCount;
        });

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(NamelessAIType.PerpendicularPortalLaserbeams, DoBehavior_PerpendicularPortalLaserbeams);
    }

    public void DoBehavior_PerpendicularPortalLaserbeams()
    {
        int closeRedirectTime = PerpendicularPortalLaserbeams_CloseRedirectTime;
        int farRedirectTime = PerpendicularPortalLaserbeams_FarRedirectTime;
        int chargeTime = (int)Clamp(PerpendicularPortalLaserbeams_ChargeTime / DifficultyFactor, 24f, 1000f);
        int portalExistTime = PerpendicularPortalLaserbeams_PortalExistTime;
        int laserShootTime = PerpendicularPortalLaserbeams_LaserShootTime;
        int chargeCount = PerpendicularPortalLaserbeams_ChargeCount;
        float chargeSpeedFactor = PerpendicularPortalLaserbeams_ChargeSpeedFactor;
        ref float chargeDirectionSign = ref PerpendicularPortalLaserbeams_ChargeDirectionSign;

        bool verticalCharges = PerpendicularPortalLaserbeams_ChargeCounter % 2f == 1f;
        float laserAngularVariance = verticalCharges ? 0.02f : 0.05f;
        float fastChargeSpeedInterpolant = verticalCharges ? 0.184f : 0.13f;
        int portalReleaseRate = verticalCharges ? PerpendicularPortalLaserbeams_VerticalPortalReleaseRate : PerpendicularPortalLaserbeams_HorizontalPortalReleaseRate;
        int minPortalReleaseRate = 3;
        if (DifficultyFactor >= 2f)
            minPortalReleaseRate = 2;

        portalReleaseRate = (int)Clamp(Round(portalReleaseRate / DifficultyFactor), minPortalReleaseRate, portalReleaseRate);

        // Flap wings.
        UpdateWings(AITimer / 42f);

        // Update universal hands.
        DefaultUniversalHandMotion();

        // Make the fan animation speed increase.
        RenderComposite.Find<SideFinsStep>().FanAnimationSpeed = Lerp(RenderComposite.Find<SideFinsStep>().FanAnimationSpeed, 7f, 0.166f);

        // Move to the side of the player.
        if (AITimer <= closeRedirectTime)
        {
            Vector2 hoverDestination = Target.Center + new Vector2((Target.Center.X < NPC.Center.X).ToDirectionInt() * 350f, 300f);

            // Teleport to the hover destination on the first frame.
            if (AITimer == 1f)
            {
                ImmediateTeleportTo(hoverDestination);
                RerollAllSwappableTextures();
                ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 8f);
            }

            // Fade in.
            NPC.Opacity = InverseLerp(3f, 10f, AITimer);

            NPC.Center = Vector2.Lerp(NPC.Center, hoverDestination, 0.39f);
            NPC.velocity *= 0.85f;
            return;
        }

        // Move back a bit from the player.
        if (AITimer <= closeRedirectTime + farRedirectTime)
        {
            PerpendicularPortalLaserbeams_HasPreparedLaserSound = false;

            float flySpeed = Utils.Remap(AITimer - closeRedirectTime, 0f, farRedirectTime - 4f, 45f, 80f) * chargeSpeedFactor;
            Vector2 hoverDestination = Target.Center + new Vector2((Target.Center.X < NPC.Center.X).ToDirectionInt() * 1450f, -Target.Velocity.Y * 12f - 542f);
            if (verticalCharges)
                hoverDestination = Target.Center + new Vector2((Target.Center.X < NPC.Center.X).ToDirectionInt() * 960f - Target.Velocity.X * 12f, 1075f);

            // Handle movement.
            NPC.Center = Vector2.Lerp(NPC.Center, hoverDestination, 0.026f);
            NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.SafeDirectionTo(hoverDestination) * flySpeed, 0.15f);

            if (AITimer == closeRedirectTime + 1)
            {
                chargeDirectionSign = (Target.Center.X > NPC.Center.X).ToDirectionInt();
                if (verticalCharges)
                    chargeDirectionSign = -1f;

                NPC.velocity.Y *= 0.6f;
                NPC.netUpdate = true;
            }

            return;
        }

        // Perform the charge.
        if (AITimer <= closeRedirectTime + farRedirectTime + chargeTime)
        {
            // Release primordial stardust from the fans.
            Vector2 leftFanPosition = NPC.Center + new Vector2(-280f, -54f).RotatedBy(NPC.rotation) * TeleportVisualsAdjustedScale;
            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer % 6f == 1f)
            {
                Vector2 stardustVelocity = -Vector2.UnitY.RotatedByRandom(0.58f) * Main.rand.NextFloat(3f, 40f);
                if (verticalCharges)
                {
                    bool stardustGoesRight = NPC.Center.X >= Target.Center.X;
                    stardustVelocity = stardustVelocity.RotatedBy(stardustGoesRight.ToDirectionInt() * PiOver2);
                }

                NPC.NewProjectileBetter(NPC.GetSource_FromAI(), leftFanPosition - Vector2.UnitX * 150f, stardustVelocity, ModContent.ProjectileType<PrimordialStardust>(), PrimordialStardustDamage, 0f);
            }

            // Release flowers. If the charge is really close to the target they appear regardless of the timer, to ensure that they can't just stand still.
            bool forcefullySpawnFlower = (Distance(NPC.Center.X, Target.Center.X) <= 90f / DifficultyFactor && !verticalCharges) ||
                                         (Distance(NPC.Center.Y, Target.Center.Y) <= 55f / DifficultyFactor && verticalCharges);
            if (Main.netMode != NetmodeID.MultiplayerClient && (AITimer % portalReleaseRate == 0f || forcefullySpawnFlower) && AITimer >= closeRedirectTime + farRedirectTime + 5f)
            {
                int remainingChargeTime = chargeTime - (AITimer - closeRedirectTime - farRedirectTime);
                int fireDelay = remainingChargeTime + 14;
                Vector2 flowerDirection = ((verticalCharges ? Vector2.UnitX : Vector2.UnitY) * NPC.SafeDirectionTo(Target.Center)).SafeNormalize(Vector2.UnitY).RotatedByRandom(laserAngularVariance);

                // Summon the flower and shoot the telegraph for the laser.
                float playSound = PerpendicularPortalLaserbeams_HasPreparedLaserSound ? 0f : 1f;
                Vector2 flowerSpawnPosition = NPC.Center - flowerDirection * 42f + flowerDirection.RotatedBy(PiOver2) * 9f;
                NPC.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, flowerDirection, ModContent.ProjectileType<TelegraphedPortalLaserbeam>(), PortalLaserbeamDamage, 0f, -1, fireDelay, laserShootTime, playSound);
                NPC.NewProjectileBetter(NPC.GetSource_FromAI(), flowerSpawnPosition, flowerDirection, ModContent.ProjectileType<LightLaserCamellia>(), 0, 0f, -1, fireDelay, portalExistTime + remainingChargeTime + 15, fireDelay);

                PerpendicularPortalLaserbeams_HasPreparedLaserSound = true;
            }

            // Go FAST.
            float oldSpeed = NPC.velocity.Length();
            Vector2 chargeDirectionVector = verticalCharges ? Vector2.UnitY * chargeDirectionSign : Vector2.UnitX * chargeDirectionSign;
            NPC.velocity = Vector2.Lerp(NPC.velocity, chargeDirectionVector * chargeSpeedFactor * 150f, fastChargeSpeedInterpolant);
            if (NPC.velocity.Length() >= chargeSpeedFactor * 92f && oldSpeed <= chargeSpeedFactor * 91f)
            {
                GeneralScreenEffectSystem.RadialBlur.Start(NPC.Center, 0.85f, 10);
                ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 5f);
            }

            return;
        }

        PerpendicularPortalLaserbeams_ChargeCounter++;
        if (PerpendicularPortalLaserbeams_ChargeCounter < chargeCount)
        {
            AITimer = 0;
            NPC.netUpdate = true;
        }
    }
}
