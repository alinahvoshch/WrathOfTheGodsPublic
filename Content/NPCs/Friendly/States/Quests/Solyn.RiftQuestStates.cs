using Luminance.Common.StateMachines;
using Luminance.Core.Cutscenes;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.UI.SolynDialogue;
using NoxusBoss.Core.World.GameScenes.SolynEventHandlers;
using NoxusBoss.Core.World.WorldGeneration;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Friendly;

public partial class Solyn : ModNPC, IPixelatedPrimitiveRenderer
{
    /// <summary>
    /// Whether Solyn is waiting for the player to enter the Ceaseless Void rift.
    /// </summary>
    public bool WaitingToEnterCVRift
    {
        get;
        set;
    }

    /// <summary>
    /// How long Solyn should use her shocked expression.
    /// </summary>
    public int ShockedExpressionCountdown
    {
        get;
        set;
    }

    /// <summary>
    /// Whether Solyn has played the rift entering sound yet or not.
    /// </summary>
    public bool FlyIntoRift_HasPlayedRiftEnterSound
    {
        get;
        set;
    }

    /// <summary>
    /// The position where Solyn started waiting
    /// </summary>
    public Vector2 WaitNearCeaselessVoidRift_WaitPosition
    {
        get;
        set;
    }

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_RiftQuestStates()
    {
        StateMachine.RegisterTransition(SolynAIType.IncospicuouslyFlyAwayToDungeon, SolynAIType.WaitNearCeaselessVoidRift, false, () =>
        {
            Player nearest = Main.player[Player.FindClosest(NPC.Center, 1, 1)];

            // Wait until no players are nearby before teleporting to the rift.
            if (!NPC.WithinRange(nearest.Center, 3032f))
            {
                Vector2 riftPosition = CeaselessVoidQuestSystem.RiftDefeatPosition;
                Vector2 teleportPosition = riftPosition;

                for (int i = 0; i < 50; i++)
                {
                    Vector2 teleportOffset = Vector2.UnitX * Main.rand.NextFloatDirection() * 200f;
                    teleportPosition = FindGround((riftPosition + teleportOffset).ToTileCoordinates(), Vector2.UnitY).ToWorldCoordinates(8f, 0f);

                    if (teleportPosition.WithinRange(riftPosition, 350f) && Distance(teleportPosition.Y, teleportPosition.Y) <= 92f)
                        break;
                }

                TeleportTo(teleportPosition);

                NPC.noTileCollide = false;
                NPC.noGravity = false;
                WaitNearCeaselessVoidRift_WaitPosition = teleportPosition - Vector2.UnitY * NPC.height * 0.5f;

                return true;
            }
            return false;
        });
        StateMachine.RegisterTransition(SolynAIType.WaitNearCeaselessVoidRift, SolynAIType.StandStill, false, () =>
        {
            Player nearest = Main.player[Player.FindClosest(NPC.Center, 1, 1)];
            return NPC.WithinRange(nearest.Center, 480f);
        }, () =>
        {
            WaitingToEnterCVRift = true;
            CurrentConversation = SolynDialogRegistry.SolynQuest_CeaselessVoidBeforeEnteringRift;
        });
        StateMachine.RegisterTransition(SolynAIType.FlyIntoRift, SolynAIType.WaitInsideRift, false, () =>
        {
            return NPC.WithinRange(CeaselessVoidQuestSystem.RiftDefeatPosition, 24f);
        });
        StateMachine.RegisterTransition(SolynAIType.WaitInsideRift, SolynAIType.ExitRift, false, () =>
        {
            return AITimer >= 30 && !CutsceneManager.IsActive(ModContent.GetInstance<SolynEnteringRiftScene>());
        });
        StateMachine.RegisterTransition(SolynAIType.ExitRift, SolynAIType.SpeakToPlayer, false, () =>
        {
            return NPC.WithinRange(WaitNearCeaselessVoidRift_WaitPosition, 18f);
        }, () =>
        {
            CurrentConversation = SolynDialogRegistry.SolynQuest_CeaselessVoidAfterEnteringRift;

            Player nearest = Main.player[Player.FindClosest(NPC.Center, 1, 1)];
            nearest.SetTalkNPC(NPC.whoAmI);

            ShockedExpressionCountdown = SecondsToFrames(1.5f);

            NPC.noTileCollide = false;
            NPC.velocity = Vector2.Zero;
            NPC.noGravity = true;
        });
        StateMachine.RegisterTransition(SolynAIType.TeleportHome, SolynAIType.StandStill, false, () =>
        {
            return AITimer >= 90;
        }, () =>
        {
            TeleportTo(SolynCampsiteWorldGen.CampSitePosition);
            WaitingToEnterCVRift = false;
        });

        StateMachine.RegisterStateBehavior(SolynAIType.IncospicuouslyFlyAwayToDungeon, DoBehavior_IncospicuouslyFlyAwayToDungeon);
        StateMachine.RegisterStateBehavior(SolynAIType.WaitNearCeaselessVoidRift, DoBehavior_WaitNearCeaselessVoidRift);
        StateMachine.RegisterStateBehavior(SolynAIType.FlyIntoRift, DoBehavior_FlyIntoRift);
        StateMachine.RegisterStateBehavior(SolynAIType.WaitInsideRift, DoBehavior_WaitInsideRift);
        StateMachine.RegisterStateBehavior(SolynAIType.ExitRift, DoBehavior_ExitRift);
    }

