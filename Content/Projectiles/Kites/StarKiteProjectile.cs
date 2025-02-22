using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Friendly;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Projectiles.Kites;

public class StarKiteProjectile : ModProjectile, IPixelatedPrimitiveRenderer
{
    /// <summary>
    /// The owner of this kite.
    /// </summary>
    public Entity Owner
    {
        get
        {
            if (Projectile.ai[0] >= 500)
                return Main.npc[(int)Projectile.ai[0] - 500];

            return Main.player[(int)Projectile.ai[0]];
        }
    }

    /// <summary>
    /// The starting point of the kite.
    /// </summary>
    public Vector2 KiteStart
    {
        get
        {
            if (Owner is NPC)
                return Owner.Center + Vector2.UnitX * Owner.direction * 22f;

            return Owner.Center + Vector2.UnitX * Owner.direction * 14f;
        }
    }

    /// <summary>
    /// The control points responsible for drawing the kite's string.
    /// </summary>
    public Vector2[] StringControlPoints
    {
        get;
        set;
    } = new Vector2[13];

    /// <summary>
    /// The ideal string length of the kite.
    /// </summary>
    public ref float StringLength => ref Projectile.ai[1];

    /// <summary>
    /// The amount of distance that should be discounted when performing string sag visuals.
    /// </summary>
    public ref float SagDistanceCheckOffset => ref Projectile.localAI[0];

    public static int TotalExtraUpdates => 60;

