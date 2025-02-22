using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using NoxusBoss.Core.CrossCompatibility.Inbound;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity;

public partial class NamelessDeityBoss : ModNPC
{
    /// <summary>
    /// The relative darkening of the entire scene as a consequence of the presence of the star during Nameless' Sun Blender Beams state.
    /// </summary>
    public float RelativeDarkening
    {
        get;
        set;
    }

    /// <summary>
    /// Whether the star summoned by Nameless during his Sun Blender Beams attack should be held in his left hand or not.
    /// </summary>
    public bool StarShouldBeHeldByLeftHand
    {
        get;
        set;
    }

    /// <summary>
    /// The amount of damage Nameless' controlled star does.
    /// </summary>
    public static int ControlledStarDamage => GetAIInt("ControlledStarDamage");

    /// <summary>
    /// How long Nameless' stolen star spends growing to its full size during his Sun Blender Beams state.
    /// </summary>
    public static int SunBlenderBeams_GrowToFullSizeTime => GetAIInt("SunBlenderBeams_GrowToFullSizeTime");

    /// <summary>
    /// How many flare bursts Nameless should perform during his Sun Blender Beams state.
    /// </summary>
    public static int SunBlenderBeams_FlareBurstShootCount => GetAIInt("SunBlenderBeams_FlareBurstShootCount");

    /// <summary>
    /// The amount of flares added to each set for each burst that has occured so far during Nameless' Sun Blender Beams state.
    /// </summary>
    public static int SunBlenderBeams_FlareCountBoostPerBurst => GetAIInt("SunBlenderBeams_FlareCountBoostPerBurst");

    /// <summary>
    /// How long Nameless' sun fire blasts should exist for during Nameless' Sun Blender Beams state.
    /// </summary>
    public static int SunBlenderBeams_FireBlastLaserShootTime => GetAIInt("SunBlenderBeams_FireBlastLaserShootTime");

    /// <summary>
    /// The rate at which Nameless' star releases starbursts during his Sun Blender Beams state.
    /// </summary>
    public static int SunBlenderBeams_StarburstReleaseRate => GetAIInt("SunBlenderBeams_StarburstReleaseRate");

    /// <summary>
    /// How many starbursts should be released at once from the main star during Nameless' Sun Blender Beams state.
    /// </summary>
    public static int SunBlenderBeams_StarburstShootCount => GetAIInt("SunBlenderBeams_StarburstShootCount");

    /// <summary>
    /// The amount of flares that should be released in the first burst, assuming no secondary modifications, during Nameless' Sun Blender Beams state.
    /// </summary>
    public static int SunBlenderBeams_BaseFlareCount => GetAIInt("SunBlenderBeams_BaseFlareCount");

    /// <summary>
    /// The amount of extra flares that should be released in the first burst during Nameless' Sun Blender Beams state in his second phase.
    /// </summary>
    public static int SunBlenderBeams_FlareCountBoostPhase2 => GetAIInt("SunBlenderBeams_FlareCountBoostPhase2");

    /// <summary>
    /// The minimum telegraph time during Nameless' Sun Blender Beams state. This is the starting value for the telegraph duration.
    /// </summary>
    public static int SunBlenderBeams_MinFlareTelegraphTime => GetAIInt("SunBlenderBeams_MinFlareTelegraphTime");

    /// <summary>
    /// The maximum telegraph time during Nameless' Sun Blender Beams state. This is the ending value for the telegraph duration.
    /// </summary>
    public static int SunBlenderBeams_MaxFlareTelegraphTime => GetAIInt("SunBlenderBeams_MaxFlareTelegraphTime");

    /// <summary>
    /// How long Nameless waits after the star grows to full size before attacking during his Sun Blender Beams state.
    /// </summary>
    public int SunBlenderBeams_AttackDelay => GetAIInt("SunBlenderBeams_AttackDelay");

    /// <summary>
    /// How long Nameless spends attacking during his Sun Blender Beams state in his first phase.
    /// </summary>
    public int SunBlenderBeams_AttackDurationPhase1 => GetAIInt("SunBlenderBeams_AttackDurationPhase1");

