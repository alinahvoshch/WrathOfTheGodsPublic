using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Items.GenesisComponents;
using NoxusBoss.Content.NPCs.Bosses.Draedon.Draedon;
using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Core.SolynEvents;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon.Projectiles;

public class SyntheticSeedlingProjectile : ModProjectile
{
    /// <summary>
    /// How long this seed has existed, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    public override string Texture => ModContent.GetInstance<SyntheticSeedling>().Texture;

    public override void SetDefaults()
    {
        Projectile.width = 36;
        Projectile.height = 50;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hostile = true;
        Projectile.timeLeft = 900000;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        Time++;

        float followSpeedInterpolant = InverseLerp(210f, 480f, Time);
        Player closest = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
        Projectile.Center = Vector2.Lerp(Projectile.Center, closest.Center, followSpeedInterpolant * 0.03f).MoveTowards(closest.Center, followSpeedInterpolant * 6f);

        foreach (Player player in Main.ActivePlayers)
        {
            if (player.Hitbox.Intersects(Projectile.Hitbox))
                Projectile.Kill();
        }

        // Bob up and down.
        float movementFadeIn = InverseLerp(0f, 60f, Time).Squared();
        float unduluation = Sin(TwoPi * Time / 150f);
        Projectile.velocity = Vector2.UnitY * unduluation * movementFadeIn * 1.25f;

        // Emit electricity.
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            float arcReachInterpolant = Main.rand.NextFloat();
            int arcLifetime = Main.rand.Next(10, 16);
            Vector2 energySpawnPosition = Projectile.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(6f, 15f) * Projectile.scale;
            Vector2 arcOffset = Projectile.SafeDirectionTo(energySpawnPosition).RotatedByRandom(0.75f) * Lerp(40f, 200f, Pow(arcReachInterpolant, 5f));
            NewProjectileBetter(Projectile.GetSource_FromThis(), energySpawnPosition, arcOffset, ModContent.ProjectileType<SmallTeslaArc>(), 0, 0f, -1, arcLifetime);

            for (int i = 0; i < 3; i++)
            {
                Dust energyDust = Dust.NewDustPerfect(energySpawnPosition, DustID.Vortex);
                energyDust.scale *= Main.rand.NextFloat(1f, 1.5f);
                energyDust.noGravity = true;
            }
        }
    }

    public override void OnKill(int timeLeft)
    {
        if (!NPC.AnyNPCs(ModContent.NPCType<QuestDraedon>()))
        {
            ModContent.GetInstance<MarsCombatEvent>().SafeSetStage(2);
            SolynEvent.Solyn?.SwitchState(SolynAIType.WaitToTeleportHome);
        }

        if (Main.netMode != NetmodeID.MultiplayerClient)
            Item.NewItem(Projectile.GetSource_Death(), Projectile.Hitbox, ModContent.ItemType<SyntheticSeedling>());
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D flare = BloomFlare.Value;
        Texture2D bloom = BloomCircleSmall.Value;

        Texture2D seedTexture = TextureAssets.Projectile[Type].Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Vector2 bloomPosition = drawPosition + Vector2.UnitY * 8f;
        Color color = Projectile.GetAlpha(Color.White);

        Main.spriteBatch.Draw(bloom, bloomPosition, null, Color.Blue with { A = 0 } * 0.32f, 0f, bloom.Size() * 0.5f, 1.2f, 0, 0f);
        Main.spriteBatch.Draw(bloom, bloomPosition, null, Color.LightBlue with { A = 0 } * 0.55f, 0f, bloom.Size() * 0.5f, 0.7f, 0, 0f);
        Main.spriteBatch.Draw(flare, bloomPosition, null, Color.Aqua with { A = 0 } * 0.6f, Main.GlobalTimeWrappedHourly * 0.4f, flare.Size() * 0.5f, 0.1f, 0, 0f);

        Main.spriteBatch.Draw(seedTexture, drawPosition, null, color, Projectile.rotation, seedTexture.Size() * 0.5f, Projectile.scale, 0, 0f);

        return false;
    }
}
