using Luminance.Core.ModCalls;
using NoxusBoss.Core.World.GameScenes.RiftEclipse;

namespace NoxusBoss.Core.CrossCompatibility.Outbound;

public class GetRiftEclipseActiveModCall : ModCall
{
    public override IEnumerable<string> GetCallCommands()
    {
        yield return "GetRiftEclipseActive";
    }

    public override IEnumerable<Type> GetInputTypes() => Array.Empty<Type>();

    protected override object SafeProcess(params object[] argsWithoutCommand) => RiftEclipseManagementSystem.RiftEclipseOngoing;
}
