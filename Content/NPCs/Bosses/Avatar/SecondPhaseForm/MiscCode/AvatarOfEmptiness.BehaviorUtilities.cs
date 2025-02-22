using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.CrossCompatibility.Inbound.BossChecklist;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness : IBossChecklistSupport
{
    public void TargetClosest()
    {
        // Search for a target.
        NPCUtils.TargetSearchResults targetSearchResults = NPCUtils.SearchForTarget(NPC, NPCUtils.TargetSearchFlag.NPCs | NPCUtils.TargetSearchFlag.Players, _ => NamelessDeityBoss.Myself is null || ParadiseReclaimedIsOngoing, NamelessSearchCheck);

        // Terminate this method immediately if no valid target of any kind was found.
        if (!targetSearchResults.FoundTarget)
            return;

        // Prioritize the Nameless Deity as a target if he's present.
        int targetIndex = targetSearchResults.NearestTargetIndex;
        if (targetSearchResults.FoundNPC && !ParadiseReclaimedIsOngoing)
            targetIndex = targetSearchResults.NearestNPCIndex + 300;

        // Save target information.
        NPC.target = targetSearchResults.NearestTargetIndex;
        NPC.targetRect = targetSearchResults.NearestTargetHitbox;
    }

    public static bool NamelessSearchCheck(NPC npc)
    {
        return npc.type == ModContent.NPCType<NamelessDeityBoss>() && npc.Opacity > 0f && !npc.immortal && !npc.dontTakeDamage;
    }

    public static TwinkleParticle CreateTwinkle(Vector2 spawnPosition, Vector2 scaleFactor, bool playSound = true)
    {
        Color twinkleColor = Color.Lerp(Color.HotPink, Color.Cyan, Main.rand.NextFloat(0.36f, 0.64f));
        TwinkleParticle twinkle = new TwinkleParticle(spawnPosition, Vector2.Zero, twinkleColor, 30, 6, scaleFactor);
        twinkle.Spawn();

        if (playSound)
            SoundEngine.PlaySound(GennedAssets.Sounds.Common.Twinkle with { MaxInstances = 5, PitchVariance = 0.16f, Pitch = -0.26f });
        return twinkle;
    }

    /// <summary>
    /// Makes the Avatar's mask look at a given point in space.
    /// </summary>
    /// <param name="destination">The position to look at.</param>
    public void LookAt(Vector2 destination)
    {
        float idealMaskRotation = (destination.Y - HeadPosition.Y) * NPC.HorizontalDirectionTo(destination) * 0.0015f;
        if (ZPosition > 0f)
            idealMaskRotation *= ZPosition + 1f;

        // Apply soft clamping.
        float maxAngleOffset = 0.243f;
        idealMaskRotation = Tanh(idealMaskRotation / maxAngleOffset) * maxAngleOffset;

        MaskRotation = MaskRotation.AngleLerp(idealMaskRotation, 0.1f);
    }

    /// <summary>
    /// Performs standard, basic updates for the front arms and head.
    /// </summary>
    /// <param name="speedInterpolantFactor">The factor for the movement speed. Defaults to 1.</param>
    /// <param name="leftArmOffset">The offset for the left arm's hover destination. Defaults to <see cref="Vector2.Zero"/>.</param>
    /// <param name="rightArmOffset">The offset for the right arm's hover destination. Defaults to <see cref="Vector2.Zero"/>.</param>
    public void PerformStandardLimbUpdates(float speedInterpolantFactor = 1f, Vector2 leftArmOffset = default, Vector2 rightArmOffset = default)
    {
        // Update the front arms.
        PerformBasicFrontArmUpdates(speedInterpolantFactor, leftArmOffset, rightArmOffset);

        // Update the head.
        PerformBasicHeadUpdates(speedInterpolantFactor);
    }

    /// <summary>
    /// Updates the Avatar's front arms in a simple manner, having them simply fly up and down.
    /// </summary>
    /// <param name="speedInterpolantFactor">The factor for the movement speed. Defaults to 1.</param>
    /// <param name="leftArmOffset">The offset for the left arm's hover destination. Defaults to <see cref="Vector2.Zero"/>.</param>
    /// <param name="rightArmOffset">The offset for the right arm's hover destination. Defaults to <see cref="Vector2.Zero"/>.</param>
    public void PerformBasicFrontArmUpdates(float speedInterpolantFactor = 1f, Vector2 leftArmOffset = default, Vector2 rightArmOffset = default)
    {
        PerformBasicFrontArmUpdates_Left(speedInterpolantFactor, leftArmOffset);
        PerformBasicFrontArmUpdates_Right(speedInterpolantFactor, rightArmOffset);
    }

    /// <summary>
    /// Updates the Avatar's left front arm in a simple manner, having it simply fly up and down.
    /// </summary>
    /// <param name="speedInterpolantFactor">The factor for the movement speed. Defaults to 1.</param>
    /// <param name="armOffset">The offset for the arm's hover destination. Defaults to <see cref="Vector2.Zero"/>.</param>
    public void PerformBasicFrontArmUpdates_Left(float speedInterpolantFactor = 1f, Vector2 armOffset = default)
    {
        // Move the arms up and down.
        Vector2 leftArmJutDirection = (-Vector2.UnitX).RotatedBy(-0.59f);
        Vector2 leftArmDestination = NPC.Center + (leftArmJutDirection * 540f + Vector2.UnitY * Cos(TwoPi * FightTimer / 150f) * 60f) * NPC.scale * LeftFrontArmScale;
        leftArmDestination.X -= NPC.scale * LeftFrontArmScale * 200f;

        // Apply offsets.
        leftArmDestination += armOffset;
        LeftArmPosition = Vector2.Lerp(LeftArmPosition, leftArmDestination, Saturate(speedInterpolantFactor * 0.16f));
    }

    /// <summary>
    /// Updates the Avatar's right front arm in a simple manner, having it simply fly up and down.
    /// </summary>
    /// <param name="speedInterpolantFactor">The factor for the movement speed. Defaults to 1.</param>
    /// <param name="armOffset">The offset for the arm's hover destination. Defaults to <see cref="Vector2.Zero"/>.</param>
    public void PerformBasicFrontArmUpdates_Right(float speedInterpolantFactor = 1f, Vector2 armOffset = default)
    {
        // Move the arms up and down.
        Vector2 rightArmJutDirection = Vector2.UnitX.RotatedBy(0.59f);
        Vector2 rightArmDestination = NPC.Center + (rightArmJutDirection * 540f + Vector2.UnitY * Cos(TwoPi * FightTimer / 150f + 1.91f) * 60f) * NPC.scale * RightFrontArmScale;
        rightArmDestination.X += NPC.scale * RightFrontArmScale * 200f;

        // Apply offsets.
        rightArmDestination += armOffset;
        RightArmPosition = Vector2.Lerp(RightArmPosition, rightArmDestination, Saturate(speedInterpolantFactor * 0.16f));
    }

    /// <summary>
    /// Updates the Avatar's head in a simple manner, simply limping along underneath the rift.
    /// </summary>
    /// <param name="speedInterpolantFactor">The factor for the movement speed. Defaults to 1.</param>
    public void PerformBasicHeadUpdates(float speedInterpolantFactor = 1f)
    {
        // Make the head dangle.
        float speedInterpolant = Saturate(speedInterpolantFactor * 0.14f);
        float verticalOffset = Cos01(TwoPi * FightTimer / 90f) * 12f + 415f;
        HeadPosition = Vector2.Lerp(HeadPosition, NPC.Center + new Vector2(3f, verticalOffset) * HeadScale * NeckAppearInterpolant, speedInterpolant);
    }
}
