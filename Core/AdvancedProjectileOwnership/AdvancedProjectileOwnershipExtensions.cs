using Microsoft.Xna.Framework;
using NoxusBoss.Core.Fixes;
using Terraria;
using Terraria.DataStructures;
using LumUtils = Luminance.Common.Utilities.Utilities;

namespace NoxusBoss.Core.AdvancedProjectileOwnership;

public static class AdvancedProjectileOwnershipExtensions
{
    /// <summary>
    /// Spawns a new projectile owned by a given entity.
    /// </summary>
    public static int NewProjectileBetter(this Entity owner, IEntitySource source, Vector2 spawnPosition, Vector2 velocity, int type, int damage, float knockback, int playerOwner = -1, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f)
    {
        ProjectileSpawnManagementSystem.PrepareProjectileForSpawning(p =>
        {
            AdvancedProjectileOwnershipSystem.ownerRelationship[p.identity] = owner;
        });

        return LumUtils.NewProjectileBetter(source, spawnPosition, velocity, type, damage, knockback, playerOwner, ai0, ai1, ai2); ;
    }

    /// <summary>
    /// Spawns a new projectile from a projectile, maintaining the original owner of the projectile that spawned it.
    /// </summary>
    public static int NewProjectileBetter_InheritedOwner(this Projectile projectile, IEntitySource source, Vector2 spawnPosition, Vector2 velocity, int type, int damage, float knockback, int playerOwner = -1, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f)
    {
        ProjectileSpawnManagementSystem.PrepareProjectileForSpawning(p =>
        {
            if (AdvancedProjectileOwnershipSystem.ownerRelationship.TryGetValue(projectile.identity, out Entity? originalOwner))
                AdvancedProjectileOwnershipSystem.ownerRelationship[p.identity] = originalOwner;
        });

        return LumUtils.NewProjectileBetter(source, spawnPosition, velocity, type, damage, knockback, playerOwner, ai0, ai1, ai2); ;
    }

    /// <summary>
    /// Attempts to retrieve an owner of a given type for a given projectile.
    /// </summary>
    /// <typeparam name="T">The type of owner sought.</typeparam>
    /// <param name="projectile">The projectile.</param>
    /// <param name="owner">The resulting owner. Will outputted as null if no owner is found.</param>
    public static bool TryGetGenericOwner<T>(this Projectile projectile, out T owner) where T : Entity
    {
        // Look, I know this is technically incorrect, but I'm not going to deal with the IDE complaining about the possibility of the out parameter being null
        // when this is literally a TryGetX method with safe wrapping. Frankly, if a developer misuses this method and doesn't respect the boolean output, I
        // consider that their problem.
        owner = null!;

        if (AdvancedProjectileOwnershipSystem.ownerRelationship.TryGetValue(projectile.identity, out Entity? ownerEntity) &&
            ownerEntity is T typedOwner)
        {
            owner = typedOwner;
            return true;
        }

        return false;
    }
}
