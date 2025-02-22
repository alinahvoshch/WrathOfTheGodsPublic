using Luminance.Core.ModCalls;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;

namespace NoxusBoss.Core.CrossCompatibility.Outbound;

public class RemoveNamelessDeathAnimationCall : ModCall
{
    public override IEnumerable<string> GetCallCommands()
    {
        yield return "MakeNextNamelessDeathAnimationNotHappen";
    }

    public override IEnumerable<Type> GetInputTypes() => Array.Empty<Type>();

    protected override object SafeProcess(params object[] argsWithoutCommand)
    {
        NamelessDeathAnimationSkipSystem.SkipNextDeathAnimation = true;
        return new object();
    }
}
