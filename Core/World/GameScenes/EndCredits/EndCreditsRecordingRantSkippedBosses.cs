using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.World.GameScenes.EndCredits;

public class EndCreditsRecordingRantSkippedBosses : NamelessDeitySubtitleSystem
{
    public override List<NamelessDeityDialog> Sentences => new List<NamelessDeityDialog>()
    {
        // Hey. You know...I can't really tell you what to do and all, but...
        new(0f, "RecordingRantBossesSubtitle1"),

        // It would be nice if you left things like this a secret for others to find in-game, you know?
        new(4.819f, "RecordingRantBossesSubtitle2"),
        
        // You don't have to upload absolutely everything for others to see.
        new(9.446f, "RecordingRantBossesSubtitle3"),
        
        // I created this cutscene for those who have earned it, after all.
        new(13.381f, "RecordingRantBossesSubtitle4"),
       
        // Though honestly, you probably just downloaded the mod to see this cutscene in-game anyway.
        new(17.131f, "RecordingRantBossesSubtitle5"),
        
        // I literally watched you skip like, half the bosses!
        new(21.553f, "RecordingRantBossesSubtitle6"),
        
        // Either you're really good at the game, or you cheated your way in here.
        new(24.488f, "RecordingRantBossesSubtitle7"),
        
        // Whatever the answer is, only you know for sure.
        new(28.612f, "RecordingRantBossesSubtitle8"),
        
        // And so will everyone watching this, now that I've called you out for it.
        new(31.943f, "RecordingRantBossesSubtitle9"),
        
        // Anyways, I hope you at least consider my word.
        new(35.353f, "RecordingRantBossesSubtitle10"),

        // All I ask from you is to do the right thing.
        new(38.427f, "RecordingRantBossesSubtitle11"),
    };

    public override float SubtitleDisappearTime => SecondsToFrames(41.936f);

    public override void UpdateUI(GameTime gameTime)
    {
        DialogueTimer = 0;
        if (!ModContent.GetInstance<EndCreditsScene>().IsActive || ModContent.GetInstance<EndCreditsScene>().State != EndCreditsScene.CreditsState.RecordingSoftwareRant_WhyDidYouSkipTheBosses)
            return;

        // Disallow game pausing, to ensure that the "music" doesn't drift.
        Main.gamePaused = false;
        DialogueTimer = ModContent.GetInstance<EndCreditsScene>().RantTimer;

        if (DialogueTimer >= SubtitleDisappearTime - 5 && NamelessDeityBoss.Myself is not null)
            NamelessDeityBoss.Myself.active = false;
    }
}
