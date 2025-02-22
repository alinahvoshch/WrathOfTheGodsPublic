using Terraria.ModLoader.IO;

namespace NoxusBoss.Core.Graphics.UI.GraphicalUniverseImager;

public class GraphicalUniverseImagerSettings
{
    public enum EclipseSecondaryAmbienceSetting : byte
    {
        None,
        Blizzard,
        BloodRain,
        Fog
    }

    /// <summary>
    /// The size of the rift in the background. Does not apply if the main eclipse setting is not chosen.
    /// </summary>
    public float RiftSize;

    /// <summary>
    /// The settings of the eclipse ambience. Does not apply if the main eclipse setting is not chosen.
    /// </summary>
    public EclipseSecondaryAmbienceSetting EclipseAmbienceSettings;

    /// <summary>
    /// The chosen GUI option by this user.
    /// </summary>
    public GraphicalUniverseImagerOption? Option
    {
        get;
        set;
    }

    /// <summary>
    /// The chosen GUI option by this user.
    /// </summary>
    public GraphicalUniverseImagerMusicOption Music
    {
        get;
        set;
    } = GraphicalUniverseImagerMusicManager.Default;

    /// <summary>
    /// Saves these settings.
    /// </summary>
    public void Save(TagCompound tag)
    {
        tag["RiftSize"] = RiftSize;
        tag["EclipseAmbienceSettings"] = (byte)EclipseAmbienceSettings;
        tag["OptionKey"] = Option?.LocalizationKey ?? "None";
        tag["MusicKey"] = Music.LocalizationKey;
    }

    /// <summary>
    /// Loads these settings.
    /// </summary>
    public void Load(TagCompound tag)
    {
        RiftSize = tag.GetFloat("RiftSize");
        EclipseAmbienceSettings = (EclipseSecondaryAmbienceSetting)tag.GetByte("EclipseAmbienceSettings");
        if (GraphicalUniverseImagerOptionManager.options.TryGetValue(tag.GetString("OptionKey"), out GraphicalUniverseImagerOption? option))
            Option = option;

        if (GraphicalUniverseImagerMusicManager.musicOptions.TryGetValue(tag.GetString("MusicKey"), out GraphicalUniverseImagerMusicOption? music))
            Music = music;
        else
            Music = GraphicalUniverseImagerMusicManager.Default;
    }

    /// <summary>
    /// Sends these settings to a given <see cref="BinaryWriter"/>, for the purposes of sending settings across the network.
    /// </summary>
    public void Send(BinaryWriter writer)
    {
        writer.Write(RiftSize);
        writer.Write((byte)EclipseAmbienceSettings);
        writer.Write(Option?.LocalizationKey ?? "None");
        writer.Write(Music.LocalizationKey);
    }

    /// <summary>
    /// Receives these settings from a given <see cref="BinaryReader"/>, for the purposes of receiving settings across the network.
    /// </summary>
    public void Receive(BinaryReader reader)
    {
        RiftSize = reader.ReadSingle();
        EclipseAmbienceSettings = (EclipseSecondaryAmbienceSetting)reader.ReadByte();

        string optionsKey = reader.ReadString();
        if (GraphicalUniverseImagerOptionManager.options.TryGetValue(optionsKey, out GraphicalUniverseImagerOption? option))
            Option = option;

        string musicKey = reader.ReadString();
        if (GraphicalUniverseImagerMusicManager.musicOptions.TryGetValue(musicKey, out GraphicalUniverseImagerMusicOption? music))
            Music = music;
        else
            Music = GraphicalUniverseImagerMusicManager.Default;
    }

    public GraphicalUniverseImagerSettings GenerateCopy()
    {
        GraphicalUniverseImagerSettings copy = new GraphicalUniverseImagerSettings
        {
            RiftSize = RiftSize,
            EclipseAmbienceSettings = EclipseAmbienceSettings,
            Option = Option,
            Music = Music
        };

        return copy;
    }

    public bool SameAs(GraphicalUniverseImagerSettings other)
    {
        if (other.RiftSize != RiftSize)
            return false;
        if (other.EclipseAmbienceSettings != EclipseAmbienceSettings)
            return false;
        if (other.Music.LocalizationKey != Music.LocalizationKey)
            return false;
        if ((other.Option?.LocalizationKey ?? "None") != (Option?.LocalizationKey ?? "None"))
            return false;

        return true;
    }
}
