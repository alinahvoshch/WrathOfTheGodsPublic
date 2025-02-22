using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Core.World.WorldGeneration;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Friendly;

public partial class Solyn : ModNPC, IPixelatedPrimitiveRenderer
{
    /// <summary>
    /// Whether Solyn is considered close to a player.
    /// </summary>
    public bool CloseToPlayer
    {
        get
        {
            Player closest = Main.player[Player.FindClosest(NPC.Center, 1, 1)];
            return NPC.WithinRange(closest.Center, 150f);
        }
    }

    /// <summary>
    /// How long Solyn has been stuck during her wander about state, in frames.
    /// </summary>
    public ref float WanderAbout_StuckTimer => ref NPC.ai[0];

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_WanderAbout()
    {
        StateMachine.RegisterTransition(SolynAIType.WanderAbout, SolynAIType.StandStill, false, () =>
        {
            bool reachedDestination = AITimer >= 10 && NPC.WithinRange(WanderDestination, 150f);
            return reachedDestination || WanderAbout_StuckTimer >= 150f || CloseToPlayer;
        }, () =>
        {
            WanderAbout_StuckTimer = 0f;

            Player closest = Main.player[Player.FindClosest(NPC.Center, 1, 1)];
            if (CloseToPlayer)
            {
                NPC.velocity.X = 0f;
                NPC.spriteDirection = (closest.Center.X - NPC.Center.X).NonZeroSign();
            }
        });

        StateMachine.RegisterStateBehavior(SolynAIType.WanderAbout, DoBehavior_WanderAbout);
    }

    /// <summary>
    /// Performs Solyn's wandering about state.
    /// </summary>
    public void DoBehavior_WanderAbout()
    {
        float walkSpeed = 1.2f;
        float walkAccelerationInterpolant = 0.15f;
        bool onGround = Abs(NPC.velocity.Y) <= 0.3f && Collision.SolidCollision(NPC.BottomLeft, NPC.width, 16, true);

        // Keep the kite out.
        ReelInKite = false;

        // Silently teleport home if there are no players nearby to see and she's stuck.
        Player closest = Main.player[Player.FindClosest(NPC.Center, 1, 1)];
        bool stuck = SolynCampsiteWorldGen.TentPosition != Vector2.Zero &&
            !NPC.WithinRange(SolynCampsiteWorldGen.TentPosition, 750f) &&
            !Collision.CanHitLine(NPC.Center, 1, 1, SolynCampsiteWorldGen.TentPosition, 1, 1);
        if (!NPC.WithinRange(closest.Center, 2000f) && !closest.WithinRange(SolynCampsiteWorldGen.TentPosition, 2000f) && stuck)
        {
            NPC.Center = SolynCampsiteWorldGen.TentPosition - Vector2.UnitY * 32f;
            NPC.velocity = Vector2.Zero;
            NPC.netUpdate = true;
        }

        // Choose a location to wander towards at first.
        if (Main.netMode != NetmodeID.MultiplayerClient && AITimer == 1)
        {
            for (int i = 0; i < 400; i++)
            {
                // Pick a random horizontal offset to move towards, taking care to not stray too far away from the initial spawn position, so that Solyn doesn't outright leave the town and get lost.
                float horizontalOffset = Main.rand.NextFloat(400f, 750f) * Main.rand.NextFromList(-1f, 1f);
                WanderDestination = NPC.Center + Vector2.UnitX * horizontalOffset;
                WanderDestination = new Vector2(Lerp(WanderDestination.X, SpawnPositionX, Main.rand.NextFloat(0.5f)), WanderDestination.Y);

                Vector2 startingDestination = WanderDestination;

                // Shift the elevation as necessary so that the destination isn't in the ground or air.
                if (WorldGen.InWorld((int)(WanderDestination.X / 16f), (int)(WanderDestination.Y / 16f), 5))
                    WanderDestination = FindGroundVertical(WanderDestination.ToTileCoordinates()).ToWorldCoordinates();

                // Don't walk into water, dummy!
                if (Collision.WetCollision(WanderDestination - Vector2.UnitY * 54f, 1, 54))
                    continue;

                // Don't fall into holes, dummy!
                bool wouldFallIntoHole = !startingDestination.WithinRange(WanderDestination, 50f);
                if (!wouldFallIntoHole)
                    break;
            }

            // Send a net update.
            NPC.spriteDirection = (WanderDestination.X - NPC.Center.X).NonZeroSign();
            NPC.velocity.X = NPC.spriteDirection * 0.5f;
            NPC.netUpdate = true;
            return;
        }

        // Calculate the ideal walk speed, slowing down in the face of an obstacle.
        float idealWalkSpeed = NPC.SafeDirectionTo(WanderDestination).X * walkSpeed;
        bool wallAhead = Collision.SolidTilesVersatile((int)(NPC.Center.X / 16f), (int)(NPC.Center.X + NPC.spriteDirection * 120) / 16, (int)NPC.Top.Y / 16, (int)NPC.Bottom.Y / 16 - 3);
        if (wallAhead)
        {
            idealWalkSpeed = 0f;
            WanderAbout_StuckTimer++;
        }
        else
            WanderAbout_StuckTimer = 0f;

        // Walk towards the wander destination.
        if (onGround)
        {
            NPC.velocity.X = Lerp(NPC.velocity.X, idealWalkSpeed, walkAccelerationInterpolant);

            // Look forward.
            if (Abs(NPC.velocity.X) >= 0.4f)
                NPC.spriteDirection = NPC.velocity.X.NonZeroSign();
        }

        // Jump if there's a wall closely ahead.
        bool wallCloselyAhead = Collision.SolidTilesVersatile((int)(NPC.Center.X / 16f), (int)(NPC.Center.X + NPC.spriteDirection * 50) / 16, (int)NPC.Top.Y / 16, (int)NPC.Bottom.Y / 16 - 2);
        if (wallCloselyAhead && onGround)
        {
            NPC.velocity.Y = -6.4f;
            NPC.velocity.X = NPC.spriteDirection * 3f;
            NPC.netUpdate = true;
        }

        // Jump if there's a gap closely ahead.
        bool gapAhead = !Collision.SolidTiles(NPC.Bottom + Vector2.UnitX * NPC.spriteDirection * 48f, 80, 32);
        if (gapAhead && onGround)
        {
            NPC.velocity.Y = -5f;
            NPC.velocity.X = NPC.spriteDirection * 4f;
        }

        // Walk up tiles.
        float _ = 0.3f;
        Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref _, ref NPC.gfxOffY);

        PerformStandardFraming();
    }
}
