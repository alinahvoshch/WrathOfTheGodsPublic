using Terraria.Audio;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon.Draedon;

public partial class QuestDraedon : ModNPC
{
    /// <summary>
    /// How long Draedon spends appearing as a hologram.
    /// </summary>
    public static int HologramAppearTime => SecondsToFrames(1f);

    /// <summary>
    /// The AI method that makes Draedon appear as a hologram in front of the player.
    /// </summary>
    public void DoBehavior_AppearAsHologram()
    {
        if (AITimer == 1)
        {
            NPC.TargetClosest();
            SoundEngine.PlaySound(CalamityMod.NPCs.ExoMechs.Draedon.TeleportSound, PlayerToFollow.Center);
        }

        // Look at the player.
        NPC.spriteDirection = NPC.HorizontalDirectionTo(PlayerToFollow.Center).NonZeroSign();

        HologramOverlayInterpolant = InverseLerp(HologramAppearTime, 0f, AITimer);

        if (AITimer >= HologramAppearTime)
            ChangeAIState(DraedonAIType.DialogueWithSolyn);
    }
}