    /// <summary>
    /// How long Nameless spends attacking during his Sun Blender Beams state in his second phase.
    /// </summary>
    public int SunBlenderBeams_AttackDurationPhase2 => GetAIInt("SunBlenderBeams_AttackDurationPhase2");

    /// <summary>
    /// How long the attacking part of the Sun Blender Beams state goes on for.
    /// </summary>
    public int SunBlenderBeams_AttackDuration => CurrentPhase >= 1 ? SunBlenderBeams_AttackDurationPhase2 : SunBlenderBeams_AttackDurationPhase1;

    /// <summary>
    /// The starting speed of starbursts created during Nameless' Sun Blender Beams state.
    /// </summary>
    public static float SunBlenderBeams_StartingStarburstSpeed => GetAIFloat("SunBlenderBeams_StartingStarburstSpeed");

    /// <summary>
    /// How many flare bursts Nameless has released so far during his Sun Blender Beams state.
    /// </summary>
    public ref float SunBlenderBeams_FlareBurstCounter => ref NPC.ai[2];

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_SunBlenderBeams()
    {
        StateMachine.RegisterTransition(NamelessAIType.SunBlenderBeams, null, false, () =>
        {
            bool endAttackEarly = !AnyProjectiles(ModContent.ProjectileType<ControlledStar>()) && AITimer >= 5;
            return AITimer >= SunBlenderBeams_GrowToFullSizeTime + SunBlenderBeams_AttackDelay + SunBlenderBeams_AttackDuration || endAttackEarly;
        }, () => StarShouldBeHeldByLeftHand = false);

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(NamelessAIType.SunBlenderBeams, DoBehavior_SunBlenderBeams);
    }

