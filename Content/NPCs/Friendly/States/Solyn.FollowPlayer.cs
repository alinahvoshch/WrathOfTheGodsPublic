using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics;
using NoxusBoss.Core.Pathfinding;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Friendly;

public partial class Solyn : ModNPC, IPixelatedPrimitiveRenderer
{
    /// <summary>
    /// Whether Solyn should fall through platforms or not.
    /// </summary>
    public bool DescendThroughSlopes
    {
        get;
        set;
    }

    /// <summary>
    /// The fly destination that Solyn should approach when following the player.
    /// </summary>
    public Vector2 FollowPlayer_FlyDestinationOverride
    {
        get;
        set;
    }

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_FollowPlayer()
    {
        StateMachine.RegisterStateBehavior(SolynAIType.FollowPlayer, DoBehavior_FollowPlayer);
    }

    public void PathfindWalkToDestination(Vector2 walkDestination, bool teleportIfSuperFar, bool returnToGroundIfPossible, float walkSpeed, float flySpeed, ref float flying, ref float tripTimer)
    {
        // Disallow departure.
        CanDepart = false;

        // Keep the kite out.
        ReelInKite = false;

        int tripAnimationTime = 120;
        bool onGround = Abs(NPC.velocity.Y) <= 0.3f && Collision.SolidCollision(NPC.BottomLeft, NPC.width, 16, true);

        if (flying == 1f)
        {
            Vector2 flyDestination = walkDestination;
            if (FollowPlayer_FlyDestinationOverride != Vector2.Zero)
                flyDestination = FollowPlayer_FlyDestinationOverride;

            DoBehavior_FollowPlayer_FlyToDestination(flyDestination, returnToGroundIfPossible, flySpeed, ref flying);
            tripTimer = 0f;
        }
        else if (tripTimer >= 1f)
            DoBehavior_FollowPlayer_HandlePostTripBehaviors(tripAnimationTime, ref tripTimer);
        else
            DoBehavior_FollowPlayer_WalkToDestination(walkDestination, onGround, teleportIfSuperFar, walkSpeed, ref flying, ref tripTimer);

        // Handle framing.
        if (flying == 1f)
            Frame = 25f;
        else if (tripTimer >= 1f)
        {
            float tripAnimationCompletion = tripTimer / tripAnimationTime;
            if (tripAnimationCompletion <= 1f)
                Frame = 24f;
            if (tripAnimationCompletion <= 0.76f)
                Frame = 23f;
            if (tripAnimationCompletion <= 0.19f)
                Frame = 22f;
        }
        else if (!onGround)
            Frame = 1f;
        else
            PerformStandardFraming();
    }

    /// <summary>
    /// Performs Solyn's player following state.
    /// </summary>
    public void DoBehavior_FollowPlayer()
    {
        Player playerToFollow = Main.player[Player.FindClosest(NPC.Center, 1, 1)];
        if (!NPC.WithinRange(playerToFollow.Center, 200f) || playerToFollow.talkNPC != NPC.whoAmI)
        {
            CanBeSpokenTo = false;
            if (playerToFollow.talkNPC == NPC.whoAmI)
                playerToFollow.SetTalkNPC(-1);
        }

        // Rapidly heal to prevent any chance of natural death.
        NPC.life = Utils.Clamp(NPC.life + 1, NPC.lifeMax / 4, NPC.lifeMax);
        NPC.immortal = true;

        ref float flying = ref NPC.ai[0];
        ref float tripTimer = ref NPC.ai[1];

        // Handle movement.
        Vector2 walkDestination = playerToFollow.Center - Vector2.UnitX * playerToFollow.direction * 60f;
        if (Collision.SolidCollision(walkDestination, 1, 1))
            walkDestination = playerToFollow.Center;

        while (Collision.LavaCollision(walkDestination, 1, 50))
        {
            walkDestination.Y -= 32f;
            flying = 1f;
        }

        float flySpeed = playerToFollow.velocity.Length() * 0.85f + 7.5f;
        float walkSpeed = playerToFollow.maxRunSpeed;
        if (walkSpeed < 4.5f)
            walkSpeed = 4.5f;

        PathfindWalkToDestination(walkDestination, true, false, walkSpeed, flySpeed, ref flying, ref tripTimer);
    }

    public void DoBehavior_FollowPlayer_HandlePostTripBehaviors(int tripAnimationTime, ref float tripTimer)
    {
        NPC.velocity.X *= 0.92f;

        tripTimer++;

        if (tripTimer == (int)(tripAnimationTime * 0.78f))
        {
            string text = Language.GetTextValue($"Mods.NoxusBoss.Solyn.TripDialogue{Main.rand.Next(1, 4)}");
            CombatText.NewText(NPC.Hitbox, DialogColorRegistry.SolynTextColor, text, true);
        }

        if (tripTimer == 4f)
            SoundEngine.PlaySound(GennedAssets.Sounds.Solyn.TripSplat, NPC.Center);

        if (tripTimer >= tripAnimationTime)
        {
            tripTimer = 0f;
            NPC.netUpdate = true;
        }
    }

