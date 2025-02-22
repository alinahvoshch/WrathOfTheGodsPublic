using Luminance.Core.Hooking;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using NoxusBoss.Content.Particles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Fishing;

public class DreamcatcherBobber : ModProjectile
{
    public enum CatchType
    {
        // These numbers correspond to frames on the sheet.
        RegularFish = 0,
        Crates = 1,
        QuestFish = 2,
        Count = 3
    }

    /// <summary>
    /// The type of bobber this is.
    /// </summary>
    public CatchType BobberType
    {
        get => (CatchType)Projectile.ai[2];
        set => Projectile.ai[2] = (int)value;
    }

    public delegate void FishingAttemptDelegate(ref FishingAttempt attempt);

    public override string Texture => "NoxusBoss/Assets/Textures/Content/Items/Fishing/DreamcatcherBobber";

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = (int)CatchType.Count;

        new ManagedILEdit("Modify Dreamcatcher Yields", Mod, edit =>
        {
            IL_Projectile.FishingCheck += edit.SubscriptionWrapper;
        }, edit =>
        {
            IL_Projectile.FishingCheck -= edit.SubscriptionWrapper;
        }, UseSpecialYields).Apply(false);
    }

    private void UseSpecialYields(ILContext context, ManagedILEdit edit)
    {
        ILCursor cursor = new ILCursor(context);

        if (!cursor.TryGotoNext(MoveType.After, c => c.MatchCallOrCallvirt<Projectile>("FishingCheck_RollItemDrop")))
        {
            edit.LogFailure("The FishingCheck_RollItemDrop call could not be found!");
            return;
        }

        int afterItemRolls = cursor.Index;

        int fishingInfoIndex = 0;
        if (!cursor.TryGotoPrev(MoveType.Before, c => c.MatchLdloca(out fishingInfoIndex)))
        {
            edit.LogFailure("The FishingInfo instance could not be found!");
            return;
        }

        cursor.Goto(afterItemRolls);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldloca, fishingInfoIndex);
        cursor.EmitDelegate(ForceQuestFish);

        cursor.Goto(0);
        if (!cursor.TryGotoNext(MoveType.After, c => c.MatchCallOrCallvirt<Projectile>("FishingCheck_RollEnemySpawns")))
        {
            edit.LogFailure("The FishingCheck_RollEnemySpawns call could not be found!");
            return;
        }

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldloca, fishingInfoIndex);
        cursor.EmitDelegate(ModifyFishingYields);
    }

    private void ModifyFishingYields(Projectile projectile, ref FishingAttempt attempt)
    {
        if (projectile.type != Type || projectile.ModProjectile is not DreamcatcherBobber dreamcatcherBobber)
            return;

        CatchType bobberType = dreamcatcherBobber.BobberType;
        switch (bobberType)
        {
            case CatchType.RegularFish:
                if (Main.rand.NextBool(16))
                    attempt.rare = true;
                else if (Main.rand.NextBool(6))
                    attempt.uncommon = true;
                else
                    attempt.common = true;
                attempt.crate = false;
                break;
            case CatchType.Crates:
                attempt.crate = true;
                break;
        }
    }

    private void ForceQuestFish(Projectile projectile, ref FishingAttempt attempt)
    {
        if (projectile.type != Type || projectile.ModProjectile is not DreamcatcherBobber dreamcatcherBobber)
            return;

        CatchType bobberType = dreamcatcherBobber.BobberType;
        switch (bobberType)
        {
            case CatchType.QuestFish:
                attempt.rolledItemDrop = Main.anglerQuestItemNetIDs[Main.anglerQuest];
                break;
        }
    }

    public override void SetDefaults()
    {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.aiStyle = ProjAIStyleID.Bobber;
        Projectile.bobber = true;
        DrawOriginOffsetY = -8;
    }

    public override void AI()
    {
        Projectile.frame = (int)BobberType;

        if (Main.rand.NextBool(20))
        {
            int starPoints = Main.rand.Next(3, 9);
            float starScaleInterpolant = Main.rand.NextFloat();
            int starLifetime = (int)Lerp(20f, 54f, starScaleInterpolant);
            float starScale = Lerp(0.2f, 0.4f, starScaleInterpolant);
            Color starColor = Color.Lerp(Color.Yellow, Color.HotPink, 0.85f) * 0.85f;
            Vector2 starVelocity = Main.rand.NextVector2Circular(3f, 4f) - Vector2.UnitY * 2.9f;
            TwinkleParticle star = new TwinkleParticle(Projectile.Center, starVelocity, starColor, starLifetime, starPoints, new Vector2(Main.rand.NextFloat(0.4f, 1.6f), 1f) * starScale, starColor * 0.5f);
            star.Spawn();
        }

        Lighting.AddLight(Projectile.Center, Vector3.One * 0.5f);
    }
}