    public override string Texture => GetAssetPath("Content/MiscProjectiles", Name);

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = TotalExtraUpdates;
    }

    public override void SetDefaults()
    {
        Projectile.netImportant = true;
        Projectile.width = 40;
        Projectile.height = 40;
        Projectile.penetrate = -1;
        Projectile.extraUpdates = TotalExtraUpdates;
    }

    public override void PostAI()
    {
        bool shouldDie = false;

        if (Owner is Player player)
        {
            // Die if the owner is frozen, stoned by Medusa, or has the cursed debuff.
            if (player.CCed || player.noItems)
                shouldDie = true;

            // Die if the owner is no longer holding the kite item.
            if (player.inventory[player.selectedItem].shoot != Type)
                shouldDie = true;

            // Die if the owner is being pulled by a hook.
            if (player.pulley)
                shouldDie = true;

            // Die if the owner is dead.
            if (player.dead)
                shouldDie = true;
        }
        else if (Owner is NPC npc)
        {
            // Die if the owner is dead.
            if (!npc.active)
                shouldDie = true;
        }

        // Die if outrageously far from the owner.
        if (!shouldDie)
            shouldDie = (Owner.Center - Projectile.Center).Length() > 2000f;

        if (shouldDie)
        {
            Projectile.Kill();
            return;
        }

        // Move the kite to or from the owner.
        float minKiteDistance = 4f;
        float maxKiteDistance = 420f;
        if (Projectile.owner == Main.myPlayer && Projectile.extraUpdates == 0)
        {
            float oldKiteDistance = StringLength;
            if (StringLength == 0f)
                StringLength = maxKiteDistance * 0.5f;

            float newStringLength = StringLength;
            if (Owner is Player)
            {
                if (Main.mouseRight)
                    newStringLength -= 5f;
                if (Main.mouseLeft)
                    newStringLength += 5f;
            }

            else if (Owner is NPC npc && npc.type == ModContent.NPCType<Solyn>())
            {
                float idealStringLength = Lerp(0.84f, 0.96f, Cos01(Main.GlobalTimeWrappedHourly * 0.8f)) * maxKiteDistance;
                if (npc.As<Solyn>().ReelInKite)
                    newStringLength -= 12f;
                else
                    newStringLength = Lerp(newStringLength, idealStringLength, 0.09f);
            }

            StringLength = Clamp(newStringLength, minKiteDistance, maxKiteDistance);
            if (oldKiteDistance != newStringLength)
                Projectile.netUpdate = true;
        }

        // Vanilla spaghetti ig?
        if (Projectile.numUpdates == 1)
            Projectile.extraUpdates = 0;

        float cloudAlpha = Main.cloudAlpha;
        float windSpeed = 0f;
        if (WorldGen.InAPlaceWithWind(Projectile.position, Projectile.width, Projectile.height))
            windSpeed = Main.WindForVisuals;
        float verticalSpeed = InverseLerp(0.2f, 0.5f, Math.Abs(windSpeed)) * 0.5f;

        Vector2 hoverDestination = Projectile.Center + new Vector2(windSpeed, Sin(Main.GlobalTimeWrappedHourly * 1.1f) + cloudAlpha * 5f) * 25f;
        Vector2 idealVelocity = hoverDestination - Projectile.Center;
        idealVelocity = idealVelocity.SafeNormalize(Vector2.Zero) * (3f + cloudAlpha * 7f) * new Vector2(1f, 0.5f);
        if (verticalSpeed == 0f)
            idealVelocity = Projectile.velocity;

        float oldYSpeed = Projectile.velocity.Y;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealVelocity, 0.075f);

        // Move upward and downward.
        Projectile.velocity.Y = oldYSpeed - verticalSpeed + (0.02f + verticalSpeed * 0.25f);
        Projectile.velocity.Y = Clamp(Projectile.velocity.Y, -2f, 2f);

        // Naturally decelerate and fall.
        if (Projectile.Center.Y + Projectile.velocity.Y < hoverDestination.Y)
            Projectile.velocity.Y = Lerp(Projectile.velocity.Y, Projectile.velocity.Y + verticalSpeed + 0.01f, 0.75f);
        Projectile.velocity.X = Projectile.velocity.X * 0.98f;

        // Make the string approach the starting point upon being too far from the player.
        float currentStringLength = Projectile.Distance(KiteStart);
        Vector2 directionToStart = Projectile.DirectionTo(KiteStart);
        if (currentStringLength > StringLength)
        {
            float stringLengthDeviance = currentStringLength - StringLength;
            Projectile.Center += directionToStart * stringLengthDeviance;
            Projectile.velocity.Y += directionToStart.Y * 0.05f;
            if (verticalSpeed > 0f)
                Projectile.velocity.Y = Projectile.velocity.Y - 0.15f;

            Projectile.velocity.X += directionToStart.X * 0.2f;

            // Die if the string length is at its minimum.
            if (StringLength == minKiteDistance && Projectile.owner == Main.myPlayer)
            {
                Projectile.Kill();
                return;
            }
        }

        // Make the string not sag as much if the owner is moving quickly away from the kite.
        float moveAwayInterpolant = Vector2.Dot(Owner.velocity.SafeNormalize(Vector2.Zero), directionToStart);
        SagDistanceCheckOffset = Lerp(SagDistanceCheckOffset, Owner.velocity.Length() * moveAwayInterpolant * 15f, 0.08f);
        if (float.IsNaN(SagDistanceCheckOffset))
            SagDistanceCheckOffset = 0f;

        // Update the owner's direction.
        Vector2 offsetFromCenter = Projectile.Center - KiteStart;
        int dir = offsetFromCenter.X.NonZeroSign();
        if ((Math.Abs(offsetFromCenter.X) > Math.Abs(offsetFromCenter.Y) * 0.5f || Owner is NPC) && Abs(offsetFromCenter.X) >= 32f)
            Owner.direction = dir;

        if (verticalSpeed == 0f && Projectile.velocity.Y > -0.02f)
            Projectile.rotation *= 0.95f;
        else
        {
            float baseRotation = (-directionToStart).ToRotation() + PiOver4;
            if (Projectile.spriteDirection == -1)
                baseRotation -= Owner.direction * PiOver2;

            Projectile.rotation = baseRotation + Projectile.velocity.X * 0.05f;
        }

        // Determine sag values based on proximity to the owner.
        float sagRatio = InverseLerp(400f, 75f, currentStringLength + SagDistanceCheckOffset) * 0.5f + 0.015f;
        Vector2 sagOffset = Vector2.UnitY * currentStringLength * sagRatio;

        for (int i = 0; i < StringControlPoints.Length; i++)
        {
            float completionRatio = i / (float)(StringControlPoints.Length - 1f);
            Vector2 previousStringPosition = StringControlPoints[i];
            Vector2 linearStringPosition = Vector2.Lerp(KiteStart, Projectile.Center, completionRatio);

            // Calculate the sag offset based on how long the string is.
            // The longer the string is, closer it should be to being hung taut, and as such it sags less the longer it is.
            Vector2 newStringPosition = linearStringPosition + Vector2.UnitY * Convert01To010(completionRatio) * sagOffset;

            // Keep string positions floored to whole numbers to prevent subpixel problems with tile collision.
            newStringPosition = newStringPosition.Floor();

            // Keep string control points on the ground.
            while (Collision.SolidCollision(newStringPosition - Vector2.One, 2, 2))
                newStringPosition.Y--;

            // Update the string control point position.
            Vector2 stringOffset = (newStringPosition - previousStringPosition) * 0.5f;
            StringControlPoints[i] += Collision.TileCollision(previousStringPosition - Vector2.One * 2f, stringOffset, 4, 4);
        }

        // Prevent the kite from dying naturally by resetting its timeLeft.
        Projectile.timeLeft = 2;
    }

    public void DrawWithPixelation()
    {
        // Draw the kite's end.
        Texture2D kiteEndTexture = TextureAssets.Projectile[Type].Value;
        Vector2 kiteEnd = Projectile.Center - Main.screenPosition;
        SpriteEffects kiteDirection = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        Main.EntitySpriteDraw(kiteEndTexture, kiteEnd, null, Projectile.GetAlpha(Color.White), Projectile.rotation, kiteEndTexture.Size() * 0.5f, Projectile.scale, kiteDirection, 0f);
    }

    // Disable death by tile collision.
    public override bool OnTileCollide(Vector2 oldVelocity) => false;

    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        PrimitiveSettings settings = new PrimitiveSettings(_ => 1f, _ => Projectile.GetAlpha(Color.White), _ => Vector2.Zero, Shader: ShaderManager.GetShader("Luminance.StandardPrimitiveShader"), Pixelate: true);
        PrimitiveRenderer.RenderTrail(StringControlPoints, settings, 30);
    }
}
