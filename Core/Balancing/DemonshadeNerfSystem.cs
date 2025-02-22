using System.Reflection;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Armor.Demonshade;
using Luminance.Core.Hooking;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Balancing;

[JITWhenModsEnabled(CalamityCompatibility.ModName)]
[ExtendsFromMod(CalamityCompatibility.ModName)]
public sealed class DemonshadeNerfSystem : ModSystem
{
    public class DemonshadeNerfSystem_BuffChanger : GlobalBuff
    {
        public override void ModifyBuffText(int type, ref string buffName, ref string tip, ref int rare)
        {
            if (type == ModContent.BuffType<Enraged>())
            {
                tip = tip.
                    Replace("2.25", $"{Round(DemonshadeDamageToNPCPercent * 0.01f + 1f, 2)}").
                    Replace("1.25", $"{Round(DemonshadeDamageToPlayerPercent * 0.01f + 1f, 2)}");
            }
        }
    }

    /// <summary>
    /// The amount of damage players incur upon having the Enraged buff.
    /// </summary>
    public static float DemonshadeDamageToPlayerPercent => 50f;

    /// <summary>
    /// The amount of damage players do to NPCs upon having the Enraged buff.
    /// </summary>
    public static float DemonshadeDamageToNPCPercent => 50f;

    /// <summary>
    /// The enraged field in CalamityPlayer.
    /// </summary>
    public static FieldInfo? CalPlayerEnragedField
    {
        get;
        private set;
    }

    /// <summary>
    /// The IL edit responsible for the increasing damage to the player.
    /// </summary>
    public static ILHook PlayerDamageIncreaseHook
    {
        get;
        private set;
    }

    /// <summary>
    /// The IL edit responsible for the increasing damage to NPCs via projectiles.
    /// </summary>
    public static ILHook ProjectileDamageIncreaseHook
    {
        get;
        private set;
    }

    /// <summary>
    /// The IL edit responsible for changing the armor set bonus text of the demonshade helm.
    /// </summary>
    public static ILHook DemonshadeHelmArmorBonusTextHook
    {
        get;
        private set;
    }

    /// <summary>
    /// The IL edit responsible for the increasing damage to NPCs via items.
    /// </summary>
    public static ILHook ItemDamageIncreaseHook
    {
        get;
        private set;
    }

    public override void OnModLoad()
    {
        // Load fields via reflection.
        CalPlayerEnragedField = typeof(CalamityPlayer).GetField("enraged", UniversalBindingFlags);
        MethodInfo? calPlayerModifyHurtMethod = typeof(CalamityPlayer).GetMethod("ModifyHurt", UniversalBindingFlags);
        MethodInfo? calPlayerModifyHitProjMethod = typeof(CalamityPlayer).GetMethod("ModifyHitNPCWithProj", UniversalBindingFlags);
        MethodInfo? calPlayerModifyHitItemMethod = typeof(CalamityPlayer).GetMethod("ModifyHitNPCWithItem", UniversalBindingFlags);
        MethodInfo? demonshadeHelmUpdateArmorSetMethod = typeof(DemonshadeHelm).GetMethod("UpdateArmorSet", UniversalBindingFlags);
        if (CalPlayerEnragedField is null)
        {
            Mod.Logger.Warn("Could not find the CalamityPlayer enraged field for the Demonshade Nerf system.");
            return;
        }
        if (calPlayerModifyHurtMethod is null)
        {
            Mod.Logger.Warn("Could not find the ModifyHurt method for the Demonshade Nerf system.");
            return;
        }
        if (calPlayerModifyHitProjMethod is null)
        {
            Mod.Logger.Warn("Could not find the ModifyHitNPCWithProj method for the Demonshade Nerf system.");
            return;
        }
        if (calPlayerModifyHitItemMethod is null)
        {
            Mod.Logger.Warn("Could not find the ModifyHitNPCWithItem method for the Demonshade Nerf system.");
            return;
        }
        if (demonshadeHelmUpdateArmorSetMethod is null)
        {
            Mod.Logger.Warn("Could not find the UpdateArmorSet method for the Demonshade Nerf system.");
            return;
        }

        // Apply edits.
        new ManagedILEdit("Increase Demonshade Damage to Player", Mod, edit =>
        {
            PlayerDamageIncreaseHook = new(calPlayerModifyHurtMethod, edit.SubscriptionWrapper);
        }, _ =>
        {
            PlayerDamageIncreaseHook?.Undo();
        }, TweakPlayerDamage).Apply();

        new ManagedILEdit("Increase Demonshade Damage to NPCs (Projectiles)", Mod, edit =>
        {
            ProjectileDamageIncreaseHook = new(calPlayerModifyHitProjMethod, edit.SubscriptionWrapper);
        }, _ =>
        {
            ProjectileDamageIncreaseHook?.Undo();
        }, (c, e) => TweakNPCDamage(c, e, 1.25f)).Apply();

        new ManagedILEdit("Increase Demonshade Damage to NPCs (Items)", Mod, edit =>
        {
            ItemDamageIncreaseHook = new(calPlayerModifyHitItemMethod, edit.SubscriptionWrapper);
        }, _ =>
        {
            ItemDamageIncreaseHook?.Undo();
        }, (c, e) => TweakNPCDamage(c, e, 1.25f)).Apply();

        new ManagedILEdit("Change Demonshade Armor Armor Bonus Text", Mod, edit =>
        {
            DemonshadeHelmArmorBonusTextHook = new(demonshadeHelmUpdateArmorSetMethod, edit.SubscriptionWrapper);
        }, _ =>
        {
            DemonshadeHelmArmorBonusTextHook?.Undo();
        }, UpdateDemonshadeText).Apply();
    }

    private static void TweakPlayerDamage(ILContext context, ManagedILEdit edit)
    {
        ILCursor cursor = new ILCursor(context);

        if (CalPlayerEnragedField is null)
            return;

        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdfld(CalPlayerEnragedField)))
        {
            edit.LogFailure("The eranged field load could not be found.");
            return;
        }
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdcR8(0.25)))
        {
            edit.LogFailure("The 0.25 load could not be found.");
            return;
        }
        cursor.Emit(OpCodes.Pop);
        cursor.EmitDelegate(() => DemonshadeDamageToPlayerPercent * 0.01f);
    }

    private static void TweakNPCDamage(ILContext context, ManagedILEdit edit, float originalDamageFactor)
    {
        ILCursor cursor = new ILCursor(context);

        if (CalPlayerEnragedField is null)
            return;

        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdfld(CalPlayerEnragedField)))
        {
            edit.LogFailure("The eranged field load could not be found.");
            return;
        }
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdcR4(originalDamageFactor)))
        {
            edit.LogFailure($"The {originalDamageFactor} load could not be found.");
            return;
        }

        cursor.Emit(OpCodes.Pop);
        cursor.EmitDelegate(() => DemonshadeDamageToNPCPercent * 0.01f);
    }

    private static void UpdateDemonshadeText(ILContext context, ManagedILEdit edit)
    {
        ILCursor cursor = new ILCursor(context);

        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchStfld<Player>("setBonus")))
        {
            edit.LogFailure("The Player.setBonus storage could not be found.");
            return;
        }

        cursor.Emit(OpCodes.Ldarg_1);
        cursor.EmitDelegate((string originalText, Player player) =>
        {
            return originalText.
                Replace("2.25", $"{Round(DemonshadeDamageToNPCPercent * 0.01f + 1f, 2)}").
                Replace("1.25", $"{Round(DemonshadeDamageToPlayerPercent * 0.01f + 1f, 2)}");
        });
    }
}
