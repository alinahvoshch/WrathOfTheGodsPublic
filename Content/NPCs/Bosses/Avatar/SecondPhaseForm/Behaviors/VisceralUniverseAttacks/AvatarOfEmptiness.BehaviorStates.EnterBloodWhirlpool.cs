using Luminance.Common.DataStructures;
using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    /// <summary>
    /// How long the Avatar spends sending the player into the blood whirlpool.
    /// </summary>
    public static int EnterBloodWhirlpool_StateDuration => GetAIInt("EnterBloodWhirlpool_StateDuration");

    /// <summary>
    /// How long the Avatar waits before the darkness overlay effect starts consuming the screen when sending the player into the blood whirlpool.
    /// </summary>
    public static int EnterBloodWhirlpool_DarknessStartTime => GetAIInt("EnterBloodWhirlpool_DarknessStartTime");

    /// <summary>
    /// How long the Avatar waits before the darkness overlay effect completely consumes the screen when sending the player into the blood whirlpool.
    /// </summary>
    public static int EnterBloodWhirlpool_DarknessEndTime => GetAIInt("EnterBloodWhirlpool_DarknessEndTime");

    [AutomatedMethodInvoke]
    public void LoadState_EnterBloodWhirlpool()
    {
        StateMachine.RegisterTransition(AvatarAIType.EnterBloodWhirlpool, null, false, () =>
        {
            return AITimer >= EnterBloodWhirlpool_StateDuration;
        });

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(AvatarAIType.EnterBloodWhirlpool, DoBehavior_EnterBloodWhirlpool);

        StatesToNotStartTeleportDuring.Add(AvatarAIType.EnterBloodWhirlpool);
        AttackDimensionRelationship[AvatarAIType.EnterBloodWhirlpool] = AvatarDimensionVariants.VisceralDimension;
    }

    public void DoBehavior_EnterBloodWhirlpool()
    {
        HideBar = true;
        NPC.Center = Vector2.Lerp(NPC.Center, Target.Center - Vector2.UnitY * 300f, 0.04f);
        NPC.dontTakeDamage = true;
        NPC.Opacity = InverseLerp(10f, 6f, ZPosition);
        ZPosition = MathF.Max(ZPosition, InverseLerp(0f, 40f, AITimer).Squared() * 11f);

        // Neutralize all damaging projectiles, since the swirl will result in unreadable damageables otherwise.
        if (AITimer <= 5)
        {
            foreach (Projectile projectile in Main.ActiveProjectiles)
            {
                if (projectile.ModProjectile is not null and IProjOwnedByBoss<AvatarOfEmptiness>)
                    projectile.damage = 0;
            }
        }

        if (AITimer == 1)
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.AntishadowSwirl);

        if (Main.netMode != NetmodeID.Server)
        {
            ManagedScreenFilter swirlShader = ShaderManager.GetFilter("NoxusBoss.BloodWhirlpoolSwirlShader");
            swirlShader.TrySetParameter("animationCompletion", InverseLerp(0f, EnterBloodWhirlpool_StateDuration - 30, AITimer));
            swirlShader.TrySetParameter("overlayInterpolant", InverseLerpBump(EnterBloodWhirlpool_DarknessStartTime, EnterBloodWhirlpool_DarknessEndTime, EnterBloodWhirlpool_StateDuration - 5, EnterBloodWhirlpool_StateDuration, AITimer));
            swirlShader.TrySetParameter("overlayColor", AntishadowBackgroundColor);
            swirlShader.TrySetParameter("performSwirl", AITimer <= EnterBloodWhirlpool_StateDuration - 20);
            swirlShader.Activate();

            CameraPanSystem.ZoomIn(SmoothStep(0f, 0.32f, InverseLerp(0f, EnterBloodWhirlpool_StateDuration - 30, AITimer)));
        }

        if (AITimer >= EnterBloodWhirlpool_StateDuration - 30)
        {
            TotalScreenOverlaySystem.OverlayInterpolant = InverseLerp(EnterBloodWhirlpool_StateDuration - 30, EnterBloodWhirlpool_StateDuration - 15, AITimer) * 1.2f;
            TotalScreenOverlaySystem.OverlayColor = AntishadowBackgroundColor;
        }

        PerformStandardLimbUpdates(2f);
    }
}