    public void DoBehavior_FollowPlayer_FlyToDestination(Vector2 flyDestination, bool returnToGroundIfPossible, float flySpeed, ref float flying)
    {
        Vector2 idealVelocity = (flyDestination - Vector2.UnitY * 20f - NPC.Center).ClampLength(0f, flySpeed);

        NPC.velocity = Vector2.Lerp(NPC.velocity, idealVelocity, 0.05f).MoveTowards(idealVelocity, 0.2f);
        if (Vector2.Dot(NPC.velocity, idealVelocity) < 0f)
            NPC.velocity *= 0.5f;

        // Try to not fly through tiles unless necessary.
        NPC.noTileCollide = !NPC.WithinRange(flyDestination, 600f) || FollowPlayer_FlyDestinationOverride != Vector2.Zero;
        bool inTiles = Collision.SolidCollision(NPC.TopLeft, NPC.width, NPC.height - 4);
        if (inTiles)
            NPC.velocity.Y -= 0.8f;

        NPC.noGravity = true;
        NPC.spriteDirection = (flyDestination.X - NPC.Center.X).NonZeroSign();
        if (FollowPlayer_FlyDestinationOverride != Vector2.Zero)
            NPC.spriteDirection = NPC.velocity.X.NonZeroSign();

        bool nearbyGround = FindGroundVertical(NPC.Center.ToTileCoordinates()).ToWorldCoordinates().WithinRange(NPC.Center, 250f);
        if (NPC.WithinRange(flyDestination, 75f) && !inTiles && Collision.CanHit(NPC.TopLeft, NPC.width, NPC.height, flyDestination, 1, 1) && NPC.velocity.Length() <= 5f && nearbyGround)
        {
            flying = 0f;
            NPC.netUpdate = true;
        }

        if (returnToGroundIfPossible &&
            FindGroundVertical(NPC.Center.ToTileCoordinates()).ToWorldCoordinates().WithinRange(NPC.Center, 360f) &&
            !Collision.WetCollision(NPC.Center, 1, 360) &&
            !Collision.SolidCollision(NPC.TopLeft, NPC.width, NPC.height))
        {
            flying = 0f;
            NPC.netUpdate = true;
        }

        if (FollowPlayer_FlyDestinationOverride != Vector2.Zero && NPC.WithinRange(FollowPlayer_FlyDestinationOverride, 40f) && NPC.Center.Y <= FollowPlayer_FlyDestinationOverride.Y)
        {
            flying = 0f;
            NPC.velocity.Y *= 0.12f;
            FollowPlayer_FlyDestinationOverride = Vector2.Zero;
            NPC.netUpdate = true;
        }

        UseStarFlyEffects();
    }

    public void DoBehavior_FollowPlayer_WalkToDestination(Vector2 walkDestination, bool onGround, bool teleportIfSuperFar, float walkSpeed, ref float flying, ref float tripTimer)
    {
        // Calculate the ideal walk speed.
        float walkAccelerationInterpolant = 0.081f;

        List<Vector2> path = AStarPathfinding.PathfindThroughTiles(NPC.Center - Vector2.UnitX * NPC.spriteDirection * 10f, walkDestination, point =>
        {
            if (!WorldGen.InWorld(point.X, point.Y, 20))
                return 100f;

            return 0f;
        });
        Vector2 aheadPathfindingPoint = walkDestination;
        if (path.Count >= 6)
            aheadPathfindingPoint = path[5];

        float idealWalkSpeed = NPC.SafeDirectionTo(aheadPathfindingPoint).X * walkSpeed;
        walkAccelerationInterpolant += Utils.Remap(NPC.Distance(walkDestination), 300f, 145f, 0f, 0.25f);

        DoBehavior_FollowPlayer_BeginFlyingIfNecessary(onGround, aheadPathfindingPoint, walkDestination, teleportIfSuperFar, path, ref flying);

        // Walk towards the wander destination.
        if (onGround)
        {
            if (NPC.WithinRange(walkDestination, 84f))
                NPC.velocity.X *= 0.9f;
            else
                NPC.velocity.X = Lerp(NPC.velocity.X, idealWalkSpeed, walkAccelerationInterpolant);

            // Look forward.
            if (Abs(NPC.velocity.X) >= 0.4f)
                NPC.spriteDirection = NPC.velocity.X.NonZeroSign();
        }

        // Walk up tiles.
        float _ = 0.3f;
        Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref _, ref NPC.gfxOffY);

