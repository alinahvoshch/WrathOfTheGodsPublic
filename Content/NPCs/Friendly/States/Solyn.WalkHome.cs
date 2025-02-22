using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Core.Graphics.UI.SolynDialogue;
using NoxusBoss.Core.Pathfinding;
using NoxusBoss.Core.World.WorldGeneration;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Friendly;

public partial class Solyn : ModNPC, IPixelatedPrimitiveRenderer
{
    public class PathfindingStep
    {
        public Vector2 Position;

        public PathfindingState ActionState;

        public PathfindingStep? Next;

        public PathfindingStep(Vector2 position, PathfindingState action, PathfindingStep? next = null)
        {
            Position = position;
            ActionState = action;
            Next = next;
        }
    }

    /// <summary>
    /// Solyn's desired position for her current pathfinding substate.
    /// </summary>
    public Vector2 CurrentPathfindingDestination
    {
        get;
        set;
    }

    /// <summary>
    /// Solyn's current pathfinding substate.
    /// </summary>
    public PathfindingState CurrentPathfindingState
    {
        get;
        set;
    }

    /// <summary>
    /// Solyn's upcoming pathfinding substate.
    /// </summary>
    public PathfindingState NextPathfindingState
    {
        get;
        set;
    }

    /// <summary>
    /// Solyn's walk path.
    /// </summary>
    public List<Vector2> WalkPath
    {
        get;
        private set;
    } = [];

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_WalkHome()
    {
        StateMachine.RegisterTransition(SolynAIType.WalkHome, SolynAIType.WanderAbout, false, () =>
        {
            return NPC.WithinRange(SolynCampsiteWorldGen.CampSitePosition, 400f) && !Collision.SolidCollision(NPC.TopLeft, NPC.width, NPC.height);
        }, () =>
        {
            CurrentPathfindingState = PathfindingState.Walk;
            NextPathfindingState = PathfindingState.Walk;
            NPC.noTileCollide = false;
        });

        StateMachine.ApplyToAllStatesExcept(state =>
        {
            StateMachine.RegisterTransition(state, SolynAIType.WalkHome, true, () =>
            {
                bool past6PM = Main.time >= Main.dayLength - 5400f && Main.dayTime;
                bool readyToLeave = !Main.dayTime || past6PM;

                if (WaitingToEnterCVRift)
                    return false;

                // If Solyn fell from the sky, the check is changed to be near dawn instead of dusk.
                if (SummonedByStarFall)
                {
                    bool past4AM = Main.time >= Main.nightLength - 1800f && !Main.dayTime;
                    readyToLeave = Main.dayTime || past4AM;
                }

                if (!CanDepart)
                    readyToLeave = false;

                return readyToLeave && !NPC.WithinRange(SolynCampsiteWorldGen.CampSitePosition, 800f);
            }, () =>
            {
                CurrentConversation = SolynDialogSystem.ChooseSolynConversation();
                if (SummonedByStarFall)
                {
                    SummonedByStarFall = false;
                    NPC.netUpdate = true;
                }
            });
        }, SolynAIType.Shimmering, SolynAIType.WalkHome, SolynAIType.TeleportFromPermafrostKeep,
           SolynAIType.WaitAtPermafrostKeep, SolynAIType.WalkAroundPermafrostKeep, SolynAIType.WaitNearCeaselessVoidRift, SolynAIType.IncospicuouslyFlyAwayToDungeon, SolynAIType.FollowPlayerToCodebreaker,
           SolynAIType.FlyIntoRift, SolynAIType.WaitInsideRift, SolynAIType.ExitRift, SolynAIType.FollowPlayerToGenesis, SolynAIType.PontificateAboutGenesis);

        StateMachine.RegisterStateBehavior(SolynAIType.WalkHome, DoBehavior_WalkHome);
    }

    /// <summary>
    /// Performs Solyn's walk home state.
    /// </summary>
    public void DoBehavior_WalkHome()
    {
        Vector2 flyDestination = SolynCampsiteWorldGen.CampSitePosition - Vector2.UnitY * 32f;

        NPC.noTileCollide = true;
        NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.SafeDirectionTo(flyDestination) * 13f, 0.1f);

        float riseUpwardAcceleration = InverseLerp(600f, 1800f, NPC.Distance(flyDestination)) * 1.2f;
        if (Collision.SolidCollision(NPC.TopLeft - Vector2.UnitX * 200f, NPC.width + 400, NPC.height + 200) || !Collision.CanHit(NPC.Center, 1, 1, NPC.Center + Vector2.UnitX * NPC.spriteDirection * 800f, 1, 1))
            NPC.velocity.Y -= riseUpwardAcceleration;
        NPC.spriteDirection = NPC.velocity.X.NonZeroSign();

        CanBeSpokenTo = false;

