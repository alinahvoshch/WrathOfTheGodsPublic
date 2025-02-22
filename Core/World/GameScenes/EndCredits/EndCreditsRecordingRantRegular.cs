using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.World.GameScenes.EndCredits;

public class EndCreditsRecordingRantRegular : NamelessDeitySubtitleSystem
{
    public override List<NamelessDeityDialog> Sentences => new List<NamelessDeityDialog>()
    {
        // Hey. You know...I can't really tell you what to do and all, but...
        new(0f, "RecordingRantRegularSubtitle1"),

        // It would be nice if you left things like this a secret for others to find in-game, you know?
        new(4.504f, "RecordingRantRegularSubtitle2"),
        
        // You don't have to upload absolutely everything for others to see.
        new(9.304f, "RecordingRantRegularSubtitle3"),
        
        // I created this cutscene for those who have earned it, after all.
        new(13.381f, "RecordingRantRegularSubtitle4"),
       
        // And it would be nice if you just... thought about it.
        new(17.042f, "RecordingRantRegularSubtitle5"),
        
        // Okay?
        new(19.848f, "RecordingRantRegularSubtitle6"),
        
        // Like, what if you just... enjoyed the moment?
        new(20.971f, "RecordingRantRegularSubtitle7"),
        
        // Take pride in your own accomplishments instead of uploading them online for the approval of others.
        new(23.806f, "RecordingRantRegularSubtitle8"),
        
        // Anyways, I hope you at least consider my word.
        new(29.241f, "RecordingRantRegularSubtitle9"),
        
        // Don't do it for me, but do it for others.
        new(32.504f, "RecordingRantRegularSubtitle10")
    };

    public override float SubtitleDisappearTime => SecondsToFrames(35.925f);

    public override void UpdateUI(GameTime gameTime)
    {
        DialogueTimer = 0;
        if (!ModContent.GetInstance<EndCreditsScene>().IsActive || ModContent.GetInstance<EndCreditsScene>().State != EndCreditsScene.CreditsState.RecordingSoftwareRant_Regular)
            return;

        // Disallow game pausing, to ensure that the "music" doesn't drift.
        Main.gamePaused = false;
        DialogueTimer = ModContent.GetInstance<EndCreditsScene>().RantTimer;

        if (DialogueTimer >= SubtitleDisappearTime - 5 && NamelessDeityBoss.Myself is not null)
            NamelessDeityBoss.Myself.active = false;
    }
}
