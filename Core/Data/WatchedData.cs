namespace NoxusBoss.Core.Data;

public class WatchedData
{
    /// <summary>
    /// The data to watch.
    /// </summary>
    public Dictionary<string, object> Data;

    /// <summary>
    /// The intended type of the data.
    /// </summary>
    public readonly Type IntendedType;

    public WatchedData(Dictionary<string, object> data, Type intendedType)
    {
        Data = data;
        IntendedType = intendedType;
    }
}
