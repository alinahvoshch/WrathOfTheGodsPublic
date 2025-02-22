using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Friendly;

public partial class Solyn : ModNPC, IPixelatedPrimitiveRenderer
{
    /// <summary>
    /// How long the screen rumbles during Solyn's confetti trick state.
    /// </summary>
    public static int ConfettiTeleportMagicTrick_RumbleTime => SecondsToFrames(2f);

    /// <summary>
    /// How long Solyn waits before teleporting during her confetti trick state.
    /// </summary>
    public static int ConfettiTeleportMagicTrick_TeleportDelay => SecondsToFrames(1.5f);

    /// <summary>
    /// How long Solyn waits before speaking during her confetti trick state.
    /// </summary>
    public static int ConfettiTeleportMagicTrick_SpeakDelay => SecondsToFrames(0.4f);

    /// <summary>
    /// How long Solyn waits before noticing her hat didn't teleport with her during her confetti trick state.
    /// </summary>
    public static int ConfettiTeleportMagicTrick_HatNoticeDelay => SecondsToFrames(1f);

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_ConfettiTeleportMagicTrick()
    {
        StateMachine.RegisterTransition(SolynAIType.ConfettiTeleportMagicTrick, SolynAIType.StandStill, false, () =>
        {
            return AITimer >= ConfettiTeleportMagicTrick_RumbleTime + ConfettiTeleportMagicTrick_TeleportDelay + ConfettiTeleportMagicTrick_SpeakDelay + ConfettiTeleportMagicTrick_HatNoticeDelay;
        }, () =>
        {
            // Jump up and notice the hat.
            if (Main.netMode != NetmodeID.Server)
            {
                string hatText = Language.GetTextValue("Mods.NoxusBoss.Solyn.SolynPartyTalk.SolynHatText");
                CombatText.NewText(NPC.Hitbox, DialogColorRegistry.SolynTextColor, hatText, true);
            }
            NPC.velocity.Y -= 5f;
        });

        StateMachine.RegisterStateBehavior(SolynAIType.ConfettiTeleportMagicTrick, DoBehavior_ConfettiTeleportMagicTrick);
    }

    /// <summary>
    /// Performs Solyn's confetti teleport magic trick state.
    /// </summary>
    public void DoBehavior_ConfettiTeleportMagicTrick()
    {
        int rumbleTime = ConfettiTeleportMagicTrick_RumbleTime;
        int teleportDelay = ConfettiTeleportMagicTrick_TeleportDelay;
        int speakDelay = ConfettiTeleportMagicTrick_SpeakDelay;

        // Horizontally slow down.
        NPC.velocity.X *= 0.8f;

        // Make the screen rumble.
        if (AITimer <= rumbleTime)
        {
            float rumbleAnimationCompletion = AITimer / (float)rumbleTime;
            ScreenShakeSystem.SetUniversalRumble(rumbleAnimationCompletion.Squared() * 4.67f, TwoPi, null, 0.2f);
        }

        // Wait a short moment before teleporting.
        if (AITimer == rumbleTime + teleportDelay)
        {
            // Shake the screen.
            ScreenShakeSystem.StartShake(9.5f);

            Vector2 teleportDestination = NPC.Center + Vector2.UnitX * NPC.spriteDirection * Main.rand.NextFloat(160f, 216f);
            Vector2 teleportGroundPosition = new Vector2(teleportDestination.X, 16f);
            for (int dx = -2; dx < 2; dx++)
            {
                Point ground = FindGroundVertical(teleportDestination.ToTileCoordinates());
                teleportGroundPosition.Y = MathF.Max(ground.Y * 16f + 8f, teleportGroundPosition.Y);
            }
            while (Collision.SolidCollision(teleportGroundPosition - Vector2.UnitX * NPC.width * 0.5f, NPC.width, 2))
                teleportGroundPosition.Y--;

            // Leave the hat behind.
            if (HasHat)
            {
                PartyHatParticle hat = new PartyHatParticle(NPC.Top, 240, -NPC.spriteDirection);
                hat.Spawn();
                HasHat = false;
            }

            // Teleport.
            Player closestPlayer = Main.player[Player.FindClosest(NPC.Center, 1, 1)];
            NPC.spriteDirection = (closestPlayer.Center.X - NPC.Center.X).NonZeroSign();
            TeleportTo(teleportGroundPosition);

            // Create a bunch of confetti.
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 75; i++)
                {
                    // Decide confetti attributes.
                    int confettiID = Main.rand.Next(276, 283);
                    Vector2 confettiSpawnPosition = NPC.Center + Main.rand.NextVector2Circular(24f, 34f);
                    Vector2 confettiVelocity = Main.rand.NextVector2Circular(10f, 9.5f);
                    confettiVelocity.Y -= Main.rand.NextFloat(9.5f, 10f);

                    Gore.NewGorePerfect(NPC.GetSource_FromAI(), confettiSpawnPosition, confettiVelocity, confettiID, Main.rand.NextFloat(0.4f, 0.9f));
                }
            }
        }

        // Say tada.
        if (AITimer == rumbleTime + teleportDelay + speakDelay)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                string tadaText = Language.GetTextValue("Mods.NoxusBoss.Solyn.SolynPartyTalk.SolynTada");
                CombatText.NewText(NPC.Hitbox, DialogColorRegistry.SolynTextColor, tadaText);
            }
        }

        PerformStandardFraming();
    }

    /// <summary>
    /// Makes Solyn teleport to a given position.
    /// </summary>
    /// <param name="teleportGroundPosition">Where Solyn's <see cref="Entity.Bottom"/> position should be teleported to.</param>
    public void TeleportTo(Vector2 teleportGroundPosition)
    {
        // Create teleport particles at the starting position.
        ExpandingGreyscaleCircleParticle circle = new ExpandingGreyscaleCircleParticle(NPC.Center, Vector2.Zero, Color.IndianRed, 8, 0.1f);
        circle.Spawn();
        MagicBurstParticle burst = new MagicBurstParticle(NPC.Center, Vector2.Zero, Color.Wheat, 20, 1.04f);
        burst.Spawn();

        // Play a teleport sound.
        SoundEngine.PlaySound(GennedAssets.Sounds.Common.TeleportOut with { Volume = 0.5f, Pitch = 0.3f, MaxInstances = 5, PitchVariance = 0.16f }, NPC.Center);

        // Teleport.
        NPC.Bottom = teleportGroundPosition;

        // Create teleport particles at the ending position.
        circle = new(NPC.Center, Vector2.Zero, Color.IndianRed, 8, 0.1f);
        circle.Spawn();
        burst = new(NPC.Center, Vector2.Zero, Color.Wheat, 20, 1.04f);
        burst.Spawn();
    }
}
