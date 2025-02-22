using Luminance.Core.ModCalls;
using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Draedon;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Core.World.WorldSaving;

namespace NoxusBoss.Core.CrossCompatibility.Outbound;

public class GetBossDefeatedModCall : ModCall
{
    internal static string[] MarsNames =
    [
        "mars"
    ];

    internal static string[] RiftNames =
    [
        "godlessspawn",
        "godless spawn",
        "noxusegg",
        "noxus egg",
        "rift",
        "noxus rift",
        "avatar rift"
    ];

    internal static string[] AvatarOfEmptinessNames =
    [
        "avatarofemptiness",
        "entropic god",
        "noxus",
    ];

    internal static string[] NamelessDeityNames =
    [
        "namelessdeity",
        "nameless deity",
    ];

    public override IEnumerable<string> GetCallCommands()
    {
        yield return "GetBossDefeated";
    }

    public override IEnumerable<Type> GetInputTypes()
    {
        yield return typeof(string);
    }

    protected override object SafeProcess(params object[] argsWithoutCommand)
    {
        string caseInvariantInput = ((string)argsWithoutCommand[0]).ToLower();

        if (MarsNames.Contains(caseInvariantInput))
            return BossDownedSaveSystem.HasDefeated<MarsBody>();

        if (RiftNames.Contains(caseInvariantInput))
            return BossDownedSaveSystem.HasDefeated<AvatarRift>();

        if (AvatarOfEmptinessNames.Contains(caseInvariantInput))
            return BossDownedSaveSystem.HasDefeated<AvatarOfEmptiness>();

        if (NamelessDeityNames.Contains(caseInvariantInput))
            return BossDownedSaveSystem.HasDefeated<NamelessDeityBoss>();

        return false;
    }
}