        // Occasionally trip and fall.
        if (Abs(NPC.velocity.X) >= walkSpeed * 0.9f && Main.rand.NextBool(TripFallChance))
        {
            tripTimer = 1f;
            NPC.netUpdate = true;
        }

        // Safety check to ensure that Solyn doesn't end up inside of tiles.
        if (Collision.SolidCollision(NPC.TopLeft, NPC.width, NPC.height))
            NPC.position += NPC.SafeDirectionTo(walkDestination) * 2f;

        DoBehavior_FollowPlayer_InteractWithDoors();

        NPC.frameCounter += InverseLerp(0.18f, 0.75f, Abs(NPC.velocity.X) / walkSpeed) * 1.75f;
        NPC.noTileCollide = JustJumped;
        NPC.noGravity = false;
        DescendThroughSlopes = aheadPathfindingPoint.Y >= NPC.Center.Y + 16f;
    }

    public void DoBehavior_FollowPlayer_BeginFlyingIfNecessary(bool onGround, Vector2 aheadPathfindingPoint, Vector2 endDestination, bool teleportIfSuperFar, List<Vector2> path, ref float flying)
    {
        bool nextStepIsUpwards = NPC.velocity.Length() <= 4f && Distance(NPC.Center.X, aheadPathfindingPoint.X) <= 30f && Distance(NPC.Center.Y, aheadPathfindingPoint.Y) >= 30f;
        bool wallInWay = !Collision.CanHitLine(NPC.TopLeft, NPC.width, NPC.height, NPC.TopLeft + Vector2.UnitX * NPC.spriteDirection * 32f, NPC.width, NPC.height);
        bool needsToJump = nextStepIsUpwards || (wallInWay && AITimer % 10 == 9);
        if (onGround && needsToJump)
        {
            for (int i = 5; i < path.Count - 3; i++)
            {
                if (Collision.CanHitLine(path[i], 1, 1, NPC.Center, 1, 1))
                    continue;

                FollowPlayer_FlyDestinationOverride = path[i + 3] - Vector2.UnitY * 12f;
                NPC.velocity.X *= 0.3f;
                flying = 1f;
                break;
            }
        }

        if (teleportIfSuperFar && !NPC.WithinRange(endDestination, 2500f))
        {
            flying = 1f;
            FollowPlayer_FlyDestinationOverride = endDestination;
        }

        bool closeToWater = Collision.WetCollision(NPC.TopLeft, NPC.width, NPC.height + (int)(NPC.velocity.Y + 25f));
        bool closeToLava = Collision.LavaCollision(NPC.TopLeft, NPC.width, NPC.height + (int)(NPC.velocity.Y + 25f));
        if (closeToWater || closeToLava)
        {
            flying = 1f;
            NPC.netUpdate = true;
        }
    }

    public void DoBehavior_FollowPlayer_InteractWithDoors()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        int checkX = (int)((NPC.Center.X + NPC.spriteDirection * 15f) / 16f);
        int checkY = (int)(NPC.Bottom.Y / 16f) - 3;
        Tile checkTile = Framing.GetTileSafely(checkX, checkY);

        if (checkTile.HasUnactuatedTile && (checkTile.TileType == TileID.ClosedDoor || TileID.Sets.OpenDoorID[checkTile.TileType] != -1))
        {
            if (WorldGen.OpenDoor(checkX, checkY, -NPC.spriteDirection))
            {
                NPC.closeDoor = true;
                NPC.doorX = checkX;
                NPC.doorY = checkY;
                NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 0, checkX, checkY, -NPC.spriteDirection);
            }
            else if (WorldGen.OpenDoor(checkX, checkY, NPC.spriteDirection))
            {
                NPC.closeDoor = true;
                NPC.doorX = checkX;
                NPC.doorY = checkY;
                NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 0, checkX, checkY, NPC.spriteDirection);
            }
        }

        Vector2 doorPosition = new Vector2(NPC.doorX, NPC.doorY).ToWorldCoordinates();
        if (!NPC.WithinRange(doorPosition, 60f))
        {
            Tile doorTile = Framing.GetTileSafely(NPC.doorX, NPC.doorY);
            if (doorTile.HasUnactuatedTile && (doorTile.TileType == TileID.OpenDoor || TileID.Sets.CloseDoorID[doorTile.TileType] != -1) && WorldGen.CloseDoor(NPC.doorX, NPC.doorY))
            {
                NPC.closeDoor = false;
                NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 0, NPC.doorX, NPC.doorY, NPC.spriteDirection);
            }
        }
    }
}
