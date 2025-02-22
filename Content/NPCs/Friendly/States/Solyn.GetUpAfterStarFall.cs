using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.Particles;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Friendly;

public partial class Solyn : ModNPC, IPixelatedPrimitiveRenderer
{
    [AutomatedMethodInvoke]
    public void LoadStateTransitions_GetUpAfterStarFall()
    {
        StateMachine.RegisterTransition(SolynAIType.GetUpAfterStarFall, SolynAIType.StandStill, false, () =>
        {
            return AITimer >= 154;
        });

        StateMachine.RegisterStateBehavior(SolynAIType.GetUpAfterStarFall, DoBehavior_GetUpAfterStarFall);
    }

    /// <summary>
    /// Performs Solyn's getting up state.
    /// </summary>
    public void DoBehavior_GetUpAfterStarFall()
    {
        NPC.velocity.X *= 0.8f;
        while (Collision.SolidCollision(NPC.BottomLeft, NPC.width, 2))
            NPC.position.Y -= 2f;

        // Disable speaking.
        CanBeSpokenTo = false;

        if (AITimer == 1)
        {
            StrongBloom bloom = new StrongBloom(NPC.Center, Vector2.Zero, Color.Yellow, 0.7f, 8);
            bloom.Spawn();

            bloom = new(NPC.Center, Vector2.Zero, Color.HotPink * 0.85f, 1.3f, 10);
            bloom.Spawn();

            for (int i = 0; i < 24; i++)
            {
                int starPoints = Main.rand.Next(3, 9);
                float starScaleInterpolant = Main.rand.NextFloat();
                int starLifetime = (int)Lerp(30f, 67f, starScaleInterpolant);
                float starScale = Lerp(0.42f, 0.7f, starScaleInterpolant) * NPC.scale;
                Color starColor = Main.hslToRgb(Main.rand.NextFloat(), 0.9f, 0.64f);
                starColor = Color.Lerp(starColor, Color.Wheat, 0.4f) * 0.4f;

                Vector2 starVelocity = Main.rand.NextVector2Circular(12f, 3f) - Vector2.UnitY * Main.rand.NextFloat(4f, 7.5f);
                TwinkleParticle star = new TwinkleParticle(NPC.Center, starVelocity, starColor, starLifetime, starPoints, new Vector2(Main.rand.NextFloat(0.4f, 1.6f), 1f) * starScale, starColor * 0.5f);
                star.Spawn();
            }

            // Lose a bit of HP from the impact.
            CombatText.NewText(NPC.Hitbox, CombatText.DamagedFriendly, 10, true);
            NPC.life = Utils.Clamp(NPC.life - 10, 1, NPC.lifeMax);
            NPC.netUpdate = true;

            // Move forward.
            NPC.velocity.X = NPC.spriteDirection * 40f;
        }

        NPC.rotation = 0f;

        if (AITimer <= 154)
            Frame = 24f;
        if (AITimer <= 120)
            Frame = 23f;
        if (AITimer <= 30)
            Frame = 22f;
    }
}
