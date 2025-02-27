using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers.SolynDialogue;
using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Content.Tiles.GenesisComponents.Seedling;
using NoxusBoss.Core.DialogueSystem;
using NoxusBoss.Core.World.Subworlds;
using NoxusBoss.Core.World.WorldSaving;
using SubworldLibrary;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace NoxusBoss.Core.SolynEvents;

public class GenesisCompletionEvent : SolynEvent
{
    public static int DialogueDuration => 240;

    public static int NextLineDelay => 90;

    public static int SpeakDelay => 60;

    // Not intended to be completed, to ensure that if Solyn is respawned after Avatar is dead for some reason she follows the player as usual.
    public override int TotalStages => 3;

    public static bool CanStart => ModContent.GetInstance<MarsCombatEvent>().Finished && WorldSaveSystem.HasCompletedGenesis;

    public override void OnModLoad()
    {
        ConversationSelector.PriorityConversationSelectionEvent += SelectIntroductionDialogue;
    }

    private Conversation? SelectIntroductionDialogue()
    {
        if (!Finished && CanStart)
            return DialogueManager.FindByRelativePrefix("GenesisRevealDiscussion");

        return null;
    }

    public override void PostUpdateNPCs()
    {
        if (CanStart && !Finished && Solyn is not null && !SubworldSystem.IsActive<EternalGardenNew>())
        {
            if (Solyn.CurrentState != SolynAIType.PuppeteeredByQuest)
                Solyn.SwitchState(SolynAIType.PuppeteeredByQuest);

            NPC npc = Solyn.NPC;
            Player player = Main.player[Player.FindClosest(npc.Center, 1, 1)];
            Vector2 flyDestination = player.Center + new Vector2(player.direction * -74f, -60f);
            Vector2 idealVelocity = (flyDestination - npc.Center).ClampLength(0f, player.velocity.Length() + 5f);

            npc.Center = Vector2.Lerp(npc.Center, flyDestination, 0.0125f);
            npc.velocity = Vector2.Lerp(npc.velocity, idealVelocity, 0.05f).MoveTowards(idealVelocity, 0.2f);
            if (Vector2.Dot(npc.velocity, idealVelocity) < 0f)
                npc.velocity *= 0.74f;

            npc.noGravity = true;
            npc.spriteDirection = (player.Center.X - npc.Center.X).NonZeroSign();

            Solyn.CanBeSpokenTo = false;
            Solyn.UseStarFlyEffects();

            // Pontificate about the Genesis.
            Point? nearestGenesis = GrowingGenesisRenderSystem.NearestGenesisPoint(npc.Center.ToTileCoordinates());
            if (nearestGenesis is not null && npc.WithinRange(nearestGenesis.Value.ToWorldCoordinates(), 356f) && Stage == 0)
            {
                Solyn.AITimer = 0;
                npc.netUpdate = true;
                SafeSetStage(1);
            }

            if (Stage == 1)
            {
                int dialogueDuration = DialogueDuration;
                int nextLineDelay = NextLineDelay;
                int speakDelay = SpeakDelay;

                if (Solyn.AITimer == 1)
                    BlockerSystem.Start(true, false, () => Stage == 1);
                if (Solyn.AITimer == speakDelay)
                    SolynWorldDialogueManager.CreateNew("Mods.NoxusBoss.Solyn.AvatarFightLeadUp.Solyn1", -npc.spriteDirection, npc.Top - Vector2.UnitY * 40f, dialogueDuration, false);
                if (Solyn.AITimer == speakDelay + dialogueDuration + nextLineDelay)
                    SolynWorldDialogueManager.CreateNew("Mods.NoxusBoss.Solyn.AvatarFightLeadUp.Solyn2", -npc.spriteDirection, npc.Top - Vector2.UnitY * 40f, dialogueDuration, false);
                if (Solyn.AITimer == speakDelay + (dialogueDuration + nextLineDelay) * 2)
                    SolynWorldDialogueManager.CreateNew("Mods.NoxusBoss.Solyn.AvatarFightLeadUp.Solyn3", -npc.spriteDirection, npc.Top - Vector2.UnitY * 40f, dialogueDuration, false);
                if (Solyn.AITimer == speakDelay + (dialogueDuration + nextLineDelay) * 3)
                    SolynWorldDialogueManager.CreateNew("Mods.NoxusBoss.Solyn.AvatarFightLeadUp.Solyn4", -npc.spriteDirection, npc.Top - Vector2.UnitY * 40f, dialogueDuration, false);

                int attackCycleTimer = (Solyn.AITimer - speakDelay) % (dialogueDuration + nextLineDelay);
                bool speaking = Solyn.AITimer >= speakDelay && attackCycleTimer < 50;
                if (speaking && Solyn.AITimer % 3 == 2)
                    SoundEngine.PlaySound(GennedAssets.Sounds.Solyn.Speak with { Volume = 0.4f, MaxInstances = 1, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew }, npc.Center);

                float zoomInterpolant = InverseLerpBump(0f, 30f, speakDelay + (dialogueDuration + nextLineDelay) * 4 - 30f, speakDelay + (dialogueDuration + nextLineDelay) * 4, Solyn.AITimer);
                CameraPanSystem.ZoomIn(SmoothStep(0f, 0.3f, zoomInterpolant));

                if (Solyn.AITimer >= speakDelay + (dialogueDuration + nextLineDelay) * 4)
                    SafeSetStage(2);
            }
        }
    }
}
