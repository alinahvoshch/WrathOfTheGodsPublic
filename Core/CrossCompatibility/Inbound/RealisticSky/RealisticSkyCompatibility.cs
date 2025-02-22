namespace NoxusBoss.Core.CrossCompatibility.Inbound.RealisticSky;

public static class RealisticSkyCompatibility
{
    /// <summary>
    /// The intensity of bloom from the sun.
    /// </summary>
    public static float SunBloomOpacity
    {
        set
        {
            if (ModReferences.RealisticSky is null)
                return;

            ModReferences.RealisticSky.Call("setsunbloomopacity", value);
        }
    }

    public static void TemporarilyDisable() => ModReferences.RealisticSky?.Call("temporarilydisable");
}
