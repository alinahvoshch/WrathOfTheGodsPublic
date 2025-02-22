namespace NoxusBoss.Core.World.WorldSaving;

public interface IBossDowned
{
    public bool AutomaticallyRegisterDeathGlobally
    {
        get;
    }

    public void OnDefeat() { }
}
