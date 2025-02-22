using Luminance.Assets;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.Items.Pets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static NoxusBoss.Content.NPCs.Bosses.NamelessDeity.NamelessDeityBoss;

namespace NoxusBoss.Content.Projectiles.Pets;

public class BabyNameless : ModProjectile
{
    /// <summary>
    /// Nameless' current censor position.
    /// </summary>
    public Vector2 CensorPosition
    {
        get;
        set;
    }

    /// <summary>
    /// The render composite that Nameless uses.
    /// </summary>
    public NamelessDeityRenderComposite RenderComposite
    {
        get;
        private set;
    }

    /// <summary>
    /// The position of Nameless' left hand.
    /// </summary>
    public Vector2 LeftHandPosition
    {
        get;
        set;
    }
    /// <summary>
    /// The position of Nameless' right hand.
    /// </summary>

    public Vector2 RightHandPosition
    {
        get;
        set;
    }

    /// <summary>
    /// The ideal position of Nameless' censor.
    /// </summary>
    public Vector2 IdealCensorPosition => Projectile.Center - Vector2.UnitY.RotatedBy(Projectile.rotation) * Projectile.scale * 120f;

    /// <summary>
    /// The owner of this pet.
    /// </summary>
    public Player Owner => Main.player[Projectile.owner];

    /// <summary>
    /// How long Nameless has existed for overall, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        Main.projPet[Projectile.type] = true;
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2750;

        ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, 1).
            WithOffset(new Vector2(-274f, 324f));
    }

    public override void SetDefaults()
    {
        Projectile.width = 800;
        Projectile.height = 800;
        Projectile.scale = 0.8f;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 90000;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
    }

    public override void AI()
    {
        if (Projectile.localAI[0] == 0f)
        {
            Projectile.localAI[0] = 1f;
            LeftHandPosition = RightHandPosition = Projectile.Center;
            RenderComposite = new(Projectile);
        }

        CheckActive();

        // Hover near the owner.
        Vector2 hoverDestination = Owner.Center + new Vector2(Owner.direction * -50f, -36f);
        if (!Projectile.WithinRange(Owner.Center, 250f))
            Projectile.velocity = (Projectile.velocity + Projectile.SafeDirectionTo(hoverDestination) * 0.2f).ClampLength(0f, 24f);
        else
            Projectile.velocity *= 0.975f;
        if (Vector2.Dot(Projectile.velocity, Projectile.SafeDirectionTo(hoverDestination)) < 0f)
            Projectile.velocity *= 0.95f;
        Projectile.rotation = Projectile.velocity.X * 0.004f;

        if (!Projectile.WithinRange(Owner.Center, 2300f))
        {
            Projectile.Center = Owner.Center - Vector2.UnitX * Owner.direction * 300f;
            Projectile.velocity *= 0.05f;
            Projectile.netUpdate = true;
        }

        // Have hands hover near Nameless.
        float horizontalOffset = Cos(RenderComposite.Time / 20f) * 25f;
        float handHoverOffset = 100f;
        Vector2 leftHandDestination = Projectile.Center + new Vector2(-660f - horizontalOffset, handHoverOffset).RotatedBy(Projectile.rotation) * Projectile.scale;
        Vector2 rightHandDestination = Projectile.Center + new Vector2(660f + horizontalOffset, handHoverOffset).RotatedBy(Projectile.rotation) * Projectile.scale;
        LeftHandPosition = Utils.MoveTowards(Vector2.Lerp(LeftHandPosition, leftHandDestination, 0.09f), leftHandDestination, 23f);
        RightHandPosition = Utils.MoveTowards(Vector2.Lerp(RightHandPosition, rightHandDestination, 0.09f), rightHandDestination, 23f);

        // Update hands.
        ArmsStep armHandler = RenderComposite.Find<ArmsStep>();
        while (armHandler.Hands.Count < 2)
            armHandler.Hands.Add(new(Projectile.Center, true));

        armHandler.Hands[0].FreeCenter = LeftHandPosition;
        armHandler.Hands[1].FreeCenter = RightHandPosition;
        foreach (NamelessDeityHand hand in armHandler.Hands)
            hand.Update();

        if (!IdealCensorPosition.WithinRange(CensorPosition, Projectile.scale * 40f))
            CensorPosition = IdealCensorPosition;

        // Update the composite.
        RenderComposite.Update();
        if (Time < 10f)
        {
            DanglingVinesStep vineHandler = RenderComposite.Find<DanglingVinesStep>();
            for (int i = 0; i < 25; i++)
                vineHandler.HandleDanglingVineRotation(Projectile);
        }
        Time++;

        // Update wings.
        RenderComposite.Find<WingsStep>().Wings.Update(WingMotionState.Flap, RenderComposite.Time / 60f % 1f);
    }

    public void CheckActive()
    {
        // Keep the projectile from disappearing as long as the player isn't dead and has the pet buff.
        if (!Owner.dead && Owner.HasBuff(Erilucyxwyn.BuffID))
            Projectile.timeLeft = 2;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        RenderComposite ??= new(Projectile);
        if (Main.gameMenu)
        {
            Projectile.scale = 0.075f;
            CensorPosition = IdealCensorPosition;
            RenderComposite.Update();
            RenderComposite.Find<WingsStep>().Wings.Update(WingMotionState.Flap, RenderComposite.Time / 60f % 1f);
        }

        RenderComposite.Render(CensorPosition - Main.screenPosition, Projectile.Center - Main.screenPosition, Projectile.rotation, Projectile.scale);
        return false;
    }
}
