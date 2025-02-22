using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.GlobalInstances;

[LegacyName("NoxusGlobalProjectile")]
public class GlobalProjectileEventHandlers : GlobalProjectile
{
    public delegate bool ProjectileConditionDelegate(Projectile projectile);

    public static event ProjectileConditionDelegate? PreAIEvent;

    public static event ProjectileConditionDelegate? PreKillEvent;

    public override void Unload()
    {
        // Reset all events on mod unload.
        PreAIEvent = null;
        PreKillEvent = null;
    }

    public override bool PreAI(Projectile projectile)
    {
        // Use default behavior if the event has no subscribers.
        if (PreAIEvent is null)
            return true;

        bool result = true;
        foreach (Delegate d in PreAIEvent.GetInvocationList())
            result &= ((ProjectileConditionDelegate)d).Invoke(projectile);

        return result;
    }

    public override bool PreKill(Projectile projectile, int timeLeft)
    {
        // Use default behavior if the event has no subscribers.
        if (PreKillEvent is null)
            return true;

        bool result = true;
        foreach (Delegate d in PreKillEvent.GetInvocationList())
            result &= ((ProjectileConditionDelegate)d).Invoke(projectile);

        return result;
    }
}
