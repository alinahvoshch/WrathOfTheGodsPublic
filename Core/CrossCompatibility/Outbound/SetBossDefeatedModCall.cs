using Luminance.Core.ModCalls;
using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Draedon;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Core.World.WorldSaving;
using static NoxusBoss.Core.CrossCompatibility.Outbound.GetBossDefeatedModCall;

namespace NoxusBoss.Core.CrossCompatibility.Outbound;

public class SetBossDefeatedModCall : ModCall
{
    public override IEnumerable<string> GetCallCommands()
    {
        yield return "GetBossDefeated";
    }

    public override IEnumerable<Type> GetInputTypes()
    {
        yield return typeof(string);
        yield return typeof(bool);
    }

    protected override object SafeProcess(params object[] argsWithoutCommand)
    {
        string caseInvariantInput = ((string)argsWithoutCommand[0]).ToLower();
        bool setValue = (bool)argsWithoutCommand[1];

        if (MarsNames.Contains(caseInvariantInput))
            BossDownedSaveSystem.SetDefeatState<MarsBody>(setValue);

        if (RiftNames.Contains(caseInvariantInput))
            BossDownedSaveSystem.SetDefeatState<AvatarRift>(setValue);

        if (AvatarOfEmptinessNames.Contains(caseInvariantInput))
            BossDownedSaveSystem.SetDefeatState<AvatarOfEmptiness>(setValue);

        if (NamelessDeityNames.Contains(caseInvariantInput))
            BossDownedSaveSystem.SetDefeatState<NamelessDeityBoss>(setValue);

        return new object();
    }
}