        UseStarFlyEffects();
    }

    /// <summary>
    /// Makes Solyn pathfind move to a given destination.
    /// </summary>
    /// <param name="endDestination">The position that Solyn will attempt to reach.</param>
    public void PathFindMoveTowards(Vector2 endDestination)
    {
        bool closeEnough = Distance(NPC.Center.X, CurrentPathfindingDestination.X) <= 36f || NPC.WithinRange(endDestination, 40f);
        if (CurrentPathfindingDestination == Vector2.Zero || (closeEnough && !Collision.SolidCollision(NPC.Center, 1, 1)))
        {
            WalkPath = AStarPathfinding.PathfindThroughTiles(NPC.Center - Vector2.UnitX * NPC.spriteDirection * 10f, endDestination, point =>
            {
                if (!WorldGen.InWorld(point.X, point.Y, 20))
                    return 100f;

                return 0f;
            }, 25000);
            AITimer = 0;
            CurrentPathfindingDestination = DetermineWalkDestination(endDestination, out PathfindingState nextState);
            CurrentPathfindingState = NextPathfindingState;
            NextPathfindingState = nextState;
            NPC.netUpdate = true;
        }

        DebugDrawPath(WalkPath);
        Dust.QuickDust(CurrentPathfindingDestination, Color.Blue);
        NPC.noTileCollide = false;

        float slowdownFactor = InverseLerp(-10f, 90f, Distance(CurrentPathfindingDestination.X, NPC.Center.X));
        switch (CurrentPathfindingState)
        {
            case PathfindingState.Walk:
                PerformStandardFraming();
                if (NPC.velocity.X == 0f)
                {
                    AITimer = 0;
                    CurrentPathfindingState = PathfindingState.Jump;
                    NPC.netUpdate = true;
                }

                if (Abs(NPC.velocity.Y) >= 0.5f)
                    NPC.velocity.X *= 0.94f;
                else
                    NPC.velocity.X = Lerp(NPC.velocity.X, NPC.SafeDirectionTo(CurrentPathfindingDestination).X * slowdownFactor * 7f, 0.08f);
                Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

                break;
            case PathfindingState.Jump:
                bool onGround = Abs(NPC.velocity.Y) <= 0.3f && Collision.SolidCollision(NPC.BottomLeft, NPC.width, 16, true);
                if (AITimer <= 3)
                {
                    if (onGround)
                    {
                        NPC.velocity.Y = -6f;
                        NPC.netUpdate = true;
                    }
                }
                Frame = 1f;
                NPC.velocity.X = Lerp(NPC.velocity.X, NPC.SafeDirectionTo(CurrentPathfindingDestination).X * slowdownFactor * 9f, 0.15f);

                if (AITimer >= 15)
                    AITimer = 0;

                break;
            case PathfindingState.Fly:
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.SafeDirectionTo(endDestination) * slowdownFactor * 12f, 0.18f);
                if (Collision.SolidCollision(NPC.Top, 1, NPC.height) || Collision.WetCollision(NPC.Top, 1, NPC.height))
                    NPC.velocity.Y -= 2f;

                NPC.noTileCollide = true;
                UseStarFlyEffects();
                break;
        }

        NPC.spriteDirection = NPC.velocity.X.NonZeroSign();
    }

    /// <summary>
    /// Calculates a set of steps Solyn should take from the results of a pathfinding search.
    /// </summary>
    /// <param name="path">The path to calculate steps from.</param>
    public static List<PathfindingStep> TurnPathIntoSteps(List<Vector2> path)
    {
        PathfindingStep[] baseSteps = new PathfindingStep[path.Count];

        // Loop from back to the front for ease of access when acquiring next steps.
        for (int i = path.Count - 1; i >= 0; i--)
        {
            PathfindingStep? nextStep = null;
            if (i < path.Count - 1)
                nextStep = baseSteps[i + 1];

            baseSteps[i] = new(path[i], PathfindingState.Walk, nextStep);
        }

        // Walk through the steps, searching for gaps and heights to evaluate.
        bool performingHoleSearch = false;
        bool performingWallSearch = false;
        bool performingLakeSearch = false;
        int startingSearchIndex = 1;

        for (int i = 0; i < baseSteps.Length - 3; i += (performingHoleSearch || performingWallSearch || performingLakeSearch ? 1 : 3))
        {
            PathfindingStep stepA = baseSteps[i];
            PathfindingStep stepB = baseSteps[i + 1];
            PathfindingStep stepC = baseSteps[i + 2];

            Vector2 groundedStepAPosition = FindGroundVertical(baseSteps[i].Position.ToTileCoordinates()).ToWorldCoordinates();

            bool hole = (stepB.Position.Y > stepA.Position.Y && stepC.Position.Y > stepB.Position.Y) || Collision.CanHit(stepA.Position, 1, 1, stepA.Position + Vector2.UnitY * 48f, 1, 1);
            bool wall = stepB.Position.Y < stepA.Position.Y && stepC.Position.Y < stepB.Position.Y;
            bool liquid = Framing.GetTileSafely(groundedStepAPosition.ToTileCoordinates()).LiquidAmount >= 1;
            if (i >= baseSteps.Length - 3)
            {
                hole = false;
                wall = false;
                liquid = false;
            }

            // If a liquid has been found and no search is ongoing, start a search to figure out when it ends.
            if (liquid && !performingHoleSearch && !performingWallSearch && !performingLakeSearch)
            {
                performingLakeSearch = true;
                startingSearchIndex = i;
            }

            // If a hole has been found and no search is ongoing, start a search to figure out when it ends.
            else if (hole && !performingHoleSearch && !performingWallSearch && !performingLakeSearch)
            {
                performingHoleSearch = true;
                startingSearchIndex = i;
            }

            // If a wall has been found and no search is ongoing, start a search to figure out when it ends.
            else if (wall && !performingHoleSearch && !performingWallSearch && !performingLakeSearch)
            {
                performingWallSearch = true;
                startingSearchIndex = i;
            }

            // Continue the hole search if it's ongoing.
            if (performingHoleSearch && i > startingSearchIndex)
            {
                // If ground patters out, that means that the end of the hole has been reached.
                if (!hole)
                {
                    bool shouldFlyToNextStep = Distance(stepA.Position.Y, baseSteps[startingSearchIndex].Position.Y) >= 210f;
                    baseSteps[startingSearchIndex].Next = stepA;
                    baseSteps[startingSearchIndex].ActionState = shouldFlyToNextStep ? PathfindingState.Fly : PathfindingState.Jump;

                    performingHoleSearch = false;
                }
            }

            // Continue the wall search if it's ongoing.
            if (performingWallSearch && i > startingSearchIndex)
            {
                // If ground patters out, that means that the top of the wall has been reached.
                if (!wall)
                {
                    bool shouldFlyToNextStep = Distance(stepA.Position.Y, baseSteps[startingSearchIndex].Position.Y) >= 60f;
                    baseSteps[startingSearchIndex].Next = stepA;
                    baseSteps[startingSearchIndex].ActionState = shouldFlyToNextStep ? PathfindingState.Fly : PathfindingState.Jump;

                    performingWallSearch = false;
                }
            }

            // Continue the lake search if it's ongoing.
            if (performingLakeSearch && i > startingSearchIndex)
            {
                // If ground patters out, that means that the top of the wall has been reached.
                if (Framing.GetTileSafely(stepA.Position.ToTileCoordinates()).LiquidAmount <= 0)
                {
                    baseSteps[startingSearchIndex].Next = stepA;
                    baseSteps[startingSearchIndex].ActionState = PathfindingState.Fly;

                    performingLakeSearch = false;
                }
            }
        }

        List<PathfindingStep> finalSteps = new List<PathfindingStep>(path.Count)
        {
            baseSteps.First()
        };

        while (finalSteps.Last().Next is not null && finalSteps.Count <= 10000)
            finalSteps.Add(finalSteps.Last().Next!);

        return finalSteps;
    }

    /// <summary>
    /// Determines where Solyn should attempt to walk based on the pathfinding points.
    /// </summary>
    /// <param name="fallback">The fallback position that should be used if the path is invalid.</param>
    public Vector2 DetermineWalkDestination(Vector2 fallback, out PathfindingState nextState)
    {
        nextState = CurrentPathfindingState;
        if (WalkPath.Count <= 0)
            return fallback;

        // Look forward, attempting to go to the final "unproblematic" step in the chain.
        // "Unproblematic" in this context mainly refers to gaps and walls.
        // If a gap or wall greater than 1 tile tall is found, she needs to either jump or fly.
        List<PathfindingStep> steps = TurnPathIntoSteps(WalkPath);
        PathfindingStep? firstStepToNotWalk = steps.FirstOrDefault(s => s.ActionState != CurrentPathfindingState && Distance(s.Position.X, NPC.Center.X) >= 10f);
        PathfindingStep destinationStep = firstStepToNotWalk ?? steps.Last();

        nextState = destinationStep.ActionState;
        TurnPathIntoSteps(WalkPath);

        return destinationStep.Position;
    }

    public static void DebugDrawPath(List<Vector2> path)
    {
        if (path.Count == 0)
            return;

        List<PathfindingStep> steps = TurnPathIntoSteps(path);
        foreach (PathfindingStep step in steps)
        {
            Color color = Color.Green;
            if (step.ActionState == PathfindingState.Jump)
                color = Color.Orange;
            if (step.ActionState == PathfindingState.Fly)
                color = Color.Red;

            Dust.QuickDust(step.Position, color);
        }
    }
}
