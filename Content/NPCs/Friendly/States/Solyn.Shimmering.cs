using System.Reflection;
using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Friendly;

public partial class Solyn : ModNPC, IPixelatedPrimitiveRenderer
{
    /// <summary>
    /// How long Solyn spends submerged while shimmering.
    /// </summary>
    public static int Shimmering_SubmergeTime => SecondsToFrames(0.75f);

    /// <summary>
    /// How long Solyn spends glowing after shimmering.
    /// </summary>
    public static int Shimmering_GlowTime => SecondsToFrames(0.1667f);

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_Shimmering()
    {
        StateMachine.ApplyToAllStatesExcept(state =>
        {
            StateMachine.RegisterTransition(state, SolynAIType.Shimmering, true, () => NPC.shimmering);
        }, SolynAIType.Shimmering, SolynAIType.WalkHome);

        StateMachine.RegisterTransition(SolynAIType.Shimmering, null, false, () =>
        {
            return Main.netMode != NetmodeID.MultiplayerClient && AITimer >= Shimmering_SubmergeTime + Shimmering_GlowTime + 15 && NPC.shimmerTransparency <= 0f;
        }, () =>
        {
            NPC.velocity = -Vector2.UnitY * 4f;
            NPC.netUpdate = true;
            NPC.townNpcVariationIndex = (NPC.townNpcVariationIndex == 1) ? 0 : 1;

            typeof(NPC).GetMethod("AI_007_TownEntities_Shimmer_TeleportToLandingSpot", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(NPC, []);

            NetMessage.SendData(MessageID.UniqueTownNPCInfoSyncRequest, -1, -1, null, NPC.whoAmI);

            NPC.Teleport(NPC.position, 12, 0);
            ParticleOrchestrator.BroadcastParticleSpawn(ParticleOrchestraType.ShimmerTownNPC, new ParticleOrchestraSettings
            {
                PositionInWorld = NPC.Center
            });
        });

        StateMachine.RegisterStateBehavior(SolynAIType.Shimmering, DoBehavior_Shimmering);
    }

    /// <summary>
    /// Performs Solyn's shimmering state.
    /// </summary>
    public void DoBehavior_Shimmering()
    {
        int shimmerSubmergeTime = 45;
        int shimmerGlowTime = 10;

        NPC.dontTakeDamage = true;
        if (AITimer == 0)
            NPC.velocity.X = 0f;

        NPC.shimmerWet = false;
        NPC.wet = false;
        NPC.lavaWet = false;
        NPC.honeyWet = false;
        if (AITimer == 0 && Main.netMode == NetmodeID.MultiplayerClient)
            return;

        if (AITimer >= shimmerSubmergeTime)
        {
            if (!Collision.WetCollision(NPC.position, NPC.width, NPC.height))
                NPC.shimmerTransparency = Clamp(NPC.shimmerTransparency - 0.0167f, 0f, 1f);
            else
                AITimer = shimmerSubmergeTime;

            NPC.velocity = Vector2.UnitY * NPC.shimmerTransparency * -4f;

            Frame = 21f;
        }
        else
            Frame = 20f;

        Rectangle shimmerBox = NPC.Hitbox;
        shimmerBox.Y += 20;
        shimmerBox.Height -= 20;

        if (Main.rand.NextFloat() > Utils.Remap(AITimer, shimmerSubmergeTime, shimmerSubmergeTime + shimmerGlowTime, 1f, 0.5f, true))
        {
            float directionalOffset = Main.rand.NextFloatDirection();
            Vector2 shimmerDustSpawnPosition = Main.rand.NextVector2FromRectangle(shimmerBox) + Main.rand.NextVector2Circular(8f, 0f) + new Vector2(0f, 4f);
            Dust.NewDustPerfect(shimmerDustSpawnPosition, 309, -Vector2.UnitY.RotatedBy(directionalOffset * TwoPi * 0.11f) * 2f, 0, default, 1.7f - Math.Abs(directionalOffset) * 1.3f);
        }
        if (AITimer > shimmerSubmergeTime + shimmerGlowTime && Main.rand.NextBool(15))
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 shimmerSpawnPosition = Main.rand.NextVector2FromRectangle(NPC.Hitbox);
                ParticleOrchestrator.RequestParticleSpawn(true, ParticleOrchestraType.ShimmerBlock, new ParticleOrchestraSettings
                {
                    PositionInWorld = shimmerSpawnPosition,
                    MovementVector = NPC.SafeDirectionTo(shimmerSpawnPosition).RotatedBy(TwoPi * 0.225f * (Main.rand.Next(2) * 2 - 1)) * Main.rand.NextFloat()
                }, null);
            }
        }
    }
}
