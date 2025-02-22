using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using NoxusBoss.Core.Fixes;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity;

public partial class NamelessDeityBoss : ModNPC
{
    // AMERICA GRAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA!!
    /// <summary>
    /// How long Nameless spends shooting during his Glock state.
    /// </summary>
    public static int Glock_ShootTime => GetAIInt("Glock_ShootTime");

    /// <summary>
    /// The rate at which Nameless shoots during his Glock state.
    /// </summary>
    public static int Glock_ShootRate => (int)Clamp(Round(GetAIInt("Glock_ShootRate") / MathF.Max(Myself_DifficultyFactor - 1.85f, 1f)), 3f, 1000f);

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_Glock()
    {
        // Load the transition from SwordConstellation to the next in the cycle.
        StateMachine.RegisterTransition(NamelessAIType.Glock, null, false, () =>
        {
            return AITimer >= Glock_ShootTime;
        }, () =>
        {
            for (int i = 0; i < Hands.Count; i++)
                Hands[i].HasGlock = false;
        });

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(NamelessAIType.Glock, DoBehavior_Glock);
    }

    public void DoBehavior_Glock()
    {
        int shootRate = Glock_ShootRate;
        int shootTime = Glock_ShootTime;
        ref float verticalRecoilOffset = ref NPC.ai[2];

        // Update wings.
        UpdateWings(AITimer / 50f);

        // Teleport to the right of the player on the first frame.
        if (AITimer == 1)
            StartTeleportAnimation(() => Target.Center - Vector2.UnitX * 900f, 12, 12);

        // Hover near the player.
        NPC.SmoothFlyNearWithSlowdownRadius(Target.Center - Vector2.UnitX * 1108f, 0.15f, 0.85f, 55f);

        // Shoot stars from the glock.
        if (AITimer % shootRate == shootRate - 1)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.LowQualityGunShootSound with { Volume = 0.96f, MaxInstances = 10 });
            ScreenShakeSystem.StartShake(15f, shakeStrengthDissipationIncrement: 0.8f);
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 glockEnd = Hands[0].FreeCenter;

                ProjectileSpawnManagementSystem.PrepareProjectileForSpawning(star =>
                {
                    star.As<BackgroundStar>().ApproachingScreen = true;
                });
                NPC.NewProjectileBetter(NPC.GetSource_FromAI(), glockEnd, Vector2.Zero, ModContent.ProjectileType<BackgroundStar>(), 0, 0f, -1, 0.3f);

                // Apply a bit of recoil to Nameless.
                Vector2 pushBackForce = (Target.Center - glockEnd).SafeNormalize(Vector2.UnitY) * 20f;
                NPC.velocity -= pushBackForce;
                NPC.netUpdate = true;
            }

            verticalRecoilOffset = -450f;
        }

        // Use the United States flag after a round has been fired.
        if (AITimer >= shootRate)
            NamelessDeitySky.UnitedStatesFlagOpacity = 0.3f;

        // Play an eagle sound the first time the gun is shot.
        if (AITimer == shootRate)
            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.NotActuallyAnEagleSound);

        // Ensure the background sky stays normal.
        NamelessDeitySky.HeavenlyBackgroundIntensity = 1f;

        // Make the recoil go away over time.
        verticalRecoilOffset *= 0.87f;

        // Update hands.
        if (Hands.Count >= 2)
        {
            DefaultHandDrift(Hands[0], NPC.Center + NPC.SafeDirectionTo(Target.Center + Vector2.UnitY * (verticalRecoilOffset + 150f)) * 990f + Vector2.UnitX * verticalRecoilOffset * 0.56f, 3.1f);
            Hands[0].HasGlock = AITimer >= 2;
            Hands[0].HandType = NamelessDeityHandType.ClosedFist;
            Hands[0].DirectionOverride = -1;
            Hands[1].DirectionOverride = 1;

            DefaultHandDrift(Hands[1], NPC.Center + new Vector2(-900f, -120f) * TeleportVisualsAdjustedScale, 2.5f);
        }
    }
}
