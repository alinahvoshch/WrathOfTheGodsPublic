using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    /// <summary>
    /// How long the Avatar's Arm Portal Strikes attack should wait before releasing portals.
    /// </summary>
    public static int ArmPortalStrikes_AttackDelay => GetAIInt("ArmPortalStrikes_AttackDelay");

    /// <summary>
    /// How long the Avatar's Arm Portal Strikes attack should go on for.
    /// </summary>
    public static int ArmPortalStrikes_AttackDuration => GetAIInt("ArmPortalStrikes_AttackDuration");

    /// <summary>
    /// How long portals summoned during the Avatar's Arm Portal Strikes attack last for.
    /// </summary>
    public static int ArmPortalStrikes_PortalLifetime => GetAIInt("ArmPortalStrikes_PortalLifetime");

    /// <summary>
    /// The rate at which the Avatar summons portals during his Arm Portal Strikes attack.
    /// </summary>
    public static int ArmPortalStrikes_PortalSummonRate => GetAIInt("ArmPortalStrikes_PortalSummonRate");

    /// <summary>
    /// The amount of damage the Avatar's antimatter blasts do.
    /// </summary>
    public static int AntimatterBlastDamage => GetAIInt("AntimatterBlastDamage");

    [AutomatedMethodInvoke]
    public void LoadState_ArmPortalStrikes()
    {
        StateMachine.RegisterTransition(AvatarAIType.ArmPortalStrikes, null, false, () =>
        {
            return AITimer >= ArmPortalStrikes_AttackDuration;
        });

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(AvatarAIType.ArmPortalStrikes, DoBehavior_ArmPortalStrikes);

        StatesToNotStartTeleportDuring.Add(AvatarAIType.ArmPortalStrikes);
        AttackDimensionRelationship[AvatarAIType.ArmPortalStrikes] = AvatarDimensionVariants.DarkDimension;
    }

    public void DoBehavior_ArmPortalStrikes()
    {
        float portalScale = 0.8f;
        int portalLifetime = ArmPortalStrikes_PortalLifetime;

        NPC.Center = Vector2.Lerp(NPC.Center, Target.Center - Vector2.UnitY * 300f, 0.04f);
        ZPosition = Lerp(ZPosition, InverseLerp(0f, 40f, AITimer).Squared() * 2.3f, 0.16f);

        // For visual clarity, disable Solyn's attacking behavior.
        SolynAction = s => StandardSolynBehavior_FlyNearPlayer(s, NPC);

        Vector2 leftArmHoverOffset = -Vector2.UnitX * 310f;
        Vector2 rightArmHoverOffset = -leftArmHoverOffset;
        NPC.velocity *= 0.6f;

        bool releaseCycleHit = AITimer % ArmPortalStrikes_PortalSummonRate == 0;
        if (Main.netMode != NetmodeID.MultiplayerClient && AITimer <= ArmPortalStrikes_AttackDuration - 75 && AITimer >= ArmPortalStrikes_AttackDelay && releaseCycleHit)
        {
            float predictiveAimInterpolant = InverseLerp(6f, 11f, Target.Velocity.Length()) * 0.35f;
            if (NPC.HasPlayerTarget)
            {
                Player playerTarget = Main.player[NPC.TranslatedTargetIndex];
                if (playerTarget.mount?.Active ?? false)
                    predictiveAimInterpolant *= 2.3f;
            }

            Vector2 randomSpawnPosition = Target.Center + Target.Velocity * Main.rand.NextFloat(24f, 90f) + Main.rand.NextVector2Circular(200f, 200f);
            Vector2 predictiveSpawnPosition = Target.Center + Target.Velocity * new Vector2(96f, 142f) + Main.rand.NextVector2Circular(50f, 50f);
            Vector2 portalSpawnPosition = Vector2.Lerp(randomSpawnPosition, predictiveSpawnPosition, Saturate(predictiveAimInterpolant));

            Vector2 portalDirection = portalSpawnPosition.SafeDirectionTo(Target.Center);
            int shootType = (int)DarkPortal.PortalAttackAction.StrikingArm;
            if (Main.rand.NextBool(5))
            {
                shootType = (int)DarkPortal.PortalAttackAction.AntimatterBlasts;
                portalScale *= 0.6f;
            }
            else
                portalDirection *= 5f;

            NewProjectileBetter(NPC.GetSource_FromAI(), portalSpawnPosition, portalDirection, ModContent.ProjectileType<DarkPortal>(), 0, 0f, -1, portalScale, portalLifetime, shootType);
        }

        PerformStandardLimbUpdates(0.9f, leftArmHoverOffset, rightArmHoverOffset);
    }
}
