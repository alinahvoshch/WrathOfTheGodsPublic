using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Content.Particles;
using NoxusBoss.Content.Tiles.TileEntities;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity;

public partial class NamelessDeityBoss : ModNPC
{
    /// <summary>
    /// How long Nameless spends screaming during his Intro Scream Animation state.
    /// </summary>
    public static int IntroScreamAnimation_ScreamTime => GetAIInt("IntroScreamAnimation_ScreamTime");

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_IntroScreamAnimation()
    {
        StateMachine.RegisterTransition(NamelessAIType.IntroScreamAnimation, NamelessAIType.ResetCycle, false, () =>
        {
            return AITimer >= IntroScreamAnimation_ScreamTime;
        });

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(NamelessAIType.IntroScreamAnimation, DoBehavior_IntroScreamAnimation);
    }

    public void DoBehavior_IntroScreamAnimation()
    {
        // Appear on the foreground.
        if (AITimer == 1)
        {
            NPC.Center = Target.Center - Vector2.UnitY * 300f;
            NPC.velocity = Vector2.Zero;
            NPC.netUpdate = true;

            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.GenericBurst with { Volume = 1.3f, PitchVariance = 0.15f });
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<LightWave>(), 0, 0f);

                foreach (TileEntity te in TileEntity.ByPosition.Values)
                {
                    if (te is TEGoodAppleTree tree)
                        tree.DropApples();
                }
            }
        }

        // Rapidly unfold the vines.
        if (AITimer <= 6)
        {
            for (int i = 0; i < 20; i++)
                RenderComposite.Find<DanglingVinesStep>().HandleDanglingVineRotation(NPC);
        }

        // Bring the music.
        if (Music == 0 && AITimer >= IntroScreamAnimation_ScreamTime - 10)
            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/NamelessDeity");

        SceneEffectPriority = SceneEffectPriority.BossHigh;

        // Flap wings.
        UpdateWings(AITimer / 54f);

        // Jitter in place and scream.
        if (AITimer == 1f)
            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.ScreamLong with { Volume = 1.2f, Pitch = -0.075f });
        if (AITimer % 10f == 0f && AITimer <= IntroScreamAnimation_ScreamTime - 75f)
        {
            Color burstColor = Main.rand.NextBool() ? Color.LightGoldenrodYellow : Color.Lerp(Color.White, Color.IndianRed, 0.7f);

            // Create blur and burst particle effects.
            ExpandingChromaticBurstParticle burst = new ExpandingChromaticBurstParticle(EyePosition, Vector2.Zero, burstColor, 16, 0.1f);
            burst.Spawn();
            GeneralScreenEffectSystem.RadialBlur.Start(NPC.Center, 1f, 30);
            NamelessDeityKeyboardShader.BrightnessIntensity += 0.3f;

            if (ScreenShakeSystem.OverallShakeIntensity <= 11f)
                ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 5f);
        }

        NPC.Center += Main.rand.NextVector2Circular(12.5f, 12.5f);

        // Become completely opaque.
        NPC.Opacity = 1f;

        // Disable incoming damage, to prevent the player taking away 10% of Nameless' health while he's not moving.
        NPC.dontTakeDamage = true;

        // Update universal hands.
        DefaultUniversalHandMotion();

        UpdateScreenTear();
    }
}
