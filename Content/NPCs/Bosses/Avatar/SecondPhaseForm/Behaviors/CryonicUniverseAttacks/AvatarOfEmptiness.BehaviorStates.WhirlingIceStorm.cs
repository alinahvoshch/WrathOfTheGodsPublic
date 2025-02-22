using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    /// <summary>
    /// The horizontal direction wind spinds during the Avatar's Whirling Ice Storm attack.
    /// </summary>
    public ref float WhirlingIceStorm_WindSpindDirection => ref NPC.ai[2];

    /// <summary>
    /// The countdown until the latest frost column telegraph bursts during the Avatar's Whirling Ice Storm attack.
    /// </summary>
    public ref float WhirlingIceStorm_ColumnBurstCountdown => ref NPC.ai[3];

    /// <summary>
    /// How long the Avatar's Whirling Ice Storm attack should go on for, in frames.
    /// </summary>
    public static int WhirlingIceStorm_AttackDuration => GetAIInt("WhirlingIceStorm_AttackDuration");

    /// <summary>
    /// The rate at which ice blasts should be created during the Avatar's Whirling Ice Storm attack, in frames.
    /// </summary>
    public static int WhirlingIceStorm_BlastReleaseRate => GetAIInt("WhirlingIceStorm_BlastReleaseRate");

    /// <summary>
    /// The rate at which frost illusions should be created during the Avatar's Whirling Ice Storm attack, in frames.
    /// </summary>
    public static int WhirlingIceStorm_FrostIllusionReleaseRate => GetAIInt("WhirlingIceStorm_FrostIllusionReleaseRate");

    /// <summary>
    /// The rate at which frost columns should be created during the Avatar's Whirling Ice Storm attack, in frames.
    /// </summary>
    public static int WhirlingIceStorm_FrostColumnReleaseRate => GetAIInt("WhirlingIceStorm_FrostColumnReleaseRate");

    /// <summary>
    /// How long it takes for arctic blasts, in frames, to start doing damage to players.
    /// </summary>
    public static int WhirlingIceStorm_ArcticBlastDamageGracePeriod => GetAIInt("WhirlingIceStorm_ArcticBlastDamageGracePeriod");

    /// <summary>
    /// How many arctic blasts should be summoned at the very start of the Avatar's Whirling Ice Storm attack.
    /// </summary>
    public static int WhirlingIceStorm_InitialArcticBlastCount => GetAIInt("WhirlingIceStorm_InitialArcticBlastCount");

    /// <summary>
    /// The amount of damage the Avatar's ice blasts do.
    /// </summary>
    public static int IceBlastDamage => GetAIInt("IceBlastDamage");

    /// <summary>
    /// The amount of damage frost columns summoned by the Avatar do.
    /// </summary>
    public static int FrostColumnDamage => GetAIInt("FrostColumnDamage");

    [AutomatedMethodInvoke]
    public void LoadState_WhirlingIceStorm()
    {
        StateMachine.RegisterTransition(AvatarAIType.WhirlingIceStorm, null, false, () =>
        {
            return AITimer >= WhirlingIceStorm_AttackDuration;
        });

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(AvatarAIType.WhirlingIceStorm, DoBehavior_WhirlingIceStorm);

        StatesToNotStartTeleportDuring.Add(AvatarAIType.WhirlingIceStorm);
        AttackDimensionRelationship[AvatarAIType.WhirlingIceStorm] = AvatarDimensionVariants.CryonicDimension;
    }

    public void DoBehavior_WhirlingIceStorm()
    {
        if (AITimer <= 3)
        {
            NPC.Opacity = 0f;
            ZPosition = 40f;
        }

        NPC.Opacity = Saturate(NPC.Opacity + 0.045f);
        ZPosition = Lerp(ZPosition, 6.4f, 0.15f);
        NPC.SmoothFlyNear(Target.Center - Vector2.UnitY * 270f, 0.15f, 0.8f);

        // Summon a bunch of blasts on the first frame, to give the impression that the storm was already ongoing when the player arrived.
        if (AITimer == 1)
        {
            WhirlingIceStorm_WindSpindDirection = Main.rand.NextFromList(-1f, 1f);
            NPC.netUpdate = true;

            int blastCount = WhirlingIceStorm_InitialArcticBlastCount;
            for (int i = 0; i < blastCount; i++)
                DoBehavior_WhirlingIceStorm_CreateBlasts(Main.rand.Next(120));
        }

        if (AITimer % WhirlingIceStorm_BlastReleaseRate == 0)
            DoBehavior_WhirlingIceStorm_CreateBlasts(0);

        if (AITimer % WhirlingIceStorm_FrostIllusionReleaseRate == 1)
            DoBehavior_WhirlingIceStorm_CreateFrostIllusion();

        Vector2 leftArmOffset = Vector2.Zero;
        Vector2 rightArmOffset = Vector2.Zero;
        if (AITimer < WhirlingIceStorm_AttackDuration - WhirlingIceStorm_FrostColumnReleaseRate)
        {
            int wrappedTimer = AITimer % WhirlingIceStorm_FrostColumnReleaseRate;
            if (wrappedTimer == WhirlingIceStorm_FrostColumnReleaseRate / 2)
                DoBehavior_WhirlingIceStorm_CreateFrostColumnTelegraph();
        }

        if (WhirlingIceStorm_ColumnBurstCountdown >= 1f)
        {
            leftArmOffset.Y += 450f;
            rightArmOffset.Y += 400f;
            HandGraspAngle *= 0.9f;
        }
        else
        {
            leftArmOffset.X += 150f;
            leftArmOffset.Y -= 200f;
            rightArmOffset.Y -= 200f;
            HandGraspAngle = -0.29f;
        }

        if (WhirlingIceStorm_ColumnBurstCountdown >= 1f)
            WhirlingIceStorm_ColumnBurstCountdown--;

        PerformStandardLimbUpdates(1.3f, leftArmOffset, rightArmOffset);
        GenerateCryonicDimensionSnowflakes();
    }

    public void GenerateCryonicDimensionSnowflakes()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(13))
        {
            float snowOffsetAngle = Main.rand.NextFloat(TwoPi);
            Vector2 snowSpawnPosition = Target.Center + new Vector2(Main.rand.NextFloatDirection() * 1400f, -1200f).RotatedBy(snowOffsetAngle);
            Vector2 snowVelocity = Vector2.UnitY.RotatedBy(snowOffsetAngle) * 7.5f;
            NewProjectileBetter(NPC.GetSource_FromAI(), snowSpawnPosition, snowVelocity, ModContent.ProjectileType<Snowflake>(), 0, 0f);
        }
    }

    public void DoBehavior_WhirlingIceStorm_CreateBlasts(int timeOffset)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        Vector2 blastSpawnPosition = Target.Center + new Vector2(Target.Velocity.X * 80f, Main.rand.NextFloatDirection() * 1900f);

        if (timeOffset > 0)
        {
            float z = 0f;
            Vector2 positionOffset = Vector2.Zero;
            for (int i = 0; i < timeOffset; i++)
                positionOffset += ArcticBlast.SpinAround(WhirlingIceStorm_WindSpindDirection, i, blastSpawnPosition.Y, ref z);
        }

        NewProjectileBetter(NPC.GetSource_FromAI(), blastSpawnPosition, Vector2.Zero, ModContent.ProjectileType<ArcticBlast>(), IceBlastDamage, 0f, -1, WhirlingIceStorm_WindSpindDirection, timeOffset);
    }

    public void DoBehavior_WhirlingIceStorm_CreateFrostIllusion()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        Vector2 illusionSpawnPosition = Target.Center + Main.rand.NextVector2CircularEdge(1f, 0.5f) * Main.rand.NextFloat(800f, 960f);
        NewProjectileBetter(NPC.GetSource_FromAI(), illusionSpawnPosition, Vector2.Zero, ModContent.ProjectileType<FrostIllusion>(), 0, 0f);
    }

    public void DoBehavior_WhirlingIceStorm_CreateFrostColumnTelegraph()
    {
        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.FrostColumnChargeUp);
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        WhirlingIceStorm_ColumnBurstCountdown = FrostColumnTelegraph.Lifetime;
        NPC.netUpdate = true;

        Vector2 columnSpawnPosition = Target.Center + new Vector2(Target.Velocity.X * 75f + Main.rand.NextFloatDirection() * 225f, 1500f);
        columnSpawnPosition = Vector2.Clamp(columnSpawnPosition, Vector2.One * 300f, new Vector2(Main.rightWorld, Main.bottomWorld) - Vector2.One * 300f);

        NewProjectileBetter(NPC.GetSource_FromAI(), columnSpawnPosition, Vector2.Zero, ModContent.ProjectileType<FrostColumnTelegraph>(), 0, 0f);
    }
}
