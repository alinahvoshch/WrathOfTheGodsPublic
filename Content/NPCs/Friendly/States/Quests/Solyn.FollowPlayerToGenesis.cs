using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers.SolynDialogue;
using NoxusBoss.Content.Tiles.GenesisComponents.Seedling;
using NoxusBoss.Core.World.WorldSaving;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Friendly;

public partial class Solyn : ModNPC, IPixelatedPrimitiveRenderer
{
    public static int PontificateAboutGenesis_DialogueDuration => 240;

    public static int PontificateAboutGenesis_NextLineDelay => 90;

    public static int PontificateAboutGenesis_SpeakDelay => 60;

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_FollowPlayerToGenesis()
    {
        StateMachine.RegisterStateBehavior(SolynAIType.FollowPlayerToGenesis, DoBehavior_FollowPlayerToGenesis);
        StateMachine.RegisterStateBehavior(SolynAIType.PontificateAboutGenesis, DoBehavior_PontificateAboutGenesis);

        StateMachine.RegisterTransition(SolynAIType.FollowPlayerToGenesis, SolynAIType.PontificateAboutGenesis, true, () =>
        {
            Point? nearestGenesis = GrowingGenesisRenderSystem.NearestGenesisPoint(NPC.Center.ToTileCoordinates());
            if (nearestGenesis is null)
                return false;

            bool closeEnoughToGenesis = NPC.WithinRange(nearestGenesis.Value.ToWorldCoordinates(), 356f);
            return !WorldSaveSystem.CanUseGenesis && closeEnoughToGenesis;
        });
        StateMachine.RegisterTransition(SolynAIType.PontificateAboutGenesis, null, false, () =>
        {
            int dialogueDuration = PontificateAboutGenesis_DialogueDuration;
            int nextLineDelay = PontificateAboutGenesis_NextLineDelay;
            int speakDelay = PontificateAboutGenesis_SpeakDelay;
            return AITimer >= speakDelay + (dialogueDuration + nextLineDelay) * 4;
        });
    }

    /// <summary>
    /// Performs Solyn's fly-with-player-to-genesis behavior.
    /// </summary>
    public void DoBehavior_FollowPlayerToGenesis()
    {
        DoBehavior_FollowPlayerToCodebreaker();
    }

    public void DoBehavior_PontificateAboutGenesis()
    {
        int dialogueDuration = PontificateAboutGenesis_DialogueDuration;
        int nextLineDelay = PontificateAboutGenesis_NextLineDelay;
        int speakDelay = PontificateAboutGenesis_SpeakDelay;

        if (AITimer == 1)
            BlockerSystem.Start(true, false, () => NPC.active && CurrentState == SolynAIType.PontificateAboutGenesis);
        if (AITimer == speakDelay)
            SolynWorldDialogueManager.CreateNew("Mods.NoxusBoss.Solyn.AvatarFightLeadUp.Solyn1", -NPC.spriteDirection, NPC.Top - Vector2.UnitY * 40f, dialogueDuration, false);
        if (AITimer == speakDelay + dialogueDuration + nextLineDelay)
            SolynWorldDialogueManager.CreateNew("Mods.NoxusBoss.Solyn.AvatarFightLeadUp.Solyn2", -NPC.spriteDirection, NPC.Top - Vector2.UnitY * 40f, dialogueDuration, false);
        if (AITimer == speakDelay + (dialogueDuration + nextLineDelay) * 2)
            SolynWorldDialogueManager.CreateNew("Mods.NoxusBoss.Solyn.AvatarFightLeadUp.Solyn3", -NPC.spriteDirection, NPC.Top - Vector2.UnitY * 40f, dialogueDuration, false);
        if (AITimer == speakDelay + (dialogueDuration + nextLineDelay) * 3)
            SolynWorldDialogueManager.CreateNew("Mods.NoxusBoss.Solyn.AvatarFightLeadUp.Solyn4", -NPC.spriteDirection, NPC.Top - Vector2.UnitY * 40f, dialogueDuration, false);
        if (AITimer >= speakDelay + (dialogueDuration + nextLineDelay) * 3)
            WorldSaveSystem.CanUseGenesis = true;

        int attackCycleTimer = (AITimer - speakDelay) % (dialogueDuration + nextLineDelay);
        bool speaking = AITimer >= speakDelay && attackCycleTimer < 50;
        if (speaking && AITimer % 3 == 2)
            SoundEngine.PlaySound(GennedAssets.Sounds.Solyn.Speak with { Volume = 0.4f, MaxInstances = 1, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew }, NPC.Center);

        float zoomInterpolant = InverseLerpBump(0f, 30f, speakDelay + (dialogueDuration + nextLineDelay) * 4 - 30f, speakDelay + (dialogueDuration + nextLineDelay) * 4, AITimer);
        CameraPanSystem.ZoomIn(SmoothStep(0f, 0.3f, zoomInterpolant));

        DoBehavior_FollowPlayerToCodebreaker();
    }
}