    /// <summary>
    /// Performs Solyn's fly-away-to-the-dungeon behavior.
    /// </summary>
    public void DoBehavior_IncospicuouslyFlyAwayToDungeon()
    {
        float flySpeedInterpolant = InverseLerp(0f, 60f, AITimer).Squared() * 0.1f;
        Vector2 flyDestination = new Vector2(Main.dungeonX, Main.dungeonY) * 16f;

        NPC.noTileCollide = true;
        NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.SafeDirectionTo(flyDestination) * 14f, flySpeedInterpolant);

        float riseUpwardAcceleration = InverseLerp(600f, 1800f, NPC.Distance(flyDestination)) * 1.2f;
        if (Collision.SolidCollision(NPC.TopLeft - Vector2.UnitX * 200f, NPC.width + 400, NPC.height + 200) || !Collision.CanHit(NPC.Center, 1, 1, NPC.Center + Vector2.UnitX * NPC.spriteDirection * 800f, 1, 1))
            NPC.velocity.Y -= riseUpwardAcceleration;

        UseStarFlyEffects();
        NPC.spriteDirection = NPC.velocity.X.NonZeroSign();
    }

    /// <summary>
    /// Performs Solyn's wait-near-the-Ceaseless-Void-rift behavior.
    /// </summary>
    public void DoBehavior_WaitNearCeaselessVoidRift()
    {
        NPC.velocity.X = 0f;
        PerformStandardFraming();
    }

    /// <summary>
    /// Performs Solyn's fly-into-the-rift behavior.
    /// </summary>
    public void DoBehavior_FlyIntoRift()
    {
        WaitingToEnterCVRift = true;

        float flySpeedInterpolant = InverseLerp(0f, 60f, AITimer).Squared();
        Vector2 flyDestination = CeaselessVoidQuestSystem.RiftDefeatPosition;
        Vector2 idealVelocity = NPC.SafeDirectionTo(flyDestination) * flySpeedInterpolant * 12f;
        if (NPC.WithinRange(flyDestination, 20f))
            NPC.velocity = Vector2.Zero;
        else
        {
            NPC.velocity = Vector2.Lerp(NPC.velocity, idealVelocity, flySpeedInterpolant * 0.1f);
            NPC.spriteDirection = NPC.velocity.X.NonZeroSign();
        }

        NPC.Opacity = InverseLerp(36f, 90f, NPC.Distance(flyDestination));
        NPC.scale = Pow(NPC.Opacity, 1.5f);
        NPC.noTileCollide = true;
        NPC.noGravity = true;

        if (AITimer < 5)
            FlyIntoRift_HasPlayedRiftEnterSound = false;
        else if (!FlyIntoRift_HasPlayedRiftEnterSound && NPC.WithinRange(flyDestination, 120f))
        {
            ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 5f);
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.RiftPlayerAbsorb, NPC.Center);
            FlyIntoRift_HasPlayedRiftEnterSound = true;
        }

        UseStarFlyEffects();
    }

    /// <summary>
    /// Performs Solyn's wait-inside-of-rift behavior.
    /// </summary>
    public void DoBehavior_WaitInsideRift()
    {
        NPC.velocity = Vector2.Zero;
        NPC.Center = CeaselessVoidQuestSystem.RiftDefeatPosition;

        if (AITimer == 1)
            CutsceneManager.QueueCutscene(ModContent.GetInstance<SolynEnteringRiftScene>());
    }

    /// <summary>
    /// Performs Solyn's rift exiting behavior.
    /// </summary>
    public void DoBehavior_ExitRift()
    {
        if (AITimer == 1)
        {
            ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 6f);
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.RiftShoot with { MaxInstances = 8, Volume = 0.6f }, NPC.Center);

            // Fly out of the rift in a hurry, and with reduced health.
            NPC.life = (int)(NPC.lifeMax * 0.51f);
            NPC.velocity.X += (WaitNearCeaselessVoidRift_WaitPosition.X - CeaselessVoidQuestSystem.RiftDefeatPosition.X).NonZeroSign() * 56f;
            NPC.netUpdate = true;
        }

        NPC.scale = Saturate(NPC.scale + 0.05f);
        NPC.Opacity = Saturate(NPC.Opacity + 0.075f);

        NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.SafeDirectionTo(WaitNearCeaselessVoidRift_WaitPosition) * 7.5f, 0.08f);
        NPC.spriteDirection = NPC.velocity.X.NonZeroSign();

        NPC.noTileCollide = true;
        NPC.noGravity = true;

        UseStarFlyEffects();
        Frame = 43f;
    }

    /// <summary>
    /// Performs Solyn's teleport-home behavior.
    /// </summary>
    public void DoBehavior_TeleportHome()
    {
        NPC.velocity.X *= 0.81f;
        PerformStandardFraming();
    }
}
