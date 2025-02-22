using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using Terraria;
using Terraria.Audio;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    public static int GivePlayerHeadPats_Duration => SecondsToFrames(10f);

    [AutomatedMethodInvoke]
    public void LoadState_GivePlayerHeadpats()
    {
        StateMachine.RegisterTransition(AvatarAIType.GivePlayerHeadpats, null, false, () =>
        {
            return AITimer >= GivePlayerHeadPats_Duration;
        });
        StateMachine.RegisterTransition(AvatarAIType.GivePlayerHeadpats, AvatarAIType.LeaveAfterBeingHit, false, () => NPC.justHit);

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(AvatarAIType.GivePlayerHeadpats, DoBehavior_GivePlayerHeadpats);
    }

    public void DoBehavior_GivePlayerHeadpats()
    {
        int patRate = SecondsToFrames(0.85f);

        // Attempt to hover above the target.
        Vector2 hoverDestination = Target.Center + new Vector2(640f, -760f) * NPC.scale;
        NPC.SmoothFlyNear(hoverDestination, 0.09f, 0.925f);

        // Decide the headpat position and provide headpats to the player.
        Vector2 headpatPosition = Target.Center + Vector2.UnitY * Lerp(-50f, -110f, Sin01(TwoPi * FightTimer / patRate)) - Vector2.UnitY * 300f;
        if (Target.Center.X < NPC.Center.X)
        {
            LeftArmPosition = Vector2.Lerp(LeftArmPosition, headpatPosition - Vector2.UnitX * 98f, 0.16f);
            PerformBasicFrontArmUpdates_Right();
        }
        else
        {
            PerformBasicFrontArmUpdates_Left();
            RightArmPosition = Vector2.Lerp(RightArmPosition, headpatPosition + Vector2.UnitX * 98f, 0.16f);
        }

        HandBaseGraspAngle = 0.29f;
        HandGraspAngle = -0.1f;

        // Release cute particles.
        if (FightTimer % patRate == patRate - 10 && LeftArmPosition.WithinRange(Target.Center, 90f))
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Headpat with { Volume = 0.6f, MaxInstances = 0, PitchVariance = 0.14f }, Target.Center);
            for (int i = 0; i < 4; i++)
                Gore.NewGorePerfect(NPC.GetSource_FromThis(), Target.Center - Vector2.UnitY.RotatedByRandom(0.479f) * 30f, Main.rand.NextVector2CircularEdge(4f, 2f), 331);
        }

        // Make the head dangle.
        float verticalOffset = Cos01(TwoPi * FightTimer / 90f) * 12f + 415f;
        HeadPosition = Vector2.Lerp(HeadPosition, NPC.Center + new Vector2(3f, verticalOffset) * HeadScale, 0.25f);
    }
}