    public void DoBehavior_SunBlenderBeams()
    {
        int starGrowTime = SunBlenderBeams_GrowToFullSizeTime;
        int attackDelay = SunBlenderBeams_AttackDelay;
        int flareShootCount = SunBlenderBeams_FlareBurstShootCount;
        int fireBlastLaserShootTime = SunBlenderBeams_FireBlastLaserShootTime;
        int attackDuration = SunBlenderBeams_AttackDuration;
        int starburstReleaseRate = SunBlenderBeams_StarburstReleaseRate;
        int starburstCount = SunBlenderBeams_StarburstShootCount;
        int baseFlareCount = SunBlenderBeams_BaseFlareCount;
        int minTelegraphTime = SunBlenderBeams_MinFlareTelegraphTime;
        int maxTelegraphTime = SunBlenderBeams_MaxFlareTelegraphTime;
        int flaresToAddPerBurst = (int)Round(DifficultyFactor * 2f);
        float starburstStartingSpeed = SunBlenderBeams_StartingStarburstSpeed * DifficultyFactor;

        // Make things faster in successive phases.
        if (CurrentPhase >= 1)
        {
            baseFlareCount = SunBlenderBeams_FlareCountBoostPhase2;
            minTelegraphTime -= 4;
            maxTelegraphTime -= 13;

            if (minTelegraphTime < 1)
                minTelegraphTime = 1;
            if (maxTelegraphTime < 9)
                maxTelegraphTime = 9;
        }
        flaresToAddPerBurst += (int)Round(DifficultyFactor - 1f);

        // Use the robed arm variant.
        if (!RenderComposite.Find<ArmsStep>().UsingPreset)
        {
            RenderComposite.Find<ArmsStep>().ArmTexture.ForceToTexture("Arm1");
            RenderComposite.Find<ArmsStep>().ForearmTexture.ForceToTexture("Forearm1");
            RenderComposite.Find<ArmsStep>().HandTexture.ForceToTexture("Hand1");
        }

        // Flap wings.
        UpdateWings(AITimer / 45f);

        // Create a suitable star and two hands on the first frame.
        // The star will give the appearance of actually coming from the background.
        if (AITimer == 1)
        {
            // Teleport behind the target.
            StartTeleportAnimation(() => Target.Center - Vector2.UnitX * TargetDirection * 400f, DefaultTeleportTime / 2, DefaultTeleportTime / 2);

            // Create the star.
            Vector2 starSpawnPosition = NPC.Center + new Vector2(300f, -350f) * TeleportVisualsAdjustedScale;
            CreateTwinkle(starSpawnPosition, Vector2.One * 1.3f);
            if (Main.netMode != NetmodeID.MultiplayerClient)
                NPC.NewProjectileBetter(NPC.GetSource_FromAI(), starSpawnPosition, Vector2.Zero, ModContent.ProjectileType<ControlledStar>(), ControlledStarDamage, 0f);

            // Play mumble sounds.
            PerformMumble();
            return;
        }

        // Return to the foreground if necessary.
        ZPosition = Lerp(ZPosition, 0f, 0.16f);

        // Decide which hand should hold the star.
        if (AITimer <= 10)
            StarShouldBeHeldByLeftHand = NPC.Center.X > Target.Center.X;

        if (AITimer == 52f)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.FingerSnap with
            {
                Volume = 4f
            });
            NamelessDeityKeyboardShader.BrightnessIntensity += 0.45f;
        }

        // Verify that a star actually exists. If not, terminate this attack immediately.
        List<Projectile> stars = AllProjectilesByID(ModContent.ProjectileType<ControlledStar>()).ToList();
        if (stars.Count == 0)
        {
            DestroyAllHands();
            SunBlenderBeams_FlareBurstCounter = 0f;
            AITimer = 0;
            return;
        }

        // Become darker.
        RelativeDarkening = Clamp(RelativeDarkening + 0.06f, 0f, 0.5f);
        NamelessDeitySky.HeavenlyBackgroundIntensity = 1f - RelativeDarkening;
        NPC.Opacity = 1f;

        // Fly to the side of the target before the attack begins.
        if (AITimer < attackDelay)
        {
            Vector2 hoverDestination = Target.Center + new Vector2((Target.Center.X < NPC.Center.X).ToDirectionInt() * 592f, -250f);
            NPC.SmoothFlyNear(hoverDestination, 0.12f, 0.87f);
        }

        // Drift towards the target once the attack has begun.
        else
        {
            // Rapidly slow down if there's any remnant speed from prior movement.
            if (NPC.velocity.Length() > 4f)
                NPC.velocity *= 0.8f;

            Vector2 hoverDestination = Target.Center + Target.Center.SafeDirectionTo(NPC.Center) * new Vector2(450f, 20f) + Vector2.UnitY * 75f;
            float hoverSpeedInterpolant = Utils.Remap(NPC.Distance(hoverDestination), 980f, 1900f, 0.0018f, 0.04f);
            NPC.Center = Vector2.Lerp(NPC.Center, hoverDestination, hoverSpeedInterpolant);
            NPC.SimpleFlyMovement(NPC.SafeDirectionTo(hoverDestination) * 4f, 0.1f);
        }

        // Find the star to control.
        Projectile star = stars.First();

        // Crate a light wave if the star released lasers.
        if (NPC.ai[3] == 1f)
        {
            RadialScreenShoveSystem.Start(EyePosition, 35);

            NamelessDeityKeyboardShader.BrightnessIntensity += 0.72f;
            NPC.ai[3] = 0f;
            NPC.netUpdate = true;
        }

        // Update hand positions.
        int handHoldingStarIndex = StarShouldBeHeldByLeftHand ? 0 : 1;
        Vector2 leftHandOffset = Vector2.Zero;
        Vector2 rightHandOffset = Vector2.Zero;
        Vector2 hoverVerticalOffset = Vector2.UnitY * Cos01(AITimer / 13f) * 50f;
        if (StarShouldBeHeldByLeftHand)
            leftHandOffset += new Vector2(-150f, -160f) + hoverVerticalOffset;
        else
            rightHandOffset += new Vector2(150f, -160f) + hoverVerticalOffset;

        DefaultHandDrift(Hands[0], NPC.Center - Vector2.UnitX * TeleportVisualsAdjustedScale * 720f + leftHandOffset, 1.8f);
        DefaultHandDrift(Hands[1], NPC.Center + Vector2.UnitX * TeleportVisualsAdjustedScale * 720f + rightHandOffset, 1.8f);
        Hands[handHoldingStarIndex].RotationOffset = PiOver4 - 0.2f;
        Hands[1 - handHoldingStarIndex].RotationOffset = PiOver4 - 0.32f;
        if (StarShouldBeHeldByLeftHand)
        {
            Hands[handHoldingStarIndex].RotationOffset *= -1f;
            Hands[1 - handHoldingStarIndex].RotationOffset *= -1f;
        }

        // Hold the star in Nameless' right hand.
        float verticalOffset = -200f;
        Vector2 starPosition = Hands[handHoldingStarIndex].FreeCenter + (new Vector2(StarShouldBeHeldByLeftHand.ToDirectionInt() * 100f, verticalOffset) - hoverVerticalOffset) * TeleportVisualsAdjustedScale;
        star.Center = starPosition;

        // Release accelerating bursts of starbursts over time.
        if (AITimer >= starGrowTime + attackDelay && AITimer % starburstReleaseRate == 0f)
        {
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with
            {
                MaxInstances = 5
            }, star.Center);
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int starburstID = ModContent.ProjectileType<Starburst>();
                int starburstCounter = (int)Round(AITimer / starburstReleaseRate);
                float shootOffsetAngle = starburstCounter % 2 == 0 ? Pi / starburstCount : 0f;
                if (CommonCalamityVariables.DeathModeActive)
                    shootOffsetAngle = Main.rand.NextFloat(TwoPi);

                for (int i = 0; i < starburstCount; i++)
                {
                    Vector2 starburstVelocity = star.SafeDirectionTo(Target.Center).RotatedBy(TwoPi * i / starburstCount + shootOffsetAngle) * starburstStartingSpeed;
                    NPC.NewProjectileBetter(NPC.GetSource_FromAI(), star.Center + starburstVelocity * 3f, starburstVelocity, starburstID, StarburstDamage, 0f);
                }
                for (int i = 0; i < starburstCount / 2; i++)
                {
                    Vector2 starburstVelocity = star.SafeDirectionTo(Target.Center).RotatedBy(TwoPi * i / starburstCount / 2f + shootOffsetAngle) * starburstStartingSpeed * 0.6f;
                    NPC.NewProjectileBetter(NPC.GetSource_FromAI(), star.Center + starburstVelocity * 3f, starburstVelocity, starburstID, StarburstDamage, 0f);
                }
            }
        }

        // Release telegraphed solar flare lasers over time. The quantity of lasers increases as time goes on.
        if (AITimer >= starGrowTime + attackDelay && SunBlenderBeams_FlareBurstCounter < flareShootCount && !AllProjectilesByID(ModContent.ProjectileType<TelegraphedStarLaserbeam>()).Any())
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Create the flares.
                int flareCount = (int)(SunBlenderBeams_FlareBurstCounter * flaresToAddPerBurst) + baseFlareCount;
                int flareTelegraphTime = (int)Utils.Remap(AITimer - attackDelay, 0f, 300f, minTelegraphTime, maxTelegraphTime) + fireBlastLaserShootTime;
                float flareSpinDirection = (SunBlenderBeams_FlareBurstCounter % 2f == 0f).ToDirectionInt();
                float flareSpinCoverage = PiOver2 * flareSpinDirection;
                Vector2 directionToTarget = star.SafeDirectionTo(Target.Center);
                for (int i = 0; i < flareCount; i++)
                {
                    Vector2 flareDirection = directionToTarget.RotatedBy(TwoPi * i / flareCount - PiOver2);
                    NPC.NewProjectileBetter(NPC.GetSource_FromAI(), star.Center, flareDirection, ModContent.ProjectileType<TelegraphedStarLaserbeam>(), SunLaserDamage, 0f, -1, flareTelegraphTime, fireBlastLaserShootTime, flareSpinCoverage / flareTelegraphTime);
                }

                SunBlenderBeams_FlareBurstCounter++;
                NPC.netUpdate = true;
            }
        }
    }
}
