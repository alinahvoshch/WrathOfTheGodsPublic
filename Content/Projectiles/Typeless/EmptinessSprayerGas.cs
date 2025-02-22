using System.Reflection;
using Luminance.Assets;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.Items.MiscOPTools;
using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Content.Particles.Metaballs;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Projectiles.Typeless;

[LegacyName("NoxusSprayerGas")]
public class EmptinessSprayerGas : ModProjectile
{
    public bool PlayerHasMadeIncalculableMistake
    {
        get => Projectile.ai[0] == 1f;
        set => Projectile.ai[0] = value.ToInt();
    }

    public ref float Time => ref Projectile.ai[1];

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
    }

    public override void SetDefaults()
    {
        Projectile.width = 30;
        Projectile.height = 30;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.MaxUpdates = 9;
        Projectile.timeLeft = Projectile.MaxUpdates * 13;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        // Create gas.
        float particleScale = InverseLerp(0f, 25f, Time) + (Time - 32f) * 0.008f + Main.rand.NextFloat(0.075f);

        // Create more gas.
        MetaballType metaball = ModContent.GetInstance<EmptinessSprayerMetaball>();
        for (int i = 0; i < 2; i++)
        {
            if (!Main.rand.NextBool(5))
                continue;

            metaball.CreateParticle(Projectile.Center, Projectile.velocity * 0.04f + particleScale * Main.rand.NextVector2Circular(3f, 3f), particleScale * 50f, 0f, 0.92f);
        }

        // Get rid of the player if the spray was reflected by Nameless and it touches the player.
        if (PlayerHasMadeIncalculableMistake && Projectile.Hitbox.Intersects(Main.player[Projectile.owner].Hitbox) && Main.netMode == NetmodeID.SinglePlayer && Time >= 20f)
        {
            Player player = Main.player[Projectile.owner];
            for (int i = 0; i < 4; i++)
            {
                float gasSize = player.width * Main.rand.NextFloat(0.1f, 0.8f);
                metaball.CreateParticle(player.Center + Main.rand.NextVector2Circular(40f, 40f), Main.rand.NextVector2Circular(4f, 4f), gasSize);
            }

            typeof(SubworldSystem).GetField("current", BindingFlags.NonPublic | BindingFlags.Static)?.SetValue(null, null);
            typeof(SubworldSystem).GetField("cache", BindingFlags.NonPublic | BindingFlags.Static)?.SetValue(null, null);
            EmptinessSprayPlayerDeletionSystem.PlayerWasDeleted = true;
        }

        if (Main.zenithWorld)
            CreateAnything();
        else
            DeleteEverything();

        Time++;
    }

    public void CreateAnything()
    {
        // The sprayer should wait a small amount of time before spawning things.
        if (Time < 30f)
            return;

        // Check if the surrounding area of the gas is open. If it isn't, don't do anything.
        if (Collision.SolidCollision(Projectile.Center - Vector2.One * 80f, 160, 160, true))
            return;

        // NPC spawns happen randomly.
        if (!Main.rand.NextBool(120))
            return;

        // NPC spawns can only happen serverside.
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        int npcID = Main.rand.Next(NPCLoader.NPCCount);
        NPC dummyNPC = new NPC();
        dummyNPC.SetDefaults(npcID);

        // Various NPCs could cause problems and should not spawn.
        bool isAvatar = npcID == ModContent.NPCType<AvatarRift>() || npcID == ModContent.NPCType<AvatarOfEmptiness>();
        bool isNameless = npcID == ModContent.NPCType<NamelessDeityBoss>();
        bool isSolyn = npcID == ModContent.NPCType<Solyn>() || npcID == ModContent.NPCType<BattleSolyn>();
        bool isWoF = npcID == NPCID.WallofFlesh || npcID == NPCID.WallofFleshEye;
        bool isOOAEntity = npcID == NPCID.DD2EterniaCrystal || npcID == NPCID.DD2LanePortal;
        bool isTownNPC = NPCID.Sets.IsTownSlime[npcID] || NPCID.Sets.IsTownPet[npcID] || dummyNPC.aiStyle == 7;
        bool isRegularDummy = npcID == NPCID.TargetDummy;
        if (isAvatar || isNameless || isSolyn || isWoF || isOOAEntity || isTownNPC || isRegularDummy)
            return;

        // Spawn the NPC.
        NPC.NewNPC(Projectile.GetSource_FromThis(), (int)Projectile.Center.X, (int)Projectile.Center.Y, npcID, 1);
    }

    public void DeleteEverything()
    {
        MetaballType metaball = ModContent.GetInstance<EmptinessSprayerMetaball>();
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC n = Main.npc[i];
            if (!n.active || !n.Hitbox.Intersects(Projectile.Hitbox) || EmptinessSprayer.NPCsToNotDelete[n.type])
                continue;

            // Reflect the spray if the player has misused it by daring to try and delete Nameless.
            if (EmptinessSprayer.NPCsThatReflectSpray[n.type])
            {
                if (!PlayerHasMadeIncalculableMistake && n.Opacity >= 0.02f)
                {
                    PlayerHasMadeIncalculableMistake = true;
                    EmptinessSprayPlayerDeletionSystem.PlayerWasDeletedByNamelessDeity = n.type == ModContent.NPCType<NamelessDeityBoss>();
                    EmptinessSprayPlayerDeletionSystem.PlayerWasDeletedByLaRuga = n.type == EmptinessSprayer.LaRugaID;
                    Projectile.velocity *= -0.6f;
                    Projectile.netUpdate = true;
                }
                continue;
            }

            n.active = false;
            for (int j = 0; j < 20; j++)
            {
                float npcSize = (n.width + n.height) * 0.5f;
                float gasSize = npcSize * Main.rand.NextFloat(0.25f, 0.9f);
                metaball.CreateParticle(n.Center + Main.rand.NextVector2Circular(40f, 40f), Main.rand.NextVector2Circular(4f, 4f), gasSize, 0f, 0.95f);
            }
        }
    }

    public override bool PreDraw(ref Color lightColor) => false;
}
