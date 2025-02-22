using Luminance.Common.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;
using NoxusBoss.Core.World.GameScenes.RiftEclipse;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;

public class AcceleratingRubble : ModProjectile, IProjOwnedByBoss<AvatarOfEmptiness>
{
    /// <summary>
    /// The X frame of this rubble.
    /// </summary>
    public ref float FrameX => ref Projectile.ai[0];

    /// <summary>
    /// The Y frame of this rubble.
    /// </summary>
    public ref float FrameY => ref Projectile.ai[1];

    /// <summary>
    /// The initial scale of this rubble.
    /// </summary>
    public ref float StartingScale => ref Projectile.localAI[2];

    public override string Texture => GetAssetPath("Content/NPCs/Bosses/Avatar/Projectiles", Name);

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
    }

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 300;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void OnSpawn(IEntitySource source)
    {
        bool inSpace = Projectile.Center.Y <= Main.maxTilesY * 3.2f;
        if (inSpace)
            FrameX = Main.rand.NextFromList(1f, 2f);
        else
            FrameX = Main.rand.NextFromList(0f, 2f);

        FrameY = 0f;
        if (Main.rand.NextBool(4))
            FrameY = 1f;
        if (Main.rand.NextBool(16))
            FrameY = 2f;

        StartingScale = 1f;
    }

    public override void AI()
    {
        switch (FrameY)
        {
            case 0:
                Projectile.Resize(46, 46);
                break;
            case 1:
                Projectile.Resize(68, 68);
                break;
            case 2:
                Projectile.Resize(136, 136);
                break;
        }

        // Ensure the rift is present. If it isn't, die immediately.
        int riftIndex = NPC.FindFirstNPC(ModContent.NPCType<AvatarRift>());
        if (riftIndex == -1)
        {
            Projectile.Kill();
            return;
        }

        // Move towards the rift if the suck attack is ongoing.
        NPC rift = Main.npc[riftIndex];
        bool suckAttack = rift.As<AvatarRift>().CurrentAttack == AvatarRift.RiftAttackType.SuckPlayerIn;
        if (suckAttack)
        {
            Vector2 flyDestination = rift.Center;
            float distanceToDestination = Projectile.Distance(flyDestination);
            float flySpeed = Utils.Remap(distanceToDestination, 546f, 240f, 11.25f, 26f);
            Vector2 idealVelocity = Projectile.SafeDirectionTo(flyDestination) * flySpeed;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealVelocity, 0.06f);

            // Shrink when really close to the rift to give an indication of disappearing.
            Projectile.scale = InverseLerp(40f, 150f, distanceToDestination) * StartingScale;

            // Die once close enough to the rift, indicating that it has been completely sucked up.
            if (Projectile.scale <= 0.02f)
            {
                AvatarRiftSuckVisualsManager.AddNewRubble((int)FrameX, (int)FrameY);
                Projectile.Kill();
            }
        }

        // If the suck attack is not ongoing, adhere to gravity and stop doing damage.
        else
        {
            // Immediately disappear if the rubble was already disappearing anyway.
            if (Projectile.scale <= 0.6f)
                Projectile.Kill();

            Projectile.velocity.X *= 0.985f;
            Projectile.velocity.Y = Clamp(Projectile.velocity.Y + 0.4f, -10f, 18f);

            // Break if ground was hit.
            Projectile.tileCollide = true;

            // Disable damage.
            Projectile.damage = 0;
        }

        // Spin based on velocity.
        Projectile.rotation += Projectile.velocity.X * 1.06f / Projectile.width;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        SoundEngine.PlaySound(SoundID.Item50, Projectile.Center);

        // Create dust.
        for (int i = 0; i < 25; i++)
        {
            int dustID = FrameY == 0 ? DustID.Dirt : DustID.Stone;
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(3f, 3f), dustID);
            dust.velocity = -Vector2.UnitY.RotatedByRandom(0.8f) * Main.rand.NextFloat(2f, 10f);
            dust.scale = Main.rand.NextFloat(0.75f, 2.4f);
            dust.noGravity = dust.velocity.Length() >= 7f;
        }

        return true;
    }

    public override Color? GetAlpha(Color lightColor)
    {
        // Make rubble glow purple as it enters the portal.
        float glowInterpolant = InverseLerp(0.9f, 0.5f, Projectile.scale / StartingScale);
        Color glowColor = Color.Lerp(Color.MediumPurple, Color.Violet, Projectile.identity % 9f / 12f);
        Color baseColor = Color.Lerp(lightColor, glowColor with { A = 0 }, glowInterpolant);
        return baseColor * Projectile.Opacity;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        lightColor *= 5f;

        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Texture2D snowTexture = GennedAssets.Textures.Projectiles.AcceleratingRubbleSnow;
        float snowInterpolant = RiftEclipseSnowSystem.SnowHeight / RiftEclipseSnowSystem.MaxSnowHeight;
        Rectangle frame = texture.Frame(3, 3, (int)FrameX, (int)FrameY);
        Vector2 origin = frame.Size() * 0.5f;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Main.spriteBatch.Draw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor) * (1f - snowInterpolant), Projectile.rotation, origin, Projectile.scale, 0, 0f);
        Main.spriteBatch.Draw(snowTexture, drawPosition, frame, Projectile.GetAlpha(lightColor) * snowInterpolant, Projectile.rotation, origin, Projectile.scale, 0, 0f);

        return false;
    }
}
