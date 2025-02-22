using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.World.WorldSaving;
using Terraria;
using Terraria.Audio;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    public static int LeaveAfterBeingHit_Duration => SecondsToFrames(10f);

    [AutomatedMethodInvoke]
    public void LoadState_LeaveAfterBeingHit()
    {
        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(AvatarAIType.LeaveAfterBeingHit, DoBehavior_LeaveAfterBeingHit);
    }

    public void DoBehavior_LeaveAfterBeingHit()
    {
        // Disable music.
        Music = 0;

        // Play a sound at first.
        if (AITimer == 1)
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Murmur);

        // Disable damage.
        NPC.dontTakeDamage = true;

        // Slow down.
        NPC.velocity *= 0.9f;

        // Begin crying.
        BloodyTearsAnimationStartInterpolant = Saturate(BloodyTearsAnimationStartInterpolant + 0.03f);

        // Update hand grips.
        HandBaseGraspAngle = HandBaseGraspAngle.AngleTowards(0f, 0.03f);
        HandGraspAngle = HandGraspAngle.AngleTowards(0f, 0.03f);

        if (AITimer == 240)
        {
            StartTeleportAnimation(() =>
            {
                BossDownedSaveSystem.SetDefeatState<AvatarOfEmptiness>(true);
                NPC.active = false;
                return Target.Center + Vector2.UnitY * 10000f;
            });
        }

        // Make the head dangle.
        PerformBasicHeadUpdates(1.3f);
        HeadPosition += Main.rand.NextVector2Circular(6f, 2.5f);

        // Make the Avatar put his hands near his head.
        float animationInterpolant = BloodyTearsAnimationStartInterpolant;
        Vector2 leftArmDestination = HeadPosition + new Vector2(-400f, 300f) * NPC.scale * RightFrontArmScale;
        Vector2 rightArmDestination = HeadPosition + new Vector2(400f, 300f) * NPC.scale * RightFrontArmScale;
        LeftArmPosition = Vector2.Lerp(LeftArmPosition, leftArmDestination, 0.23f) + Main.rand.NextVector2Circular(6f, 2f) * animationInterpolant;
        RightArmPosition = Vector2.Lerp(RightArmPosition, rightArmDestination, 0.23f) + Main.rand.NextVector2Circular(6f, 2f) * animationInterpolant;

        // Release a bunch of blood particles on the Avatar's face.
        Vector2 bloodSpawnPosition = HeadPosition + Vector2.UnitY * NPC.scale * 160f;
        Color bloodColor = Color.Lerp(new(255, 0, 30), Color.Brown, Main.rand.NextFloat(0.4f, 0.8f)) * animationInterpolant * 0.45f;
        LargeMistParticle blood = new LargeMistParticle(bloodSpawnPosition, Main.rand.NextVector2Circular(8f, 6f) + Vector2.UnitY * 2.9f, bloodColor, 1f, 0f, 45, 0f, true);
        blood.Spawn();
        for (int i = 0; i < animationInterpolant * 3f; i++)
        {
            bloodColor = Color.Lerp(new(255, 36, 0), new(73, 10, 2), Main.rand.NextFloat(0.15f, 0.7f));
            BloodParticle blood2 = new BloodParticle(bloodSpawnPosition + Main.rand.NextVector2Circular(80f, 50f), Main.rand.NextVector2Circular(4f, 3f) - Vector2.UnitY * 2f, 30, Main.rand.NextFloat(1.25f), bloodColor);
            blood2.Spawn();
        }

        // Shake the screen.
        ScreenShakeSystem.SetUniversalRumble(3f, TwoPi, null, 0.2f);
    }
}
