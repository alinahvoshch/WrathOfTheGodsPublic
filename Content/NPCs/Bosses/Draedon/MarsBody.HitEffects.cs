using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon;

public partial class MarsBody : ModNPC
{
    /// <summary>
    /// The event that dictates what happens when Mars is hit by a tag team beam from the player and Solyn.
    /// </summary>
    public event Action<Projectile> TeamBeamHitEffectEvent;

    /// <summary>
    /// Registers an oncoming hit from a tag team beam.
    /// </summary>
    /// <param name="beam">The beam projectile.</param>
    public void RegisterHitByTeamBeam(Projectile beam) => TeamBeamHitEffectEvent?.Invoke(beam);
}
