using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.Draedon.Projectiles;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon.Draedon;

public partial class QuestDraedon : ModNPC
{
    /// <summary>
    /// The AI method that makes Draedon wait for someone to take the synthetic seed.
    /// </summary>
    public void DoBehavior_WaitForSomeoneToTakeSeed()
    {
        Vector2 hoverDestination = PlayerToFollow.Center + PlayerToFollow.SafeDirectionTo(NPC.Center) * Vector2.UnitX * 120f;
        NPC.SmoothFlyNear(hoverDestination, 0.06f, 0.9f);
        NPC.spriteDirection = (int)NPC.HorizontalDirectionTo(PlayerToFollow.Center);

        if (FrameTimer % 7f == 6f)
        {
            Frame++;
            if (Frame >= 48)
                Frame = 23;
        }

        if (!AnyProjectiles(ModContent.ProjectileType<SyntheticSeedlingProjectile>()))
            ChangeAIState(DraedonAIType.EndingMonologue);
    }
}
