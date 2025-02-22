using CalRemix.UI;
using NoxusBoss.Core.Autoloaders.SolynBooks;
using NoxusBoss.Core.CrossCompatibility.Inbound;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    /// <summary>
    /// A cooldown timer that ensures that the Fanatical Ramblings book cannot spawn in rapid succession.
    /// </summary>
    public static int FanaticalRamblingsSpawnCooldown
    {
        get;
        set;
    }

    private static void DecrementFanaticalRamblingsSpawnCooldown()
    {
        if (FanaticalRamblingsSpawnCooldown >= 1)
            FanaticalRamblingsSpawnCooldown--;
    }

    private static void PerformFanaticalRamblingsCheck_Wrapper()
    {
        if (ModReferences.CalamityRemixMod is null)
            return;

        PerformFanaticalRamblingsCheck();
    }

    [JITWhenModsEnabled("CalRemix")]
    private static void PerformFanaticalRamblingsCheck()
    {
        if (ScreenHelpersUIState.FannyTheFire.tickle >= 22f && FanaticalRamblingsSpawnCooldown <= 0)
        {
            FanaticalRamblingsSpawnCooldown = MinutesToFrames(60f);
            Item.NewItem(new EntitySource_WorldEvent(), Main.LocalPlayer.Center, SolynBookAutoloader.Books["FanaticalRamblings"].Type);

            SoundEngine.PlaySound(ScreenHelpersUIState.FannyTheFire.speakingSound);
            ScreenHelpersUIState.FannyTheFire.tickle = 0f;
        }
    }
}
