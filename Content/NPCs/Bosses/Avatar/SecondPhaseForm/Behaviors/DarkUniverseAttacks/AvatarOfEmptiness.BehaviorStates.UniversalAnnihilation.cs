using Luminance.Common.DataStructures;
using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;
using NoxusBoss.Core.Fixes;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    public ref float UniversalAnnihilation_GlimmerInterpolant => ref NPC.ai[2];

    public ref float UniversalAnnihilation_BackgroundBrightnessInterpolant => ref NPC.ai[3];

    /// <summary>
    /// How long the Avatar spends charging up before summoning stars during his Universal Annihilation attack.
    /// </summary>
    public static int UniversalAnnihilation_StarChargeUpTime => GetAIInt("UniversalAnnihilation_StarChargeUpTime");

    /// <summary>
    /// How long the Avatar spends waiting before summoning stars.
    /// </summary>
    public static int UniversalAnnihilation_StarExplodeDelay => GetAIInt("UniversalAnnihilation_StarExplodeDelay");

    /// <summary>
    /// How long the Avatar spends waiting before creating his annihilation sphere.
    /// </summary>
    public static int UniversalAnnihilation_AnnihilationSphereAppearDelay => GetAIInt("UniversalAnnihilation_AnnihilationSphereAppearDelay");

    /// <summary>
    /// How long the annihilation sphere spends expanding.
    /// </summary>
    public static int UniversalAnnihilation_AnnihilationSphereExpandTime => GetAIInt("UniversalAnnihilation_AnnihilationSphereExpandTime");

    /// <summary>
    /// How many stars are summoned by the Avatar during his Universal Annihilation attack.
    /// </summary>
    public static int UniversalAnnihilation_StellarRemnantCount => GetAIInt("UniversalAnnihilation_StellarRemnantCount");

    /// <summary>
    /// How much damage stellar remnants summoned by the Avatar do.
    /// </summary>
    public static int StellarRemnantDamage => GetAIInt("StellarRemnantDamage");

    /// <summary>
    /// How much damage the annihilation sphere summoned by the Avatar does per frame at maximum intensity.
    /// </summary>
    public static int AnnihilationSphereMaxPlayerDPS => GetAIInt("AnnihilationSphereMaxPlayerDPS");

    /// <summary>
    /// How much damage the annihilation sphere summoned by the Avatar does per frame at minimum intensity.
    /// </summary>
    public static int AnnihilationSphereMinPlayerDPS => GetAIInt("AnnihilationSphereMinPlayerDPS");

    /// <summary>
    /// How densely stars are clumped together during the Avatar's Universal Annihilation attack.
    /// </summary>
    public static float UniversalAnnihilation_StellarRemnantClumpingFactor => GetAIFloat("UniversalAnnihilation_StellarRemnantClumpingFactor");

    /// <summary>
    /// The minimum orbit radius of stellar remnants summoned by the Avatar during his Universal Annihilation attack.
    /// </summary>
    public static float UniversalAnnihilation_MinStarOrbitRadius => GetAIFloat("UniversalAnnihilation_MinStarOrbitRadius");

    /// <summary>
    /// The maximum orbit radius of stellar remnants summoned by the Avatar during his Universal Annihilation attack.
    /// </summary>
    public static float UniversalAnnihilation_MaxStarOrbitRadius => GetAIFloat("UniversalAnnihilation_MaxStarOrbitRadius");

    [AutomatedMethodInvoke]
    public void LoadState_UniversalAnnihilation()
    {
        StateMachine.RegisterTransition(AvatarAIType.UniversalAnnihilation, null, false, () =>
        {
            return AITimer >= UniversalAnnihilation_StarChargeUpTime + UniversalAnnihilation_StarExplodeDelay + UniversalAnnihilation_AnnihilationSphereAppearDelay + UniversalAnnihilation_AnnihilationSphereExpandTime;
        }, IProjOwnedByBoss<AvatarOfEmptiness>.KillAll);

        StateMachine.RegisterStateBehavior(AvatarAIType.UniversalAnnihilation, DoBehavior_UniversalAnnihilation);
        AttackDimensionRelationship[AvatarAIType.UniversalAnnihilation] = AvatarDimensionVariants.UniversalAnnihilationDimension;
    }

    public void DoBehavior_UniversalAnnihilation()
    {
        LookAt(Target.Center);

        NeedsToSelectNewDimensionAttacksSoon = false;

        NPC.dontTakeDamage = true;
        ZPosition = 0f;

        if (AITimer == 2 && !NPC.WithinRange(Target.Center, 720f))
            StartTeleportAnimation(() => Target.Center - Vector2.UnitY * 300f);

        if (AITimer == 3)
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.UniversalAnnihilationCharge with { Volume = 1.3f });

        UniversalAnnihilation_GlimmerInterpolant = InverseLerp(0f, UniversalAnnihilation_StarChargeUpTime, AITimer).Squared();
        LilyGlowIntensityBoost = InverseLerp(0.25f, 0.85f, UniversalAnnihilation_GlimmerInterpolant) * 3f;

        Vector2 armOffset = Vector2.UnitX * Lerp(-250f, 280f, UniversalAnnihilation_GlimmerInterpolant.Squared()) * InverseLerp(0f, 12f, AITimer);
        armOffset.X -= InverseLerp(0f, 12f, AITimer - UniversalAnnihilation_StarChargeUpTime - UniversalAnnihilation_StarExplodeDelay) * 540f;

        // Release a ton of stars outward.
        if (AITimer == UniversalAnnihilation_StarChargeUpTime + UniversalAnnihilation_StarExplodeDelay)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.UniversalAnnihilationBlast with { Volume = 1.5f });
            ScreenShakeSystem.StartShake(75f, shakeStrengthDissipationIncrement: 1f);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int remnantCount = UniversalAnnihilation_StellarRemnantCount;
                float clumpingFactor = UniversalAnnihilation_StellarRemnantClumpingFactor;
                for (int i = 0; i < remnantCount; i++)
                {
                    float radiusInterpolant = i / (float)(remnantCount - 1f) * 0.85f + Sqrt(Main.rand.NextFloat()) * 0.15f;
                    float starOffsetAngle = TwoPi * i / (clumpingFactor * remnantCount);
                    float starRadius = Lerp(UniversalAnnihilation_MinStarOrbitRadius, UniversalAnnihilation_MaxStarOrbitRadius, radiusInterpolant);
                    float starOrbitSquish = Main.rand.NextFloat(0.45f, 1f);

                    ProjectileSpawnManagementSystem.PrepareProjectileForSpawning(star =>
                    {
                        if (i % 19 == 0)
                            star.Resize(star.width + 900, star.height + 900);
                    });

                    NewProjectileBetter(NPC.GetSource_FromAI(), SpiderLilyPosition, Main.rand.NextVector2Circular(40f, 40f), ModContent.ProjectileType<StellarRemnant>(), StellarRemnantDamage, 0f, -1, starOffsetAngle, starOrbitSquish, starRadius);
                }

                UniversalAnnihilation_BackgroundBrightnessInterpolant = 1f;
                NPC.netUpdate = true;
            }
        }

        // Summon the annihilation sphere.
        if (AITimer == UniversalAnnihilation_StarChargeUpTime + UniversalAnnihilation_StarExplodeDelay + UniversalAnnihilation_AnnihilationSphereAppearDelay)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                NewProjectileBetter(NPC.GetSource_FromAI(), SpiderLilyPosition, Vector2.Zero, ModContent.ProjectileType<AnnihilationSphere>(), 0, 0f);
        }

        float superGlowInterpolant = InverseLerp(0f, 45f, AITimer - UniversalAnnihilation_StarChargeUpTime - UniversalAnnihilation_StarExplodeDelay);
        LilyGlowIntensityBoost += Convert01To010(superGlowInterpolant) * 18f + InverseLerp(0.6f, 1f, superGlowInterpolant) * Lerp(6.7f, 7f, Cos01(TwoPi * AITimer / 90f));

        if (UniversalAnnihilation_BackgroundBrightnessInterpolant > 0f)
            UniversalAnnihilation_BackgroundBrightnessInterpolant *= 0.998f;

        PerformStandardLimbUpdates(4f, armOffset, -armOffset);
    }

    public void UniversalAnnihilation_DrawGleam(Vector2 screenPos)
    {
        if (CurrentState != AvatarAIType.UniversalAnnihilation || UniversalAnnihilation_GlimmerInterpolant <= 0.001f)
            return;

        Vector2 drawPosition = SpiderLilyPosition + Vector2.UnitY * NPC.scale * 30f - screenPos;
        Texture2D flare = Luminance.Assets.MiscTexturesRegistry.ShineFlareTexture.Value;
        Texture2D bloom = BloomCircleSmall.Value;

        float flareOpacity = InverseLerp(1f, 0.9f, UniversalAnnihilation_GlimmerInterpolant);
        float flareScale = Pow(Convert01To010(UniversalAnnihilation_GlimmerInterpolant), 1.2f) * 7f + 0.1f;
        float flareRotation = SmoothStep(0f, TwoPi, Pow(UniversalAnnihilation_GlimmerInterpolant, 0.2f)) + PiOver4;
        Color flareColorA = new Color(254, 10, 90);
        Color flareColorB = Color.Orange;
        Color flareColorC = Color.Wheat;

        Main.spriteBatch.Draw(bloom, drawPosition, null, flareColorA with { A = 0 } * flareOpacity * 0.6f, 0f, bloom.Size() * 0.5f, flareScale * 1.9f, 0, 0f);
        Main.spriteBatch.Draw(bloom, drawPosition, null, flareColorB with { A = 0 } * flareOpacity * 0.8f, 0f, bloom.Size() * 0.5f, flareScale, 0, 0f);
        Main.spriteBatch.Draw(flare, drawPosition, null, flareColorC with { A = 0 } * flareOpacity, flareRotation, flare.Size() * 0.5f, flareScale, 0, 0f);
    }
}
