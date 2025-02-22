using Microsoft.Xna.Framework;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;

public class NamelessDeityRodOfHarmonyRantSystem : NamelessDeitySubtitleSystem
{
    public override List<NamelessDeityDialog> Sentences => new List<NamelessDeityDialog>()
    {
        // Get that Rod of Harmony out of here, I don't ever want to see that ever again!
        new(0f, "RodOfHarmonyRantSubtitle1"),

        // I mean, what was Re-Logic thinking? A rod of discord with no cooldown? Seriously?
        new(3.399f, "RodOfHarmonyRantSubtitle2"),
        
        // Post-Moon Lord modding is RUINED...
        new(8.651f, "RodOfHarmonyRantSubtitle3"),
        
        // Come on, man.
        new(10.909f, "RodOfHarmonyRantSubtitle4"),
        
        // I wanted to create a universe that is fair and square. But the Rod of Harmony ruins EVERYTHING.
        new(12.216f, "RodOfHarmonyRantSubtitle5"),

        // So no. I'm not letting you use it.
        new(19.298f, "RodOfHarmonyRantSubtitle6"),
        
        // You thought you could troll me, and it turns out YOU'RE the one getting trolled.
        new(21.081f, "RodOfHarmonyRantSubtitle7"),
        
        // And you know that.
        new(25.216f, "RodOfHarmonyRantSubtitle8"),
        
        // Hell, you probably already knew that anyway.
        new(26.547f, "RodOfHarmonyRantSubtitle9"),
        
        // You probably looked up the fight on the Internet.
        new(28.662f, "RodOfHarmonyRantSubtitle10"),
        
        // You probably KNOW that I'm doing this.
        new(30.848f, "RodOfHarmonyRantSubtitle11"),
        
        // I mean, I can't see why you wouldn't expect it, I already smashed the Rage and Adrenaline, of COURSE I'm going to smash the Rod of Harmony!
        new(33.131f, "RodOfHarmonyRantSubtitle12"),
        
        // But now, I'm gonna have to smash you.
        new(39.499f, "RodOfHarmonyRantSubtitle13"),
        
        // Ooh, that came out wrong...
        new(41.852f, "RodOfHarmonyRantSubtitle14"),
        
        // Basically, what it boils down to is: I don't like cheaters.
        new(43.302f, "RodOfHarmonyRantSubtitle15"),
        
        // And if you disagree with that, I don't know what you're doing here.
        new(46.748f, "RodOfHarmonyRantSubtitle16"),
        
        // And you probably should've never brought that damn item in here.
        new(49.885f, "RodOfHarmonyRantSubtitle17"),
    };

    public override float SubtitleDisappearTime => SecondsToFrames(52.761f);

    public override void UpdateUI(GameTime gameTime)
    {
        DialogueTimer = 0;
        if (NamelessDeityBoss.Myself is null || NamelessDeityBoss.Myself.As<NamelessDeityBoss>().CurrentState != NamelessDeityBoss.NamelessAIType.RodOfHarmonyRant)
            return;

        // Disallow game pausing, to ensure that the "music" doesn't drift from Nameless' attack timer.
        Main.gamePaused = false;
        DialogueTimer = NamelessDeityBoss.Myself.As<NamelessDeityBoss>().AITimer - NamelessDeityBoss.RodOfHarmonyRantDelay + 10;
    }
}
