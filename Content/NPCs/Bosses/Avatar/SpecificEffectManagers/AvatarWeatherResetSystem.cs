using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;

public class AvatarWeatherResetSystem : ModSystem
{
    /// <summary>
    /// Whether extreme weather is ongoing as a result of one of the Avatar's dimensions.
    /// </summary>
    public static bool ExtremeWeatherOngoing
    {
        get;
        set;
    }

    // This variable is saved to account for the possibility of the player saving and quitting during the Avatar's fight.
    // By doing this, it ensures that when the player returns the weather is reset due to ExtremeWeatherOngoing being acknowledged as true at the time of world exit.
    public override void SaveWorldData(TagCompound tag)
    {
        if (ExtremeWeatherOngoing)
            tag["ExtremeWeatherOngoing"] = true;
    }

    public override void LoadWorldData(TagCompound tag) => ExtremeWeatherOngoing = tag.ContainsKey("ExtremeWeatherOngoing");

    public override void PreUpdateNPCs()
    {
        bool extremeWeatherWasOngoing = ExtremeWeatherOngoing;
        ExtremeWeatherOngoing = AvatarOfEmptinessSky.Dimension is not null && Abs(Main.windSpeedCurrent) >= 1.32f;

        if (Main.netMode != NetmodeID.MultiplayerClient && !ExtremeWeatherOngoing && extremeWeatherWasOngoing)
        {
            Main.windSpeedTarget = Main.rand.NextFloatDirection() * 0.25f;
            Main.windSpeedCurrent = 0f;
            NetMessage.SendData(MessageID.WorldData);
        }
    }
}
