using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon.Draedon;

public partial class QuestDraedon : ModNPC
{
    /// <summary>
    /// The AI method that makes Draedon disappear and leave.
    /// </summary>
    public void DoBehavior_Leave()
    {
        HologramOverlayInterpolant = InverseLerp(0f, HologramAppearTime, AITimer);

        if (FrameTimer % 7f == 6f)
        {
            Frame++;
            if (Frame >= 48)
                Frame = 23;
        }

        NPC.velocity *= 0.9f;

        if (AITimer >= HologramAppearTime)
            NPC.active = false;
    }
}
